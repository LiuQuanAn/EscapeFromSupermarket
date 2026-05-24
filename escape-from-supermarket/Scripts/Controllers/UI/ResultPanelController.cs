using System.Text;
using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Commands;
using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Events;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers.UI
{
    public partial class ResultPanelController : Control, IController
    {
        private Label _titleLabel;
        private Label _reasonLabel;
        private Label _itemsLabel;
        private Label _metaLabel;
        private Button _capacityUpgradeButton;
        private Button _speedUpgradeButton;
        private Button _restartButton;
        private RoundResult _lastResult;

        public override void _Ready()
        {
            _titleLabel = GetNodeOrNull<Label>("Margin/Content/TitleLabel");
            _reasonLabel = GetNodeOrNull<Label>("Margin/Content/ReasonLabel");
            _itemsLabel = GetNodeOrNull<Label>("Margin/Content/ItemsScroll/ItemsLabel")
                ?? GetNodeOrNull<Label>("Margin/Content/ItemsLabel");
            _metaLabel = GetNodeOrNull<Label>("Margin/Content/MetaLabel");
            _capacityUpgradeButton = GetNodeOrNull<Button>("Margin/Content/CapacityUpgradeButton");
            _speedUpgradeButton = GetNodeOrNull<Button>("Margin/Content/SpeedUpgradeButton");
            _restartButton = GetNodeOrNull<Button>("Margin/Content/RestartButton");

            Visible = false;
            if (_capacityUpgradeButton != null) _capacityUpgradeButton.Pressed += () => BuyUpgrade(UpgradeType.CartCapacity);
            if (_speedUpgradeButton != null) _speedUpgradeButton.Pressed += () => BuyUpgrade(UpgradeType.PlayerSpeed);
            if (_restartButton != null) _restartButton.Pressed += StartNextRound;

            this.RegisterEvent<RoundEndedEvent>(OnRoundEnded)
                .UnRegisterWhenNodeExitTree(this);
        }

        private void OnRoundEnded(RoundEndedEvent e)
        {
            _lastResult = e.Result;
            Refresh();
        }

        private void Refresh()
        {
            var gameState = this.GetModel<GameStateModel>();
            var cart = this.GetModel<CartModel>();
            var meta = this.GetModel<MetaProgressModel>();
            var objective = this.GetModel<RoundObjectiveModel>();

            Visible = true;
            MoveToFront();
            if (_titleLabel != null) _titleLabel.Text = _lastResult == RoundResult.Won ? "撤离成功" : "撤离失败";
            if (_reasonLabel != null) _reasonLabel.Text = BuildReason(gameState);
            if (_itemsLabel != null) _itemsLabel.Text = BuildItemSummary(cart, objective);
            if (_metaLabel != null) _metaLabel.Text = BuildMetaSummary(meta);

            UpdateUpgradeButton(_capacityUpgradeButton, UpgradeType.CartCapacity, "购物车容量", meta);
            UpdateUpgradeButton(_speedUpgradeButton, UpgradeType.PlayerSpeed, "基础速度", meta);
        }

        private static string BuildReason(GameStateModel gameState)
        {
            if (gameState == null) return string.Empty;
            if (gameState.Win.Value == WinReason.Extracted) return "原因：到达撤离区";
            if (gameState.Loss.Value == LossReason.Timeout) return "原因：关店倒计时归零";
            if (gameState.Loss.Value == LossReason.Caught) return "原因：被保安抓住";
            return string.Empty;
        }

        private static string BuildItemSummary(CartModel cart, RoundObjectiveModel objective)
        {
            if (cart == null || cart.Items.Count == 0)
            {
                return BuildRouterLine(objective) + "\n购物车为空\n总价值：0";
            }

            var sb = new StringBuilder();
            sb.AppendLine(BuildRouterLine(objective));
            sb.AppendLine("购物车：");
            foreach (var item in cart.Items)
            {
                sb.AppendLine($"- {item.Product.Name}  价值 {item.Product.Value}");
            }
            sb.Append($"总价值：{cart.CurrentValue.Value}");
            return sb.ToString();
        }

        private static string BuildRouterLine(RoundObjectiveModel objective)
        {
            return objective?.RouterExtractedThisRound.Value == true
                ? "导航修复进度 +1"
                : "导航修复进度 +0";
        }

        private static string BuildMetaSummary(MetaProgressModel meta)
        {
            if (meta == null) return string.Empty;
            return $"钱：{meta.Money.Value}\n导航修复进度：{meta.NavigationProgress.Value}";
        }

        private void UpdateUpgradeButton(Button button, UpgradeType upgradeType, string label, MetaProgressModel meta)
        {
            if (button == null || meta == null) return;

            var balance = this.GetUtility<PrototypeBalance>();
            int level = meta.GetLevel(upgradeType);
            int price = balance.GetUpgradePrice(upgradeType, level);
            button.Text = $"{label} Lv.{level}  价格 {price}";
            button.Disabled = meta.Money.Value < price;
        }

        private void BuyUpgrade(UpgradeType upgradeType)
        {
            this.SendCommand(new BuyUpgradeCommand(upgradeType));
            Refresh();
        }

        private void StartNextRound()
        {
            // Models live in the architecture singleton; scene reload resets Area monitoring and controller-local state.
            this.SendCommand(new StartNextRoundCommand());
            GetTree().ReloadCurrentScene();
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
