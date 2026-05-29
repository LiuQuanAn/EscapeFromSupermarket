# Harness 工程补足计划

日期：2026-05-29

依据《Learn Harness Engineering》12 讲（https://walkinglabs.github.io/learn-harness-engineering/en/），把课程要求的 harness 原语逐项对照本仓库现状，列出缺口与补足计划。本文件保留原始计划，同时在「现状对照」和「缺口清单」中记录当前执行状态；每批完成后在 `docs/implementation-log.md` 记录。

## 课程要求的 harness 子系统

12 讲归纳出 5 个子系统 + 若干原语：

1. 指令子系统 — 入口路由文件 + 按需加载的主题文档（讲 2/3/4）
2. 工具子系统 — 足够的 shell/命令访问（讲 2）
3. 环境子系统 — 锁定依赖、固定运行时版本、可复现（讲 2/6）
4. 状态子系统 — 跨 session 的进度/决策延续（讲 5/6/12）
5. 反馈子系统 — 显式可执行的验证命令与门禁（讲 1/2/9/10/12，最高 ROI）

附加原语：机器可读 feature list + 状态机（讲 7/8）、Definition of Done + 三层验证（讲 9/10）、session 收尾干净状态（讲 12）、可观测性（讲 11）。

## 现状对照

已具备：

- `AGENTS.md` — 顶层入口路由 + 通用工作硬约束；实现侧 run/verify 命令和 Definition of Done 位于 `docs/harness/IMPLEMENT_AGENT.md`。
- `docs/` 主题文档 — `prototype-spec.md` / `v0.2-vertical-slice-spec.md` / `shelf-item-identification-spec.md` / `gameplay-design.md`。按需加载，符合讲 3/4 的「就近、模块化」。
- `docs/implementation-log.md` — 带 commit SHA 的完成项 / 待办 / 计划修订，顶替状态子系统的「what done」延续。
- `docs/design-log.md` — 设计决策档案，部分顶替 DECISIONS.md。
- `docs/features.md` — 机器可读 feature list，记录 ID / 行为 / 验证方式 / 状态 / 证据，支持 WIP=1 和 pass-state gating。
- `docs/harness/DESIGN_AGENT.md` 与 `docs/harness/IMPLEMENT_AGENT.md` — 分别给设计师 agent 和程序员 agent 的角色入口与收尾规则。
- `docs/harness/DESIGN_PROGRESS.md` 与 `docs/harness/IMPLEMENTATION_PROGRESS.md` — 分离设计进度和实现进度，避免记忆交叉。
- `tools/harness/verify.ps1` — 标准门禁：`dotnet build` + Godot headless 加载 `Scenes/Main.tscn`，日志写入 `tools/harness/logs/`。
- `escape-from-supermarket/Scripts/ARCHITECTURE.md` — 与代码就近的 QFramework 分层和本地 fork 偏差说明。
- `global.json` — 固定 .NET SDK `10.0.202`；`AGENTS.md` 写明 Godot `4.6.3.stable.mono` 和 console 默认路径。
- `.gitignore` — 正确忽略 `.godot/` `bin/` `obj/`、`.codex_review_input.txt` 和 `tools/harness/logs/`，符合讲 12 干净状态。

## 缺口清单

| # | 对应讲座 | 当前状态 | 剩余影响 | 优先级 |
|---|---|---|---|---|
| A | 1/2/9/10/12 | 已落地：`tools/harness/verify.ps1` 可执行门禁已存在，`docs/harness/IMPLEMENT_AGENT.md` 已指向它；最近一次通过记录在 `docs/harness/IMPLEMENTATION_PROGRESS.md` 和 `docs/implementation-log.md`。 | 沙箱内可能因 NuGet/Godot.NET.Sdk 访问失败，需要按工具权限机制请求批准后用同一命令重跑。 | 已完成 |
| B | 7/8 | 已落地：`docs/features.md` 记录 feature ID、行为、验证方式、状态和证据。 | 行为类 feature 仍依赖人工 playtest 证据，不能只靠 headless 冒烟。 | 已完成 |
| C | 6/9/10 | 部分落地：`verify.ps1` 已有编译 + 主场景 headless 冒烟。 | 仍缺更细的 unit/integration 测试；`Architecture<Supermarket>.Reset()` 隔离验证尚未独立成自动测试。 | Tier 2 |
| D | 3/10 | 已落地：`escape-from-supermarket/Scripts/ARCHITECTURE.md` 记录 QFramework 分层、本地 fork 偏差和实现入口。 | 随代码结构变化需要人工维护。 | 已完成 |
| E | 2/6 | 已落地：`global.json` 固定 .NET SDK；AGENTS.md 写明 Godot 版本和默认 console 路径。 | 其他机器如果缺对应 SDK/Godot，仍需安装环境。 | 已完成 |
| F | 5/12 | 已落地：设计/实现进度拆到两个文件；实现状态记录最新验证和 dirty worktree 预期；实现侧 session 收尾清单位于 `docs/harness/IMPLEMENT_AGENT.md`。 | 若后续任务不维护进度文件，会再次失真。 | 已完成 |
| G | 11 | 继续跳过：sprint contract / 评分 rubric / OpenTelemetry trace。 | 单人原型阶段收益低。 | 跳过 |

## 原始补足计划（历史）

以下为 2026-05-29 的原始落地计划。当前状态以「缺口清单」为准；其中 A/B/D/E/F 已完成，C 仍是剩余 Tier 2 缺口，G 跳过。

### Tier 1 — 核心门禁（最高 ROI）

**A. `tools/harness/verify.ps1`**
- 步骤 1：在 `escape-from-supermarket` 运行 `dotnet build .\EscapeFromSupermarket.sln`，编译失败立即非零退出。
- 步骤 2：headless 启动 Godot 加载 `Scenes/Main.tscn`（短时运行后退出），捕获 stderr，出现 script/parse 错误判为失败。
- 步骤 3：聚合结果，打印通过/失败摘要，设置退出码。
- 收尾：把 AGENTS.md「未来如果存在 `tools/harness/verify.ps1`」改成既定事实，标准验证命令指向它。
- 验证标准：干净 checkout 上运行脚本通过；故意引入编译错误时脚本非零退出。

**B. `docs/features.md`**
- 把 implementation-log 的 Completed/Pending 转成结构化表：`ID | 行为描述 | 验证命令 | 状态(not_started/active/blocked/passing) | 证据(commit/log)`。
- 已落地功能标 `passing` 并填 commit SHA；Pending 的 5-10 轮 playtest / 调参标 `not_started` 或 `active`。
- AGENTS.md 接上「同步 feature 状态」的具体文件路径。
- 验证标准：每条 passing 项的验证命令可实际跑；新 session 只读此表即可判断 WIP 与下一步。

### Tier 2 — 验证与延续

**C. headless 冒烟测试**
- 加 1 个最小测试：headless 加载主场景断言无 parse/script 错误，并验证 `Architecture<Supermarket>.Reset()` 跑通（场景重载隔离）。
- 接进 `verify.ps1` 作为讲 6 要求的「示例通过测试」。
- 验证标准：测试在 verify.ps1 中被调用并通过；破坏 Reset 时测试失败。

**D. `escape-from-supermarket/Scripts/ARCHITECTURE.md`**
- 从 `QFramework.cs` banner + impl-log 提炼：4 层规则（Model/System/Command/Query/Controller/Event 职责与依赖方向）、已接受偏差（System 可 SendCommand 等）、`Scripts/` 目录图。
- 与代码就近放置（讲 3）。
- 验证标准：能回答「某逻辑该放哪一层」「为何 System 能发 Command」而无需读整份 fork。

### Tier 3 — 可选

**E. 环境锁定** — 加 `global.json` 固定 .NET SDK 主版本；AGENTS.md「项目入口」写明所需 Godot 版本（与 `project.godot` config_version 对应）。

**F. session 收尾** — AGENTS.md 增「session 收尾清单」：build 通过 / 测试通过 / feature 状态已更新 / 无调试残留 / 标准启动路径可用；可选增机器可读 STATUS 块（最新 commit + build/test 状态）。

### 跳过

**G. 可观测性重型件**（sprint contract / rubric / trace）— 单人原型阶段 ROI 过低，记录于此备查，暂不实现。

## 历史落地顺序

1. Tier 1 A → B（已完成）
2. Tier 2 C → D（D 已完成，C 保留为剩余缺口）
3. Tier 3 E / F（已完成）
4. 每批完成在 `docs/implementation-log.md` 追加条目，并把对应 feature 状态翻成 `passing`
