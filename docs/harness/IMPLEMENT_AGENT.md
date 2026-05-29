# Implement Agent

本文件只给程序员 agent 使用。程序员 agent 负责 C#、Godot 场景、控制器、模型、系统、bug 修复、构建和验证脚本。

## 入口文件

常规启动只读：

1. `docs/harness/IMPLEMENTATION_PROGRESS.md`
2. `docs/features.md`
3. `escape-from-supermarket/Scripts/ARCHITECTURE.md`
4. 当前功能相关 spec 或设计交接

按需检索：

- `docs/implementation-log.md` 是历史施工收据，不要每次全读。需要确认既往实现、计划修订、commit 证据时，先用 `rg` 按 feature、文件名、日期或关键词查对应段落，再只读命中的上下文。

## 允许改动

- `escape-from-supermarket/`
- `tools/harness/`
- `docs/harness/`
- `docs/harness-gap-plan.md`
- `docs/features.md`
- `docs/implementation-log.md`
- `docs/harness/IMPLEMENTATION_PROGRESS.md`
- 必要时更新实现相关架构文档

## 禁止事项

- 不改玩法设计方向，除非用户明确要求程序员修订 spec。
- 不把未验证行为标成完成。
- 不跳过 `tools/harness/verify.ps1`，除非本次任务纯文档且汇报中说明原因。
- 不把设计待定问题写进实现状态表。

## 标准验证命令

- 首选门禁：仓库根目录运行 `powershell -ExecutionPolicy Bypass -File tools/harness/verify.ps1`（dotnet build + 加载 `Scenes/Main.tscn` 的 headless 冒烟；出错非零退出，日志写入 `tools/harness/logs/`）。把结果写入本次任务汇报。
- 单独编译：在 `escape-from-supermarket` 目录运行 `dotnet build .\EscapeFromSupermarket.sln`。
- Godot 不在默认路径时，用 `$env:GODOT_BIN` 或 `-GodotBin` 指定 console 版可执行文件。

## Definition of Done

- 代码、场景或验证脚本改动已通过对应验证；如果只完成编译但未完成运行时验证，必须明确说明。
- 更新 `docs/harness/IMPLEMENTATION_PROGRESS.md` 的当前快照。
- 只有 feature 状态变化时更新 `docs/features.md`。
- 只有完成批次、计划变化或提交收据需要长期记录时更新 `docs/implementation-log.md`。
- 汇报时列出具体改动、验证结果、未验证风险和下一步。
- 结束前检查 `git status --short`，区分本次改动和用户/既有改动。

## Session 收尾清单

- 更新 `docs/harness/IMPLEMENTATION_PROGRESS.md`。
- 运行或说明为何跳过 `tools/harness/verify.ps1`。
- 只有 feature 状态变化时同步 `docs/features.md`。
- 只有完成批次、计划变化或提交收据需要长期记录时同步 `docs/implementation-log.md`。
- 汇报中列出 `git status --short` 摘要。

## 进度更新

- 实现任务开始时：更新 `docs/harness/IMPLEMENTATION_PROGRESS.md` 的 active task、预期改动文件、验证计划。
- 实现任务结束时：记录结果、验证证据、失败分类、dirty worktree 摘要、下一步。
- 实现任务阻塞时：记录 blocked 原因，不只留在聊天里。
- 纯问答、只读审查、未改变项目状态：不更新进度文件。

## 输出

实现交接包含：

- 具体改动
- 影响范围
- 验证命令和结果
- 未验证风险
- 是否需要设计师复审
- 下一步建议
