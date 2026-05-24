using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Commands;
using EscapeFromSupermarket.Events;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers.UI
{
    public partial class ShelfPanelController : Control, IController
    {
        private Label _titleLabel;
        private Label _statusLabel;
        private VBoxContainer _itemList;
        private Button _closeButton;
        private int _currentShelfId;

        public override void _Ready()
        {
            _titleLabel = GetNodeOrNull<Label>("Margin/Content/TitleLabel");
            _statusLabel = GetNodeOrNull<Label>("Margin/Content/StatusLabel");
            _itemList = GetNodeOrNull<VBoxContainer>("Margin/Content/ItemsScroll/ItemList")
                ?? GetNodeOrNull<VBoxContainer>("Margin/Content/ItemList");
            _closeButton = GetNodeOrNull<Button>("Margin/Content/CloseButton");

            Visible = false;
            if (_closeButton != null) _closeButton.Pressed += () => Visible = false;

            this.RegisterEvent<ShelfChangedEvent>(OnShelfChanged)
                .UnRegisterWhenNodeExitTree(this);
            this.RegisterEvent<PickFailedEvent>(OnPickFailed)
                .UnRegisterWhenNodeExitTree(this);
            this.RegisterEvent<RoundEndedEvent>(_ => Visible = false)
                .UnRegisterWhenNodeExitTree(this);
        }

        public void OpenShelf(int shelfId)
        {
            _currentShelfId = shelfId;
            Visible = true;
            Rebuild();
        }

        public void ToggleShelf(int shelfId)
        {
            if (Visible && _currentShelfId == shelfId)
            {
                Visible = false;
                return;
            }

            OpenShelf(shelfId);
        }

        private void OnShelfChanged(ShelfChangedEvent e)
        {
            if (e.ShelfId == _currentShelfId) Rebuild();
        }

        private void OnPickFailed(PickFailedEvent e)
        {
            if (e.ShelfId != _currentShelfId) return;
            // Rebuild first (it resets _statusLabel.Text to empty), then surface reason.
            Rebuild();
            if (_statusLabel != null) _statusLabel.Text = e.Reason;
        }

        private void Rebuild()
        {
            if (_titleLabel != null) _titleLabel.Text = $"货架 {_currentShelfId}";
            if (_statusLabel != null) _statusLabel.Text = string.Empty;
            if (_itemList == null) return;

            foreach (var child in _itemList.GetChildren())
            {
                _itemList.RemoveChild(child);
                child.QueueFree();
            }

            var shelf = this.GetModel<ShelfModel>();
            if (shelf == null || !shelf.Inventories.TryGetValue(_currentShelfId, out var items) || items.Count == 0)
            {
                _itemList.AddChild(new Label { Text = "已空" });
                return;
            }

            foreach (var item in items)
            {
                var button = new Button
                {
                    Text = $"{item.Product.Name}  价值 {item.Product.Value}  {item.Product.Slots}格  重量 {item.Product.Weight}",
                    CustomMinimumSize = new Vector2(280, 32),
                };

                int shelfId = _currentShelfId;
                int instanceId = item.InstanceId;
                button.Pressed += () => PickItem(button, shelfId, instanceId);
                _itemList.AddChild(button);
            }
        }

        private void PickItem(Button button, int shelfId, int instanceId)
        {
            button.Disabled = true;
            this.SendCommand(new PickProductCommand(shelfId, instanceId));
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
