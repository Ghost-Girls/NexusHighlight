using BetterCommentsPlus.CommentsTagging;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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
        
        public bool IsDragging { get; set; }
        private bool _isSyncing;

        private readonly ObservableCollection<CommentToken> commentTokens
            = new ObservableCollection<CommentToken>
            {
                new CommentToken(type: CommentType.Important,       defaultValue: "#IMPORTANT",     value: "#IMPORTANT",   colorHex: "#FFFF0000"),
                new CommentToken(type: CommentType.Remove,          defaultValue: "#REMOVE",        value: "#REMOVE",      colorHex: "#FF808080"),
                new CommentToken(type: CommentType.Question,        defaultValue: "#QUESTION",      value: "#QUESTION",    colorHex: "#FFFFFF00"),
                new CommentToken(type: CommentType.Task,            defaultValue: "#TASK",          value: "#TASK",        colorHex: "#FFEB690A"),
            };

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
            unifiedConfig = UnifiedConfig.CreateDefault();
            SettingsStore.LoadSettings(this);
            SyncCommentTokensToUnifiedConfig();
            SyncCommentTokensToStyleRules();
            
            foreach (var token in commentTokens)
            {
                token.PropertyChanged += CommentToken_PropertyChanged;
            }
            
            commentTokens.CollectionChanged += CommentTokens_CollectionChanged;
            styleRules.CollectionChanged += StyleRules_CollectionChanged;
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
        public string CommentTokensAsString
        {
            get { return ConvertCommentTokensToString(); }
            set { UpdateCommentTokens(value); }
        }

        #endregion Settings Properties

        #region Non-Settings Properties & Commands

        public RelayCommand ResetTokens { get; }

        public ObservableCollection<CommentToken> CommentTokens
        {
            get { return commentTokens; }
        }

        public ObservableCollection<StyleRule> StyleRules
        {
            get { return styleRules; }
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
            return commentTokens.FirstOrDefault(t => t.Type == type);
        }

        public string GetTokenValue(CommentType type)
        {
            var token = GetToken(type);
            return token?.CurrentValue ?? "";
        }

        #endregion Public Methods

        #region Private Helpers

        private void SetTokensToDefault()
        {
            _isSyncing = true;
            try
            {
                foreach (var token in commentTokens)
                {
                    token.CurrentValue = token.DefaultValue;
                    token.IsBold = false;
                    token.IsItalic = false;
                    token.HasUnderline = false;
                    token.HasStrikethrough = false;
                    
                    switch (token.Type)
                    {
                        case CommentType.Important:
                            token.ColorHex = "#FFFF0000";
                            token.IsBold = true;
                            token.IsItalic = false;
                            break;
                        case CommentType.Question:
                            token.ColorHex = "#FFFFFF00";
                            break;
                        case CommentType.Remove:
                            token.ColorHex = "#FF808080";
                            token.HasStrikethrough = true;
                            break;
                        case CommentType.Task:
                            token.ColorHex = "#FFEB690A";
                            break;
                    }
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

        private void UpdateCommentTokens(string tokensString)
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
                    
                    var token = commentTokens.FirstOrDefault(t => t.IsOfType(pair[0]));

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
                    }
                }
                
                EnsureDefaultTokensExist();
            }
            finally
            {
                _isSyncing = false;
            }
        }
        
        private void EnsureDefaultTokensExist()
        {
            var defaultTypes = new[] 
            { 
                CommentType.Important, 
                CommentType.Question, 
                CommentType.Remove, 
                CommentType.Task 
            };
            
            foreach (var type in defaultTypes)
            {
                if (!commentTokens.Any(t => t.Type == type))
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
                        newToken.PropertyChanged += CommentToken_PropertyChanged;
                        commentTokens.Add(newToken);
                    }
                }
            }
        }

        private string ConvertCommentTokensToString()
        {
            var r = string.Join("|", commentTokens.Select(t => t.ToString()));
            return r;
        }

        public void SyncCommentTokensToUnifiedConfig()
        {
            foreach (var token in commentTokens)
            {
                var rule = unifiedConfig.Comments.FirstOrDefault(r => r.Criteria == token.CurrentValue);
                if (rule == null)
                {
                    rule = new StyleRule
                    {
                        Order = unifiedConfig.Comments.Count + 1,
                        IsActive = true,
                        IsPredefined = true,
                        Criteria = token.CurrentValue,
                        Id = $"comment-{token.Type.ToString().ToLower()}"
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
                }
            }
        }

        public void SyncUnifiedConfigToCommentTokens()
        {
            foreach (var rule in unifiedConfig.Comments)
            {
                var token = commentTokens.FirstOrDefault(t => t.CurrentValue == rule.Criteria);
                if (token != null && rule.Foreground != null)
                {
                    token.ColorHex = rule.Foreground.ColorHex;
                    token.IsBold = rule.Foreground.IsBold;
                    token.IsItalic = rule.Foreground.IsItalic;
                    token.HasUnderline = rule.Foreground.HasUnderline;
                    token.HasStrikethrough = rule.Foreground.HasStrikethrough;
                }
            }
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
            foreach (var token in commentTokens)
            {
                var rule = ConvertCommentTokenToStyleRule(token, order);
                styleRules.Add(rule);
                order++;
            }
        }

        public void SyncStyleRulesToCommentTokens()
        {
            commentTokens.Clear();
            foreach (var rule in styleRules)
            {
                var token = ConvertStyleRuleToCommentToken(rule);
                if (token != null)
                {
                    commentTokens.Add(token);
                }
            }
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

            return rule;
        }

        private CommentToken ConvertStyleRuleToCommentToken(StyleRule rule)
        {
            CommentType? type = null;
            bool isDynamic = false;
            
            if (rule.Id != null)
            {
                if (rule.Id.Contains("important"))
                    type = CommentType.Important;
                else if (rule.Id.Contains("question"))
                    type = CommentType.Question;
                else if (rule.Id.Contains("remove"))
                    type = CommentType.Remove;
                else if (rule.Id.Contains("task"))
                    type = CommentType.Task;
                else
                {
                    isDynamic = true;
                    type = CommentType.Important;
                }
            }
            else
            {
                isDynamic = true;
                type = CommentType.Important;
            }

            var defaultValue = rule.IsPredefined ? rule.Criteria : "";
            var token = new CommentToken(type.Value, defaultValue, rule.Criteria, rule.Foreground.ColorHex);
            
            token.RuleId = rule.Id ?? Guid.NewGuid().ToString();
            token.IsDynamic = isDynamic;
            token.IsBold = rule.Foreground.IsBold;
            token.IsItalic = rule.Foreground.IsItalic;
            token.HasUnderline = rule.Foreground.HasUnderline;
            token.HasStrikethrough = rule.Foreground.HasStrikethrough;
            
            return token;
        }

        #endregion Private Helpers
    }
}
