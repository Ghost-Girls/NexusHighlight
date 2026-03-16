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

        private readonly ObservableCollection<CommentToken> commentTokens
            = new ObservableCollection<CommentToken>
            {
                new CommentToken(type: CommentType.Important,       defaultValue: "#IMPORTANT",     value: "#IMPORTANT",   colorHex: "#FFFF0000"),
                new CommentToken(type: CommentType.Remove,          defaultValue: "#REMOVE",        value: "#REMOVE",      colorHex: "#FF808080"),
                new CommentToken(type: CommentType.Question,        defaultValue: "#QUESTION",      value: "#QUESTION",    colorHex: "#FFFFFF00"),
                new CommentToken(type: CommentType.Task,            defaultValue: "#TASK",          value: "#TASK",        colorHex: "#FFEB690A"),
            };

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
            
            foreach (var token in commentTokens)
            {
                token.PropertyChanged += CommentToken_PropertyChanged;
            }
            
            commentTokens.CollectionChanged += CommentTokens_CollectionChanged;
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
            return commentTokens.Single(t => t.Type == type);
        }

        public string GetTokenValue(CommentType type)
        {
            return GetToken(type).CurrentValue;
        }

        #endregion Public Methods

        #region Private Helpers

        private void SetTokensToDefault()
        {
            foreach (var token in commentTokens)
            {
                token.CurrentValue = token.DefaultValue;
                switch (token.Type)
                {
                    case CommentType.Important:
                        token.ColorHex = "#FFFF0000";
                        break;
                    case CommentType.Question:
                        token.ColorHex = "#FFFFFF00";
                        break;
                    case CommentType.Remove:
                        token.ColorHex = "#FF808080";
                        break;
                    case CommentType.Task:
                        token.ColorHex = "#FFEB690A";
                        break;
                }
            }
            SyncCommentTokensToUnifiedConfig();
            OnConfigurationChanged();
        }

        private void UpdateCommentTokens(string tokensString)
        {
            if (tokensString.IsNullOrWhiteSpace())
                return;

            foreach (var pair in tokensString.Split('|').Select(p => p.Split(',')))
            {
                var token = commentTokens.SingleOrDefault(t => t.IsOfType(pair[0]));

                if (token != null)
                {
                    token.CurrentValue = pair[1].Trim();
                    if (pair.Length > 2 && !string.IsNullOrWhiteSpace(pair[2]))
                        token.ColorHex = pair[2].Trim();
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
                if (rule != null && rule.Foreground != null)
                {
                    rule.Foreground.ColorHex = token.ColorHex;
                }
            }
            OnConfigurationChanged();
        }

        public void SyncUnifiedConfigToCommentTokens()
        {
            foreach (var rule in unifiedConfig.Comments)
            {
                var token = commentTokens.FirstOrDefault(t => t.CurrentValue == rule.Criteria);
                if (token != null && rule.Foreground != null)
                {
                    token.ColorHex = rule.Foreground.ColorHex;
                }
            }
        }

        private void CommentToken_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SyncCommentTokensToUnifiedConfig();
            OnConfigurationChanged();
        }

        private void CommentTokens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
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
                SyncCommentTokensToUnifiedConfig();
                OnConfigurationChanged();
            }
        }

        #endregion Private Helpers
    }
}
