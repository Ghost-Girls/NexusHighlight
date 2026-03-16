# Better Comments Plus + Highlighter 合并项目交互设计方案

## 1. 项目概述

### 1.1 现有项目分析

| 特性 | Better Comments Plus | Highlighter |
|------|---------------------|-------------|
| **核心功能** | 修改注释文字的前景色(Foreground) | 修改任意文本的背景色(Background) |
| **作用范围** | 仅针对代码注释 | 任意选中文本 |
| **触发方式** | 自动识别注释中的Token | 右键菜单「Create/Edit Highlight Rule」 |
| **配置层级** | 全局配置 | 全局规则(Global) + 解决方案规则(Solution) |
| **样式属性** | 颜色、粗细、斜体、删除线、下划线 | 颜色、形状、模糊度、透明度 |
| **作用域** | 基于ClassificationType | 基于Adorner层 |

### 1.2 合并核心挑战

1. **独立使用原则**：用户可能只想修改Foreground而不修改Background，反之亦然
2. **作用域差异**：Better Comments Plus只针对注释，Highlighter针对任意文本
3. **配置层级差异**：Better Comments Plus是全局配置，Highlighter支持Global+Solution两级
4. **交互方式差异**：Better Comments Plus通过Options页面配置，Highlighter通过右键菜单+Options页面

---

## 2. 合并后的架构设计

### 2.1 统一的数据模型

```csharp
// 统一的样式规则基类
public class StyleRule
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Criteria { get; set; }  // Token值或匹配文本
    
    // Foreground 样式（来自 Better Comments Plus）
    public ForegroundStyle Foreground { get; set; }
    
    // Background 样式（来自 Highlighter）
    public BackgroundStyle Background { get; set; }
    
    // 规则作用域
    public RuleScope Scope { get; set; }  // CommentOnly / AnyText
    
    // 配置层级
    public ConfigLevel Level { get; set; }  // Global / Solution
    
    // 启用状态
    public bool IsEnabled { get; set; }
    public int Order { get; set; }
}

public class ForegroundStyle
{
    public bool IsEnabled { get; set; }  // 是否启用前景色修改
    public Color Color { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool HasUnderline { get; set; }
    public bool HasStrikethrough { get; set; }
    public double Opacity { get; set; }
    public string Font { get; set; }
    public double Size { get; set; }
}

public class BackgroundStyle
{
    public bool IsEnabled { get; set; }  // 是否启用背景色修改
    public Color Color { get; set; }
    public TagShape Shape { get; set; }  // Tag / TagUnder / Line / LineUnder
    public BlurIntensity Blur { get; set; }
    public FillAlpha Alpha { get; set; }
}

public enum RuleScope
{
    CommentOnly,  // 仅针对注释（原Better Comments Plus行为）
    AnyText       // 针对任意文本（原Highlighter行为）
}

public enum ConfigLevel
{
    Global,   // 全局规则
    Solution  // 解决方案级规则
}
```

### 2.2 配置结构

```json
{
  "version": "2.0",
  "exportDate": "2025-03-14T10:30:00",
  "globalRules": [
    {
      "id": "token-important",
      "displayName": "Important",
      "criteria": "!",
      "scope": "CommentOnly",
      "level": "Global",
      "isEnabled": true,
      "order": 1,
      "foreground": {
        "isEnabled": true,
        "color": "#FF0000",
        "isBold": true,
        "isItalic": false,
        "hasUnderline": false,
        "hasStrikethrough": false,
        "opacity": 1.0
      },
      "background": {
        "isEnabled": false,
        "color": "#000000",
        "shape": "Tag",
        "blur": "None",
        "alpha": "Alpha_10_10"
      }
    },
    {
      "id": "highlight-todo",
      "displayName": "TODO Highlight",
      "criteria": "TODO",
      "scope": "AnyText",
      "level": "Global",
      "isEnabled": true,
      "order": 2,
      "foreground": {
        "isEnabled": false
      },
      "background": {
        "isEnabled": true,
        "color": "#FFD700",
        "shape": "TagUnder",
        "blur": "Low",
        "alpha": "Alpha_20_20"
      }
    }
  ],
  "solutionRules": [],
  "settings": {
    "performance": "Normal",
    "defaultForegroundEnabled": true,
    "defaultBackgroundEnabled": true
  }
}
```

---

## 3. 交互设计方案

### 3.1 设置页面结构

采用 **TabControl** 组织设置页面，分为三个主要Tab：

```
┌─────────────────────────────────────────────────────────────┐
│  Better Comments Plus + Highlighter                      [X] │
├─────────────────────────────────────────────────────────────┤
│  [ General ] [ Comment Tokens ] [ Highlight Rules ] [ Import/Export ] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [当前选中Tab的内容区域]                                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### Tab 1: General（通用设置）

包含全局开关和默认行为设置：

```
┌─────────────────────────────────────────────────────────────┐
│ General Settings                                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ☑ Enable Comment Styling (Foreground)                      │
│    [当勾选时，注释Token的前景色样式生效]                      │
│                                                             │
│  ☑ Enable Text Highlighting (Background)                    │
│    [当勾选时，文本高亮的背景色样式生效]                       │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  Performance: [Normal ▼]                                    │
│  [Fast / Normal / No Effects]                               │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  Default Style Settings:                                    │
│  When creating new rule, default enable:                    │
│    ☑ Foreground styling                                     │
│    ☑ Background styling                                     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### Tab 2: Comment Tokens（注释Token样式）

继承自 Better Comments Plus 的Token列表，但增强支持Background样式：

```
┌─────────────────────────────────────────────────────────────┐
│ Comment Tokens                                              │
├─────────────────────────────────────────────────────────────┤
│ [Add Token] [Import] [Export] [Reset to Default]           │
│ [↑ Move Up] [↓ Move Down]                                   │
├─────────────────────────────────────────────────────────────┤
│ Display Name │ Token │ Foreground │ Background │ Enabled │ Actions │
├─────────────────────────────────────────────────────────────┤
│ Important    │   !   │   [🔴]     │   [⚪]      │   [☑]   │ [Edit][Delete] │
│ Question     │   ?   │   [🔵]     │   [⚪]      │   [☑]   │ [Edit][Delete] │
│ Task         │  TODO │   [🟢]     │   [🟡]      │   [☑]   │ [Edit][Delete] │
│ ...          │  ...  │   [...]    │   [...]     │   [...] │ ...            │
└─────────────────────────────────────────────────────────────┘
```

**Token编辑对话框**：

```
┌─────────────────────────────────────────────────────────────┐
│ Edit Comment Token                                      [X] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Display Name: [Important                    ]              │
│  Token Value:  [!                             ]             │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│  ☑ Enable Foreground Styling                                │
│                                                             │
│     Color: [🔴 #FF0000    ]  [Color Picker]                 │
│     ☑ Bold    ☐ Italic    ☐ Underline    ☐ Strikethrough   │
│     Opacity: [██████░░░░] 60%                               │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│  ☑ Enable Background Styling                                │
│                                                             │
│     Color: [🟡 #FFD700    ]  [Color Picker]                 │
│     Shape: [Tag Under ▼]  [Tag/Tag Under/Full Line/Full Underline]
│     Blur:  [Low ▼]        [None/Low/Medium/High/Ultra]      │
│     Alpha: [20% ▼]        [0%-100%]                         │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  Preview:                                                   │
│  ┌─────────────────────────────────────┐                    │
│  │ // ! This is important comment      │                    │
│  │   [应用了Foreground+Background效果]  │                    │
│  └─────────────────────────────────────┘                    │
│                                                             │
│              [Cancel]              [Save]                   │
└─────────────────────────────────────────────────────────────┘
```

#### Tab 3: Highlight Rules（高亮规则）

继承自 Highlighter 的设计，支持两级规则（Global + Solution）：

```
┌─────────────────────────────────────────────────────────────┐
│ Highlight Rules                                             │
├─────────────────────────────────────────────────────────────┤
│ [Global Rules] [Solution Rules]                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  These rules are applied across all projects.               │
│                                                             │
│  [Add Rule] [Import] [Export]                               │
│                                                             │
│ Active │ Color │ Criteria │ Shape │ Blur │ Alpha │ Scope │ Actions │
├─────────────────────────────────────────────────────────────┤
│  [☑]   │ [🟡]  │ TODO     │ TagUnder│ Low  │ 20%  │ AnyText │ [Edit][Delete] │
│  [☑]   │ [🟢]  │ FIXME    │ Tag     │ None │ 10%  │ AnyText │ [Edit][Delete] │
│  [☑]   │ [🔴]  │ BUG      │ Line    │ High │ 30%  │ AnyText │ [Edit][Delete] │
│  ...   │ ...   │ ...      │ ...     │ ...  │ ...  │ ...     │ ...            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**高亮规则编辑对话框**：

```
┌─────────────────────────────────────────────────────────────┐
│ Edit Highlight Rule                                     [X] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Criteria: [TODO                              ]             │
│                                                             │
│  ☑ Case Sensitive    ☑ Allow Partial Match                  │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│  ☑ Enable Foreground Styling                                │
│                                                             │
│     Color: [⚫ #000000    ]  [Color Picker]                 │
│     ☑ Bold    ☐ Italic    ☐ Underline    ☐ Strikethrough   │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│  ☑ Enable Background Styling                                │
│                                                             │
│     Color: [🟡 #FFD700    ]  [Color Picker]                 │
│     Shape: [Tag Under ▼]                                    │
│     Blur:  [Low ▼]                                          │
│     Alpha: [20% ▼]                                          │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  Rule Level:  (○ Global)  (● Solution)                      │
│                                                             │
│  Preview:                                                   │
│  ┌─────────────────────────────────────┐                    │
│  │ TODO: Implement this feature        │                    │
│  │   [应用了Foreground+Background效果]  │                    │
│  └─────────────────────────────────────┘                    │
│                                                             │
│              [Cancel]              [Save]                   │
└─────────────────────────────────────────────────────────────┘
```

#### Tab 4: Import/Export（导入导出）

```
┌─────────────────────────────────────────────────────────────┐
│ Import / Export                                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Export Configuration:                                      │
│  [Export All Settings...]                                   │
│                                                             │
│  Import Configuration:                                      │
│  [Select File...]  [config.json                    ] [Import]│
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│                                                             │
│  Import Options:                                            │
│  (○ Replace all existing settings)                          │
│  (● Merge with existing settings)                           │
│                                                             │
│  ☑ Import Comment Tokens                                    │
│  ☑ Import Highlight Rules                                   │
│  ☑ Import General Settings                                  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

### 3.2 右键菜单集成

保留 Highlighter 的右键菜单功能，但增强以支持Foreground样式：

```
右键菜单:
├─ Cut
├─ Copy
├─ Paste
├─ ...
├─ Create/Edit Style Rule ───────┐
│                                │
│   [输入框: 选中的文本]          │
│                                │
│   Apply to:                    │
│   (● This text only)           │
│   (○ All comments with token)  │
│                                │
│   ─────────────────────────    │
│   ☑ Foreground Style           │
│      Color: [🔴] Bold ☑        │
│   ─────────────────────────    │
│   ☑ Background Style           │
│      Color: [🟡] Shape: [Tag ▼]│
│   ─────────────────────────    │
│                                │
│   [Cancel]  [Create Rule]      │
│                                │
└────────────────────────────────┘
```

---

### 3.3 渲染优先级与冲突解决

当同一个文本同时匹配多个规则时，按以下优先级处理：

1. **作用域优先级**：`CommentOnly` 规则优先于 `AnyText` 规则（在注释内）
2. **顺序优先级**：Order值小的优先
3. **样式合并规则**：
   - Foreground：如果多个规则都启用了Foreground，以第一个匹配的规则为准
   - Background：如果多个规则都启用了Background，以第一个匹配的规则为准
   - 如果同一个规则同时启用了Foreground和Background，两者都应用

---

## 4. 数据迁移策略

### 4.1 从旧版本迁移

```csharp
public class ConfigurationMigrator
{
    public static UnifiedConfig Migrate(
        PluginConfiguration bcpConfig, 
        HighlighterConfig hlConfig)
    {
        var unified = new UnifiedConfig();
        
        // 迁移 Better Comments Plus 的Token
        foreach (var token in bcpConfig.Tokens)
        {
            unified.GlobalRules.Add(new StyleRule
            {
                Id = token.Id,
                DisplayName = token.DisplayName,
                Criteria = token.TokenValue,
                Scope = RuleScope.CommentOnly,
                Level = ConfigLevel.Global,
                IsEnabled = token.IsEnabled,
                Order = token.Order,
                Foreground = new ForegroundStyle
                {
                    IsEnabled = true,
                    Color = token.Color,
                    IsBold = token.IsBold,
                    IsItalic = token.IsItalic,
                    HasUnderline = token.HasUnderline,
                    HasStrikethrough = token.HasStrikethrough
                },
                Background = new BackgroundStyle
                {
                    IsEnabled = token.BackgroundColor.HasValue,
                    Color = token.BackgroundColor ?? Colors.Transparent,
                    Shape = TagShape.Tag,
                    Blur = BlurIntensity.None,
                    Alpha = FillAlpha.Alpha_10_10
                }
            });
        }
        
        // 迁移 Highlighter 的规则
        foreach (var rule in hlConfig.GlobalRules)
        {
            unified.GlobalRules.Add(new StyleRule
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = rule.Criteria,
                Criteria = rule.Criteria,
                Scope = RuleScope.AnyText,
                Level = ConfigLevel.Global,
                IsEnabled = rule.IsActive,
                Order = 100,  // 高亮规则默认排在后面
                Foreground = new ForegroundStyle { IsEnabled = false },
                Background = new BackgroundStyle
                {
                    IsEnabled = true,
                    Color = Helper.HexToColor(rule.Color),
                    Shape = (TagShape)Enum.Parse(typeof(TagShape), rule.Shape),
                    Blur = (BlurIntensity)Enum.Parse(typeof(BlurIntensity), rule.Blur),
                    Alpha = (FillAlpha)Enum.Parse(typeof(FillAlpha), rule.Alpha)
                }
            });
        }
        
        // 迁移 Solution 规则...
        
        return unified;
    }
}
```

---

## 5. 技术实现要点

### 5.1 渲染管线

```
文本编辑器渲染流程:
│
├─> CommentTagger (Better Comments Plus)
│   └─> 识别注释中的Token
│       └─> 应用 ClassificationTag (Foreground样式)
│
├─> Adorner (Highlighter)
│   └─> 扫描所有文本
│       └─> 匹配规则
│           └─> 绘制背景装饰 (Background样式)
│
└─> CommentViewDecorator (Better Comments Plus)
    └─> 应用全局字体设置
```

### 5.2 关键类设计

```csharp
// 统一的配置管理器
public class UnifiedConfigurationManager
{
    public static UnifiedConfigurationManager Instance { get; }
    
    public List<StyleRule> GlobalRules { get; }
    public List<StyleRule> SolutionRules { get; }
    
    public event EventHandler ConfigurationChanged;
    
    public void Save();
    public void Load();
    public void Export(string path);
    public void Import(string path, ImportOptions options);
}

// 样式应用器
public class StyleApplicator
{
    // 应用Foreground样式（通过Classification）
    public void ApplyForeground(StyleRule rule, IClassificationFormatMap formatMap);
    
    // 应用Background样式（通过Adorner）
    public void ApplyBackground(StyleRule rule, IWpfTextView textView);
}

// 规则匹配器
public class RuleMatcher
{
    public List<StyleRule> GetMatchingRules(string text, RuleScope scope);
    public StyleRule GetBestMatch(string text, RuleScope scope, StyleType type);
}
```

---

## 6. 用户场景示例

### 场景1：仅使用注释样式（原BCP用户）

```
用户操作:
1. 安装合并后的扩展
2. 进入 Settings → General
3. ☑ Enable Comment Styling
4. ☐ Enable Text Highlighting
5. 进入 Comment Tokens Tab，配置Token样式
6. 仅启用Foreground，关闭Background

结果:
- 注释中的 //! 显示为红色粗体（仅Foreground）
- 不产生任何Background高亮效果
```

### 场景2：仅使用高亮功能（原Highlighter用户）

```
用户操作:
1. 安装合并后的扩展
2. 进入 Settings → General
3. ☐ Enable Comment Styling
4. ☑ Enable Text Highlighting
5. 进入 Highlight Rules Tab，添加规则
6. 仅启用Background，关闭Foreground

结果:
- 选中的TODO文本显示黄色背景（仅Background）
- 不产生任何Foreground样式修改
```

### 场景3：组合使用（新用户）

```
用户操作:
1. 安装合并后的扩展
2. 进入 Settings → General
3. ☑ Enable Comment Styling
4. ☑ Enable Text Highlighting
5. 配置 Comment Token "!"：
   - Foreground: 红色 + 粗体
   - Background: 黄色 + TagUnder
6. 配置 Highlight Rule "TODO"：
   - Foreground: 绿色
   - Background: 浅蓝色 + Line

结果:
- 注释中的 //! 显示红色粗体文字 + 黄色下划线背景
- 任意位置的TODO显示绿色文字 + 浅蓝色整行背景
```

---

## 7. 总结

### 7.1 设计原则

1. **向后兼容**：自动迁移旧版本配置
2. **独立控制**：Foreground和Background可以独立启用/禁用
3. **统一入口**：所有配置集中在一个Options页面
4. **灵活作用域**：支持CommentOnly和AnyText两种作用域
5. **层级配置**：保留Global和Solution两级规则体系

### 7.2 待决策事项

1. 是否需要保留 Highlighter 的「右键快速添加规则」功能？
2. Comment Token 是否也需要支持 Solution 级别？
3. 是否需要支持规则的导入导出筛选（只导出Comment Tokens或只导出Highlight Rules）？
4. 性能设置是否需要在规则级别（每个规则可设置Performance）？

---

*文档版本: 1.0*
*创建日期: 2025-03-14*
