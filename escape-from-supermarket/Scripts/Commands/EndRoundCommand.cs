using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Events;
using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class EndRoundCommand : AbstractCommand
    {
        private readonly RoundResult _result;
        private readonly LossReason? _lossReason;
        private readonly WinReason? _winReason;

        private EndRoundCommand(RoundResult result, LossReason? lossReason, WinReason? winReason)
        {
            _result = result;
            _lossReason = lossReason;
            _winReason = winReason;
        }

        public static EndRoundCommand Lose(LossReason reason)
        {
            return new EndRoundCommand(RoundResult.Lost, reason, null);
        }

        public static EndRoundCommand Win(WinReason reason)
        {
            return new EndRoundCommand(RoundResult.Won, null, reason);
        }

        protected override void OnExecute()
        {
            var gameState = this.GetModel<GameStateModel>();
            if (gameState.State.Value != RoundResult.Running) return;

            if (_result == RoundResult.Won)
            {
                ApplyWinRewards();
            }

            gameState.Loss.Value = _lossReason;
            gameState.Win.Value = _winReason;
            gameState.State.Value = _result;
            this.SendEvent(new RoundEndedEvent(_result));
        }

        private void ApplyWinRewards()
        {
            var cart = this.GetModel<CartModel>();
            var meta = this.GetModel<MetaProgressModel>();
            var objective = this.GetModel<RoundObjectiveModel>();

            meta.Money.Value += cart.CurrentValue.Value;

            bool routerExtracted = cart.ContainsTaskItem(PrototypeBalance.RouterTaskKey);
            objective.RouterExtractedThisRound.Value = routerExtracted;
            if (routerExtracted)
            {
                meta.NavigationProgress.Value++;
            }
        }
    }
}
