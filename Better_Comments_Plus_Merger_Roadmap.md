# Better Comments Plus + Highlighter 合并项目规划与实施指南

## 1. 规划可行性与优先级分析

### 1.1 可行性评估

| 需求项 | 可行性 | 理由 |
|--------|--------|------|
| 1. 从Visual Studio「字体和颜色」中移除CommentColors，改用OptionsTokensPageControl.xaml中的ColorPicker | ✅ **高度可行** | 目前Highlighter已经实现了类似功能，可复用其实现方式 |
| 2. 动态生成CommentToken，取消与Visual Studio「字体和颜色」的关联 | ✅ **高度可行** | 通过移除ClassificationFormatDefinition的硬编码，改用动态注册机制 |
| 3. 合并两个项目的数据结构（统一前景和背景配置） | ✅ **高度可行** | 现有代码基础已提供ForegorundStyle和BackgroundStyle类，可扩展 |
| 4. 使用Listbox实现可拖拽的Criteria列表 | ✅ **可行** | WPF支持ListBox拖拽功能，有成熟实现方案 |
| 5. 数据写入JSON后效果立即生效 | ✅ **高度可行** | 通过事件通知机制和配置重新加载实现 |

### 1.2 优先级建议

| 优先级 | 阶段 | 需求项 |
|--------|------|--------|
| P0（最高） | 第一阶段 | 1. 从VS「字体和颜色」中移除CommentColors<br>2. 实现ColorPicker选择颜色<br>3. 完善JSON数据结构定义 |
| P1 | 第二阶段 | 1. 动态生成CommentToken机制<br>2. 移除与VS「字体和颜色」的硬编码关联<br>3. 基础ListBox展示（不含拖拽） |
| P2 | 第三阶段 | 1. ListBox拖拽排序功能<br>2. 完整的配置UI（前景+背景）<br>3. 配置变更实时生效 |
| P3 | 第四阶段 | 1. 完整的导入导出功能<br>2. 右键菜单集成<br>3. 数据迁移（从旧版本升级） |

---

## 2. 术语统一与数据结构规范化

### 2.1 核心术语统一

基于Merger_Interaction_Discussion.md 1-403中的讨论，统一以下术语：

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
    public string Color { get; set; }         // Hex颜色，如 "#FF0000"
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool HasUnderline { get; set; }
    public bool HasStrikethrough { get; set; }
}

public class BackgroundStyle
{
    public bool IsActive { get; set; }
    public string Color { get; set; }         // Hex颜色，如 "#FF0000"
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

### 2.3 JSON示例（规范化）

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

### 3.1 阶段一：解除与Visual Studio「字体和颜色」的绑定 ✅

**目标**：移除硬编码的ClassificationFormatDefinition，改用ColorPicker选择颜色

**状态**：✅ **已完成** (2026-03-16)

#### 任务清单：
- [x] 1.1 创建新的ColorPicker用户控件（ColorPickerDialog.xaml和ColorPickerDialog.xaml.cs）
- [x] 1.2 修改OptionsTokensPageControl.xaml，添加颜色选择列和预览功能
- [x] 1.3 修改CommentToken类，添加ColorHex属性
- [x] 1.4 修改Settings.cs，将颜色保存到配置中
- [x] 1.5 创建HexToColorConverter值转换器
- [x] 1.6 创建ColorHelper辅助类（颜色转换和预设颜色）
- [x] 1.7 创建ForegroundStyle、BackgroundStyle、StyleRule、UnifiedConfig数据结构
- [x] 1.8 修改CommentViewDecorator.cs，实现动态颜色应用
- [x] 1.9 修复所有编译错误
- [x] 1.10 在ColorPickerDialog中添加HEX值输入功能（2026-03-17）

**验证点**：
- [x] 可以在Options页面中通过ColorPicker选择颜色
- [x] 颜色选择后能够正确保存和加载
- [x] 注释颜色显示正常（不依赖VS「字体和颜色」设置）
- [x] 颜色预览功能正常工作
- [x] 数据本地化（JSON）正常
- [x] 可以通过输入HEX值来自定义颜色（支持带#或不带#，6位或8位格式）
- [x] 输入HEX值时实时预览颜色
- [x] 点击预设颜色时自动填充对应的HEX值

**已创建/修改的文件**：
- Options/Infrastructure/ForegroundStyle.cs ✅
- Options/Infrastructure/BackgroundStyle.cs ✅
- Options/Infrastructure/StyleRule.cs ✅
- Options/Infrastructure/UnifiedConfig.cs ✅
- Options/Infrastructure/CommentToken.cs ✅ (添加ColorHex属性)
- Options/Infrastructure/HexToColorConverter.cs ✅
- Utils/ColorHelper.cs ✅
- Options/Settings.cs ✅
- Options/OptionsTokensPageControl.xaml ✅
- Options/OptionsTokensPageControl.xaml.cs ✅
- Options/ColorPickerDialog.xaml ✅
- Options/ColorPickerDialog.xaml.cs ✅
- CommentsViewCustomization/CommentViewDecorator.cs ✅

---

### 3.2 阶段二：实现动态CommentToken生成 🔄

**目标**：取消固定的CommentType枚举，支持动态添加/删除规则

**状态**：📋 **待执行**

#### 任务清单：
- [ ] 2.1 移除CommentClassificationFormatDefinitions.cs中的硬编码ClassificationFormatDefinition
- [ ] 2.2 移除CommentType枚举
- [ ] 2.3 修改Settings.cs，完全使用新的UnifiedConfig
- [ ] 2.4 完善JSON序列化/反序列化
- [ ] 2.5 修改CommentParser，支持动态规则匹配
- [ ] 2.6 实现动态ClassificationType注册机制

**验证点**：
- [ ] 可以动态添加新的StyleRule
- [ ] 可以删除现有StyleRule（预定义规则除外）
- [ ] JSON配置能够正确保存和加载
- [ ] 注释高亮能够正确应用动态规则
- [ ] 不再依赖VS「字体和颜色」中的CommentColors

---

### 3.3 阶段三：实现可拖拽的ListBox 📋

**目标**：使用ListBox展示规则列表，支持拖拽排序

**状态**：📋 **待执行**

#### 任务清单：
- [ ] 3.1 修改OptionsTokensPageControl.xaml，使用ListBox替代ItemsControl
- [ ] 3.2 实现ListBox项的数据模板（包含Criteria、Foreground预览、Background预览、Actions）
- [ ] 3.3 实现拖拽排序功能（DragDrop API）
- [ ] 3.4 拖拽后更新Order属性
- [ ] 3.5 实现规则的Edit/Delete按钮

**验证点**：
- [ ] ListBox正确显示所有StyleRule
- [ ] 可以通过拖拽调整规则顺序
- [ ] 规则顺序变更后能够正确保存
- [ ] Edit/Delete按钮功能正常

---

### 3.4 阶段四：确保配置变更实时生效 📋

**目标**：修改配置后，注释高亮立即更新

**状态**：📋 **待执行**

#### 任务清单：
- [ ] 4.1 实现配置变更事件（ConfigurationChanged）
- [ ] 4.2 CommentTagger监听配置变更事件
- [ ] 4.3 配置变更后重新标记文本
- [ ] 4.4 Adorner监听配置变更事件
- [ ] 4.5 配置变更后重新绘制背景装饰

**验证点**：
- [ ] 在Options页面修改颜色后，编辑器中的注释立即更新
- [ ] 添加/删除规则后，立即生效
- [ ] 调整规则顺序后，优先级立即生效

---

### 3.5 阶段五：完整的前景+背景配置UI 📋

**目标**：在Options页面中同时配置前景和背景样式

**状态**：📋 **待执行**

#### 任务清单：
- [ ] 5.1 创建StyleRule编辑对话框
- [ ] 5.2 实现Foreground配置面板（Color、Bold、Italic、Underline、Strikethrough）
- [ ] 5.3 实现Background配置面板（Color、Shape、Blur、Alpha、CaseSensitive、AllowPartialMatch）
- [ ] 5.4 实现预览功能
- [ ] 5.5 集成到Options页面

**验证点**：
- [ ] 可以独立配置前景和背景样式
- [ ] 可以独立启用/禁用前景或背景
- [ ] 预览功能正常工作
- [ ] 所有样式属性都能正确保存和加载

---

## 4. 关键技术实现要点

### 4.1 ListBox拖拽排序实现方案

推荐使用WPF原生的DragDrop API实现：

```csharp
// 在ListBox的PreviewMouseLeftButtonDown事件中启动拖拽
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

// 在ListBox的Drop事件中处理放置
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

// 在CommentTagger中监听
public class CommentTagger : ITagger<ClassificationTag>
{
    public CommentTagger()
    {
        UnifiedConfigurationManager.Instance.ConfigurationChanged += OnConfigurationChanged;
    }
    
    private void OnConfigurationChanged(object sender, EventArgs e)
    {
        // 触发TagsChanged事件，让VS重新标记
        TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(...));
    }
}
```

### 4.3 动态Classification注册（避免VS「字体和颜色」）

不使用[Export]特性硬编码，而是动态创建ClassificationType：

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
        
        // 通过IClassificationFormatMap应用样式
        var formatMap = FormatMapService.GetClassificationFormatMap(category);
        var format = new TextFormattingRunProperties();
        format = format.SetForegroundBrush(new SolidColorBrush(foregroundColor));
        formatMap.SetTextProperties(classificationType, format);
    }
}
```

---

## 5. 风险与应对措施

| 风险 | 影响 | 概率 | 应对措施 |
|------|------|------|---------|
| 移除VS「字体和颜色」后，用户习惯受到影响 | 中 | 高 | 提供迁移向导，告知用户新的配置位置 |
| 动态ClassificationType注册在某些VS版本中不工作 | 高 | 中 | 充分测试不同VS版本，保留降级方案 |
| ListBox拖拽在高DPI下行为异常 | 低 | 中 | 使用WPF官方推荐的拖拽实现，测试高DPI场景 |
| 配置变更后性能下降（频繁重新标记） | 中 | 中 | 添加防抖机制，避免频繁触发重新标记 |
| JSON格式变更导致旧版本配置无法读取 | 高 | 中 | 实现配置迁移器，自动升级旧版本配置 |

---

## 6. 验收标准

### 6.1 功能验收
- [x] 可以通过ColorPicker选择颜色，不依赖VS「字体和颜色」设置 ✅
- [ ] 可以动态添加、删除、编辑StyleRule
- [ ] 可以通过拖拽调整StyleRule的顺序
- [ ] 配置变更后立即在编辑器中生效
- [ ] 可以独立配置前景和背景样式
- [ ] JSON配置能够正确导入和导出

### 6.2 兼容性验收
- [ ] 旧版本配置能够自动迁移到新版本
- [ ] 在VS 2019、VS 2022上都能正常工作
- [ ] 在高DPI显示器上显示正常

### 6.3 性能验收
- [ ] 配置变更后的重新标记时间 < 500ms
- [ ] 打开大型文件（>10000行）时无明显卡顿
- [ ] 内存占用与旧版本相比增加不超过20%

---

## 进度概览

| 阶段 | 状态 | 完成度 |
|------|------|--------|
| 阶段一：解除与VS「字体和颜色」的绑定 | ✅ 已完成 | 100% |
| 阶段二：实现动态CommentToken生成 | 📋 待执行 | 0% |
| 阶段三：实现可拖拽的ListBox | 📋 待执行 | 0% |
| 阶段四：配置变更实时生效 | 📋 待执行 | 0% |
| 阶段五：完整的前景+背景配置UI | 📋 待执行 | 0% |
| **总体进度** | | **20%** |

---

*文档版本: 1.2*
*更新日期: 2026-03-17*
*上次更新: 在ColorPickerDialog中添加HEX值输入功能*
