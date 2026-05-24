using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Commands;
using EscapeFromSupermarket.Events;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers.UI
{
    public partial class CartPanelController : Control, IController
    {
        private VBoxContainer _itemList;
        private Button _closeButton;

        public override void _Ready()
        {
            _itemList = GetNodeOrNull<VBoxContainer>("Margin/Content/ItemsScroll/ItemList")
                ?? GetNodeOrNull<VBoxContainer>("Margin/Content/ItemList");
            _closeButton = GetNodeOrNull<Button>("Margin/Content/CloseButton");

            Visible = false;
            if (_closeButton != null) _closeButton.Pressed += () => Visible = false;

            this.RegisterEvent<CartItemsChangedEvent>(_ => Rebuild())
                .UnRegisterWhenNodeExitTree(this);
            this.RegisterEvent<RoundEndedEvent>(_ => Visible = false)
                .UnRegisterWhenNodeExitTree(this);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (!@event.IsActionPressed("toggle_cart")) return;

            var gameState = this.GetModel<GameStateModel>();
            if (gameState.State.Value != RoundResult.Running) return;

            Visible = !Visible;
            if (Visible) Rebuild();
            GetViewport().SetInputAsHandled();
        }

        private void Rebuild()
        {
            if (_itemList == null) return;

            foreach (var child in _itemList.GetChildren())
            {
                _itemList.RemoveChild(child);
                child.QueueFree();
            }

            var cart = this.GetModel<CartModel>();
            if (cart == null || cart.Items.Count == 0)
            {
                _itemList.AddChild(new Label { Text = "购物车为空" });
                return;
            }

            foreach (var item in cart.Items)
            {
                var row = new HBoxContainer();
                row.AddChild(new Label
                {
                    Text = $"{item.Product.Name}  价值 {item.Product.Value}  {item.Product.Slots}格  重量 {item.Product.Weight}",
                    CustomMinimumSize = new Vector2(260, 28),
                });

                var button = new Button { Text = "丢弃" };
                int instanceId = item.InstanceId;
                button.Pressed += () =>
                {
                    button.Disabled = true;
                    this.SendCommand(new DropProductCommand(instanceId));
                };
                row.AddChild(button);
                _itemList.AddChild(row);
            }
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
