# Better Comments Plus + Highlighter 合并项目交互方式讨论

## 核心问题：两个项目的交互逻辑根本不同

### Better Comments Plus 的交互逻辑

**设计哲学**：「基于 Criteria(当前叫CommentToken) 的被动识别」

```
用户操作流程：
1. 打开 Options 页面
2. 看到一个预定义的 Criteria(当前叫CommentToken) 列表（#TASK, #TODO, etc.）
3. 修改这些 Criteria(当前叫CommentToken) 的样式属性
4. 在代码中写注释时，输入 "//#TASK" 或 "//#TODO"
5. 扩展自动识别并应用样式

关键特点：
- 用户不直接告诉扩展「给我高亮这段文字」
- 用户通过「在注释中写特定 Criteria(当前叫CommentToken)」来触发样式
- Criteria(当前叫CommentToken) 和样式是预先配置好的对应关系
```

**交互入口**：

- 唯一的入口是 Options 页面中的 Criteria(当前叫CommentToken) 列表
- 没有右键菜单
- 没有即时添加功能

***

### Highlighter 的交互逻辑

**设计哲学**：「基于选择的主动创建」

```
用户操作流程：
1. 在编辑器中选中任意文本（如 "#TODO"）
2. 右键 → "Create/Edit Highlight Rule"
3. 弹出对话框，基于选中的文本创建规则
4. 调整颜色、形状等属性
5. 点击 Create，立即生效

关键特点：
- 用户主动选择「我要高亮什么」
- 可以是任意文本，不限于注释
- 即时创建，即时生效
- 支持 Global 和 Solution 两级
```

**交互入口**：

- 右键菜单（主要入口）
- Options 页面（管理已有规则）

***

## 合并后的交互冲突与解决方案

### 冲突 1：触发方式不同 → **解决方案：Criteria 同时应用 Foreground + Background**

| 场景              | Better Comments Plus | Highlighter    |
| --------------- | -------------------- | -------------- |
| 用户在注释中写 "#TASK" | 自动变红                 | 无反应（除非预先配置了规则） |
| 用户选中 "#TASK" 右键 | 无菜单项                 | 弹出创建规则对话框      |

**决策**：Criteria 同时应用 Foreground + Background（如果配置了 Background）

```
用户在注释中写 "//#TASK" 时：
├─ 自动匹配 Criteria "!"
├─ 应用 Foreground（如果配置了）→ 红色文字
└─ 应用 Background（如果配置了）→ 黄色背景

结果：红字 + 黄底（两者独立配置，同时生效）
```

***

### 冲突 2：配置层级不同 → **解决方案：Criteria 支持 Solution 级别，共用数据结构**

**决策理由**：

- 不同代码语言（C#、C语言）的代码高亮不同，可能需要调整
- 支持 Solution 级后，可以共用数据结构

```
统一的数据结构（简化示意）：

StyleRule
├─ Criteria: string        // 匹配文本（如 "#TASK" 或 "#TASK"）
├─ Scope: Comment/AnyText  // 作用域：仅注释 或 任意文本
├─ Level: Global/Solution  // 配置层级
├─ Foreground:             // 前景色样式
│   ├─ IsActive: bool
│   ├─ Color: Color
│   ├─ IsBold: bool
│   └─ ...
└─ Background:             // 背景色样式
    ├─ IsActive: bool
    ├─ Color: Color
    ├─ Shape: TagShape
    └─ ...
```

**Options 页面结构**：

```
├─ [General]
│   └─ 全局开关（Enable/Disable 整个扩展）
│
├─ [Global Rules]           ← 合并后的统一规则
│   ├─ Foreground Rules（原 BCP 的 CommentCriteria）
│   └─ Background Rules（原 Highlighter 的规则）
│
└─ [Solution Rules]         ← 当前解决方案特有规则
    ├─ Foreground Rules
    └─ Background Rules
```

***

### 冲突 3：用户意图不明确 → **解决方案：统一右键菜单为「Create/Edit Styles Rule」**

**决策**：将「Create/Edit Highlight Rule」改为「Create/Edit Styles Rule」，同时支持 Foreground 和 Background 配置

```
右键菜单（选中任意文本时）：
└─ Create/Edit Styles Rule ───→ 打开统一对话框

对话框内容：
┌─────────────────────────────────────┐
│ Create/Edit Styles Rule         [X] │
├─────────────────────────────────────┤
│                                     │
│  Match Text: [TODO                ] │
│                                     │
│  Apply To:                         │
│  (● Comments only)                 │
│  (○ Any text)                      │
│                                     │
│  Save To:                          │
│  (● Global)  (○ Solution)          │
│                                     │
│  ─────────────────────────────────  │
│  ☑ Foreground Style                │
│     Color: [🔴]  Bold ☑  Italic ☐  │
│     Opacity: [██████░░░░]          │
│                                     │
│  ─────────────────────────────────  │
│  ☑ Background Style                │
│     Color: [🟡]  Shape: [Tag ▼]    │
│     Blur: [None ▼]  Alpha: [20% ▼] │
│                                     │
│  Preview:                           │
│  ┌─────────────────────────────┐   │
│  │ // TODO: Fix this bug       │   │
│  │    [红字+黄底效果预览]       │   │
│  └─────────────────────────────┘   │
│                                     │
│         [Cancel]      [Create]      │
└─────────────────────────────────────┘
```

**用户意图满足方式**：

| 用户意图          | 操作方式                            |
| ------------- | ------------------------------- |
| 只改 Foreground | 勾选 ☑ Foreground，取消 ☐ Background |
| 只改 Background | 取消 ☐ Foreground，勾选 ☑ Background |
| 两者都改          | 同时勾选两者                          |

***

## 新的交互方案：统一规则系统

基于以上决策，提出新的交互方案：

### Options 页面结构

```
┌─────────────────────────────────────────────────────────────┐
│  Better Comments Plus + Highlighter                      [X] │
├─────────────────────────────────────────────────────────────┤
│  [ General ] [ Global Rules ] [ Solution Rules ]             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Global Rules（应用于所有项目）                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Criteria │ Scope      │ Foreground │ Background │ Actions │
│  ├─────────────────────────────────────────────────────┤   │
│  │ #TASK     │ Comment    │ [🔴 Bold]  │ [⚪]       │ [Edit][Delete] │
│  │ #HACK     │ Comment    │ [🔵]       │ [⚪]       │ [Edit][Delete] │
│  │ #TODO     │ Comment    │ [🟢]       │ [🟡 Tag]   │ [Edit][Delete] │
│  │ #FIXME    │ AnyText    │ [⚪]       │ [🔴 Line]  │ [Edit][Delete] │
│  │ ...       │ ...        │ ...        │ ...        │ ...            │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  [Add Rule] [Import] [Export]                              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 规则编辑对话框（统一）

```
┌─────────────────────────────────────────────────────────────┐
│ Edit Rule                                               [X] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Basic Info:                                                │
│  ─────────────────────────────────────────────────────────  │
│  Match Criteria: [#TASK                                   ] │
│                                                             │
│  Scope:  (● Comment only)  (○ Any text)                    │
│  Level:  (● Global)  (○ Solution)                          │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│  ☑ Foreground Style                                         │
│                                                             │
│     Color:    [🔴 #FF0000    ]  [Color Picker]              │
│     Style:    ☑ Bold    ☐ Italic    ☐ Underline            │
│     Opacity:  [██████░░░░] 60%                              │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│  ☑ Background Style                                         │
│                                                             │
│     Color:    [🟡 #FFD700    ]  [Color Picker]              │
│     Shape:    [Tag Under ▼]                                 │
│     Blur:     [None ▼]                                      │
│     Alpha:    [20% ▼]                                       │
│                                                             │
│  ─────────────────────────────────────────────────────────  │
│  Preview:                                                   │
│  ┌─────────────────────────────────────────────┐           │
│  │ // ! This is an important comment           │           │
│  │    [红字+黄底下划线效果预览]                 │           │
│  └─────────────────────────────────────────────┘           │
│                                                             │
│              [Cancel]              [Save]                   │
└─────────────────────────────────────────────────────────────┘
```

### 右键菜单流程

```
用户在编辑器中选中 "TODO"
         ↓
    右键点击
         ↓
┌─────────────────────────────┐
│ Cut                         │
│ Copy                        │
│ Paste                       │
│ ...                         │                        
│ 右键菜单（选中任意文本时）:   │
├─ Create/Edit Foreground Styles Rule  ← Foreground 默认启用  ←──│── 入口
└─ Create/Edit Background Styles Rule  ← Background 默认启用  ←──┼── 入口
└─────────────────────────────┘
         ↓
┌─────────────────────────────────────┐
│ Create/Edit Styles Rule             │
├─────────────────────────────────────┤
│                                     │
│  Match Text: [#TODO             ]   │ ← 自动填充选中文本
│                                     │
│  Apply To:                          │
│  (● Comments only)                  │ ← 如果当前在注释中，默认选中
│  (○ Any text)                       │
│                                     │
│  Save To:                           │
│  (○ Global)                         │
│  (● Solution)                       │ ← 默认 Solution 级别
│                                     │
│  [☑] Foreground: [🔴] Bold ☑       │
│  [☑] Background: [🟡] Shape: [Tag] │
│                                     │
│         [Cancel]      [Create]      │
└─────────────────────────────────────┘
         ↓
    点击 Create
         ↓
    规则立即生效
```

***

## 关键交互流程对比

### 场景 1：用户想在注释中标记重要内容

**传统 BCP 用户**：

```
1. 写注释 "//#IMPORTANT"
2. 自动显示红色（如果 Criteria "#IMPORTANT" 已配置）
```

**新方案**：

```
1. 写注释 "//#IMPORTANT"
2. 自动显示红色 + 黄色背景（如果 Criteria "#IMPORTANT" 配置了两者）
3. 如果 Criteria "#IMPORTANT" 未配置，可以：
   - 选中 "#IMPORTANT" → 右键 → Create/Edit Foreground Styles Rule或Create/Edit Background Styles Rule → 配置并创建
```

### 场景 2：用户想高亮代码中的关键字

**传统 Highlighter 用户**：

```
1. 选中 "#TODO"
2. 右键 → Create/Edit Foreground Styles Rule或Create/Edit
3. 配置 Background 样式
4. 创建规则
```

**新方案**：

```
1. 选中 "#TODO"
2. 右键 → Create/Edit Foreground Styles Rule或Create/Edit Background Styles Rule → 配置并创建
3. 配置 "Any text" 作用域
4. 配置 Foreground 和/或 Background
5. 创建规则
```

***

## 待确认的细节

### 1. 默认行为

**两个右键入口，共用一个面板**：

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

**共用面板**：

- 两个入口打开的是同一个编辑对话框
- 只是默认勾选项不同
- 用户可以在面板中随时切换启用状态

### 2. Criteria 预定义列表

**决策**：

- ✅ **保留**预定义 Criteria 列表（#IMPORTANT、#REMOVE 等）
- ✅ **可以删除**用户不需要的预定义 Criteria

**预定义列表（Global Rules 默认值）**：

| Criteria   | Foreground | Background |
| ---------- | ---------- | ---------- |
| #IMPORTANT | 红色 + 粗体    | 禁用         |
| #REMOVE    | 灰色         | 禁用         |
| #QUESTION  | 白色         | 禁用         |
| #TASK      | 绿色         | 禁用         |

**用户操作**：

- 可以修改任何预定义 Criteria 的样式
- 可以删除不需要的预定义 Criteria
- 可以添加自定义 Criteria

### 3. 规则优先级

当多个规则匹配同一文本时：

- **优先级顺序**：
  1. Solution Rules（当前解决方案）> Global Rules
  2. Comment Rules（如果在注释中）> AnyText Rules

***

## 总结

### 核心决策回顾

1. **Criteria 同时应用 Foreground + Background** ✓
2. **Criteria 支持 Solution 级别，共用数据结构** ✓
3. **右键菜单统一添加两项入口，分别对应默认前景/背景 **✓

### 新方案的优势

1. **统一概念**：不再区分 "Criteria" 和 "Highlight Rule"，统一为 "Criteria Rule"
2. **灵活配置**：每个规则可以独立配置 Foreground 和 Background
3. **统一入口**：右键菜单和 Options 页面使用相同的编辑对话框
4. **向后兼容**：预定义 Criteria 列表保留，老用户习惯不受影响


*文档版本: 2.0（基于用户反馈更新）*
*更新日期: 2025-03-14*
