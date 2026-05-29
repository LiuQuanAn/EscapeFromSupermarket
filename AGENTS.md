## 对话和工作方式

- 和用户对话必须使用简体中文。
- 每次对话调用 caveman 插件：表达要短，保留技术细节，去掉寒暄和填充话。
- 编写代码前先说明关键假设、成功标准和验证方式；不清楚就先问，不要默默猜。
- 简单优先：用能解决问题的最少代码，不做请求之外的功能，不为只用一次的代码抽象。
- 精准修改：只改和当前任务直接相关的文件。不要顺手重构、顺手格式化、顺手删除原本存在的死代码。
- 避免防御性兜底。需要报错就报错；确实需要防御性处理时，必须留下明确警告或失败信号。
- 一次只推进一个 active task。未完成当前任务前，不开启新的无关修复。

## 项目入口

- 仓库根目录：`C:\Users\Administrator\Documents\速通超市`
- Godot 项目目录：`escape-from-supermarket`
- 主场景：`escape-from-supermarket/Scenes/Main.tscn`
- Godot C# 工程：`escape-from-supermarket/EscapeFromSupermarket.sln`
- .NET SDK：`10.0.202`（由仓库根目录 `global.json` 固定）
- Godot 版本：`4.6.3.stable.mono`
- Godot console 默认路径：`D:\Program Files\Godot\Godot_v4.6.3-stable_mono_win64_console.exe`

## 开工前必读

- 玩法、数值、spec、playtest、文案任务走设计师 agent；C#、Godot 场景、构建、bug、harness 任务走程序员 agent。
- 设计师 agent 读取 `docs/harness/DESIGN_AGENT.md`。
- 程序员 agent 读取 `docs/harness/IMPLEMENT_AGENT.md`。
- 涉及 QFramework API 前，先读 `escape-from-supermarket/addons/qframework/QFramework.cs` 文件头的 `LOCAL FORK NOTICE`。本项目使用本地 fork，不以上游文档为准。
