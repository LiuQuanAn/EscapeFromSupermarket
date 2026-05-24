using EscapeFromSupermarket.Events;
using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class DropProductCommand : AbstractCommand
    {
        private readonly int _instanceId;

        public DropProductCommand(int instanceId)
        {
            _instanceId = instanceId;
        }

        protected override void OnExecute()
        {
            var cart = this.GetModel<CartModel>();
            if (!cart.TryRemoveItem(_instanceId, out _)) return;

            this.SendEvent(new CartItemsChangedEvent());
        }
    }
}
