using QFramework;

namespace EscapeFromSupermarket.Models
{
    public enum GuardState { Patrolling, Chasing }

    public class GuardModel : AbstractModel
    {
        public BindableProperty<float> Alert { get; } = new(0.0f);
        public BindableProperty<GuardState> State { get; } = new(GuardState.Patrolling);

        protected override void OnInit()
        {
        }

        public void ResetRound()
        {
            Alert.Value = 0.0f;
            State.Value = GuardState.Patrolling;
        }
    }
}
