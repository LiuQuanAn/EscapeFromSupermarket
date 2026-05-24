using EscapeFromSupermarket.Config;
using QFramework;

namespace EscapeFromSupermarket.Models
{
    public enum RoundResult { Running, Won, Lost }
    public enum LossReason { Timeout, Caught }
    public enum WinReason { Extracted }
    public enum ExtractionExitType { FrontDoor, StaffDoor }

    public class GameStateModel : AbstractModel
    {
        public BindableProperty<RoundResult> State { get; } = new(RoundResult.Running);
        public BindableProperty<LossReason?> Loss { get; } = new(null);
        public BindableProperty<WinReason?> Win { get; } = new(null);
        public BindableProperty<float> Countdown { get; } = new(PrototypeBalance.Default.RoundSeconds);
        public BindableProperty<float> ExtractionProgress { get; } = new(0.0f);
        public BindableProperty<float> CurrentExtractionRequiredSeconds { get; } = new(PrototypeBalance.Default.FrontDoorExtractionSeconds);
        public BindableProperty<bool> IsExtracting { get; } = new(false);

        private PrototypeBalance _balance = PrototypeBalance.Default;

        protected override void OnInit()
        {
            _balance = this.GetUtility<PrototypeBalance>();
            ResetRound();
        }

        public void ResetRound()
        {
            State.Value = RoundResult.Running;
            Loss.Value = null;
            Win.Value = null;
            Countdown.Value = _balance.RoundSeconds;
            ExtractionProgress.Value = 0.0f;
            CurrentExtractionRequiredSeconds.Value = _balance.FrontDoorExtractionSeconds;
            IsExtracting.Value = false;
        }
    }
}
