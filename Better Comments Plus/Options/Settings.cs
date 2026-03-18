using BetterCommentsPlus.CommentsTagging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;

namespace BetterCommentsPlus.Options
{
    public class Settings : PropertyChangeNotifier, ISettings
    {
        #region Fields

        private string font = string.Empty;
        private double size = -1.5;
        private double opacity = 0.8;
        private bool italic = true;
        private bool highlightTaskKeywordOnly = false;
        private bool underlineImportantComments = false;
        private bool strikethroughDoubleComments = false;
        private bool highlightCriteriaItself = false;

        public bool IsDragging { get; set; }
        private bool _isSyncing;

        private readonly ObservableCollection<CommentToken> globalCommentTokens
            = new ObservableCollection<CommentToken>
            {
                new CommentToken(type: CommentType.Important,       defaultValue: "#IMPORTANT",     value: "#IMPORTANT",   colorHex: "#FFFF0000"),
                new CommentToken(type: CommentType.Remove,          defaultValue: "#REMOVE",        value: "#REMOVE",      colorHex: "#FF808080"),
                new CommentToken(type: CommentType.Question,        defaultValue: "#QUESTION",      value: "#QUESTION",    colorHex: "#FFFFFF00"),
                new CommentToken(type: CommentType.Task,            defaultValue: "#TASK",          value: "#TASK",        colorHex: "#FFEB690A"),
            };

        private readonly ObservableCollection<CommentToken> solutionCommentTokens
            = new ObservableCollection<CommentToken>();

        private readonly ObservableCollection<StyleRule> styleRules
            = new ObservableCollection<StyleRule>();

        private UnifiedConfig unifiedConfig;

        public event EventHandler ConfigurationChanged;

        public virtual void OnConfigurationChanged()
        {
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion Fields

        #region Singleton

        private static volatile Settings instance;
        private static readonly object syncLock = new object();

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                            instance = new Settings();
                    }
                }

                return instance;
            }
        }

        private Settings()
        {
            ResetTokens = new RelayCommand(SetTokensToDefault);
            ResetSolutionTokens = new RelayCommand(ClearSolutionTokens);
            unifiedConfig = UnifiedConfig.CreateDefault();
            SettingsStore.LoadSettings(this);

            InitializeTokenCollection(globalCommentTokens);
            InitializeTokenCollection(solutionCommentTokens);

            SyncCommentTokensToUnifiedConfig();
            SyncCommentTokensToStyleRules();

            globalCommentTokens.CollectionChanged += CommentTokens_CollectionChanged;
            solutionCommentTokens.CollectionChanged += CommentTokens_CollectionChanged;
            styleRules.CollectionChanged += StyleRules_CollectionChanged;
        }

        private void InitializeTokenCollection(ObservableCollection<CommentToken> tokens)
        {
            foreach (var token in tokens)
            {
                if (token.BackgroundStyle == null)
                {
                    token.BackgroundStyle = new BackgroundStyle();
                }

                if (string.IsNullOrEmpty(token.BackgroundStyle.Shape))
                    token.BackgroundStyle.Shape = "Tag";
                if (string.IsNullOrEmpty(token.BackgroundStyle.Blur))
                    token.BackgroundStyle.Blur = "None";
                if (string.IsNullOrEmpty(token.BackgroundStyle.Alpha))
                    token.BackgroundStyle.Alpha = "1/10";
                if (string.IsNullOrEmpty(token.BackgroundStyle.ColorHex))
                    token.BackgroundStyle.ColorHex = token.ColorHex;

                token.PropertyChanged += CommentToken_PropertyChanged;
            }
        }

        #endregion Singleton

        #region Settings Properties

        [Setting]
        public string Font
        {
            get { return font; }
            set { SetField(ref font, value); }
        }

        [Setting]
        public double Size
        {
            get { return size; }
            set { SetField(ref size, value); }
        }

        [Setting]
        public bool Italic
        {
            get { return italic; }
            set { SetField(ref italic, value); }
        }

        [Setting]
        public double Opacity
        {
            get { return opacity; }
            set { SetField(ref opacity, value); }
        }

        [Setting]
        public bool HighlightTaskKeywordOnly
        {
            get { return highlightTaskKeywordOnly; }
            set { SetField(ref highlightTaskKeywordOnly, value); }
        }

        [Setting]
        public bool UnderlineImportantComments
        {
            get { return underlineImportantComments; }
            set { SetField(ref underlineImportantComments, value); }
        }

        [Setting]
        public bool StrikethroughDoubleComments
        {
            get { return strikethroughDoubleComments; }
            set { SetField(ref strikethroughDoubleComments, value); }
        }

        [Setting]
        public bool HighlightCriteriaItself
        {
            get { return highlightCriteriaItself; }
            set { SetField(ref highlightCriteriaItself, value); }
        }

        [Setting]
        public string GlobalCommentTokensAsString
        {
            get { return ConvertCommentTokensToString(globalCommentTokens); }
            set { UpdateCommentTokens(value, globalCommentTokens); }
        }

        #endregion Settings Properties

        #region Non-Settings Properties & Commands

        public RelayCommand ResetTokens { get; }
        public RelayCommand ResetSolutionTokens { get; }

        public ObservableCollection<CommentToken> GlobalCommentTokens
        {
            get { return globalCommentTokens; }
        }

        public ObservableCollection<CommentToken> SolutionCommentTokens
        {
            get { return solutionCommentTokens; }
        }

        public ObservableCollection<StyleRule> StyleRules
        {
            get { return styleRules; }
        }

        public System.Collections.Generic.IEnumerable<CommentToken> CommentTokens
        {
            get { return solutionCommentTokens.Concat(globalCommentTokens); }
        }

        public UnifiedConfig UnifiedConfig
        {
            get { return unifiedConfig; }
        }

        #endregion Non-Settings Properties & Commands

        #region ISettings Members

        public string Key => "BetterCommentsPlus";

        #endregion ISettings Members

        #region Public Methods

        public CommentToken GetToken(CommentType type)
        {
            return globalCommentTokens.FirstOrDefault(t => t.Type == type);
        }

        public string GetTokenValue(CommentType type)
        {
            var token = GetToken(type);
            return token?.CurrentValue ?? "";
        }

        public void ClearSolutionTokens()
        {
            _isSyncing = true;
            try
            {
                solutionCommentTokens.Clear();
                SyncCommentTokensToUnifiedConfig();
                SyncCommentTokensToStyleRules();
                OnConfigurationChanged();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        public void CopyAllFromGlobalToSolution()
        {
            _isSyncing = true;
            try
            {
                foreach (var globalToken in globalCommentTokens)
                {
                    var newToken = new CommentToken(
                        type: globalToken.Type,
                        defaultValue: globalToken.DefaultValue,
                        value: globalToken.CurrentValue,
                        colorHex: globalToken.ColorHex)
                    {
                        IsBold = globalToken.IsBold,
                        IsItalic = globalToken.IsItalic,
                        HasUnderline = globalToken.HasUnderline,
                        HasStrikethrough = globalToken.HasStrikethrough,
                        IsForegroundActive = globalToken.IsForegroundActive,
                        IsDynamic = true,
                        RuleId = Guid.NewGuid().ToString(),
                        BackgroundStyle = new BackgroundStyle
                        {
                            IsActive = globalToken.BackgroundStyle?.IsActive ?? false,
                            ColorHex = globalToken.BackgroundStyle?.ColorHex,
                            Shape = globalToken.BackgroundStyle?.Shape ?? "Tag",
                            Blur = globalToken.BackgroundStyle?.Blur ?? "None",
                            Alpha = globalToken.BackgroundStyle?.Alpha ?? "1/10",
                            IsCaseSensitive = globalToken.BackgroundStyle?.IsCaseSensitive ?? true,
                            AllowPartialMatch = globalToken.BackgroundStyle?.AllowPartialMatch ?? false
                        }
                    };
                    newToken.PropertyChanged += CommentToken_PropertyChanged;
                    solutionCommentTokens.Add(newToken);
                }
                SyncCommentTokensToUnifiedConfig();
                SyncCommentTokensToStyleRules();
                OnConfigurationChanged();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        #endregion Public Methods

        #region Private Helpers

        private void SetTokensToDefault()
        {
            _isSyncing = true;
            try
            {
                globalCommentTokens.Clear();

                var defaultTokens = new[]
                {
                    new { Type = CommentType.Important, DefaultValue = "#IMPORTANT", Value = "#IMPORTANT", ColorHex = "#FFFF0000", IsBold = true, IsItalic = false, HasUnderline = false, HasStrikethrough = false },
                    new { Type = CommentType.Remove, DefaultValue = "#REMOVE", Value = "#REMOVE", ColorHex = "#FF808080", IsBold = false, IsItalic = false, HasUnderline = false, HasStrikethrough = true },
                    new { Type = CommentType.Question, DefaultValue = "#QUESTION", Value = "#QUESTION", ColorHex = "#FFFFFF00", IsBold = false, IsItalic = false, HasUnderline = false, HasStrikethrough = false },
                    new { Type = CommentType.Task, DefaultValue = "#TASK", Value = "#TASK", ColorHex = "#FFEB690A", IsBold = false, IsItalic = false, HasUnderline = false, HasStrikethrough = false }
                };

                foreach (var tokenInfo in defaultTokens)
                {
                    var newToken = new CommentToken(
                        type: tokenInfo.Type,
                        defaultValue: tokenInfo.DefaultValue,
                        value: tokenInfo.Value,
                        colorHex: tokenInfo.ColorHex)
                    {
                        IsBold = tokenInfo.IsBold,
                        IsItalic = tokenInfo.IsItalic,
                        HasUnderline = tokenInfo.HasUnderline,
                        HasStrikethrough = tokenInfo.HasStrikethrough,
                        BackgroundStyle = new BackgroundStyle
                        {
                            IsActive = false,
                            ColorHex = null,
                            Shape = "Tag",
                            Blur = "None",
                            Alpha = "1/10",
                            IsCaseSensitive = true,
                            AllowPartialMatch = false
                        }
                    };

                    newToken.PropertyChanged += CommentToken_PropertyChanged;
                    globalCommentTokens.Add(newToken);
                }

                SyncCommentTokensToUnifiedConfig();
                SyncCommentTokensToStyleRules();
                OnConfigurationChanged();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void UpdateCommentTokens(string tokensString, ObservableCollection<CommentToken> targetCollection)
        {
            if (tokensString.IsNullOrWhiteSpace())
                return;

            _isSyncing = true;
            try
            {
                var pairs = tokensString.Split('|').Select(p => p.Split(',')).ToList();

                foreach (var pair in pairs)
                {
                    if (pair.Length < 2) continue;

                    var token = targetCollection.FirstOrDefault(t => t.IsOfType(pair[0]));

                    if (token != null)
                    {
                        token.CurrentValue = pair[1].Trim();
                        if (pair.Length > 2 && !string.IsNullOrWhiteSpace(pair[2]))
                            token.ColorHex = pair[2].Trim();
                        if (pair.Length > 3 && bool.TryParse(pair[3], out bool isBold))
                            token.IsBold = isBold;
                        if (pair.Length > 4 && bool.TryParse(pair[4], out bool isItalic))
                            token.IsItalic = isItalic;
                        if (pair.Length > 5 && bool.TryParse(pair[5], out bool hasUnderline))
                            token.HasUnderline = hasUnderline;
                        if (pair.Length > 6 && bool.TryParse(pair[6], out bool hasStrikethrough))
                            token.HasStrikethrough = hasStrikethrough;
                        if (pair.Length > 7 && bool.TryParse(pair[7], out bool isForegroundActive))
                            token.IsForegroundActive = isForegroundActive;
                    }
                }

                EnsureDefaultTokensExist(targetCollection);
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void EnsureDefaultTokensExist(ObservableCollection<CommentToken> targetCollection)
        {
            if (targetCollection != globalCommentTokens) return;

            var defaultTypes = new[]
            {
                CommentType.Important,
                CommentType.Question,
                CommentType.Remove,
                CommentType.Task
            };

            foreach (var type in defaultTypes)
            {
                if (!targetCollection.Any(t => t.Type == type))
                {
                    CommentToken newToken = null;
                    switch (type)
                    {
                        case CommentType.Important:
                            newToken = new CommentToken(type, "#IMPORTANT", "#IMPORTANT", "#FFFF0000")
                            {
                                IsBold = true,
                                IsItalic = false
                            };
                            break;
                        case CommentType.Question:
                            newToken = new CommentToken(type, "#QUESTION", "#QUESTION", "#FFFFFF00");
                            break;
                        case CommentType.Remove:
                            newToken = new CommentToken(type, "#REMOVE", "#REMOVE", "#FF808080")
                            {
                                HasStrikethrough = true
                            };
                            break;
                        case CommentType.Task:
                            newToken = new CommentToken(type, "#TASK", "#TASK", "#FFEB690A");
                            break;
                    }

                    if (newToken != null)
                    {
                        if (newToken.BackgroundStyle == null)
                        {
                            newToken.BackgroundStyle = new BackgroundStyle();
                        }

                        if (string.IsNullOrEmpty(newToken.BackgroundStyle.Shape))
                            newToken.BackgroundStyle.Shape = "Tag";
                        if (string.IsNullOrEmpty(newToken.BackgroundStyle.Blur))
                            newToken.BackgroundStyle.Blur = "None";
                        if (string.IsNullOrEmpty(newToken.BackgroundStyle.Alpha))
                            newToken.BackgroundStyle.Alpha = "1/10";
                        if (string.IsNullOrEmpty(newToken.BackgroundStyle.ColorHex))
                            newToken.BackgroundStyle.ColorHex = newToken.ColorHex;

                        newToken.PropertyChanged += CommentToken_PropertyChanged;
                        targetCollection.Add(newToken);
                    }
                }
            }
        }

        private string ConvertCommentTokensToString(ObservableCollection<CommentToken> tokens)
        {
            var r = string.Join("|", tokens.Select(t => t.ToString()));
            return r;
        }

        public void SyncCommentTokensToUnifiedConfig()
        {
            unifiedConfig.Comments.Clear();
            int order = 1;

            foreach (var token in solutionCommentTokens.Concat(globalCommentTokens))
            {
                var rule = unifiedConfig.Comments.FirstOrDefault(r => r.Criteria == token.CurrentValue);
                if (rule == null)
                {
                    rule = new StyleRule
                    {
                        Order = order++,
                        IsActive = true,
                        IsPredefined = !token.IsDynamic,
                        Criteria = token.CurrentValue,
                        Id = token.RuleId
                    };
                    unifiedConfig.Comments.Add(rule);
                }

                if (rule.Foreground != null)
                {
                    rule.Foreground.ColorHex = token.ColorHex;
                    rule.Foreground.IsBold = token.IsBold;
                    rule.Foreground.IsItalic = token.IsItalic;
                    rule.Foreground.HasUnderline = token.HasUnderline;
                    rule.Foreground.HasStrikethrough = token.HasStrikethrough;
                    rule.Foreground.IsActive = token.IsForegroundActive;
                }

                if (rule.Background != null && token.BackgroundStyle != null)
                {
                    rule.Background.IsActive = token.BackgroundStyle.IsActive;
                    rule.Background.ColorHex = token.BackgroundStyle.ColorHex;
                    rule.Background.Shape = token.BackgroundStyle.Shape;
                    rule.Background.Blur = token.BackgroundStyle.Blur;
                    rule.Background.Alpha = token.BackgroundStyle.Alpha;
                    rule.Background.IsCaseSensitive = token.BackgroundStyle.IsCaseSensitive;
                    rule.Background.AllowPartialMatch = token.BackgroundStyle.AllowPartialMatch;
                }
            }
        }

        public void SyncUnifiedConfigToCommentTokens()
        {
        }

        private void CommentToken_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isSyncing)
                return;

            SyncCommentTokensToUnifiedConfig();
            OnConfigurationChanged();
        }

        private void CommentTokens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isSyncing)
                return;

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is CommentToken token)
                    {
                        token.PropertyChanged -= CommentToken_PropertyChanged;
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is CommentToken token)
                    {
                        if (token.BackgroundStyle == null)
                        {
                            token.BackgroundStyle = new BackgroundStyle();
                        }

                        if (string.IsNullOrEmpty(token.BackgroundStyle.Shape))
                            token.BackgroundStyle.Shape = "Tag";
                        if (string.IsNullOrEmpty(token.BackgroundStyle.Blur))
                            token.BackgroundStyle.Blur = "None";
                        if (string.IsNullOrEmpty(token.BackgroundStyle.Alpha))
                            token.BackgroundStyle.Alpha = "1/10";
                        if (string.IsNullOrEmpty(token.BackgroundStyle.ColorHex))
                            token.BackgroundStyle.ColorHex = token.ColorHex;

                        token.PropertyChanged += CommentToken_PropertyChanged;
                    }
                }
            }

            if (!IsDragging)
            {
                _isSyncing = true;
                try
                {
                    SyncCommentTokensToUnifiedConfig();
                    SyncCommentTokensToStyleRules();
                    OnConfigurationChanged();
                }
                finally
                {
                    _isSyncing = false;
                }
            }
        }

        private void StyleRules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isSyncing || IsDragging)
                return;

            _isSyncing = true;
            try
            {
                SyncStyleRulesToCommentTokens();
                SyncStyleRulesToUnifiedConfig();
                OnConfigurationChanged();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        public void SyncCommentTokensToStyleRules()
        {
            styleRules.Clear();
            int order = 1;
            foreach (var token in solutionCommentTokens.Concat(globalCommentTokens))
            {
                var rule = ConvertCommentTokenToStyleRule(token, order);
                styleRules.Add(rule);
                order++;
            }
        }

        public void SyncStyleRulesToCommentTokens()
        {
        }

        public void SyncStyleRulesToUnifiedConfig()
        {
            unifiedConfig.Comments.Clear();
            foreach (var rule in styleRules)
            {
                unifiedConfig.Comments.Add(rule);
            }
        }

        private StyleRule ConvertCommentTokenToStyleRule(CommentToken token, int order)
        {
            var rule = new StyleRule
            {
                Order = order,
                IsActive = true,
                IsPredefined = !token.IsDynamic,
                Criteria = token.CurrentValue,
                Id = token.RuleId
            };

            rule.Foreground.ColorHex = token.ColorHex;
            rule.Foreground.IsBold = token.IsBold;
            rule.Foreground.IsItalic = token.IsItalic;
            rule.Foreground.HasUnderline = token.HasUnderline;
            rule.Foreground.HasStrikethrough = token.HasStrikethrough;
            rule.Foreground.IsActive = token.IsForegroundActive;

            if (token.BackgroundStyle != null)
            {
                rule.Background.IsActive = token.BackgroundStyle.IsActive;
                rule.Background.ColorHex = token.BackgroundStyle.ColorHex;
                rule.Background.Shape = token.BackgroundStyle.Shape;
                rule.Background.Blur = token.BackgroundStyle.Blur;
                rule.Background.Alpha = token.BackgroundStyle.Alpha;
                rule.Background.IsCaseSensitive = token.BackgroundStyle.IsCaseSensitive;
                rule.Background.AllowPartialMatch = token.BackgroundStyle.AllowPartialMatch;
            }

            return rule;
        }

        private CommentToken ConvertStyleRuleToCommentToken(StyleRule rule)
        {
            return null;
        }

        #endregion Private Helpers
    }
}
