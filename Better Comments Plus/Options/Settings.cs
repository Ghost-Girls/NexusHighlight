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

        // === 规则集合（使用 CommentRule）===
        private readonly ObservableCollection<CommentRule> globalRules
            = new ObservableCollection<CommentRule>
            {
                new CommentRule(CommentCategory.Important, "#IMPORTANT", "#FFFF0000"),
                new CommentRule(CommentCategory.Remove, "#REMOVE", "#FF808080"),
                new CommentRule(CommentCategory.Question, "#QUESTION", "#FFFFFF00"),
                new CommentRule(CommentCategory.Task, "#TASK", "#FFEB690A"),
            };

        private readonly ObservableCollection<CommentRule> solutionRules
            = new ObservableCollection<CommentRule>();

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
            ResetSolutionTokens = new RelayCommand(ClearSolutionRules);
            SettingsStore.LoadSettings(this);

            InitializeRuleCollection(globalRules);
            InitializeRuleCollection(solutionRules);

            globalRules.CollectionChanged += Rules_CollectionChanged;
            solutionRules.CollectionChanged += Rules_CollectionChanged;
        }

        private void InitializeRuleCollection(ObservableCollection<CommentRule> rules)
        {
            foreach (var rule in rules)
            {
                if (rule.Background == null)
                {
                    rule.Background = new BackgroundStyle();
                }

                if (string.IsNullOrEmpty(rule.Background.Shape))
                    rule.Background.Shape = "Tag";
                if (string.IsNullOrEmpty(rule.Background.Blur))
                    rule.Background.Blur = "None";
                if (string.IsNullOrEmpty(rule.Background.Alpha))
                    rule.Background.Alpha = "1/10";
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

        #endregion Settings Properties

        #region Non-Settings Properties & Commands

        public RelayCommand ResetTokens { get; }
        public RelayCommand ResetSolutionTokens { get; }

        // === 规则集合属性 ===
        public ObservableCollection<CommentRule> GlobalRules
        {
            get { return globalRules; }
        }

        public ObservableCollection<CommentRule> SolutionRules
        {
            get { return solutionRules; }
        }

        // === 合并所有规则 ===
        public System.Collections.Generic.IEnumerable<CommentRule> AllRules
        {
            get { return solutionRules.Concat(globalRules); }
        }

        #endregion Non-Settings Properties & Commands

        #region ISettings Members

        public string Key => "BetterCommentsPlus";

        #endregion ISettings Members

        #region Public Methods

        // === 基于 CommentRule 的方法 ===
        
        public CommentRule GetRule(CommentCategory category)
        {
            return globalRules.FirstOrDefault(r => r.Category == category);
        }

        public string GetRulePattern(CommentCategory category)
        {
            var rule = GetRule(category);
            return rule?.Criteria ?? "";
        }

        public string GetTokenValue(CommentCategory? category)
        {
            if (!category.HasValue)
                return "";
            
            var rule = GetRule(category.Value);
            return rule?.Criteria ?? "";
        }

        public void ClearSolutionRules()
        {
            _isSyncing = true;
            try
            {
                solutionRules.Clear();
                OnConfigurationChanged();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        public void CopyAllFromGlobalToSolutionRules()
        {
            _isSyncing = true;
            try
            {
                foreach (var globalRule in globalRules)
                {
                    var newRule = new CommentRule
                    {
                        Id = Guid.NewGuid().ToString(),
                        Criteria = globalRule.Criteria,
                        Category = globalRule.Category,
                        ColorHex = globalRule.ColorHex,
                        IsBold = globalRule.IsBold,
                        IsItalic = globalRule.IsItalic,
                        HasUnderline = globalRule.HasUnderline,
                        HasStrikethrough = globalRule.HasStrikethrough,
                        IsForegroundActive = globalRule.IsForegroundActive,
                        IsPredefined = false,
                        Background = new BackgroundStyle
                        {
                            IsActive = globalRule.Background?.IsActive ?? false,
                            ColorHex = globalRule.Background?.ColorHex,
                            Shape = globalRule.Background?.Shape ?? "Tag",
                            Blur = globalRule.Background?.Blur ?? "None",
                            Alpha = globalRule.Background?.Alpha ?? "1/10",
                            IsCaseSensitive = globalRule.Background?.IsCaseSensitive ?? true,
                            AllowPartialMatch = globalRule.Background?.AllowPartialMatch ?? false
                        }
                    };
                    solutionRules.Add(newRule);
                }
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
                globalRules.Clear();

                var defaultRules = new[]
                {
                    new { Category = CommentCategory.Important, Criteria = "#IMPORTANT", ColorHex = "#FFFF0000", IsBold = true, IsItalic = false, HasUnderline = false, HasStrikethrough = false },
                    new { Category = CommentCategory.Remove, Criteria = "#REMOVE", ColorHex = "#FF808080", IsBold = false, IsItalic = false, HasUnderline = false, HasStrikethrough = true },
                    new { Category = CommentCategory.Question, Criteria = "#QUESTION", ColorHex = "#FFFFFF00", IsBold = false, IsItalic = false, HasUnderline = false, HasStrikethrough = false },
                    new { Category = CommentCategory.Task, Criteria = "#TASK", ColorHex = "#FFEB690A", IsBold = false, IsItalic = false, HasUnderline = false, HasStrikethrough = false }
                };

                foreach (var ruleInfo in defaultRules)
                {
                    var newRule = new CommentRule
                    {
                        Id = Guid.NewGuid().ToString(),
                        Category = ruleInfo.Category,
                        Criteria = ruleInfo.Criteria,
                        ColorHex = ruleInfo.ColorHex,
                        IsBold = ruleInfo.IsBold,
                        IsItalic = ruleInfo.IsItalic,
                        HasUnderline = ruleInfo.HasUnderline,
                        HasStrikethrough = ruleInfo.HasStrikethrough,
                        IsForegroundActive = true,
                        IsPredefined = false,
                        Background = new BackgroundStyle
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

                    globalRules.Add(newRule);
                }

                OnConfigurationChanged();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void Rules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isSyncing || IsDragging)
                return;

            _isSyncing = true;
            try
            {
                OnConfigurationChanged();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        #endregion Private Helpers
    }
}
