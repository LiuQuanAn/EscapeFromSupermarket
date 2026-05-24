using QFramework;

namespace EscapeFromSupermarket.Models
{
    public enum RoundResult { Running, Won, Lost }
    public enum LossReason { Timeout, Caught }
    public enum WinReason { Extracted }

    public class GameStateModel : AbstractModel
    {
        public BindableProperty<RoundResult> State { get; } = new(RoundResult.Running);
        public BindableProperty<LossReason?> Loss { get; } = new(null);
        public BindableProperty<WinReason?> Win { get; } = new(null);
        public BindableProperty<float> Countdown { get; } = new(240.0f);
        public BindableProperty<float> ExtractionProgress { get; } = new(0.0f);
        public BindableProperty<bool> IsExtracting { get; } = new(false);

        protected override void OnInit()
        {
        }
    }
}
