using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class CollectKeycardCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var gameState = this.GetModel<GameStateModel>();
            if (gameState.State.Value != RoundResult.Running) return;

            var objective = this.GetModel<RoundObjectiveModel>();
            objective.HasKeycard.Value = true;
        }
    }
}
