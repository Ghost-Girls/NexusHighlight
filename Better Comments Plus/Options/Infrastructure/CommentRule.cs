using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BetterCommentsPlus.Options
{
    /// <summary>
    /// 评论规则 - 统一的规则定义
    /// 合并了原来的 CommentToken 和 StyleRule
    /// </summary>
    public class CommentRule : INotifyPropertyChanged
    {
        // === 标识 ===
        private string id;
        private CommentCategory? category;
        private bool isPredefined;
        private bool isActive;
        private int order;

        // === 匹配 ===
        private string criteria;

        // === 样式 ===
        private ForegroundStyle foreground;
        private BackgroundStyle background;

        // === 作用域 ===
        private RuleScope scope;

        #region 构造函数

        public CommentRule()
        {
            id = Guid.NewGuid().ToString();
            criteria = string.Empty;
            isPredefined = false;
            isActive = true;
            order = 0;
            foreground = new ForegroundStyle();
            background = new BackgroundStyle();
            scope = RuleScope.Global;
        }

        public CommentRule(CommentCategory category, string criteria, string colorHex) : this()
        {
            this.category = category;
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
        /// 评论分类（仅预定义规则使用）
        /// </summary>
        public CommentCategory? Category
        {
            get => category;
            set { category = value; OnPropertyChanged(); }
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

        /// <summary>
        /// 排序顺序
        /// </summary>
        public int Order
        {
            get => order;
            set { order = value; OnPropertyChanged(); }
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
        /// 前景样式
        /// </summary>
        public ForegroundStyle Foreground
        {
            get => foreground;
            set { foreground = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 背景样式
        /// </summary>
        public BackgroundStyle Background
        {
            get => background;
            set { background = value; OnPropertyChanged(); }
        }

        // === 作用域 ===

        /// <summary>
        /// 规则作用域（全局或解决方案）
        /// </summary>
        public RuleScope Scope
        {
            get => scope;
            set { scope = value; OnPropertyChanged(); }
        }

        #endregion

        #region 便捷属性（前景样式）

        public string ColorHex
        {
            get => foreground?.ColorHex;
            set { if (foreground != null) { foreground.ColorHex = value; OnPropertyChanged(); } }
        }

        public bool? IsBold
        {
            get => foreground?.IsBold;
            set { if (foreground != null && value.HasValue) { foreground.IsBold = value.Value; OnPropertyChanged(); } }
        }

        public bool? IsItalic
        {
            get => foreground?.IsItalic;
            set { if (foreground != null && value.HasValue) { foreground.IsItalic = value.Value; OnPropertyChanged(); } }
        }

        public bool? HasUnderline
        {
            get => foreground?.HasUnderline;
            set { if (foreground != null && value.HasValue) { foreground.HasUnderline = value.Value; OnPropertyChanged(); } }
        }

        public bool? HasStrikethrough
        {
            get => foreground?.HasStrikethrough;
            set { if (foreground != null && value.HasValue) { foreground.HasStrikethrough = value.Value; OnPropertyChanged(); } }
        }

        public bool? IsForegroundActive
        {
            get => foreground?.IsActive;
            set { if (foreground != null && value.HasValue) { foreground.IsActive = value.Value; OnPropertyChanged(); } }
        }

        #endregion

        #region 便捷属性（背景样式）

        public string BackgroundColorHex
        {
            get => background?.ColorHex;
            set { if (background != null) { background.ColorHex = value; OnPropertyChanged(); } }
        }

        public string Shape
        {
            get => background?.Shape;
            set { if (background != null) { background.Shape = value; OnPropertyChanged(); } }
        }

        public string Blur
        {
            get => background?.Blur;
            set { if (background != null) { background.Blur = value; OnPropertyChanged(); } }
        }

        public string Alpha
        {
            get => background?.Alpha;
            set { if (background != null) { background.Alpha = value; OnPropertyChanged(); } }
        }

        public bool? IsBackgroundActive
        {
            get => background?.IsActive;
            set { if (background != null && value.HasValue) { background.IsActive = value.Value; OnPropertyChanged(); } }
        }

        public bool? IsCaseSensitive
        {
            get => background?.IsCaseSensitive;
            set { if (background != null && value.HasValue) { background.IsCaseSensitive = value.Value; OnPropertyChanged(); } }
        }

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
            return $"{Criteria} ({(Category.HasValue ? Category.Value.ToString() : "Custom")})";
        }

        #endregion
    }
}
