# Better Comments Plus + Highlighter 合并项目规划与实施指南

## 1. 规划可行性与优先级分析

### 1.1 可行性评估

| 需求项 | 可行性 | 理由 |
|--------|--------|------|
| 1. 从 Visual Studio「字体和颜色」中移除 CommentColors，改用 OptionsTokensPageControl.xaml 中的 ColorPicker | ✅ **高度可行** | 目前 Highlighter 已经实现了类似功能，可复用其实现方式 |
| 2. 动态生成 CommentToken，取消与 Visual Studio「字体和颜色」的关联 | ✅ **高度可行** | 通过移除 ClassificationFormatDefinition 的硬编码，改用动态注册机制 |
| 3. 合并两个项目的数据结构（统一前景和背景配置） | ✅ **高度可行** | 现有代码基础已提供 ForegorundStyle 和 BackgroundStyle 类，可扩展 |
| 4. 使用 Listbox 实现可拖拽的 Criteria 列表 | ✅ **可行** | WPF 支持 ListBox 拖拽功能，有成熟实现方案 |
| 5. 数据写入 JSON 后效果立即生效 | ✅ **高度可行** | 通过事件通知机制和配置重新加载实现 |
| 6. 实现右键菜单创建/编辑规则功能 | ✅ **高度可行** | Highlighter 已有完整实现，可直接移植 |

### 1.2 优先级建议

| 优先级 | 阶段 | 需求项 |
|--------|------|--------|
| P0（最高） | 第一阶段 | 1. 从 VS「字体和颜色」中移除 CommentColors<br>2. 实现 ColorPicker 选择颜色<br>3. 完善 JSON 数据结构定义 |
| P1 | 第二阶段 | 1. 动态生成 CommentToken 机制<br>2. 移除与 VS「字体和颜色」的硬编码关联<br>3. 基础 ListBox 展示（不含拖拽） |
| P2 | 第三阶段 | 1. ListBox 拖拽排序功能<br>2. 完整的配置 UI（前景 + 背景）<br>3. 配置变更实时生效 |
| P3 | 第四阶段 | 1. 完整的导入导出功能<br>2. **右键菜单集成**<br>3. 数据迁移（从旧版本升级） |

---

## 2. 术语统一与数据结构规范化

### 2.1 核心术语统一

基于 Merger_Interaction_Discussion.md 1-403 中的讨论，统一以下术语：

| 旧术语（BCP） | 旧术语（Highlighter） | 统一术语 | 说明 |
|--------------|---------------------|---------|------|
| CommentToken | HighlightTag | **StyleRule** | 一个样式规则，包含匹配条件和样式配置 |
| Token | Criteria | **Criteria** | 匹配文本（如 "#IMPORTANT" 或 "TODO"） |
| Foreground color | - | **Foreground** | 前景色样式（文字颜色、加粗等） |
| - | Background color | **Background** | 背景色样式（颜色、形状等） |

### 2.2 规范化数据结构定义

```csharp
// 统一的样式规则
public class StyleRule
{
    public string Id { get; set; }           // 唯一标识符
    public int Order { get; set; }            // 排序顺序（用于拖拽）
    public bool IsActive { get; set; }        // 是否启用
    public bool IsPredefined { get; set; }    // 是否为预定义规则
    
    public string Criteria { get; set; }      // 匹配文本
    
    // 前景色样式（来自 Better Comments Plus）
    public ForegroundStyle Foreground { get; set; }
    
    // 背景色样式（来自 Highlighter）
    public BackgroundStyle Background { get; set; }
}

public class ForegroundStyle
{
    public bool IsActive { get; set; }
    public string Color { get; set; }         // Hex 颜色，如 "#FF0000"
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool HasUnderline { get; set; }
    public bool HasStrikethrough { get; set; }
}

public class BackgroundStyle
{
    public bool IsActive { get; set; }
    public string Color { get; set; }         // Hex 颜色，如 "#FF0000"
    public string Shape { get; set; }         // Tag, TagUnder, Line, LineUnder
    public string Blur { get; set; }          // None, Low, Medium, High
    public string Alpha { get; set; }         // Alpha_1_10, Alpha_2_10, ...
    public bool IsCaseSensitive { get; set; }
    public bool AllowPartialMatch { get; set; }
}

// 根配置对象
public class UnifiedConfig
{
    public string Version { get; set; } = "2.0.0";
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;
    public List<StyleRule> Comments { get; set; } = new List<StyleRule>();
}
```

### 2.3 JSON 示例（规范化）

```json
{
  "Version": "2.0.0",
  "ExportDate": "2026-03-15T00:00:00.0000000Z",
  "Comments": [
    {
      "Order": 1,
      "IsActive": true,
      "Id": "comment-important",
      "Criteria": "#IMPORTANT",
      "Foreground": {
        "IsActive": true,
        "Color": "#FF0000",
        "IsBold": true,
        "IsItalic": false,
        "HasUnderline": false,
        "HasStrikethrough": false
      },
      "Background": {
        "IsActive": false,
        "Color": null,
        "Shape": "Tag",
        "Blur": "None",
        "Alpha": "Alpha_1_10",
        "IsCaseSensitive": true,
        "AllowPartialMatch": false
      },
      "IsPredefined": true
    },
    {
      "Order": 2,
      "IsActive": true,
      "Id": "comment-question",
      "Criteria": "#QUESTION",
      "Foreground": {
        "IsActive": true,
        "Color": "#FFFF00",
        "IsBold": false,
        "IsItalic": false,
        "HasUnderline": false,
        "HasStrikethrough": false
      },
      "Background": {
        "IsActive": false,
        "Color": null,
        "Shape": "Tag",
        "Blur": "None",
        "Alpha": "Alpha_1_10",
        "IsCaseSensitive": true,
        "AllowPartialMatch": false
      },
      "IsPredefined": true
    }
  ]
}
```

---

## 3. 需求划分与逐步执行计划

### 3.1 阶段一：解除与 Visual Studio「字体和颜色」的绑定 ✅

**目标**：移除硬编码的 ClassificationFormatDefinition，改用 ColorPicker 选择颜色

**状态**：✅ **已完成** (2026-03-16)

#### 任务清单：
- [x] 1.1 创建新的 ColorPicker 用户控件（ColorPickerDialog.xaml 和 ColorPickerDialog.xaml.cs）
- [x] 1.2 修改 OptionsTokensPageControl.xaml，添加颜色选择列和预览功能
- [x] 1.3 修改 CommentToken 类，添加 ColorHex 属性
- [x] 1.4 修改 Settings.cs，将颜色保存到配置中
- [x] 1.5 创建 HexToColorConverter 值转换器
- [x] 1.6 创建 ColorHelper 辅助类（颜色转换和预设颜色）
- [x] 1.7 创建 ForegroundStyle、BackgroundStyle、StyleRule、UnifiedConfig 数据结构
- [x] 1.8 修改 CommentViewDecorator.cs，实现动态颜色应用
- [x] 1.9 修复所有编译错误
- [x] 1.10 在 ColorPickerDialog 中添加 HEX 值输入功能（2026-03-17）

**验证点**：
- [x] 可以在 Options 页面中通过 ColorPicker 选择颜色
- [x] 颜色选择后能够正确保存和加载
- [x] 注释颜色显示正常（不依赖 VS「字体和颜色」设置）
- [x] 颜色预览功能正常工作
- [x] 数据本地化（JSON）正常
- [x] 可以通过输入 HEX 值来自定义颜色（支持带#或不带#，6 位或 8 位格式）
- [x] 输入 HEX 值时实时预览颜色
- [x] 点击预设颜色时自动填充对应的 HEX 值

**已创建/修改的文件**：
- Options/Infrastructure/ForegroundStyle.cs ✅
- Options/Infrastructure/BackgroundStyle.cs ✅
- Options/Infrastructure/StyleRule.cs ✅
- Options/Infrastructure/UnifiedConfig.cs ✅
- Options/Infrastructure/CommentToken.cs ✅ (添加 ColorHex 属性)
- Options/Infrastructure/HexToColorConverter.cs ✅
- Utils/ColorHelper.cs ✅
- Options/Settings.cs ✅
- Options/OptionsTokensPageControl.xaml ✅
- Options/OptionsTokensPageControl.xaml.cs ✅
- Options/ColorPickerDialog.xaml ✅
- Options/ColorPickerDialog.xaml.cs ✅
- CommentsViewCustomization/CommentViewDecorator.cs ✅

---

### 3.2 阶段三：实现可拖拽的 ListBox 和配置变更实时生效 ✅

**目标**：使用 ListBox 展示规则列表，支持拖拽排序，配置变更实时生效

**状态**：✅ **已完成** (2026-03-17)

#### 任务清单：
- [x] 3.1 修改 OptionsTokensPageControl.xaml，使用 ListBox 替代 ItemsControl
- [x] 3.2 实现 ListBox 项的数据模板（包含 Criteria、Foreground 预览、Actions）
- [x] 3.3 实现拖拽排序功能（自定义轻量级拖拽机制）
- [x] 3.4 拖拽后立即生效
- [x] 3.5 实现规则的 Add/Delete 按钮
- [x] 3.6 实现配置变更事件（ConfigurationChanged）
- [x] 3.7 CommentViewDecorator 监听配置变更事件
- [x] 3.8 配置变更后立即更新注释高亮
- [x] 3.9 优化输入框更新逻辑（PropertyChanged → LostFocus）
- [x] 3.10 添加拖拽视觉指示（半透明效果 + 蓝色指示线）
- [x] 3.11 优化拖拽性能（完全重写拖拽机制）

**验证点**：
- [x] ListBox 正确显示所有 CommentToken
- [x] 可以通过拖拽调整规则顺序
- [x] 规则顺序变更后能够正确保存
- [x] Delete 按钮功能正常
- [x] 在 Options 页面修改颜色后，编辑器中的注释立即更新
- [x] 调整规则顺序后，立即生效
- [x] 拖拽有视觉指示（被拖拽项半透明，放置位置蓝色指示线）
- [x] 拖拽流畅，无明显卡顿
- [x] 输入框在失去焦点时才更新，改善用户体验

**已创建/修改的文件**：
- Options/OptionsTokensPageControl.xaml ✅
- Options/OptionsTokensPageControl.xaml.cs ✅
- Options/Settings.cs ✅
- CommentsViewCustomization/CommentViewDecorator.cs ✅

---

### 3.3 阶段四：实现完整的前景样式配置 ✅

**目标**：为每个注释类型配置完整的前景样式（粗体、斜体、下划线、删除线），并支持启用/禁用前景色

**状态**：✅ **已完成** (2026-03-18)

#### 任务清单：
- [x] 4.1 扩展 CommentToken 类，添加 IsBold、IsItalic、HasUnderline、HasStrikethrough 属性
- [x] 4.2 在 Options 页面添加样式配置控件（B、I、U、S 四个复选框）
- [x] 4.3 修改 CommentViewDecorator，应用完整的样式属性
- [x] 4.4 更新 Settings.cs，保存和加载新的样式属性
- [x] 4.5 优化 UI 布局，添加 ToolTip 提示
- [x] 4.6 添加左侧拖拽手柄（☰），分离拖拽区域和控件交互
- [x] 4.7 添加前景色启用/禁用 CheckBox（显示为"A"）
- [x] 4.8 扩展 CommentToken 类，添加 IsForegroundActive 属性
- [x] 4.9 修改 CommentViewDecorator，实现前景色禁用时恢复默认注释颜色
- [x] 4.10 修复类型转换错误（Brush → Color）

**验证点**：
- [x] 可以为每个注释类型独立设置粗体
- [x] 可以为每个注释类型独立设置斜体
- [x] 可以为每个注释类型独立设置下划线
- [x] 可以为每个注释类型独立设置删除线
- [x] 样式变更后立即在编辑器中生效
- [x] 所有样式属性能够正确保存和加载
- [x] 拖拽手柄和其他控件不冲突
- [x] 样式按钮有清晰的 ToolTip 提示
- [x] 可以启用/禁用前景色（通过"A"复选框）
- [x] 禁用前景色时，文字恢复为 VS 默认注释颜色
- [x] 禁用前景色时，粗体/斜体/下划线/删除线效果也被禁用
- [x] 启用前景色时，正常显示自定义颜色

**已创建/修改的文件**：
- Options/Infrastructure/CommentToken.cs ✅
- Options/OptionsTokensPageControl.xaml ✅
- CommentsViewCustomization/CommentViewDecorator.cs ✅
- Options/Settings.cs ✅
- Options/EditStyleRuleDialog.xaml ✅（添加前景色启用 CheckBox）
- Options/EditStyleRuleDialog.xaml.cs ✅
- Commands/CreateEditStyleRule.cs ✅（支持 IsForegroundActive）
- Options/OptionsTokensPageControl.xaml.cs ✅（导入导出支持 IsForegroundActive）

#### 3.3.1 修复：Add 按钮和取消功能问题 ✅
**状态**：✅ **已完成** (2026-03-17)

**问题**：
- 点击 ADD 按钮导致 VS2022 无响应（无限循环）
- 点击「取消」或关闭按钮导致断言失败

**修复内容**：
- 添加 `_isSyncing` 标志防止无限循环
- 为所有事件处理程序添加同步检查
- 保持向后兼容，不破坏现有功能

**验证点**：
- [x] 点击 ADD 按钮功能正常，不会导致 VS 无响应
- [x] 点击「确定」保存设置正常
- [x] 点击「取消」或关闭按钮恢复状态，不会导致断言失败
- [x] Criteria 高亮功能正常工作

**已修改的文件**：
- Options/Settings.cs ✅
- Options/OptionsPageBase.cs ✅（保持原样，使用原始恢复机制）

#### 3.3.2 修复：Reset 按钮完整重置所有样式属性 ✅
**状态**：✅ **已完成** (2026-03-17)

**问题**：
- 点击 Reset 按钮只重置颜色，不重置样式属性（粗体、斜体、下划线、删除线）
- 点击 Reset 按钮会清空 ListBox 中的所有选项

**修复内容**：
- 修改 SetTokensToDefault 方法，完整重置所有样式属性
- 添加 _isSyncing 标志防止反向同步清空列表
- 确保与 UnifiedConfig.CreateDefault() 的默认样式一致

**验证点**：
- [x] 点击 Reset 按钮完整重置所有样式属性
- [x] 点击 Reset 按钮不会清空 ListBox 中的选项
- [x] 样式属性与 UnifiedConfig 默认值一致

**已修改的文件**：
- Options/Settings.cs ✅

---

### 3.4 阶段四：实现动态 CommentToken 生成（渐进式激进重构）✅

**目标**：逐步移除固定的 CommentType 枚举，支持动态添加/删除规则，保持向后兼容

**状态**：✅ **已完成** (2026-03-17)

**策略**：渐进式激进重构 - 保持向后兼容，但积极推进 StyleRule 的使用

#### 任务清单：
- [x] 4.1 扩展 CommentToken，添加 RuleId 和 IsDynamic 属性（保持 Type 向后兼容）
- [x] 4.2 修改 Add 按钮，新添加的规则使用动态 StyleRule 方式
- [x] 4.3 修改 Settings.cs，完善 CommentToken ↔ StyleRule 双向同步
- [x] 4.4 修改 CommentParser，支持动态规则匹配（向后兼容旧方式）
- [x] 4.5 修改 CommentTagger，支持动态 ClassificationType
- [x] 4.6 完善 CommentViewDecorator，主动为动态规则设置样式
- [x] 4.7 确保预设规则和动态规则都能正常工作
- [x] 4.8 修复新添加 Criteria 的高亮起始位置偏移问题
- [x] 4.9 移除 Type 标签和相关的 EnumToStringConverter

**验证点**：
- [x] 可以动态添加新的 StyleRule
- [x] 可以删除现有 StyleRule（包括预设规则）
- [x] 注释高亮能够正确应用动态规则
- [x] 向后兼容：现有配置继续正常工作
- [x] 预设规则和动态规则可以混合使用
- [x] 新添加的 Criteria 高亮位置正确（从 Criteria 第一个字符开始）
- [x] Type 标签已从界面移除

---

### 3.5 阶段五：完整合并 Highlighter（背景高亮功能）✅

**目标**：完整合并 Highlighter 项目，实现完整的前景 + 背景高亮功能

**状态**：✅ **已完成** (2026-03-18)

**策略**：完整移植 Highlighter 核心代码，整合到 BCP 项目

**Highlighter 项目源码已就绪**：
- ✅ Highlighter 项目完整源码在 `c:\Users\NexusStudio\source\repos\BetterCommentsPlus\Highlighter`
- ✅ 核心类：`Adorner.cs`、`HighlightTag.cs`、`Helper.cs`、`Enums.cs`
- ✅ 配置 UI：`HighlighterOptionsPage.xaml`
- ✅ 完整的背景高亮实现

#### 任务清单：
- [x] 5.1 移植 Highlighter 基础设施类 ✅
  - [x] 5.1.1 移植 `Enums.cs` → 重命名为 `BackgroundEnums.cs`（避免命名冲突）
  - [x] 5.1.2 移植 `Helper.cs` → 整合到现有 `ColorHelper` 或创建 `BackgroundHelper`
- [x] 5.2 移植 Highlighter 核心 Adorner 机制 ✅
  - [x] 5.2.1 移植 `Adorner.cs` → 重命名为 `BackgroundAdorner.cs`
  - [x] 5.2.2 移植 `AdornerTextViewCreationListener.cs`
  - [x] 5.2.3 修改命名空间为 `BetterCommentsPlus.CommentsViewCustomization`
- [x] 5.3 扩展数据结构，支持完整的背景配置 ✅
  - [x] 5.3.1 完善 `BackgroundStyle.cs`，添加所有属性（Shape、Blur、Alpha 等）
  - [x] 5.3.2 扩展 `CommentToken.cs`，添加背景配置属性
  - [x] 5.3.3 扩展 `Settings.cs`，支持背景配置的保存/加载
- [x] 5.4 在 Options 页面添加完整的背景配置 UI ✅
  - [x] 5.4.1 添加背景色启用/禁用开关（复选框）
  - [x] 5.4.2 添加背景色选择按钮（类似前景色）
  - [x] 5.4.3 添加背景形状选择（Tag、TagUnder、Line、LineUnder）
  - [x] 5.4.4 添加背景模糊选择（None、Low、Medium、High、Ultra）
  - [x] 5.4.5 添加背景透明度选择（Alpha_0_10 到 Alpha_10_10，显示为 0/10 到 10/10）
  - [x] 5.4.6 添加高级选项（大小写敏感、部分匹配）
- [x] 5.5 整合前景和背景高亮，确保可以独立或同时工作 ✅
  - [x] 5.5.1 前景和背景可以独立启用/禁用 ✅
  - [x] 5.5.2 前景和背景可以同时工作 ✅
  - [x] 5.5.3 配置变更后立即生效 ✅
- [x] 5.6 全面测试和问题修复 ✅
  - [x] 5.6.1 修复 ComboBox 绑定类型问题（ComboBoxItem → sys:String）✅
  - [x] 5.6.2 修复属性变更通知链断裂（CommentToken 转发 BackgroundStyle 事件）✅
  - [x] 5.6.3 修复属性名错误（token.Criteria → token.CurrentValue）✅
  - [x] 5.6.4 修复颜色选择后未触发刷新（添加 OnConfigurationChanged 调用）✅
  - [x] 5.6.5 修复 ColorHex 缺少默认值（3 个位置确保有默认值）✅
  - [x] 5.6.6 修复 Adorner Layer Z-Order 问题（After Caret, Before Selection）✅
  - [x] 5.6.7 添加缺失文件到项目（BackgroundAdorner.cs 等 4 个核心文件）✅
  - [x] 5.6.8 移除无用文件（DynamicClassificationManager.cs）✅

**验证点**：
- [x] 可以独立启用/禁用前景或背景 ✅
- [x] 前景和背景可以同时工作 ✅
- [x] 所有背景形状正常显示（Tag、TagUnder、Line、LineUnder）✅
- [x] 所有模糊强度正常工作（None、Low、Medium、High、Ultra）✅
- [x] 所有透明度级别正常工作（0/10 到 10/10）✅
- [x] 单行和多行注释都正常高亮 ✅
- [x] 配置变更后立即生效 ✅
- [x] 性能良好，无明显卡顿 ✅
- [x] 向后兼容：旧配置继续正常工作 ✅

**已创建/修改的文件**：
- CommentsViewCustomization/BackgroundAdorner.cs ✅（完全重写，100% 按照 Highlighter 实现）
- CommentsViewCustomization/BackgroundAdornerTextViewCreationListener.cs ✅
- CommentsViewCustomization/BackgroundEnums.cs ✅
- CommentsViewCustomization/BackgroundHelper.cs ✅
- Options/Infrastructure/BackgroundStyle.cs ✅
- Options/Infrastructure/CommentToken.cs ✅
- Options/Settings.cs ✅
- Options/OptionsTokensPageControl.xaml ✅
- Options/OptionsTokensPageControl.xaml.cs ✅
- Better Comments Plus.csproj ✅

**关键问题与解决方案**：
1. **关键文件未添加到项目** - 最核心的问题，BackgroundAdorner.cs 等 4 个文件虽然存在但未被编译
2. **属性名称错误** - BackgroundAdorner 访问 token.Criteria，实际应为 token.CurrentValue
3. **ComboBox 绑定类型错误** - XAML 中使用 ComboBoxItem 但绑定属性是字符串类型
4. **属性变更通知链断裂** - BackgroundStyle 嵌套属性变化时，CommentToken 不会转发 PropertyChanged 事件
5. **颜色选择后未触发刷新** - 选择背景色后没有调用 OnConfigurationChanged()
6. **ColorHex 缺少默认值** - RefreshCriteria() 会过滤掉 ColorHex 为空的 Token
7. **Adorner Layer Z-Order 问题** - Adorner 层顺序不当，可能被文本格式覆盖

---

### 3.6 阶段六：实现右键菜单创建/编辑规则功能 ✅

**目标**：移植 Highlighter 的右键菜单功能，支持快速创建/编辑规则

**状态**：✅ **已完成** (2026-03-18)

**策略**：完整移植 Highlighter 的右键菜单实现，整合到 BCP 项目

**Highlighter 源码参考**：
- ✅ `Highlighter/Commands/CreateHighlight.cs` - 右键命令处理
- ✅ `Highlighter/Commands/EditColor.xaml` - 编辑对话框 UI
- ✅ `Highlighter/Commands/EditColor.xaml.cs` - 编辑对话框逻辑
- ✅ `Highlighter/VSCommandTable.vsct` - 命令定义
- ✅ `Highlighter/VSCommandTable.cs` - 命令 ID 定义

#### 任务清单：
- [x] 6.1 移植 EditColor 对话框 ✅
  - [x] 6.1.1 创建 `EditStyleRuleDialog.xaml`（基于 EditColor.xaml）
  - [x] 6.1.2 创建 `EditStyleRuleDialog.xaml.cs`（基于 EditColor.xaml.cs）
  - [x] 6.1.3 修改 UI 以同时支持 Foreground 和 Background 配置
  - [x] 6.1.4 实现预览功能（同时预览前景和背景效果）
- [x] 6.2 创建右键菜单命令 ✅
  - [x] 6.2.1 创建 `CreateEditStyleRule.cs`（基于 CreateHighlight.cs）
  - [x] 6.2.2 定义 VSCommandTable（两个菜单项：Create/Edit Foreground 和 Create/Edit Background）
  - [x] 6.2.3 实现命令处理逻辑
  - [x] 6.2.4 获取选中文本并自动填充到对话框
- [x] 6.3 实现配置层级（Global/Solution）✅
  - [x] 6.3.1 扩展 UnifiedConfig 支持 Solution 级别配置
  - [x] 6.3.2 实现配置保存/加载（Global 和 Solution 分离）
  - [x] 6.3.3 实现 Remember 类（记住用户最后使用的设置）
- [x] 6.4 集成到主项目 ✅
  - [x] 6.4.1 在 VSPackage.cs 中注册命令
  - [x] 6.4.2 确保命令只在有选中文本时可用
  - [x] 6.4.3 处理新建规则和编辑现有规则的逻辑分支
- [x] 6.5 优化用户体验 ✅
  - [x] 6.5.1 两个右键入口（Foreground 默认启用 / Background 默认启用）
  - [x] 6.5.2 智能默认值（根据选中位置判断是否在注释中）
  - [x] 6.5.3 实时预览效果
  - [x] 6.5.4 支持删除规则（编辑对话框中的 Delete 按钮）
- [x] 6.6 修复 Global 和 Solution 规则优先级问题 ✅
  - [x] 6.6.1 修改 RequiredAndUniqueRule，允许 Global 和 Solution 有相同 Criteria ✅
  - [x] 6.6.2 修改 ValidateTokens，分别验证 Global 和 Solution 集合 ✅
  - [x] 6.6.3 确保 Solution Rules 优先级高于 Global Rules ✅

**验证点**：
- [x] 选中任意文本时，右键菜单显示两个菜单项：「Create/Edit Foreground Styles Rule」和「Create/Edit Background Styles Rule」
- [x] 点击菜单项弹出编辑对话框
- [x] 对话框自动填充选中的文本
- [x] 可以配置 Foreground 样式（颜色、粗体、斜体等）
- [x] 可以配置 Background 样式（颜色、形状、模糊、透明度）
- [x] 可以选择 Global 或 Solution 级别
- [x] 新建规则立即生效
- [x] 编辑现有规则立即生效
- [x] 可以删除规则
- [x] 记住用户最后使用的设置（形状、模糊、透明度等）
- [x] Global 和 Solution 中允许有相同的 Criteria
- [x] Solution Rules 优先级高于 Global Rules
- [x] 验证逻辑正确：各自集合内部唯一，但两个集合之间允许重复

**预计创建/修改的文件**：
- Options/EditStyleRuleDialog.xaml（新建）✅
- Options/EditStyleRuleDialog.xaml.cs（新建）✅
- Commands/CreateEditStyleRule.cs（新建）✅
- VSCommandTable.vsct（修改，添加两个菜单项）✅
- VSCommandTable.cs（修改，添加命令 ID）✅
- Options/UnifiedConfig.cs（扩展，添加 Solution 级别配置）✅
- Options/Settings.cs（扩展，添加配置加载/保存）✅
- VSPackage.cs（修改，注册命令）✅
- Options/Infrastructure/RequiredAndUniqueRule.cs（修改，修复验证逻辑）✅
- Options/OptionsPageBase.cs（修改，分别验证 Global 和 Solution）✅

**交互设计要点**（来自 Merger_Interaction_Discussion.md）：

```
右键菜单（选中任意文本时）：
├─ Create/Edit Foreground Styles Rule  ← Foreground 默认启用
└─ Create/Edit Background Styles Rule  ← Background 默认启用
```

**行为差异**：

| 入口                                 | Foreground 默认 | Background 默认 | 适用场景               |
| ---------------------------------- | ------------- | ------------- | ------------------ |
| Create/Edit Foreground Styles Rule | ☑ 启用          | ☐ 禁用          | 用户主要想修改文字样式（如红色粗体） |
| Create/Edit Background Styles Rule | ☐ 禁用          | ☑ 启用          | 用户主要想添加背景高亮（如黄色底）  |

**对话框设计**：
```
┌─────────────────────────────────────────────────────────────┐
│ Create/Edit Styles Rule                                 [X] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Match Text: [#TODO                                     ]   │ ← 自动填充
│                                                             │
│  Apply To:                                                  │
│  (● Comments only)  (○ Any text)                           │
│                                                             │
│  Save To:                                                   │
│  (○ Global)  (● Solution)                                  │ ← 默认 Solution
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│  ☑ Foreground Style                                         │
│     Color:    [🔴 #FF0000    ]  [Color Picker]              │
│     Style:    ☑ Bold    ☐ Italic    ☐ Underline            │
│     Opacity:  [██████░░░░] 60%                              │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│  ☑ Background Style                                         │
│     Color:    [🟡 #FFD700    ]  [Color Picker]              │
│     Shape:    [Tag Under ▼]                                 │
│     Blur:     [None ▼]                                      │
│     Alpha:    [20% ▼]                                       │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│  Preview:                                                   │
│  ┌─────────────────────────────────────────────┐           │
│  │ // #TODO: Fix this bug                      │           │
│  │    [红字 + 黄底下划线效果预览]               │           │
│  └─────────────────────────────────────────────┘           │
│                                                             │
│              [Delete]      [Cancel]      [Create]           │
└─────────────────────────────────────────────────────────────┘
```

---

## 4. 关键技术实现要点

### 4.1 ListBox 拖拽排序实现方案

推荐使用 WPF 原生的 DragDrop API 实现：

```csharp
// 在 ListBox 的 PreviewMouseLeftButtonDown 事件中启动拖拽
private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    // 获取点击的项
    var clickedItem = GetItemAt(e.GetPosition(listBox));
    if (clickedItem != null)
    {
        // 开始拖拽
        DragDrop.DoDragDrop(listBox, clickedItem, DragDropEffects.Move);
    }
}

// 在 ListBox 的 Drop 事件中处理放置
private void ListBox_Drop(object sender, DragEventArgs e)
{
    // 获取源项和目标项
    var sourceItem = e.Data.GetData(typeof(StyleRule)) as StyleRule;
    var targetItem = GetItemAt(e.GetPosition(listBox));
    
    if (sourceItem != null && targetItem != null && sourceItem != targetItem)
    {
        // 调整顺序
        ReorderItems(sourceItem, targetItem);
    }
}
```

### 4.2 配置变更实时生效机制

使用事件通知模式：

```csharp
public class UnifiedConfigurationManager
{
    public static UnifiedConfigurationManager Instance { get; } = new UnifiedConfigurationManager();
    
    public event EventHandler ConfigurationChanged;
    
    public UnifiedConfig Config { get; private set; }
    
    public void Save()
    {
        // 保存到文件
        OnConfigurationChanged();
    }
    
    public void Load()
    {
        // 从文件加载
        OnConfigurationChanged();
    }
    
    protected virtual void OnConfigurationChanged()
    {
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }
}

// 在 CommentTagger 中监听
public class CommentTagger : ITagger<ClassificationTag>
{
    public CommentTagger()
    {
        UnifiedConfigurationManager.Instance.ConfigurationChanged += OnConfigurationChanged;
    }
    
    private void OnConfigurationChanged(object sender, EventArgs e)
    {
        // 触发 TagsChanged 事件，让 VS 重新标记
        TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(...));
    }
}
```

### 4.3 动态 Classification 注册（避免 VS「字体和颜色」）

不使用 [Export] 特性硬编码，而是动态创建 ClassificationType：

```csharp
public class DynamicClassificationManager
{
    [Import]
    internal IClassificationTypeRegistryService ClassificationRegistry { get; set; }
    
    public void CreateDynamicClassification(string classificationName, Color foregroundColor)
    {
        var classificationType = ClassificationRegistry.GetClassificationType(classificationName);
        if (classificationType == null)
        {
            classificationType = ClassificationRegistry.CreateClassificationType(
                classificationName, 
                new[] { ClassificationRegistry.GetClassificationType("comment") });
        }
        
        // 通过 IClassificationFormatMap 应用样式
        var formatMap = FormatMapService.GetClassificationFormatMap(category);
        var format = new TextFormattingRunProperties();
        format = format.SetForegroundBrush(new SolidColorBrush(foregroundColor));
        formatMap.SetTextProperties(classificationType, format);
    }
}
```

### 4.4 右键菜单命令实现（Highlighter 参考）

```csharp
[Command(PackageIds.CreateHighlight)]
internal sealed class CreateEditStyleRule : BaseCommand<CreateEditStyleRule>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var dte = (DTE)Package.GetGlobalService(typeof(DTE));
        string selection = (dte.ActiveDocument.Selection as TextSelection).Text;

        if (string.IsNullOrEmpty(selection))
            return;

        // 检查是否已存在该规则
        var existingRule = Settings.Instance.CommentTokens
            .FirstOrDefault(x => x.CurrentValue == selection);
        
        bool isNew = existingRule == null;
        
        // 准备对话框
        var dialog = new EditStyleRuleDialog
        {
            RuleToEdit = existingRule ?? CreateNewRule(selection),
            Title = isNew ? "Create Style Rule" : "Edit Style Rule",
            btnCreate = { Content = isNew ? "Create" : "Save" },
            btnDelete = { Visibility = isNew ? Visibility.Collapsed : Visibility.Visible }
        };

        bool result = dialog.ShowDialog().Value;

        if (result)
        {
            // 保存规则
            SaveRule(dialog.RuleToEdit, dialog.IsGlobal);
        }
        else if (dialog.delete)
        {
            // 删除规则
            DeleteRule(dialog.RuleToEdit);
        }
    }
}
```

---

## 5. 风险与应对措施

| 风险 | 影响 | 概率 | 应对措施 |
|------|------|------|---------|
| 移除 VS「字体和颜色」后，用户习惯受到影响 | 中 | 高 | 提供迁移向导，告知用户新的配置位置 |
| 动态 ClassificationType 注册在某些 VS 版本中不工作 | 高 | 中 | 充分测试不同 VS 版本，保留降级方案 |
| ListBox 拖拽在高 DPI 下行为异常 | 低 | 中 | 使用 WPF 官方推荐的拖拽实现，测试高 DPI 场景 |
| 配置变更后性能下降（频繁重新标记） | 中 | 中 | 添加防抖机制，避免频繁触发重新标记 |
| JSON 格式变更导致旧版本配置无法读取 | 高 | 中 | 实现配置迁移器，自动升级旧版本配置 |
| 右键菜单与 VS 其他扩展冲突 | 低 | 低 | 使用唯一的命令 ID，避免命名冲突 |

---

## 6. 验收标准

### 6.1 功能验收
- [x] 可以通过 ColorPicker 选择颜色，不依赖 VS「字体和颜色」设置 ✅
- [x] 可以通过拖拽调整规则顺序 ✅
- [x] 配置变更后立即在编辑器中生效 ✅
- [x] 拖拽有视觉指示（半透明效果 + 蓝色指示线）✅
- [x] Delete 按钮功能正常 ✅
- [x] 输入框在失去焦点时才更新，改善用户体验 ✅
- [x] 可以为每个注释类型独立设置粗体、斜体、下划线、删除线 ✅
- [x] 有左侧拖拽手柄（☰），分离拖拽区域和控件交互 ✅
- [x] Add 按钮功能正常，不会导致 VS 无响应 ✅
- [x] 点击取消或关闭按钮不会导致断言失败 ✅
- [x] Reset 按钮完整重置所有样式属性 ✅
- [x] 可以动态添加新的 StyleRule ✅
- [x] 可以删除现有 StyleRule（预定义规则也可删除）✅
- [x] 可以独立配置前景和背景样式 ✅
- [x] 前景和背景可以同时工作 ✅
- [x] 所有背景形状正常显示（Tag、TagUnder、Line、LineUnder）✅
- [x] 所有模糊强度正常工作（None、Low、Medium、High、Ultra）✅
- [x] 所有透明度级别正常工作（0/10 到 10/10）✅
- [x] JSON 配置能够正确导入和导出 ✅
- [x] 右键菜单显示两个菜单项：「Create/Edit Foreground Styles Rule」和「Create/Edit Background Styles Rule」 ✅
- [x] 两个菜单项分别默认启用 Foreground/Background ✅
- [x] 编辑对话框支持配置 Foreground 和 Background ✅
- [x] 支持 Global 和 Solution 两级配置 ✅
- [x] 新建/编辑/删除规则功能正常 ✅
- [x] 实时预览效果 ✅

### 6.2 兼容性验收
- [ ] 旧版本配置能够自动迁移到新版本
- [ ] 在 VS 2019、VS 2022 上都能正常工作
- [ ] 在高 DPI 显示器上显示正常

### 6.3 性能验收
- [x] 配置变更后的重新标记流畅 ✅
- [x] 拖拽流畅，无明显卡顿 ✅
- [ ] 打开大型文件（>10000 行）时无明显卡顿
- [ ] 内存占用与旧版本相比增加不超过 20%

---

## 进度概览

| 阶段 | 状态 | 完成度 |
|------|------|--------|
| 阶段一：解除与 VS「字体和颜色」的绑定 | ✅ 已完成 | 100% |
| 阶段二（原阶段三）：实现可拖拽的 ListBox 和配置变更实时生效 | ✅ 已完成 | 100% |
| 阶段三（原阶段四）：实现完整的前景样式配置 | ✅ 已完成 | 100% |
| 阶段四：实现动态 CommentToken 生成（渐进式激进重构） | ✅ 已完成 | 100% |
| 阶段五：完整合并 Highlighter（背景高亮功能） | ✅ 已完成 | 100% |
| 阶段六：实现右键菜单创建/编辑规则功能 | ✅ 已完成 | 100% |
| **总体进度** | | **100%** |

---

*文档版本：3.2*
*更新日期：2026-03-18*
*上次更新：完成前景色启用/禁用功能，修复所有编译错误，实现完整的前景样式控制；修复 Global 和 Solution 规则优先级问题*
