using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class AdjustAlertCommand : AbstractCommand
    {
        private readonly float _delta;

        public AdjustAlertCommand(float delta)
        {
            _delta = delta;
        }

        protected override void OnExecute()
        {
            var gameState = this.GetModel<GameStateModel>();
            if (gameState.State.Value != RoundResult.Running) return;

            var guard = this.GetModel<GuardModel>();
            var nextAlert = Mathf.Clamp(guard.Alert.Value + _delta, 0.0f, 1.0f);
            guard.Alert.Value = nextAlert;

            if (nextAlert >= 1.0f && guard.State.Value == GuardState.Patrolling)
            {
                guard.State.Value = GuardState.Chasing;
            }
            else if (nextAlert <= 0.0f && guard.State.Value == GuardState.Chasing)
            {
                guard.State.Value = GuardState.Patrolling;
            }
        }
    }
}
