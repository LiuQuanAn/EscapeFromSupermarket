using QFramework;

namespace EscapeFromSupermarket.Models
{
    public class RoundObjectiveModel : AbstractModel
    {
        public BindableProperty<bool> HasKeycard { get; } = new(false);
        public BindableProperty<bool> RouterExtractedThisRound { get; } = new(false);

        protected override void OnInit()
        {
        }

        public void ResetRound()
        {
            HasKeycard.Value = false;
            RouterExtractedThisRound.Value = false;
        }
    }
}
