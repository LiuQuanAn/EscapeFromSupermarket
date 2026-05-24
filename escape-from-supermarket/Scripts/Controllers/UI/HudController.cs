using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers.UI
{
    public partial class HudController : CanvasLayer, IController
    {
        private Label _slotsLabel;
        private Label _weightLabel;
        private Label _valueLabel;
        private Label _countdownLabel;
        private ProgressBar _extractionBar;
        private ProgressBar _alertBar;

        public override void _Ready()
        {
            _slotsLabel = GetNodeOrNull<Label>("HudPanel/Margin/Content/SlotsLabel");
            _weightLabel = GetNodeOrNull<Label>("HudPanel/Margin/Content/WeightLabel");
            _valueLabel = GetNodeOrNull<Label>("HudPanel/Margin/Content/ValueLabel");
            _countdownLabel = GetNodeOrNull<Label>("HudPanel/Margin/Content/CountdownLabel");
            _extractionBar = GetNodeOrNull<ProgressBar>("HudPanel/Margin/Content/ExtractionBar");
            _alertBar = GetNodeOrNull<ProgressBar>("HudPanel/Margin/Content/AlertBar");

            if (_countdownLabel != null) _countdownLabel.Text = "倒计时：--:--";
            if (_extractionBar != null) _extractionBar.Visible = false;
            if (_alertBar != null) _alertBar.Visible = false;

            var cart = this.GetModel<CartModel>();
            if (cart == null)
            {
                GD.PushError($"{nameof(HudController)} requires {nameof(CartModel)} to be registered before UI starts.");
                return;
            }

            cart.CurrentSlots.RegisterWithInitValue(UpdateSlots)
                .UnRegisterWhenNodeExitTree(this);
            cart.CurrentWeight.RegisterWithInitValue(UpdateWeight)
                .UnRegisterWhenNodeExitTree(this);
            cart.CurrentValue.RegisterWithInitValue(UpdateValue)
                .UnRegisterWhenNodeExitTree(this);

            var gameState = this.GetModel<GameStateModel>();
            if (gameState == null)
            {
                GD.PushError($"{nameof(HudController)} requires {nameof(GameStateModel)} to show countdown.");
                return;
            }

            gameState.Countdown.RegisterWithInitValue(UpdateCountdown)
                .UnRegisterWhenNodeExitTree(this);
            gameState.ExtractionProgress.RegisterWithInitValue(UpdateExtraction)
                .UnRegisterWhenNodeExitTree(this);

            var guard = this.GetModel<GuardModel>();
            if (guard != null)
            {
                guard.Alert.RegisterWithInitValue(UpdateAlert)
                    .UnRegisterWhenNodeExitTree(this);
            }
        }

        private void UpdateSlots(int value)
        {
            if (_slotsLabel != null) _slotsLabel.Text = $"格子：{value}/{CartModel.Capacity}";
        }

        private void UpdateWeight(int value)
        {
            if (_weightLabel != null) _weightLabel.Text = $"重量：{value}/{CartModel.WeightLimit}";
        }

        private void UpdateValue(int value)
        {
            if (_valueLabel != null) _valueLabel.Text = $"估值：{value}";
        }

        private void UpdateCountdown(float value)
        {
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(value));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            if (_countdownLabel != null) _countdownLabel.Text = $"倒计时：{minutes:00}:{seconds:00}";
        }

        private void UpdateExtraction(float value)
        {
            if (_extractionBar == null) return;
            _extractionBar.Visible = value > 0.0f;
            _extractionBar.Value = Mathf.Clamp(value / 3.0f, 0.0f, 1.0f);
        }

        private void UpdateAlert(float value)
        {
            if (_alertBar == null) return;
            _alertBar.Visible = value > 0.0f;
            _alertBar.Value = Mathf.Clamp(value, 0.0f, 1.0f);
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
