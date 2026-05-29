# Scripts Architecture

本文档是 `Scripts/` 目录的就近架构入口。更长的历史记录在仓库根目录 `docs/implementation-log.md`；QFramework 本地 fork 的底层行为以 `addons/qframework/QFramework.cs` 文件头 `LOCAL FORK NOTICE` 为准。

## 分层职责

- `Architecture/`：项目 composition root。`Supermarket` 注册 utility、model、system，并集中驱动 `ITickable`。
- `Config/`：Inspector 或代码共享的配置数据。只放调参和编辑器暴露的数据形状，不放玩法流程。
- `Models/`：长期或单局状态。状态变化优先通过 model 方法完成，避免 UI/controller 分散改字段。
- `Systems/`：无 Godot 节点依赖的持续逻辑或状态推进。需要按帧更新时实现 `ITickable`，由 `Supermarket` 注册。
- `Commands/`：一次性状态变更入口。UI、Controller、System 都应通过 Command 表达“做一件事”。
- `Queries/`：只读判定入口。不要在 Query 内修改状态。
- `Events/`：跨层通知的数据载体。事件不拥有业务流程。
- `Controllers/`：Godot Node、场景引用、输入、碰撞、raycast、UI 绑定。凡是需要 `Node`、`Area3D`、`CharacterBody3D`、`Camera3D` 或场景路径的逻辑，默认放 Controller。
- `Utilities/`：纯服务和查表逻辑，例如商品目录。

## 依赖方向

- Controller 可以读取 model/system/utility，可以发送 command/query/event。
- Command 可以读写 model，可以调用 system/utility，可以发送 event/query，也可以发送 command。
- Query 只能读取 model/system，可以发送 query；不要读取 utility，除非本地 QFramework 接口明确扩展。
- System 可以读取 model/system/utility，可以发送 event 和 command。
- Model 只保存状态和局部状态操作；不要依赖 Godot 节点。

## 本地 QFramework 偏差

本项目的 `QFramework.cs` 是本地 fork，不以上游文档为准。已接受的差异：

- `IController`、`ISystem`、`ICommand` 都可以 `ICanGetUtility`。
- `IController`、`ICommand` 可以 `ICanSendQuery`。
- `IQuery` 可以 `ICanSendQuery`，但不含 `ICanGetUtility`。
- `ISystem` 可以 `ICanSendCommand`。原因：`TimerSystem`、`ExtractionSystem` 这类系统拥有状态推进管线，需要直接结束回合或完成撤离。
- `Architecture<T>.Reset()` 是项目内新增 API，用于场景重载和测试隔离。
- `BindableProperty<T>` comparer 是实例级，不是 upstream 的静态共享 comparer。
- Unity-only API 已移除。

## 初始化约束

- `Supermarket.Init()` 是注册 model/system/utility 的唯一入口。
- 不要在 `System.OnInit()` 内假设 `Supermarket.Interface` 是安全注册句柄；架构实例在 init 循环完成后才完全稳定。
- 需要 tick 的 system 在 `Supermarket.Init()` 里创建、注册，再调用 `RegisterTickable(...)`。
- 场景重载或完整重开前使用 `Supermarket.Reset()` 清理 singleton 架构状态。

## Godot 引用归属

- Systems 不直接拥有 Godot spatial/runtime 引用。
- 视觉、输入、碰撞、交互区、raycast、场景节点查找归 Controller。
- 保安侦测保持在 `GuardController`，不引入独立 `DetectionSystem`。
- 交互资格以 `Area3D` 重叠状态为准：`IInteractionTarget.IsPlayerInInteractionArea(PlayerController)` 是选择目标的来源。

## 目录决策速查

- 新增“玩家按键触发一件事”：Controller 读取输入，发送 Command。
- 新增“一次性状态改变”：Command。
- 新增“是否允许某行为”：Query。
- 新增“每帧推进、无节点引用”：System + `ITickable`。
- 新增“需要场景节点、碰撞、raycast、UI”：Controller。
- 新增“长期数据或单局数据”：Model。
- 新增“商品/配置查表”：Utility 或 Config。
