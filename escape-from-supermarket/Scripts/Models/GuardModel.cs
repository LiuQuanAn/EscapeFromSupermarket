using EscapeFromSupermarket.Config;
using Godot;
using QFramework;
using System.Linq;

namespace EscapeFromSupermarket.Models
{
    public enum GuardState { Patrolling, Chasing }

    public class GuardModel : AbstractModel
    {
        public Vector3[] PatrolPath { get; private set; }
        public BindableProperty<float> Alert { get; } = new(0.0f);
        public BindableProperty<GuardState> State { get; } = new(GuardState.Patrolling);
        private PrototypeBalance _balance = PrototypeBalance.Default;

        protected override void OnInit()
        {
            _balance = this.GetUtility<PrototypeBalance>();
            PatrolPath = _balance.GuardPatrolPath.ToArray();
        }

        public void ResetRound()
        {
            Alert.Value = 0.0f;
            State.Value = GuardState.Patrolling;
        }
    }
}
