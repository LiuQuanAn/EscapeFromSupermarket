using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class StartExtractionCommand : AbstractCommand
    {
        private readonly ExtractionExitType _exitType;

        public StartExtractionCommand(ExtractionExitType exitType)
        {
            _exitType = exitType;
        }

        protected override void OnExecute()
        {
            var gameState = this.GetModel<GameStateModel>();
            if (gameState.State.Value != RoundResult.Running) return;

            var objective = this.GetModel<RoundObjectiveModel>();
            if (_exitType == ExtractionExitType.StaffDoor && !objective.HasKeycard.Value)
            {
                gameState.IsExtracting.Value = false;
                gameState.ExtractionProgress.Value = 0.0f;
                return;
            }

            var balance = this.GetUtility<PrototypeBalance>();
            gameState.CurrentExtractionRequiredSeconds.Value = balance.GetExtractionSeconds(_exitType);
            gameState.IsExtracting.Value = true;
        }
    }
}
