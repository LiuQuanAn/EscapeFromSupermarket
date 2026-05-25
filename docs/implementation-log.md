# Implementation Log — escape-from-supermarket

Long-term engineering memory for this project. Records what plan steps and review-driven fixes have already been implemented, with commit references, so future agents (Claude, Codex, or human) do not duplicate work.

## How to use

- **Before proposing new work**: scan §Completed for the relevant plan step or review item. If it is already checked off with a commit SHA, do not re-implement.
- **After landing a commit that closes a plan step or review batch**: append a new entry under §Completed in chronological order. Update §Pending. Update §Plan revisions if the plan file itself changed.
- **Both Claude and Codex sessions write here.** Treat it as the single source of truth for "what is done". The canonical plan lives at `~/.claude/plans/https-github-com-liangxiegame-qframework-giggly-scone.md`; this log is the implementation receipt against that plan.

## Plan source

- Canonical plan: `~/.claude/plans/https-github-com-liangxiegame-qframework-giggly-scone.md` (not in repo; on the dev workstation only)
- Design references: `docs/gameplay-design.md`, `docs/prototype-spec.md`
- Architecture banner: `escape-from-supermarket/addons/qframework/QFramework.cs` header `LOCAL FORK NOTICE` block

## Completed

### 2026-05-23 — Steps 0 + 1 — commit `51831d1`

Architecture scaffold + movable player.

- Step 0 — `Scripts/Architecture/Supermarket.cs` with `RegisterTickable` / `UnregisterTickable` / `Tick(double)` (pending add/remove queue flush, codex P2 fix already baked in); `Scripts/Core/ITickable.cs`; `Scripts/Controllers/GameRoot.cs` (`Node3D` root — necessary because `Main.tscn` root is `Node3D`); `Scenes/Main.tscn` minimal scene.
- Step 1 — `Models/CartModel.cs` (CurrentSlots/Weight/Value BindableProperties, LoadTier derived, `List<CartItem> Items` placeholder for Step 3+); `Systems/MovementSystem.cs` GetSpeedMultiplier (1.00 / 0.85 / 0.65); `Controllers/PlayerController.cs` Tunic camera (Player.Camera3D pitch −45° + yaw +45° orthographic), Visual child carries player + cart meshes and rotates to face movement direction, WASD projected through camera basis.

### 2026-05-23 — Steps 2-8 (codex-implemented while额度耗尽) — commit `d057435`

Codex finished the full prototype loop minus tuning.

- Step 2 — Map obstacles in `Scenes/Main.tscn` (outer walls + 4 shelf blocks as `StaticBody3D` + `CollisionShape3D`).
- Step 3 — `Utilities/ProductCatalog.cs` (6 hard-coded products, 3 categories); `Models/ShelfModel.cs` (4 shelves, stable `InstanceId` via `Interlocked.Increment`); `Events/ShelfChangedEvent.cs`, `Events/CartItemsChangedEvent.cs`; `Queries/CanPickProductQuery.cs`; `Commands/PickProductCommand.cs`; `Controllers/ShelfController.cs` (Area3D + `E` interact, single shelf panel); `Controllers/UI/ShelfPanelController.cs`.
- Step 4 — `Controllers/UI/HudController.cs` on `CanvasLayer`, three Labels + countdown placeholder + extraction/alert progress bars. `Main.tscn` HUD subtree.
- Step 5 — `Models/GameStateModel.cs` with `RoundResult` / `LossReason` / `WinReason` enums; `Systems/TimerSystem.cs` ITickable counting down from 240s; `Events/RoundEndedEvent.cs`; `Commands/EndRoundCommand.cs` with private ctor + `Lose(...)` / `Win(...)` factories + Running guard.
- Step 6 — `Controllers/ExtractionZoneController.cs`; `Systems/ExtractionSystem.cs` ITickable; `Commands/StartExtractionCommand.cs` + `Commands/CancelExtractionCommand.cs`; `Controllers/UI/ResultPanelController.cs` with restart sequence `Supermarket.Reset() → ReloadCurrentScene()`.
- Step 7 — `Models/GuardModel.cs` (PatrolPath + Alert + GuardState); `Commands/AdjustAlertCommand.cs`; `Controllers/GuardController.cs` (vision cone + raycast in controller, not in a System).
- Step 8 — `Commands/DropProductCommand.cs`; `Controllers/UI/CartPanelController.cs` (Tab toggle).

### 2026-05-23 — Plan revisions + bug-fix batch — commit `d057435`

Plan accepts these architectural offsets:
- `IController`/`ISystem`/`ICommand` carry `ICanGetUtility`; `IController`/`ICommand` carry `ICanSendQuery`; `ISystem` carries `ICanSendCommand`. Mirrored into `QFramework.cs` `LOCAL FORK NOTICE`.
- `CartCollision` (CollisionShape3D on Player) is acknowledged as a Step 1 extension; sync handled by `PlayerController.SyncCartCollision(yaw)`.
- `GuardController` rotates its root `CharacterBody3D` (not a Visual child) because the vision cone must follow the body forward.

Fix batch landed in the same commit:
- Major #1 (Codex review n+1): `PlayerController._PhysicsProcess` guards on `GameStateModel.State != Running`; player freezes when round ends.
- Major #2: `Scripts/Events/PickFailedEvent.cs` added; `PickProductCommand` emits it on Query / shelf-removal failure; `ShelfPanelController.PickItem` no longer pre-queries — it sends Command and updates `_statusLabel` via the new event.
- Major #3: `ISystem` granted `ICanSendCommand` mixin in the local fork; `TimerSystem` and `ExtractionSystem` use `this.SendCommand(...)` instead of routing through `Supermarket.Interface.SendCommand(...)`.
- Major #5 (Tab UI conflict): `project.godot` `[input]` action `toggle_cart` bound to physical Tab; `CartPanelController` switched to `_UnhandledInput` + `GetViewport().SetInputAsHandled()` so the focus traversal cannot eat the key.
- Major #6: `GuardController` adds `_caughtFired` flag — `EndRoundCommand.Lose(Caught)` fires once instead of every frame inside catch radius.
- Minor #8: `ShelfModel.FillShelf` switched to `Random.Shared.Next(...)` for non-deterministic per-run variance.
- Minor #10: `Main.tscn` UID changed to standard `uid://b8s1m4r2t3kqa`.
- Minor #11 (defensive cleanup): `PlayerController` uses `GetNode<T>` (no `OrNull`), drops `_visual`/`_cartCollision`/`_cart`/`_movement` null-fallbacks, drops `GD.PushError` + `SetPhysicsProcess(false)` scaffolding. `SafePlanarDirection` retained as a mathematical NaN guard.
- Minor #12: `Main.tscn` `CloseButton` declaration moved inside `ShelfPanel/Margin/Content` block (was appended at file end).
- Minor #14: `HudController` `using System;` removed; `Math.Max/Clamp/Ceiling` replaced with `Mathf.Max / Mathf.Clamp / Mathf.CeilToInt`.

### 2026-05-23 — Second codex review batch — commit `d057435`

- #1 `CartModel.AddItem` / `TryRemoveItem` methods own atomic mutation of items + counters + LoadTier; subscription on `CurrentWeight.Register` removed.
- #9 `GameStateModel.IsExtracting BindableProperty<bool>` introduced. `StartExtractionCommand` / `CancelExtractionCommand` write the model directly; `ExtractionSystem` reads it instead of holding its own `_isActive` flag.
- #10 `AdjustAlertCommand` first-line guard: non-Running → return.
- #16 `ProductCatalog` builds an immutable `Dictionary<string, IReadOnlyList<Product>>` in its ctor; `GetByCategory` no longer LINQ-allocates per call.
- #17 `GuardController.CanSeePlayer` compares squared distance against `ViewDistance * ViewDistance` — one fewer `Mathf.Sqrt` per frame.
- #18 `ShelfPanelController.OnPickFailed` rebuilds before writing the reason label, so the message is not wiped by the rebuild.
- #21 `CartPanelController._UnhandledInput` guards on `GameStateModel.State != Running`; Tab no longer toggles cart UI after the round ends.
- #23 `ShelfController._Ready` throws if `ShelfId <= 0`; `ResolveShelfId()` fallback removed (no quiet zero default).
- #25 `ExtractionSystem.Tick` sets `IsExtracting = false` before dispatching the win command — no more per-frame `EndRoundCommand` allocations after the threshold is crossed.

### 2026-05-23 — Regression fix: guard catch gated on Chasing — commit `6973569`

User回归测试: "保安发现玩家后不再追逐玩家". Root cause: `GuardController._PhysicsProcess` runs the catch check (`DistanceTo(player) <= CatchDistance`) independent of guard state, so when the patrol path crosses the player's position the round ended on the first frame of proximity — Alert bar flashed once, no observable chase, immediate Lost panel.

- `GuardController.cs` — catch condition now requires `_guard.State.Value == GuardState.Chasing` in addition to `!_caughtFired` and the distance test. Patrol-pass-by no longer ends the round; the player must be detected (Alert reaches 1.0) and the guard must be physically chasing before contact counts.

### 2026-05-24 — V0.2 vertical slice — commit `pending`

Implemented `docs/v0.2-vertical-slice-spec.md` on top of the V0.1 prototype, plus the first review cleanup batch before commit.

- Balance / models — `PrototypeBalance` became the shared tuning entry; added `MetaProgressModel` for runtime money/upgrades/navigation progress and `RoundObjectiveModel` for keycard/router state. `CartModel`, `GameStateModel`, `GuardModel`, `ShelfModel`, `MovementSystem`, and `ProductCatalog` now read V0.2 state and tuning from those models/utilities.
- Interactions / world objects — added `IInteractionTarget`, `InteractionManagerController`, `KeycardController`, and dual extraction doors. Shelves, keycard, and exits now use nearest-target world prompts; employee door requires keycard and uses a shorter extraction time.
- V0.2 loop — added router task item, guaranteed per-round router/high-value shelf refresh, win-only money rewards, router-only navigation progress, buyable cart-capacity and movement-speed upgrades, and `StartNextRoundCommand` so singleton models reset per round while meta progress survives scene reload.
- NPC / scene — added `CustomerController` and 3 simple blocking customers; added right-side employee exit, keycard, HUD objective/keycard labels, result-panel upgrade buttons, and scroll containers for shelf/cart/result item lists.
- Review cleanup — restored `Main.tscn` UID to `uid://b8s1m4r2t3kqa`, removed invalid `unique_id=<number>` scene pollution, removed new `?? PrototypeBalance.Default` and `?.ResetRound()` fallbacks, cached speed-upgrade and extraction HUD state, guarded interaction prompts behind camera, and documented the next-round reset/reload split.

### 2026-05-24 — Shelf item identification — commit `pending`

Implemented `docs/shelf-item-identification-spec.md` so shelf loot starts hidden and is revealed one item at a time while the game continues running.

- Data / tuning — added `ProductRarity`, per-rarity identification times, and task-item identification time in `PrototypeBalance`; microwave is rare, television is high rare, and router uses task timing.
- Shelf state — `ShelfModel` now tracks identified shelf item instances for the current round, clears identification on `RefreshRound()`, and removes identification state when an item leaves a shelf.
- Commands / picking — added `IdentifyShelfItemCommand`; `CanPickProductQuery` rejects unknown items before capacity and weight checks.
- UI / interruption — `ShelfPanelController` now shows unknown placeholders, one active countdown/progress bar, complete item details only after identification, and resets unfinished progress when the panel closes, round ends, or the player leaves shelf range.

## Pending

- V0.2 manual 5-10 round playtest: route choice, employee-door usage, router pickup, upgrade order, customer blocking feel, UI readability, and 3-5 minute round length.
- V0.2 tuning pass in `PrototypeBalance`: movement/load multipliers, guard vision/alert/chase, extraction timing, product value/slots/weight, customer speed/push, upgrade prices.
- Manual shelf-identification playtest: order reveal, close/reopen reset, same-round remembered identified items, and unknown-item pick rejection.

## Plan revisions

| Date | Revision | Source |
|---|---|---|
| 2026-05-23 | Initial Steps 0–9 plan written. | Plan v1 |
| 2026-05-23 | Codex first-pass plan review: ITickable registration moved to `Supermarket.Init()`, `CartModel.Items` advanced to Step 1, `DetectionSystem` dropped (Guard does its own raycast), `RoundResult` unified, `ExtractionProgress` moved to `GameStateModel`, 4 shelves, stable `InstanceId`. | Plan v2 |
| 2026-05-23 | Local-fork divergence accepted: `ISystem`/`IController`/`ICommand` extra mixins, CartCollision as Step 1 extension, Guard root rotation. | Plan v3 |
| 2026-05-23 | Second codex review batch (above) — no plan text change, just implementation refinement. | — |
| 2026-05-24 | V0.2 vertical slice scoped by `docs/v0.2-vertical-slice-spec.md`: interaction prompts, keycard/staff door, router objective, runtime meta progress, upgrades, customers, and 5-10 round tuning target. | V0.2 spec |
| 2026-05-24 | Shelf interaction now requires per-round item identification before pick, scoped by `docs/shelf-item-identification-spec.md`. | Shelf item identification spec |

## Convention for new entries

```
### YYYY-MM-DD — <short summary> — commit `<sha>`

<one paragraph context>

- <bullet per file or per sub-change, naming paths>
```

If the work is still uncommitted, write `commit `6973569`` and update the SHA when the commit lands. If a batch closes a Codex review, reference the review numbering (`Major #N`, `Minor #N`) so future readers can cross-check.
