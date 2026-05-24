using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Commands;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
    public partial class GuardController : CharacterBody3D, IController
    {
        [Export] public float PatrolSpeed { get; set; } = 2.0f;
        [Export] public float ChaseSpeed { get; set; } = 3.5f;
        [Export] public float TurnSpeed { get; set; } = 10.0f;
        [Export] public float ViewDistance { get; set; } = 7.0f;
        [Export] public float ViewAngleDegrees { get; set; } = 70.0f;
        [Export] public float RaiseRate { get; set; } = 0.55f;
        [Export] public float DecayRate { get; set; } = 0.35f;
        [Export] public float CatchDistance { get; set; } = 1.05f;

        private GuardModel _guard;
        private CartModel _cart;
        private GameStateModel _gameState;
        private PlayerController _player;
        private int _patrolIndex;
        private bool _caughtFired;

        public override void _Ready()
        {
            _guard = this.GetModel<GuardModel>();
            _cart = this.GetModel<CartModel>();
            _gameState = this.GetModel<GameStateModel>();
            _player = GetTree().CurrentScene?.GetNodeOrNull<PlayerController>("Player");
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_guard == null || _cart == null || _gameState == null) return;
            if (_gameState.State.Value != RoundResult.Running)
            {
                Velocity = Vector3.Zero;
                MoveAndSlide();
                return;
            }

            _player ??= GetTree().CurrentScene?.GetNodeOrNull<PlayerController>("Player");
            if (_player == null) return;

            UpdateDetection((float)delta);
            MoveGuard((float)delta);

            if (!_caughtFired && GlobalPosition.DistanceTo(_player.GlobalPosition) <= CatchDistance)
            {
                _caughtFired = true;
                this.SendCommand(EndRoundCommand.Lose(LossReason.Caught));
            }
        }

        private void UpdateDetection(float delta)
        {
            bool canSeeLoadedPlayer = _cart.CurrentSlots.Value > 0 && CanSeePlayer();
            float alertDelta = (canSeeLoadedPlayer ? RaiseRate : -DecayRate) * delta;
            this.SendCommand(new AdjustAlertCommand(alertDelta));
        }

        private bool CanSeePlayer()
        {
            var toPlayer = _player.GlobalPosition - GlobalPosition;
            toPlayer.Y = 0;
            float distSq = toPlayer.LengthSquared();
            if (distSq <= 0.0001f || distSq > ViewDistance * ViewDistance) return false;

            var forward = -GlobalTransform.Basis.Z;
            forward.Y = 0;
            forward = forward.Normalized();

            var direction = toPlayer.Normalized();
            float angle = Mathf.RadToDeg(forward.AngleTo(direction));
            if (angle > ViewAngleDegrees * 0.5f) return false;

            var spaceState = GetWorld3D().DirectSpaceState;
            var from = GlobalPosition + Vector3.Up * 0.8f;
            var to = _player.GlobalPosition + Vector3.Up * 0.6f;
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            query.CollideWithAreas = false;
            query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

            var hit = spaceState.IntersectRay(query);
            return hit.Count == 0 || hit.TryGetValue("collider", out var collider) && collider.AsGodotObject() == _player;
        }

        private void MoveGuard(float delta)
        {
            var target = _guard.State.Value == GuardState.Chasing
                ? _player.GlobalPosition
                : CurrentPatrolTarget();

            var toTarget = target - GlobalPosition;
            toTarget.Y = 0;

            if (_guard.State.Value == GuardState.Patrolling && toTarget.Length() < 0.35f)
            {
                _patrolIndex = (_patrolIndex + 1) % _guard.PatrolPath.Length;
                toTarget = CurrentPatrolTarget() - GlobalPosition;
                toTarget.Y = 0;
            }

            if (toTarget.LengthSquared() <= 0.0001f)
            {
                Velocity = Vector3.Zero;
                MoveAndSlide();
                return;
            }

            var direction = toTarget.Normalized();
            float speed = _guard.State.Value == GuardState.Chasing ? ChaseSpeed : PatrolSpeed;
            Velocity = direction * speed;
            MoveAndSlide();

            float targetYaw = Mathf.Atan2(-direction.X, -direction.Z);
            Rotation = new Vector3(0, Mathf.LerpAngle(Rotation.Y, targetYaw, TurnSpeed * delta), 0);
        }

        private Vector3 CurrentPatrolTarget()
        {
            if (_guard.PatrolPath == null || _guard.PatrolPath.Length == 0) return GlobalPosition;
            return _guard.PatrolPath[_patrolIndex % _guard.PatrolPath.Length];
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
