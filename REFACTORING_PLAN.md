# 术语统一与代码重构计划

**文档版本：** 1.0  
**创建日期：** 2026-03-19  
**目标：** 消除术语混用，统一命名规范，提升代码可维护性

---

## 一、术语混用现状

### 🔴 核心问题

1. **CommentToken (227 次) vs StyleRule (112 次)** - 表示相似概念但分散在两个类中
2. **CurrentValue (40 次) vs Criteria (172 次)** - 混用表示匹配文本
3. **RuleId (29 次) vs Id** - 混用表示规则标识
4. **CommentType** - 同时用于预定义和自定义规则，职责不清

### 📊 影响范围

- **核心文件：** 14 个文件需要重构
- **总代码行数：** 约 2000+ 行需要修改
- **高风险区域：** Settings.cs, CommentViewDecorator.cs, CreateEditStyleRule.cs

---

## 二、统一命名方案

### ✅ 采用的术语体系

| 旧术语 | 新术语 | 说明 | 优先级 |
|--------|--------|------|--------|
| `CommentToken` | `CommentRule` | 统一规则类 | 🔴 P0 |
| `StyleRule` | `CommentRule` | 合并到 CommentRule | 🔴 P0 |
| `CurrentValue` | `Criteria` | 统一使用 Criteria | 🔴 P0 |
| `Criteria` | `Criteria` | **保留** ✅ | - |
| `RuleId` / `Id` | `Id` | 统一标识符 | 🔴 P0 |
| `CommentType` | `CommentCategory` | 评论分类（枚举） | 🟡 P1 |
| `GlobalCommentTokens` | `GlobalRules` | 全局规则集合 | 🟡 P1 |
| `SolutionCommentTokens` | `SolutionRules` | 解决方案规则集合 | 🟡 P1 |

### 📋 新的类结构

```csharp
/// <summary>
/// 评论规则 - 统一的规则定义
/// </summary>
public class CommentRule : PropertyChangeNotifier
{
    // === 标识 ===
    public string Id { get; set; }                    // 唯一标识符
    public CommentCategory? Category { get; set; }    // 分类（可空，仅预定义规则使用）
    public bool IsPredefined { get; set; }            // 是否预定义
    public bool IsActive { get; set; }                // 是否激活
    public int Order { get; set; }                    // 排序顺序
    
    // === 匹配 ===
    public string Criteria { get; set; }              // 匹配条件（统一术语）
    
    // === 前景样式 ===
    public ForegroundStyle Foreground { get; set; }
    
    // === 背景样式 ===
    public BackgroundStyle Background { get; set; }
    
    // === 作用域 ===
    public RuleScope Scope { get; set; }              // Global 或 Solution
}

/// <summary>
/// 评论分类 - 预定义的 5 种类型
/// </summary>
public enum CommentCategory
{
    Normal,     // 普通评论
    Important,  // 重要评论 (#IMPORTANT)
    Question,   // 问题评论 (#QUESTION)
    Remove,     // 删除评论 (#REMOVE)
    Task        // 任务评论 (#TASK)
}

/// <summary>
/// 规则作用域
/// </summary>
public enum RuleScope
{
    Global,     // 全局规则
    Solution    // 解决方案规则
}
```

---

## 三、重构阶段划分

### 📍 阶段 1：核心类重构（P0 - 最高优先级）

**目标：** 合并 CommentToken 和 StyleRule，建立统一的 CommentRule 类

**任务清单：**
- [ ] 1.1 创建新的 `CommentRule.cs` 类
- [ ] 1.2 迁移 `CommentToken` 的所有属性
- [ ] 1.3 迁移 `StyleRule` 的所有属性
- [ ] 1.4 添加 `Scope` 属性（Global/Solution）
- [ ] 1.5 重命名 `CurrentValue` → `Criteria`
- [ ] 1.6 统一 `Id` 命名（移除 `RuleId`）
- [ ] 1.7 重命名 `CommentType` → `CommentCategory`

**影响文件：**
```
Options/Infrastructure/CommentToken.cs      → 删除（合并到 CommentRule）
Options/Infrastructure/StyleRule.cs         → 删除（合并到 CommentRule）
Options/Infrastructure/CommentRule.cs       → 新建
Options/Infrastructure/CommentCategory.cs   → 新建（从 CommentTagger.cs 迁移）
Options/Infrastructure/RuleScope.cs         → 新建
```

**预计工作量：** 2-3 小时  
**风险等级：** 🔴 高（核心数据结构变更）

---

### 📍 阶段 2：Settings 类重构（P0 - 最高优先级）

**目标：** 更新 Settings 类使用新的 CommentRule 和统一的集合命名

**任务清单：**
- [ ] 2.1 重命名 `GlobalCommentTokens` → `GlobalRules`
- [ ] 2.2 重命名 `SolutionCommentTokens` → `SolutionRules`
- [ ] 2.3 更新 `CommentTokens` 兼容属性 → `AllRules`
- [ ] 2.4 更新所有方法引用
- [ ] 2.5 更新 `ConvertCommentTokenToStyleRule` → `ConvertRuleToUnifiedConfig`
- [ ] 2.6 更新 `ConvertStyleRuleToCommentToken` → `ConvertUnifiedConfigToRule`
- [ ] 2.7 删除过时的转换方法

**影响文件：**
```
Options/Settings.cs  → 主要修改
```

**预计工作量：** 1-2 小时  
**风险等级：** 🟡 中

---

### 📍 阶段 3：UI 层重构（P1 - 高优先级）

**目标：** 更新所有 UI 控件和事件处理

**任务清单：**
- [ ] 3.1 更新 `OptionsTokensPageControl.xaml` 绑定
- [ ] 3.2 更新 `OptionsTokensPageControl.xaml.cs` 事件处理
- [ ] 3.3 更新 `EditStyleRuleDialog.xaml` 绑定
- [ ] 3.4 更新 `EditStyleRuleDialog.xaml.cs` 逻辑
- [ ] 3.5 更新 `CreateEditStyleRule.cs` 命令处理
- [ ] 3.6 更新导入导出数据结构

**影响文件：**
```
Options/OptionsTokensPageControl.xaml
Options/OptionsTokensPageControl.xaml.cs
Options/EditStyleRuleDialog.xaml
Options/EditStyleRuleDialog.xaml.cs
Commands/CreateEditStyleRule.cs
```

**预计工作量：** 3-4 小时  
**风险等级：** 🟡 中

---

### 📍 阶段 4：运行时层重构（P1 - 高优先级）

**目标：** 更新 CommentTagger 和 CommentViewDecorator

**任务清单：**
- [ ] 4.1 更新 `Comment.cs` 使用新的 `CommentRule`
- [ ] 4.2 更新 `CommentTagger.cs` 使用 `CommentCategory`
- [ ] 4.3 更新 `CommentParser.cs` 中的术语
- [ ] 4.4 更新 `CommentViewDecorator.cs` 样式应用逻辑
- [ ] 4.5 更新 `CSharpCommentParser.cs` 中的 `indexOfToken` → `indexOfCriteria`

**影响文件：**
```
CommentsTagging/Infrastructure/Comment.cs
CommentsTagging/CommentTagger.cs
CommentsTagging/Parsers/CommentParser.cs
CommentsTagging/Parsers/CSharpCommentParser.cs
CommentsViewCustomization/CommentViewDecorator.cs
```

**预计工作量：** 2-3 小时  
**风险等级：** 🟠 中高

---

### 📍 阶段 5：验证规则重构（P2 - 中优先级）

**目标：** 更新验证逻辑使用新术语

**任务清单：**
- [ ] 5.1 更新 `RequiredAndUniqueRule.cs`
- [ ] 5.2 更新 `OptionsPageBase.cs`
- [ ] 5.3 更新所有验证规则引用

**影响文件：**
```
Options/Infrastructure/RequiredAndUniqueRule.cs
Options/OptionsPageBase.cs
```

**预计工作量：** 1 小时  
**风险等级：** 🟢 低

---

### 📍 阶段 6：Highlighter 模块整合（P2 - 中优先级）

**目标：** 统一 Highlighter 模块的术语

**任务清单：**
- [ ] 6.1 评估 HighlightTag 是否需要重命名
- [ ] 6.2 确保与主模块术语一致（Criteria 已统一）

**影响文件：**
```
Highlighter/Core/HighlightTag.cs
Highlighter/Core/HighlightTagData.cs
```

**预计工作量：** 1-2 小时  
**风险等级：** 🟢 低

---

### 📍 阶段 7：清理与优化（P3 - 低优先级）

**目标：** 删除废弃代码，优化注释和文档

**任务清单：**
- [ ] 7.1 删除旧的 CommentToken.cs
- [ ] 7.2 删除旧的 StyleRule.cs
- [ ] 7.3 更新所有 XML 注释
- [ ] 7.4 更新项目文档
- [ ] 7.5 运行代码分析工具
- [ ] 7.6 执行完整的回归测试

**预计工作量：** 2 小时  
**风险等级：** 🟢 低

---

## 四、执行策略

### 🎯 分阶段执行原则

1. **每个阶段独立编译** - 确保每个阶段完成后代码可以编译运行
2. **向后兼容** - 在过渡期间保留必要的兼容代码
3. **逐步替换** - 使用查找替换 + 手动审查的方式
4. **充分测试** - 每个阶段完成后执行完整测试

### 📋 推荐执行顺序

```
阶段 1 (核心类) 
  ↓
阶段 2 (Settings) 
  ↓
阶段 3 (UI 层) 
  ↓
阶段 4 (运行时) 
  ↓
阶段 5 (验证) 
  ↓
阶段 6 (Highlighter) 
  ↓
阶段 7 (清理)
```

### ⚠️ 风险评估

| 风险项 | 可能性 | 影响 | 缓解措施 |
|--------|--------|------|----------|
| 编译失败 | 中 | 高 | 每阶段独立编译，及时回滚 |
| 运行时错误 | 中 | 高 | 充分测试，保留备份 |
| 功能回归 | 低 | 中 | 完整的测试用例 |
| 数据丢失 | 低 | 极高 | 备份配置文件，提供迁移工具 |

---

## 五、迁移工具

### 📦 配置文件迁移

为了支持旧配置文件的迁移，需要创建迁移工具：

```csharp
public class ConfigMigrator
{
    public static CommentRule MigrateFromCommentToken(CommentToken token)
    {
        return new CommentRule
        {
            Id = token.RuleId,
            Criteria = token.CurrentValue,  // CurrentValue → Criteria
            Category = token.Type,
            IsPredefined = !token.IsDynamic,
            Foreground = token.ForegroundStyle,
            Background = token.BackgroundStyle,
            Scope = RuleScope.Global // 需要根据上下文判断
        };
    }
    
    public static CommentRule MigrateFromStyleRule(StyleRule rule)
    {
        return new CommentRule
        {
            Id = rule.Id,
            Criteria = rule.Criteria,  // 保持不变
            IsPredefined = rule.IsPredefined,
            Foreground = rule.Foreground,
            Background = rule.Background,
            Scope = RuleScope.Global // 需要根据上下文判断
        };
    }
}
```

---

## 六、验收标准

### ✅ 代码质量

- [ ] 所有旧术语（Token, CurrentValue, RuleId）已替换
- [ ] **Criteria** 术语已统一并保留 ✅
- [ ] 代码编译无警告
- [ ] 通过代码分析工具检查
- [ ] 注释和文档已更新

### ✅ 功能完整性

- [ ] 所有现有功能正常工作
- [ ] 旧配置文件可以成功迁移
- [ ] UI 界面显示正确
- [ ] 右键菜单功能正常

### ✅ 测试覆盖

- [ ] 单元测试通过
- [ ] 集成测试通过
- [ ] 手动测试通过
- [ ] 性能无明显下降

---

## 七、时间估算

| 阶段 | 预计工时 | 累计工时 |
|------|---------|---------|
| 阶段 1：核心类 | 3 小时 | 3 小时 |
| 阶段 2：Settings | 2 小时 | 5 小时 |
| 阶段 3：UI 层 | 4 小时 | 9 小时 |
| 阶段 4：运行时 | 3 小时 | 12 小时 |
| 阶段 5：验证 | 1 小时 | 13 小时 |
| 阶段 6：Highlighter | 2 小时 | 15 小时 |
| 阶段 7：清理 | 2 小时 | 17 小时 |
| **总计** | **17 小时** | **17 小时** |

---

## 八、下一步行动

### 🚀 立即可执行

1. **确认重构方案** - 用户审查并批准本计划
2. **创建代码备份** - 使用 Git 创建新分支
3. **开始阶段 1** - 创建新的 CommentRule 类

### 📝 决策点

- [ ] 是否采用本命名方案？
- [ ] 是否分阶段执行？
- [ ] 是否需要配置文件迁移工具？
- [ ] Highlighter 模块是否同步重构？

---

**备注：** 本文档为动态文档，应在重构过程中持续更新。
