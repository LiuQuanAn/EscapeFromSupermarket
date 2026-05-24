using System.Text;
using EscapeFromSupermarket.Architecture;
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
        private Button _restartButton;

        public override void _Ready()
        {
            _titleLabel = GetNodeOrNull<Label>("Margin/Content/TitleLabel");
            _reasonLabel = GetNodeOrNull<Label>("Margin/Content/ReasonLabel");
            _itemsLabel = GetNodeOrNull<Label>("Margin/Content/ItemsLabel");
            _restartButton = GetNodeOrNull<Button>("Margin/Content/RestartButton");

            Visible = false;
            if (_restartButton != null) _restartButton.Pressed += Restart;

            this.RegisterEvent<RoundEndedEvent>(OnRoundEnded)
                .UnRegisterWhenNodeExitTree(this);
        }

        private void OnRoundEnded(RoundEndedEvent e)
        {
            var gameState = this.GetModel<GameStateModel>();
            var cart = this.GetModel<CartModel>();

            Visible = true;
            if (_titleLabel != null) _titleLabel.Text = e.Result == RoundResult.Won ? "撤离成功" : "撤离失败";
            if (_reasonLabel != null) _reasonLabel.Text = BuildReason(gameState);
            if (_itemsLabel != null) _itemsLabel.Text = BuildItemSummary(cart);
        }

        private static string BuildReason(GameStateModel gameState)
        {
            if (gameState == null) return string.Empty;
            if (gameState.Win.Value == WinReason.Extracted) return "原因：到达撤离区";
            if (gameState.Loss.Value == LossReason.Timeout) return "原因：关店倒计时归零";
            if (gameState.Loss.Value == LossReason.Caught) return "原因：被保安抓住";
            return string.Empty;
        }

        private static string BuildItemSummary(CartModel cart)
        {
            if (cart == null || cart.Items.Count == 0) return "购物车为空\n总价值：0";

            var sb = new StringBuilder();
            sb.AppendLine("购物车：");
            foreach (var item in cart.Items)
            {
                sb.AppendLine($"- {item.Product.Name}  价值 {item.Product.Value}");
            }
            sb.Append($"总价值：{cart.CurrentValue.Value}");
            return sb.ToString();
        }

        private void Restart()
        {
            Supermarket.Reset();
            GetTree().ReloadCurrentScene();
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
