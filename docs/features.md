# Feature List — escape-from-supermarket

机器可读 feature 状态机（讲座 7/8）。这是 scope 的单一真相源：调度、验证、汇报都以此表为准。

## 规则

- 状态：`not_started` → `active` → `passing`（或 `blocked`）。
- **一次只允许一个 `active`**（WIP=1，见 AGENTS.md）。
- **状态由 harness 推进，不由 agent 自评**：`passing` 必须有可执行验证通过的证据。自动化项的门禁是 `tools/harness/verify.ps1`（编译 + 主场景 headless 冒烟）；行为类需人工 playtest 通过才置 `passing`。
- 证据列填 commit SHA，或 `docs/implementation-log.md` 的对应日期条目。
- 落地新功能后：在此更新状态，并在 implementation-log 追加条目。

## 验证命令约定

- `verify` = 仓库根目录运行 `powershell -ExecutionPolicy Bypass -File tools/harness/verify.ps1`（dotnet build + 加载 `Scenes/Main.tscn` headless 冒烟）。
- `manual: <标准>` = 需人工 playtest 确认的行为，verify 只能保证不崩，不能保证手感/数值。

## Features

| ID | 行为 | 验证 | 状态 | 证据 |
|---|---|---|---|---|
| F01 | 架构脚手架：`Supermarket` tick 循环 + `ITickable` 注册/注销 + `GameRoot` | verify | passing | `51831d1` |
| F02 | 玩家移动：Tunic 斜 45° 正交相机 + WASD 沿相机基底投影 + 载重速度倍率(1.00/0.85/0.65) | verify; manual: 移动手感 | passing | `51831d1` |
| F03 | 地图碰撞：外墙 + 货架块 StaticBody3D | verify | passing | `d057435` |
| F04 | 货架拾取闭环：ProductCatalog + ShelfModel + CanPickProductQuery + PickProductCommand + ShelfController + ShelfPanel | verify; manual: E 交互拾取 | passing | `d057435` |
| F05 | HUD：格数/重量/价值标签 + 倒计时 + 撤离/警觉进度条 | verify; manual: 数值实时刷新 | passing | `d057435` |
| F06 | 回合计时 + 结束态：TimerSystem 240s 倒计时 + GameStateModel + EndRoundCommand(Win/Lose) | verify; manual: 倒计时归零判负 | passing | `d057435` |
| F07 | 撤离区 + 结果面板 + 重开：ExtractionSystem + Start/CancelExtractionCommand + ResultPanel(Reset→ReloadScene) | verify; manual: 撤离成功结算 | passing | `d057435` |
| F08 | 保安视觉/警觉/追逐/抓捕：视锥 raycast + Alert 满后 Chasing，catch 仅在 Chasing 触发 | verify; manual: 被发现→追逐→抓捕，巡逻擦肩不判负 | passing | `d057435` + `6973569` |
| F09 | 购物车面板 + 丢弃：DropProductCommand + CartPanelController(Tab 切换) | verify; manual: Tab 开关 + 丢弃 | passing | `d057435` |
| F10 | V0.2 元进度 + 升级：MetaProgressModel(金钱/升级/导航) + 容量/移速升级 + BuyUpgradeCommand + StartNextRoundCommand | verify; manual: 升级生效跨回合保留 | passing | impl-log 2026-05-24 |
| F11 | V0.2 门禁卡 + 双撤离门：KeycardController + 员工门(需卡, 更短撤离时间) | verify; manual: 取卡后员工门可用 | passing | impl-log 2026-05-24 |
| F12 | V0.2 路由器任务 + 导航进度：RoundObjectiveModel + 每回合保底刷新 + 仅路由器推进导航 | verify; manual: 取路由器推进进度 | passing | impl-log 2026-05-24 |
| F13 | V0.2 顾客 NPC：CustomerController + 3 个阻挡顾客 | verify; manual: 顾客阻挡手感 | passing | impl-log 2026-05-24 |
| F14 | 货架物品鉴定：货架 loot 初始隐藏，运行中逐件鉴定，关闭/离开/回合结束重置进度 | verify; manual: 鉴定顺序/重置/同回合记忆/未知拒拾 | passing | impl-log 2026-05-24 |
| F15 | 每货架编辑器刷新权重：ShelfSpawnEntryResource + ShelfController 实例 SpawnOptions + 加权随机 | verify; manual: 编辑器各货架 count/id/weight 生效 | passing | impl-log 2026-05-26 |
| F16 | 交互改用 Area3D 重叠：IsPlayerInInteractionArea + 最近重叠目标选择 | verify; manual: 进出区交互提示 | passing | impl-log 2026-05-28 |
| F17 | 购物车载重档位按 CartWeightLimit 百分比(Mid 50% / Heavy 80%) | verify | passing | impl-log 2026-05-28 |
| F18 | V0.2 5-10 轮 playtest：路线选择/员工门/路由器/升级顺序/顾客阻挡/UI 可读/3-5 分钟回合长 | manual: 完整 playtest 报告 | not_started | impl-log §Pending |
| F19 | V0.2 调参 pass：移动/载重倍率、保安视野/警觉/追逐、撤离时长、商品数值、顾客速度/推挤、升级价格 | manual: PrototypeBalance 调参定稿 | not_started | impl-log §Pending |
| F20 | 货架鉴定 playtest：揭示顺序、关闭重开重置、同回合已鉴定记忆、未知拾取拒绝 | manual: 鉴定 playtest | not_started | impl-log §Pending |
| F21 | 货架刷新编辑器核验：各实例 count 范围/产品 id/权重，重复回合匹配配置池 | manual: 编辑器核验 | not_started | impl-log §Pending |
| F22 | Harness 基础设施：人工维护 AGENTS、agent 分流、实现/设计进度隔离、verify 门禁、架构入口、环境锁定 | verify | passing | impl-log 2026-05-29; `verify.ps1` passed |

## 与 implementation-log 的关系

- implementation-log = 时间线施工收据（叙事 + commit）。
- 本表 = 当前 scope 状态机（机器可读、WIP 调度用）。
- 两者必须一致：落地 commit 时同步更新本表状态与 impl-log 条目。
