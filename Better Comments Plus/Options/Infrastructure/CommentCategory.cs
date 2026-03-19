namespace BetterCommentsPlus.Options
{
    /// <summary>
    /// 评论分类 - 预定义的 5 种评论类型
    /// 原来的 CommentType 重命名为 CommentCategory
    /// </summary>
    public enum CommentCategory
    {
        /// <summary>
        /// 普通评论
        /// </summary>
        Normal,

        /// <summary>
        /// 重要评论 (#IMPORTANT)
        /// </summary>
        Important,

        /// <summary>
        /// 问题评论 (#QUESTION)
        /// </summary>
        Question,

        /// <summary>
        /// 删除评论 (#REMOVE)
        /// </summary>
        Remove,

        /// <summary>
        /// 任务评论 (#TASK)
        /// </summary>
        Task
    }
}
