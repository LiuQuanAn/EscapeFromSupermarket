using System;
using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Commands;
using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Events;
using EscapeFromSupermarket.Models;
using EscapeFromSupermarket.Utilities;
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
        private ShelfModel _shelf;
        private GameStateModel _gameState;
        private PrototypeBalance _balance;
        private PlayerController _player;
        private ShelfController _currentShelf;
        private int _currentShelfId;
        private int _identifyingInstanceId;
        private float _identificationElapsed;
        private float _identificationDuration;
        private Label _identificationLabel;
        private ProgressBar _identificationBar;

        public override void _Ready()
        {
            _titleLabel = GetNodeOrNull<Label>("Margin/Content/TitleLabel");
            _statusLabel = GetNodeOrNull<Label>("Margin/Content/StatusLabel");
            _itemList = GetNodeOrNull<VBoxContainer>("Margin/Content/ItemsScroll/ItemList")
                ?? GetNodeOrNull<VBoxContainer>("Margin/Content/ItemList");
            _closeButton = GetNodeOrNull<Button>("Margin/Content/CloseButton");

            _shelf = this.GetModel<ShelfModel>();
            _gameState = this.GetModel<GameStateModel>();
            _balance = this.GetUtility<PrototypeBalance>();
            _player = GetTree().CurrentScene.GetNode<PlayerController>("Player");

            Visible = false;
            if (_closeButton != null) _closeButton.Pressed += CloseShelf;

            this.RegisterEvent<ShelfChangedEvent>(OnShelfChanged)
                .UnRegisterWhenNodeExitTree(this);
            this.RegisterEvent<PickFailedEvent>(OnPickFailed)
                .UnRegisterWhenNodeExitTree(this);
            this.RegisterEvent<RoundEndedEvent>(_ => CloseShelf())
                .UnRegisterWhenNodeExitTree(this);
        }

        public override void _Process(double delta)
        {
            if (!Visible) return;

            if (_gameState.State.Value != RoundResult.Running || PlayerLeftCurrentShelf())
            {
                CloseShelf();
                return;
            }

            UpdateIdentification((float)delta);
        }

        public void OpenShelf(ShelfController shelfController)
        {
            _currentShelf = shelfController;
            _currentShelfId = shelfController.ShelfId;
            StopIdentification();
            Visible = true;
            Rebuild();
            BeginNextIdentification();
        }

        public void ToggleShelf(ShelfController shelfController)
        {
            if (Visible && _currentShelfId == shelfController.ShelfId)
            {
                CloseShelf();
                return;
            }

            OpenShelf(shelfController);
        }

        private bool PlayerLeftCurrentShelf()
        {
            if (_currentShelf == null) return false;
            return !_currentShelf.IsPlayerInInteractionArea(_player);
        }

        private void CloseShelf()
        {
            StopIdentification();
            Visible = false;
        }

        private void StopIdentification()
        {
            _identifyingInstanceId = 0;
            _identificationElapsed = 0.0f;
            _identificationDuration = 0.0f;
            _identificationLabel = null;
            _identificationBar = null;
        }

        private void OnShelfChanged(ShelfChangedEvent e)
        {
            if (Visible && e.ShelfId == _currentShelfId) Rebuild();
        }

        private void OnPickFailed(PickFailedEvent e)
        {
            if (e.ShelfId != _currentShelfId) return;
            Rebuild();
            if (_statusLabel != null) _statusLabel.Text = e.Reason;
        }

        private void UpdateIdentification(float delta)
        {
            if (_identifyingInstanceId == 0)
            {
                BeginNextIdentification();
                return;
            }

            if (_shelf.FindItem(_currentShelfId, _identifyingInstanceId) == null)
            {
                StopIdentification();
                Rebuild();
                BeginNextIdentification();
                return;
            }

            _identificationElapsed += delta;
            if (_identificationElapsed >= _identificationDuration)
            {
                int completedInstanceId = _identifyingInstanceId;
                StopIdentification();
                this.SendCommand(new IdentifyShelfItemCommand(_currentShelfId, completedInstanceId));
                BeginNextIdentification();
                return;
            }

            RefreshIdentificationProgress();
        }

        private void BeginNextIdentification()
        {
            var item = _shelf.FindNextUnidentifiedItem(_currentShelfId);
            if (item == null)
            {
                return;
            }

            _identifyingInstanceId = item.InstanceId;
            _identificationElapsed = 0.0f;
            _identificationDuration = _balance.GetIdentificationSeconds(item.Product);
            Rebuild();
        }

        private void RefreshIdentificationProgress()
        {
            if (_identificationDuration <= 0.0f) return;

            float remaining = Mathf.Max(0.0f, _identificationDuration - _identificationElapsed);
            if (_identificationLabel != null)
            {
                _identificationLabel.Text = $"识别中 {remaining:0.0}s";
            }

            if (_identificationBar != null)
            {
                _identificationBar.Value = Mathf.Clamp(_identificationElapsed / _identificationDuration * 100.0f, 0.0f, 100.0f);
            }
        }

        private void Rebuild()
        {
            if (_titleLabel != null) _titleLabel.Text = $"货架 {_currentShelfId}";
            if (_statusLabel != null) _statusLabel.Text = string.Empty;
            if (_itemList == null) return;

            _identificationLabel = null;
            _identificationBar = null;

            foreach (var child in _itemList.GetChildren())
            {
                _itemList.RemoveChild(child);
                child.QueueFree();
            }

            if (!_shelf.Inventories.TryGetValue(_currentShelfId, out var items) || items.Count == 0)
            {
                _itemList.AddChild(new Label { Text = "已空" });
                return;
            }

            foreach (var item in items)
            {
                AddItemRow(item);
            }

            RefreshIdentificationProgress();
        }

        private void AddItemRow(ShelfItemInstance item)
        {
            if (_shelf.IsItemIdentified(item.InstanceId))
            {
                AddIdentifiedItemRow(item);
                return;
            }

            AddUnknownItemRow(item);
        }

        private void AddIdentifiedItemRow(ShelfItemInstance item)
        {
            var button = new Button
            {
                Text = BuildIdentifiedText(item.Product),
                CustomMinimumSize = new Vector2(280, 32),
            };

            int shelfId = _currentShelfId;
            int instanceId = item.InstanceId;
            button.Pressed += () => PickItem(button, shelfId, instanceId);
            _itemList.AddChild(button);
        }

        private void AddUnknownItemRow(ShelfItemInstance item)
        {
            var row = new VBoxContainer
            {
                CustomMinimumSize = item.InstanceId == _identifyingInstanceId
                    ? new Vector2(280, 58)
                    : new Vector2(280, 32),
            };

            row.AddChild(new Label
            {
                Text = "未知物品",
                CustomMinimumSize = new Vector2(280, 24),
            });

            if (item.InstanceId == _identifyingInstanceId)
            {
                _identificationLabel = new Label { CustomMinimumSize = new Vector2(280, 20) };
                _identificationBar = new ProgressBar
                {
                    MinValue = 0.0f,
                    MaxValue = 100.0f,
                    ShowPercentage = false,
                    CustomMinimumSize = new Vector2(280, 12),
                };
                row.AddChild(_identificationLabel);
                row.AddChild(_identificationBar);
            }

            _itemList.AddChild(row);
        }

        private static string BuildIdentifiedText(Product product)
        {
            string marker = string.IsNullOrEmpty(product.TaskKey) ? string.Empty : "  任务";
            string rarity = product.Rarity switch
            {
                ProductRarity.Common => string.Empty,
                ProductRarity.Rare => "  稀有",
                ProductRarity.HighRare => "  高稀有",
                _ => throw new ArgumentOutOfRangeException(nameof(product.Rarity), product.Rarity, "Unknown product rarity."),
            };

            return $"{product.Name}  价值 {product.Value}  {product.Slots}格  重量 {product.Weight}{rarity}{marker}";
        }

        private void PickItem(Button button, int shelfId, int instanceId)
        {
            button.Disabled = true;
            this.SendCommand(new PickProductCommand(shelfId, instanceId));
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
