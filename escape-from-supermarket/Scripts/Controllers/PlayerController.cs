using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Models;
using EscapeFromSupermarket.Systems;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
    public partial class PlayerController : CharacterBody3D, IController
    {
        [Export] public float BaseSpeed { get; set; } = 4.5f;
        [Export] public float TurnSpeed { get; set; } = 12.0f;
        [Export] public NodePath VisualPath { get; set; } = "Visual";
        [Export] public NodePath CameraPath { get; set; } = "Camera3D";

        private CartModel _cart;
        private MovementSystem _movement;
        private Node3D _visual;
        private Camera3D _camera;

        public override void _Ready()
        {
            _cart = this.GetModel<CartModel>();
            _movement = this.GetSystem<MovementSystem>();
            _visual = GetNodeOrNull<Node3D>(VisualPath);
            _camera = GetNodeOrNull<Camera3D>(CameraPath);
        }

        public override void _PhysicsProcess(double delta)
        {
            var input = ReadInputAxes();
            var dir = ProjectToCameraSpace(input);

            float mult = _movement.GetSpeedMultiplier(_cart.LoadTier.Value);
            Velocity = dir * BaseSpeed * mult;
            MoveAndSlide();

            if (dir.LengthSquared() > 0.01f && _visual != null)
            {
                // Align Visual's local -Z (Godot forward) with movement direction.
                var targetYaw = Mathf.Atan2(-dir.X, -dir.Z);
                _visual.Rotation = new Vector3(0, Mathf.LerpAngle(_visual.Rotation.Y, targetYaw, TurnSpeed * (float)delta), 0);
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
            if (_camera == null)
            {
                return new Vector3(input.X, 0, input.Y);
            }

            var basis = _camera.GlobalTransform.Basis;
            // Project camera forward (-Z) and right (+X) onto XZ plane.
            var camForward = new Vector3(-basis.Z.X, 0, -basis.Z.Z).Normalized();
            var camRight = new Vector3(basis.X.X, 0, basis.X.Z).Normalized();
            // input.Y = -1 (W) → +camForward; input.X = +1 (D) → +camRight.
            return camRight * input.X + camForward * -input.Y;
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
