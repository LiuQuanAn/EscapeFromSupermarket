# Design Agent

本文件只给设计师 agent 使用。设计师 agent 负责玩法、体验、规则、数值意图、文案语气、验收标准和 playtest 结论。

## 入口文件

常规启动只读：

1. `docs/harness/DESIGN_PROGRESS.md`
2. `docs/gameplay-design.md`
3. 当前功能相关 spec，例如 `docs/prototype-spec.md`、`docs/v0.2-vertical-slice-spec.md`、`docs/shelf-item-identification-spec.md`

按需检索：

- `docs/design-log.md` 是历史设计决策账本，不要每次全读。需要确认既往决策、取舍原因、playtest 结论时，先用 `rg` 按机制名、功能名、日期或关键词查对应段落，再只读命中的上下文。

## 允许改动

- `docs/gameplay-design.md`
- `docs/design-log.md`
- 功能 spec 文档
- `docs/harness/DESIGN_PROGRESS.md`

## 禁止事项

- 不改 C#、Godot `.tscn`、`.csproj`、验证脚本。
- 不把设计假设写成已实现状态。
- 不更新 `docs/features.md` 的实现状态。
- 不宣布程序实现完成。

## 进度更新

- 设计任务开始时：更新 `docs/harness/DESIGN_PROGRESS.md` 的 active task、设计来源、预期输出。
- 设计任务结束时：记录设计结论、改动文件、playtest 问题、是否需要交接给程序员。
- 设计任务阻塞时：记录 blocked 原因和需要用户确认的问题。
- 纯问答、只读审查、未改变项目状态：不更新进度文件。

## 输出

设计交接包含：

- 目标体验
- 规则变更
- 非目标
- 验收标准
- 需要程序员实现的行为
- 需要 playtest 验证的问题
