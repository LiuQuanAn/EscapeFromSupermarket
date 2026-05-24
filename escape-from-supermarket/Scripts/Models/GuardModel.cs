using Godot;
using QFramework;

namespace EscapeFromSupermarket.Models
{
    public enum GuardState { Patrolling, Chasing }

    public class GuardModel : AbstractModel
    {
        public Vector3[] PatrolPath { get; private set; }
        public BindableProperty<float> Alert { get; } = new(0.0f);
        public BindableProperty<GuardState> State { get; } = new(GuardState.Patrolling);

        protected override void OnInit()
        {
            PatrolPath = new[]
            {
                new Vector3(0, 0.8f, 10),
                new Vector3(0, 0.8f, -10),
                new Vector3(7, 0.8f, -10),
                new Vector3(7, 0.8f, 10),
            };
        }
    }
}
