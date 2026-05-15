using NexusHighlight.CommentsTagging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace NexusHighlight.Options
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
        private UILanguage uiLanguage = UILanguage.Chinese;

        public bool IsDragging { get; set; }
        private bool _isSyncing;
        private string _currentSolutionPath;

        // === 规则集合（使用 CommentRule）===
        private readonly ObservableCollection<CommentRule> globalRules
            = new ObservableCollection<CommentRule>();

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
            ResetRules = new RelayCommand(SetRulesToDefault);
            ResetSolutionRules = new RelayCommand(ClearSolutionRules);
            SettingsStore.LoadSettings(this);

            // 从 SettingsManager 加载全局规则
            var loadedGlobalRules = SettingsManager.LoadGlobalRules();
            foreach (var rule in loadedGlobalRules)
            {
                globalRules.Add(rule);
            }

            // 初始化解决方案规则（暂时为空，后续需要根据当前解决方案路径加载）
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
                    rule.Background = new Background();
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

        [Setting]
        public UILanguage UILanguage
        {
            get { return uiLanguage; }
            set { SetField(ref uiLanguage, value); }
        }

        #endregion Settings Properties

        #region Non-Settings Properties & Commands

        public RelayCommand ResetRules { get; }
        public RelayCommand ResetSolutionRules { get; }

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

        public string Key => "NexusHighlight";

        #endregion ISettings Members

        #region Public Methods

        // === 基于 CommentRule 的方法 ===
        
        public CommentRule GetRule(string criteria)
        {
            return globalRules.FirstOrDefault(r => r.Criteria == criteria);
        }

        public string GetRulePattern(string criteria)
        {
            var rule = GetRule(criteria);
            return rule?.Criteria ?? "";
        }

        public string GetRuleValue(string criteria)
        {
            var rule = GetRule(criteria);
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
                        ColorHex = globalRule.ColorHex,
                        IsBold = globalRule.IsBold,
                        IsItalic = globalRule.IsItalic,
                        HasUnderline = globalRule.HasUnderline,
                        HasStrikethrough = globalRule.HasStrikethrough,
                        IsForegroundActive = globalRule.IsForegroundActive,
                        IsPredefined = false,
                        Background = new Background
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

        private void SetRulesToDefault()
        {
            _isSyncing = true;
            try
            {
                globalRules.Clear();

                // 直接创建默认规则，而不是从文件加载
                var defaultRules = new ObservableCollection<CommentRule>
                {
                    new CommentRule
                    {
                        Id = Guid.NewGuid().ToString(),
                        Criteria = "#IMPORTANT",
                        ColorHex = "#FFFF0000",
                        IsBold = true,
                        IsItalic = false,
                        HasUnderline = false,
                        HasStrikethrough = false,
                        IsForegroundActive = true,
                        IsPredefined = false,
                        Background = new Background
                        {
                            IsActive = false,
                            ColorHex = null,
                            Shape = "Tag",
                            Blur = "None",
                            Alpha = "1/10",
                            IsCaseSensitive = true,
                            AllowPartialMatch = false
                        }
                    },
                    new CommentRule
                    {
                        Id = Guid.NewGuid().ToString(),
                        Criteria = "#REMOVE",
                        ColorHex = "#FF808080",
                        IsBold = false,
                        IsItalic = false,
                        HasUnderline = false,
                        HasStrikethrough = true,
                        IsForegroundActive = true,
                        IsPredefined = false,
                        Background = new Background
                        {
                            IsActive = false,
                            ColorHex = null,
                            Shape = "Tag",
                            Blur = "None",
                            Alpha = "1/10",
                            IsCaseSensitive = true,
                            AllowPartialMatch = false
                        }
                    },
                    new CommentRule
                    {
                        Id = Guid.NewGuid().ToString(),
                        Criteria = "#QUESTION",
                        ColorHex = "#FFFFFF00",
                        IsBold = false,
                        IsItalic = false,
                        HasUnderline = false,
                        HasStrikethrough = false,
                        IsForegroundActive = true,
                        IsPredefined = false,
                        Background = new Background
                        {
                            IsActive = false,
                            ColorHex = null,
                            Shape = "Tag",
                            Blur = "None",
                            Alpha = "1/10",
                            IsCaseSensitive = true,
                            AllowPartialMatch = false
                        }
                    },
                    new CommentRule
                    {
                        Id = Guid.NewGuid().ToString(),
                        Criteria = "#TASK",
                        ColorHex = "#FFEB690A",
                        IsBold = false,
                        IsItalic = false,
                        HasUnderline = false,
                        HasStrikethrough = false,
                        IsForegroundActive = true,
                        IsPredefined = false,
                        Background = new Background
                        {
                            IsActive = false,
                            ColorHex = null,
                            Shape = "Tag",
                            Blur = "None",
                            Alpha = "1/10",
                            IsCaseSensitive = true,
                            AllowPartialMatch = false
                        }
                    }
                };

                foreach (var rule in defaultRules)
                {
                    globalRules.Add(rule);
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
                // 将规则复制到本地列表，避免在后台线程访问 ObservableCollection 时的线程安全问题
                if (sender == globalRules)
                {
                    var rulesCopy = new List<CommentRule>(globalRules);
                    Task.Run(() => SettingsManager.SaveGlobalRules(rulesCopy));
                }
                else if (sender == solutionRules)
                {
                    if (!string.IsNullOrEmpty(_currentSolutionPath))
                    {
                        var rulesCopy = new List<CommentRule>(solutionRules);
                        var solutionPath = _currentSolutionPath;
                        Task.Run(() => SettingsManager.SaveSolutionRules(solutionPath, rulesCopy));
                    }
                }
                
                OnConfigurationChanged();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        // === 解决方案相关属性和方法 ===
        public string CurrentSolutionPath
        {
            get { return _currentSolutionPath; }
        }

        public void SetCurrentSolutionPath(string solutionPath)
        {
            if (_currentSolutionPath == solutionPath)
                return;

            _isSyncing = true;
            try
            {
                // 保存旧解决方案的规则到后台线程
                if (!string.IsNullOrEmpty(_currentSolutionPath))
                {
                    var oldSolutionPath = _currentSolutionPath;
                    var rulesCopy = new List<CommentRule>(solutionRules);
                    Task.Run(() => SettingsManager.SaveSolutionRules(oldSolutionPath, rulesCopy));
                }

                // 更新当前解决方案路径
                _currentSolutionPath = solutionPath;

                // 清空当前解决方案规则
                solutionRules.Clear();

                // 加载新解决方案的规则
                if (!string.IsNullOrEmpty(solutionPath))
                {
                    var loadedSolutionRules = SettingsManager.LoadSolutionRules(solutionPath);
                    foreach (var rule in loadedSolutionRules)
                    {
                        // 确保规则有完整的默认属性
                        if (rule.Background == null)
                        {
                            rule.Background = new Background();
                        }

                        if (string.IsNullOrEmpty(rule.Background.Shape))
                            rule.Background.Shape = "Tag";
                        if (string.IsNullOrEmpty(rule.Background.Blur))
                            rule.Background.Blur = "None";
                        if (string.IsNullOrEmpty(rule.Background.Alpha))
                            rule.Background.Alpha = "1/10";
                        
                        solutionRules.Add(rule);
                    }
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }

        #endregion Private Helpers
    }
}
