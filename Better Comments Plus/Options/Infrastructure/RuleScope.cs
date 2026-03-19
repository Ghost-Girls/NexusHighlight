namespace BetterCommentsPlus.Options
{
    /// <summary>
    /// 规则作用域 - 定义规则的应用范围
    /// </summary>
    public enum RuleScope
    {
        /// <summary>
        /// 全局规则 - 应用于所有解决方案
        /// </summary>
        Global,

        /// <summary>
        /// 解决方案规则 - 仅应用于当前解决方案
        /// </summary>
        Solution
    }
}
