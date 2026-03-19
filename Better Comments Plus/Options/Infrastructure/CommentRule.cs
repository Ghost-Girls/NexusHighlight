using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BetterCommentsPlus.Options
{
    /// <summary>
    /// 评论规则 - 统一的规则定义
    /// </summary>
    public class CommentRule : INotifyPropertyChanged
    {
        // === 标识 ===
        private string id;
        private bool isPredefined;
        private bool isActive;

        // === 匹配 ===
        private string criteria;

        // === 样式 ===
        private Foreground foreground;
        private Background background;

        #region 构造函数

        public CommentRule()
        {
            id = Guid.NewGuid().ToString();
            criteria = string.Empty;
            isPredefined = false;
            isActive = true;
            foreground = new Foreground();
            background = new Background();
        }

        public CommentRule(string criteria, string colorHex) : this()
        {
            this.criteria = criteria;
            this.isPredefined = true;
            if (foreground != null)
            {
                foreground.ColorHex = colorHex;
            }
        }

        #endregion

        #region 属性

        // === 标识 ===

        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string Id
        {
            get => id;
            set { id = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 是否预定义规则
        /// </summary>
        public bool IsPredefined
        {
            get => isPredefined;
            set { isPredefined = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set { isActive = value; OnPropertyChanged(); }
        }

        // === 匹配 ===

        /// <summary>
        /// 匹配条件（统一术语，替代原来的 CurrentValue）
        /// </summary>
        public string Criteria
        {
            get => criteria;
            set { criteria = value; OnPropertyChanged(); }
        }

        // === 样式 ===

        /// <summary>
        /// 前景
        /// </summary>
        public Foreground Foreground
        {
            get => foreground;
            set { foreground = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 背景
        /// </summary>
        public Background Background
        {
            get => background;
            set { background = value; OnPropertyChanged(); }
        }

        #endregion

        #region 便捷属性（前景样式）

        [JsonIgnore]
        public string ColorHex
        {
            get => foreground?.ColorHex;
            set { if (foreground != null) { foreground.ColorHex = value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public bool? IsBold
        {
            get => foreground?.IsBold;
            set { if (foreground != null && value.HasValue) { foreground.IsBold = value.Value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public bool? IsItalic
        {
            get => foreground?.IsItalic;
            set { if (foreground != null && value.HasValue) { foreground.IsItalic = value.Value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public bool? HasUnderline
        {
            get => foreground?.HasUnderline;
            set { if (foreground != null && value.HasValue) { foreground.HasUnderline = value.Value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public bool? HasStrikethrough
        {
            get => foreground?.HasStrikethrough;
            set { if (foreground != null && value.HasValue) { foreground.HasStrikethrough = value.Value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public bool? IsForegroundActive
        {
            get => foreground?.IsActive;
            set { if (foreground != null && value.HasValue) { foreground.IsActive = value.Value; OnPropertyChanged(); } }
        }

        #endregion

        #region 便捷属性（背景样式）

        [JsonIgnore]
        public string BackgroundColorHex
        {
            get => background?.ColorHex;
            set { if (background != null) { background.ColorHex = value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public string Shape
        {
            get => background?.Shape;
            set { if (background != null) { background.Shape = value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public string Blur
        {
            get => background?.Blur;
            set { if (background != null) { background.Blur = value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public string Alpha
        {
            get => background?.Alpha;
            set { if (background != null) { background.Alpha = value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public bool? IsBackgroundActive
        {
            get => background?.IsActive;
            set { if (background != null && value.HasValue) { background.IsActive = value.Value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public bool? IsCaseSensitive
        {
            get => background?.IsCaseSensitive;
            set { if (background != null && value.HasValue) { background.IsCaseSensitive = value.Value; OnPropertyChanged(); } }
        }

        [JsonIgnore]
        public bool? AllowPartialMatch
        {
            get => background?.AllowPartialMatch;
            set { if (background != null && value.HasValue) { background.AllowPartialMatch = value.Value; OnPropertyChanged(); } }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region 重写方法

        public override string ToString()
        {
            return $"{Criteria} ({(IsPredefined ? "Predefined" : "Custom")})";
        }

        #endregion
    }
}
