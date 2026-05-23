# Project Codex Rules

Codex-only project guidance lives above the auto-sync block. The block below is regenerated from Claude memory by `~/.claude/scripts/sync-memory-to-codex.ps1` — do not edit between the markers.

<!-- BEGIN AUTO-SYNC: claude-memory-to-codex -->
# Project Memory (synced from ~/.claude/projects/c--Users-Administrator-Documents-----/memory)

## qframework-local-fork.md

---
name: qframework-local-fork
description: QFramework.cs in this project is a local fork — do NOT treat upstream behavior as truth. Specific bug fixes already applied; see file header banner for full diff list.
metadata: 
  node_type: memory
  type: project
  originSessionId: 8d5a2042-5560-4624-939a-06d8efe37d25
---

`escape-from-supermarket/addons/QFramework/QFramework.cs` is a local fork of liangxiegame/QFramework Godot port. Upstream tracking is abandoned. The file's own header banner ("LOCAL FORK NOTICE") lists every divergence.

**Why:** Several Critical/Major bugs in upstream (static `BindableProperty.Comparer` cross-instance pollution; non-idempotent `CustomUnRegister`/`BindablePropertyUnRegister`; `OrEvent.UnRegister` cascading bug; `HashSet` modify-during-iterate in `MakeSureArchitecture`; duplicate-type re-registration init double-run; `UnRegisterWhenNodeExitTree` no signal detach; thread-unsafe `EasyEvents`/`MakeSureArchitecture`). User opted for direct source edits over patch files because struct/private-static/extension-method targets cannot be overridden externally.

**How to apply:**
- Before recommending or referencing any QFramework API behavior, read the file's header banner first (lines 1-55) — it lists every behavior that differs from public QFramework docs.
- Spec divergence accepted: `IController`/`ISystem`/`ICommand` legitimately have `ICanGetUtility`; `IController`/`ICommand` legitimately have `ICanSendQuery`. The 4-layer rule doc is informational only — code is canonical.
- Added API: `Architecture<T>.Reset()` (static) — clears `mArchitecture` + `OnRegisterPatch` for scene reload / test isolation. Use in test teardown and on full scene swap.
- Comparer is now instance, not static: `new BindableProperty<int>().WithComparer(...)` no longer pollutes other instances. Default uses `EqualityComparer<T>.Default.Equals`.
- Unity-only blocks (`UnRegisterOnDestroyTrigger`, `UnRegisterWhenGameObjectDestroyed`, `ComparerAutoRegister`, `EditorMenus`) were removed entirely.
- Codex `AGENTS.md` is auto-synced from this memory via the global sync script — no separate sync needed.
<!-- END AUTO-SYNC -->
