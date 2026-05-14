using System;

namespace BetterCommentsPlus.Options
{
    public enum UILanguage
    {
        Chinese,
        English
    }

    public static class UIStrings
    {
        public static UILanguage CurrentLanguage { get; set; } = UILanguage.Chinese;

        public static event EventHandler LanguageChanged;

        public static void SwitchLanguage(UILanguage lang)
        {
            if (CurrentLanguage == lang) return;
            CurrentLanguage = lang;
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        private static string T(string zh, string en) => CurrentLanguage == UILanguage.Chinese ? zh : en;

        // === General 页面 ===
        public static string GroupLanguage => T("界面语言", "UI Language");
        public static string LabelLanguage => T("语言", "Language");
        public static string LanguageChinese => T("中文", "Chinese");
        public static string LanguageEnglish => T("英文", "English");
        public static string LabelFont => T("字体", "Font");
        public static string LabelSize => T("大小偏移", "Size Offset");
        public static string LabelSizeNote => T("相对于默认字体大小", "Relative to the default font size");
        public static string LabelOpacity => T("透明度", "Opacity");
        public static string LabelItalic => T("斜体", "Italic");
        public static string LabelHighlightKeywordsOnly => T("仅高亮任务关键字", "Color only the 'Todo' keyword in Task comments");
        public static string LabelUnderlineImportant => T("下划线重要注释", "Underline important comments");
        public static string LabelStrikethrough => T("删除线双注释", "Strikethrough double comments");
        public static string Feedback => T("反馈", "Feedback");

        // === Rules 页面 ===
        public static string LabelHighlightCriteria => T("高亮条件本身", "Highlight Criteria");
        public static string TabGlobalRules => T("全局规则", "Global Rules");
        public static string TabSolutionRules => T("解决方案规则", "Solution Rules");
        public static string ButtonAddGlobal => T("添加全局", "Add Global");
        public static string ButtonExportGlobal => T("导出全局", "Export Global");
        public static string ButtonImportGlobal => T("导入全局", "Import Global");
        public static string NoteGlobalRules => T("这些规则适用于所有项目。", "These rules apply to all projects.");
        public static string ButtonAddSolution => T("添加解决方案", "Add Solution");
        public static string ButtonCopyFromGlobal => T("从全局复制", "Copy from Global");
        public static string ButtonImportFromGlobal => T("从全局导入", "Import from Global");
        public static string ButtonClearSolution => T("清除", "Clear");
        public static string ButtonExportSolution => T("导出", "Export");
        public static string ButtonImportSolution => T("导入", "Import");
        public static string NoteSolutionRules => T("这些规则仅适用于当前解决方案。", "These rules only apply to the current solution.");
        public static string ButtonMoveUp => T("↑ 上移", "↑ Up");
        public static string ButtonMoveDown => T("↓ 下移", "↓ Down");
        public static string ButtonReset => T("_重置", "_Reset");
        public static string ButtonDelete => T("删除", "Delete");
        public static string TooltipEnableForeground => T("启用前景色", "Enable foreground");
        public static string TooltipBold => T("粗体", "Bold");
        public static string TooltipItalic => T("斜体", "Italic");
        public static string TooltipUnderline => T("下划线", "Underline");
        public static string TooltipStrikethrough => T("删除线", "Strikethrough");
        public static string TooltipEnableBackground => T("启用背景", "Enable background");
        public static string LabelCaseSensitive => T("区分大小写", "Case sensitive");
        public static string LabelPartialMatch => T("部分匹配", "Partial match");

        // === 对话框消息 ===
        public static string ConfirmCopyTitle => T("确认复制", "Confirm Copy");
        public static string ConfirmCopyMessage => T("这将复制所有 Global Rules 到 Solution Rules，是否继续？", "This will copy all Global Rules to Solution Rules, continue?");
        public static string SelectRulesTitle => T("从 Global Rules 选择规则", "Select Rules from Global Rules");
        public static string SelectRulesNote => T("（按住 Ctrl 或 Shift 多选）", "(Hold Ctrl or Shift to multi-select)");
        public static string SelectRulesInstruction => T("请选择要导入的规则（支持多选）：", "Please select rules to import (supports multi-select):");
        public static string ButtonImportSelected => T("导入选中项", "Import Selected");
        public static string ButtonCancel => T("取消", "Cancel");
        public static string PleaseSelectAtLeastOne => T("请至少选择一个规则", "Please select at least one rule");
        public static string NoAvailableRules => T("没有可用的 Global Rules", "No available Global Rules");
        public static string ConfirmClearTitle => T("确认清除", "Confirm Clear");
        public static string ConfirmClearMessage => T("确定要清除所有 Solution Rules 吗？", "Are you sure you want to clear all Solution Rules?");
        public static string NoRulesToExport => T("没有可用的", "No available");
        public static string RulesSuffix => T(" Rules", " Rules");
        public static string ExportTitle => T("导出", "Export");
        public static string ImportTitle => T("导入", "Import");
        public static string JsonFilter => T("JSON 文件 (*.json)|*.json", "JSON files (*.json)|*.json");
        public static string ExportSuccess => T("导出成功！", "Export successful!");
        public static string ExportFailed => T("导出失败：", "Export failed: ");
        public static string ImportModeTitle => T("导入模式", "Import Mode");
        public static string ImportModeMessage => T("选择导入模式：\n\n是 - 覆盖现有规则\n否 - 合并现有规则", "Select import mode:\n\nYes - Overwrite existing rules\nNo - Merge with existing rules");
        public static string ImportSuccess => T("导入成功！", "Import successful!");
        public static string ImportFailed => T("导入失败：", "Import failed: ");
        public static string NoRulesInFile => T("文件中没有 Rules", "No rules in the file");
        public static string Prompt => T("提示", "Information");
        public static string Error => T("错误", "Error");
    }
}
