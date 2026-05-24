using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class CancelExtractionCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var gameState = this.GetModel<GameStateModel>();
            gameState.IsExtracting.Value = false;
            if (gameState.State.Value == RoundResult.Running)
            {
                gameState.ExtractionProgress.Value = 0.0f;
            }
        }
    }
}
