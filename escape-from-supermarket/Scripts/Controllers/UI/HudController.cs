using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Events;
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
        private Label _objectiveLabel;
        private Label _keycardLabel;
        private ProgressBar _extractionBar;
        private ProgressBar _alertBar;
        private PrototypeBalance _balance = PrototypeBalance.Default;
        private CartModel _cart;
        private MetaProgressModel _metaProgress;
        private RoundObjectiveModel _objective;
        private GameStateModel _gameState;

        public override void _Ready()
        {
            _balance = this.GetUtility<PrototypeBalance>();
            _slotsLabel = GetNodeOrNull<Label>("HudPanel/Margin/Content/SlotsLabel");
            _weightLabel = GetNodeOrNull<Label>("HudPanel/Margin/Content/WeightLabel");
            _valueLabel = GetNodeOrNull<Label>("HudPanel/Margin/Content/ValueLabel");
            _countdownLabel = GetNodeOrNull<Label>("HudPanel/Margin/Content/CountdownLabel");
            _objectiveLabel = GetNodeOrNull<Label>("HudPanel/Margin/Content/ObjectiveLabel");
            _keycardLabel = GetNodeOrNull<Label>("HudPanel/Margin/Content/KeycardLabel");
            _extractionBar = GetNodeOrNull<ProgressBar>("HudPanel/Margin/Content/ExtractionBar");
            _alertBar = GetNodeOrNull<ProgressBar>("HudPanel/Margin/Content/AlertBar");

            if (_countdownLabel != null) _countdownLabel.Text = "倒计时：--:--";
            if (_extractionBar != null) _extractionBar.Visible = false;
            if (_alertBar != null) _alertBar.Visible = false;

            _cart = this.GetModel<CartModel>();
            if (_cart == null)
            {
                GD.PushError($"{nameof(HudController)} requires {nameof(CartModel)} to be registered before UI starts.");
                return;
            }

            _cart.CurrentSlots.RegisterWithInitValue(UpdateSlots)
                .UnRegisterWhenNodeExitTree(this);
            _cart.CurrentWeight.RegisterWithInitValue(UpdateWeight)
                .UnRegisterWhenNodeExitTree(this);
            _cart.CurrentValue.RegisterWithInitValue(UpdateValue)
                .UnRegisterWhenNodeExitTree(this);
            this.RegisterEvent<CartItemsChangedEvent>(_ => UpdateObjective())
                .UnRegisterWhenNodeExitTree(this);
            _metaProgress = this.GetModel<MetaProgressModel>();
            if (_metaProgress != null)
            {
                _metaProgress.CartCapacityLevel.RegisterWithInitValue(_ => UpdateSlots(_cart.CurrentSlots.Value))
                    .UnRegisterWhenNodeExitTree(this);
            }

            _gameState = this.GetModel<GameStateModel>();
            if (_gameState == null)
            {
                GD.PushError($"{nameof(HudController)} requires {nameof(GameStateModel)} to show countdown.");
                return;
            }

            _gameState.Countdown.RegisterWithInitValue(UpdateCountdown)
                .UnRegisterWhenNodeExitTree(this);
            _gameState.ExtractionProgress.RegisterWithInitValue(UpdateExtraction)
                .UnRegisterWhenNodeExitTree(this);
            _gameState.CurrentExtractionRequiredSeconds.RegisterWithInitValue(_ => UpdateExtraction(_gameState.ExtractionProgress.Value))
                .UnRegisterWhenNodeExitTree(this);

            _objective = this.GetModel<RoundObjectiveModel>();
            if (_objective != null)
            {
                _objective.HasKeycard.RegisterWithInitValue(UpdateKeycard)
                    .UnRegisterWhenNodeExitTree(this);
            }

            var guard = this.GetModel<GuardModel>();
            if (guard != null)
            {
                guard.Alert.RegisterWithInitValue(UpdateAlert)
                    .UnRegisterWhenNodeExitTree(this);
            }
        }

        private void UpdateSlots(int value)
        {
            int capacity = _cart.GetCapacity(_metaProgress?.CartCapacityLevel.Value ?? 0);
            if (_slotsLabel != null) _slotsLabel.Text = $"格子：{value}/{capacity}";
        }

        private void UpdateWeight(int value)
        {
            if (_weightLabel != null) _weightLabel.Text = $"重量：{value}/{_cart.WeightLimit}";
        }

        private void UpdateValue(int value)
        {
            if (_valueLabel != null) _valueLabel.Text = $"估值：{value}";
        }

        private void UpdateObjective()
        {
            bool hasRouter = _cart?.ContainsTaskItem(PrototypeBalance.RouterTaskKey) ?? false;
            if (_objectiveLabel != null) _objectiveLabel.Text = $"目标：带出路由器 {(hasRouter ? 1 : 0)}/1";
        }

        private void UpdateKeycard(bool hasKeycard)
        {
            if (_keycardLabel != null) _keycardLabel.Text = $"钥匙卡：{(hasKeycard ? "有" : "无")}";
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
            float requiredSeconds = _gameState.CurrentExtractionRequiredSeconds.Value;
            _extractionBar.Value = Mathf.Clamp(value / Mathf.Max(requiredSeconds, 0.001f), 0.0f, 1.0f);
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
