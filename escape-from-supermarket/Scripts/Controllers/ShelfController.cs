using System;
using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Controllers.UI;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
    public partial class ShelfController : Area3D, IController
    {
        [Export] public int ShelfId { get; set; }

        private bool _playerNearby;
        private bool _wasInteractPressed;
        private ShelfPanelController _panel;

        public override void _Ready()
        {
            if (ShelfId <= 0)
            {
                throw new InvalidOperationException(
                    $"ShelfController '{Name}' requires ShelfId > 0 in the scene; got {ShelfId}.");
            }

            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
            _panel = GetTree().CurrentScene?.GetNodeOrNull<ShelfPanelController>("UI/ShelfPanel");
        }

        public override void _Process(double delta)
        {
            bool interactPressed = Input.IsPhysicalKeyPressed(Key.E);
            bool justPressed = interactPressed && !_wasInteractPressed;
            _wasInteractPressed = interactPressed;
            if (!_playerNearby || !justPressed) return;

            _panel ??= GetTree().CurrentScene?.GetNodeOrNull<ShelfPanelController>("UI/ShelfPanel");
            _panel?.ToggleShelf(ShelfId);
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body is PlayerController) _playerNearby = true;
        }

        private void OnBodyExited(Node3D body)
        {
            if (body is PlayerController) _playerNearby = false;
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
