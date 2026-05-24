using EscapeFromSupermarket.Commands;
using EscapeFromSupermarket.Core;
using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Systems
{
    public class ExtractionSystem : AbstractSystem, ITickable
    {
        private GameStateModel _gameState;

        protected override void OnInit()
        {
            _gameState = this.GetModel<GameStateModel>();
        }

        public void Tick(double delta)
        {
            if (_gameState == null || _gameState.State.Value != RoundResult.Running) return;
            if (!_gameState.IsExtracting.Value) return;

            _gameState.ExtractionProgress.Value += (float)delta;
            if (_gameState.ExtractionProgress.Value >= _gameState.CurrentExtractionRequiredSeconds.Value)
            {
                _gameState.IsExtracting.Value = false;
                this.SendCommand(EndRoundCommand.Win(WinReason.Extracted));
            }
        }
    }
}
