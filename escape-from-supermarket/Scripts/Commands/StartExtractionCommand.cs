using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class StartExtractionCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var gameState = this.GetModel<GameStateModel>();
            if (gameState.State.Value != RoundResult.Running) return;
            gameState.IsExtracting.Value = true;
        }
    }
}
