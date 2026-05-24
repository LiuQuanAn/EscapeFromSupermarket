using EscapeFromSupermarket.Events;
using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class StartNextRoundCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<CartModel>().ResetRound();
            this.GetModel<ShelfModel>().RefreshRound();
            this.GetModel<GameStateModel>().ResetRound();
            this.GetModel<RoundObjectiveModel>().ResetRound();
            this.GetModel<GuardModel>().ResetRound();

            this.SendEvent(new CartItemsChangedEvent());
        }
    }
}
