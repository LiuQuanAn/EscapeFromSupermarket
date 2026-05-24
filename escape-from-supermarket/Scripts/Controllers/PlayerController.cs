using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Models;
using EscapeFromSupermarket.Systems;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
    public partial class PlayerController : CharacterBody3D, IController
    {
        [Export] public NodePath VisualPath { get; set; } = "Visual";
        [Export] public NodePath CartCollisionPath { get; set; } = "CartCollision";
        [Export] public NodePath CameraPath { get; set; } = "Camera3D";

        private PrototypeBalance _balance = PrototypeBalance.Default;
        private CartModel _cart;
        private MovementSystem _movement;
        private GameStateModel _gameState;
        private Node3D _visual;
        private CollisionShape3D _cartCollision;
        private Camera3D _camera;
        private float _customerSlowTimer;
        private static readonly Vector3 WorldForward = new(0, 0, -1);
        private static readonly Vector3 WorldRight = new(1, 0, 0);
        private const float MinPlanarDirectionLengthSquared = 0.0001f;

        public override void _Ready()
        {
            _balance = this.GetUtility<PrototypeBalance>();
            _cart = this.GetModel<CartModel>();
            _movement = this.GetSystem<MovementSystem>();
            _gameState = this.GetModel<GameStateModel>();
            _visual = GetNode<Node3D>(VisualPath);
            _cartCollision = GetNode<CollisionShape3D>(CartCollisionPath);
            _camera = GetNode<Camera3D>(CameraPath);

            SyncCartCollision(_visual.Rotation.Y);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_gameState.State.Value != RoundResult.Running)
            {
                Velocity = Vector3.Zero;
                MoveAndSlide();
                return;
            }

            var input = ReadInputAxes();
            var dir = ProjectToCameraSpace(input);

            float mult = _movement.GetSpeedMultiplier(_cart.LoadTier.Value);
            float slowMult = _customerSlowTimer > 0.0f ? _balance.CustomerSlowMultiplier : 1.0f;
            _customerSlowTimer = Mathf.Max(0.0f, _customerSlowTimer - (float)delta);
            Velocity = dir * _balance.PlayerBaseSpeed * _movement.GetPlayerSpeedUpgradeMultiplier() * mult * slowMult;
            MoveAndSlide();

            if (dir.LengthSquared() > 0.01f)
            {
                // Align Visual's local -Z (Godot forward) with movement direction.
                var targetYaw = Mathf.Atan2(-dir.X, -dir.Z);
                var nextYaw = Mathf.LerpAngle(_visual.Rotation.Y, targetYaw, _balance.PlayerTurnSpeed * (float)delta);
                _visual.Rotation = new Vector3(0, nextYaw, 0);
                SyncCartCollision(nextYaw);
            }
        }

        private static Vector2 ReadInputAxes()
        {
            var v = new Vector2();
            if (Input.IsPhysicalKeyPressed(Key.W) || Input.IsPhysicalKeyPressed(Key.Up))    v.Y -= 1;
            if (Input.IsPhysicalKeyPressed(Key.S) || Input.IsPhysicalKeyPressed(Key.Down))  v.Y += 1;
            if (Input.IsPhysicalKeyPressed(Key.A) || Input.IsPhysicalKeyPressed(Key.Left))  v.X -= 1;
            if (Input.IsPhysicalKeyPressed(Key.D) || Input.IsPhysicalKeyPressed(Key.Right)) v.X += 1;
            return v.LengthSquared() > 1f ? v.Normalized() : v;
        }

        private Vector3 ProjectToCameraSpace(Vector2 input)
        {
            var basis = _camera.GlobalTransform.Basis;
            // Project camera forward (-Z) and right (+X) onto XZ plane.
            var camForward = SafePlanarDirection(new Vector3(-basis.Z.X, 0, -basis.Z.Z), WorldForward);
            var camRight = SafePlanarDirection(new Vector3(basis.X.X, 0, basis.X.Z), WorldRight);
            // input.Y = -1 (W) → +camForward; input.X = +1 (D) → +camRight.
            return camRight * input.X + camForward * -input.Y;
        }

        private static Vector3 SafePlanarDirection(Vector3 direction, Vector3 fallback)
        {
            return direction.LengthSquared() > MinPlanarDirectionLengthSquared
                ? direction.Normalized()
                : fallback;
        }

        private void SyncCartCollision(float yaw)
        {
            _cartCollision.Position = _balance.CartCollisionLocalOffset.Rotated(Vector3.Up, yaw);
            _cartCollision.Rotation = new Vector3(0, yaw, 0);
        }

        public void ApplyCustomerSlow()
        {
            _customerSlowTimer = _balance.CustomerSlowSeconds;
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
