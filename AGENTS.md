# Project Codex Rules

Codex-only project guidance lives above the auto-sync block. The block below is regenerated from Claude memory by `~/.claude/scripts/sync-memory-to-codex.ps1` — do not edit between the markers.

<!-- BEGIN AUTO-SYNC: claude-memory-to-codex -->
# Project Memory (synced from ~/.claude/projects/c--Users-Administrator-Documents-----/memory)

## project-implementation-log.md

---
name: project-implementation-log
description: escape-from-supermarket keeps a long-term engineering log at docs/implementation-log.md. Always read it before proposing new feature work to avoid re-implementing completed plan steps or fix batches.
metadata: 
  node_type: memory
  type: project
  originSessionId: 8d5a2042-5560-4624-939a-06d8efe37d25
---

The project's authoritative implementation history lives at `docs/implementation-log.md` in the repo (`escape-from-supermarket/...` is the Godot subdir; the log sits at repo root under `docs/`).

**Read before proposing work.** Each entry is dated and pinned to a commit SHA. If you are about to write or re-implement a plan step (Step N) or a Codex-review item (Major #N / Minor #N), grep the log first. If it is already listed with a commit, do not duplicate — propose only delta or next-step work.

**Write after landing work.** Every commit that closes a plan step, applies a Codex review batch, or revises the plan must append a new entry under §Completed, update §Pending, and update §Plan revisions when the plan text changes. The format convention is documented at the bottom of the log itself.

The canonical plan is at `~/.claude/plans/https-github-com-liangxiegame-qframework-giggly-scone.md` (workstation-local). When the plan revises, mirror the headline into §Plan revisions of the log.


## qframework-local-fork.md

---
name: qframework-local-fork
description: QFramework.cs in this project is a local fork — do NOT treat upstream behavior as truth. Specific bug fixes already applied; see file header banner for full diff list.
metadata: 
  node_type: memory
  type: project
  originSessionId: 8d5a2042-5560-4624-939a-06d8efe37d25
---

`escape-from-supermarket/addons/qframework/QFramework.cs` is a local fork of liangxiegame/QFramework Godot port. Upstream tracking is abandoned. The file's own header banner ("LOCAL FORK NOTICE") lists every divergence.

**Why:** Several Critical/Major bugs in upstream (static `BindableProperty.Comparer` cross-instance pollution; non-idempotent `CustomUnRegister`/`BindablePropertyUnRegister`; `OrEvent.UnRegister` cascading bug; `HashSet` modify-during-iterate in `MakeSureArchitecture`; duplicate-type re-registration init double-run; `UnRegisterWhenNodeExitTree` no signal detach; thread-unsafe `EasyEvents`/`MakeSureArchitecture`). User opted for direct source edits over patch files because struct/private-static/extension-method targets cannot be overridden externally.

**How to apply:**
- Before recommending or referencing any QFramework API behavior, read the file's header banner first (lines 1-55) — it lists every behavior that differs from public QFramework docs.
- Spec divergence accepted: `IController`/`ISystem`/`ICommand` legitimately have `ICanGetUtility`; `IController`/`ICommand` legitimately have `ICanSendQuery`; `ISystem` legitimately has `ICanSendCommand` (Systems own state mutation pipelines that need to dispatch Commands directly — e.g. TimerSystem ends round, ExtractionSystem completes extraction). The 4-layer rule doc is informational only — code is canonical.
- Added API: `Architecture<T>.Reset()` (static) — clears `mArchitecture` + `OnRegisterPatch` for scene reload / test isolation. Use in test teardown and on full scene swap.
- Comparer is now instance, not static: `new BindableProperty<int>().WithComparer(...)` no longer pollutes other instances. Default uses `EqualityComparer<T>.Default.Equals`.
- Unity-only blocks (`UnRegisterOnDestroyTrigger`, `UnRegisterWhenGameObjectDestroyed`, `ComparerAutoRegister`, `EditorMenus`) were removed entirely.
- Codex `AGENTS.md` is auto-synced from this memory via the global sync script — no separate sync needed.
<!-- END AUTO-SYNC -->
