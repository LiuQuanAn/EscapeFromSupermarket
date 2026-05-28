using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Core;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers.UI
{
    public partial class InteractionManagerController : Label, IController
    {
        private PlayerController _player;
        private Camera3D _camera;
        private IInteractionTarget _currentTarget;
        private Control _shelfPanel;
        private Control _cartPanel;
        private Control _resultPanel;

        public override void _Ready()
        {
            Visible = false;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            AddThemeColorOverride("font_color", Colors.White);
            AddThemeColorOverride("font_outline_color", Colors.Black);
            AddThemeConstantOverride("outline_size", 4);
            CustomMinimumSize = new Vector2(220, 32);

            _player = GetTree().CurrentScene?.GetNodeOrNull<PlayerController>("Player");
            _camera = _player?.GetNodeOrNull<Camera3D>("Camera3D");
            _shelfPanel = GetTree().CurrentScene?.GetNodeOrNull<Control>("UI/ShelfPanel");
            _cartPanel = GetTree().CurrentScene?.GetNodeOrNull<Control>("UI/CartPanel");
            _resultPanel = GetTree().CurrentScene?.GetNodeOrNull<Control>("UI/ResultPanel");
        }

        public override void _Process(double delta)
        {
            UpdateCurrentTarget();
            UpdatePrompt();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (_currentTarget == null || !_currentTarget.RequiresInteract) return;
            if (!@event.IsPressed() || @event.IsEcho()) return;
            if (@event is not InputEventKey key || key.PhysicalKeycode != Key.E) return;

            _currentTarget.Interact(_player);
            GetViewport().SetInputAsHandled();
        }

        private void UpdateCurrentTarget()
        {
            _currentTarget = null;
            if (_player == null || _camera == null || IsModalUiOpen()) return;

            var gameState = this.GetModel<GameStateModel>();
            if (gameState == null || gameState.State.Value != RoundResult.Running) return;

            float bestDistSq = float.MaxValue;
            foreach (var node in GetTree().GetNodesInGroup(InteractionGroups.Targets))
            {
                if (node is not IInteractionTarget target) continue;
                if (!target.IsInteractionAvailable) continue;
                if (!target.IsPlayerInInteractionArea(_player)) continue;

                float distSq = _player.GlobalPosition.DistanceSquaredTo(target.TargetNode.GlobalPosition);
                if (distSq >= bestDistSq) continue;

                bestDistSq = distSq;
                _currentTarget = target;
            }
        }

        private void UpdatePrompt()
        {
            if (_currentTarget == null)
            {
                Visible = false;
                Text = string.Empty;
                return;
            }

            Text = _currentTarget.GetInteractionPrompt();
            var promptPosition = _currentTarget.PromptWorldPosition;
            if (_camera.IsPositionBehind(promptPosition))
            {
                Visible = false;
                return;
            }

            var screenPos = _camera.UnprojectPosition(promptPosition);
            Position = screenPos - new Vector2(CustomMinimumSize.X * 0.5f, CustomMinimumSize.Y + 10.0f);
            Visible = true;
        }

        private bool IsModalUiOpen()
        {
            return (_shelfPanel?.Visible ?? false)
                || (_cartPanel?.Visible ?? false)
                || (_resultPanel?.Visible ?? false);
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
