using EscapeFromSupermarket.Events;
using EscapeFromSupermarket.Models;
using EscapeFromSupermarket.Queries;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class PickProductCommand : AbstractCommand
    {
        private readonly int _shelfId;
        private readonly int _instanceId;

        public PickProductCommand(int shelfId, int instanceId)
        {
            _shelfId = shelfId;
            _instanceId = instanceId;
        }

        protected override void OnExecute()
        {
            var result = this.SendQuery(new CanPickProductQuery(_shelfId, _instanceId));
            if (!result.Ok)
            {
                this.SendEvent(new PickFailedEvent(_shelfId, _instanceId, result.Reason));
                return;
            }

            var shelf = this.GetModel<ShelfModel>();
            if (!shelf.RemoveItem(_shelfId, _instanceId, out var item))
            {
                this.SendEvent(new PickFailedEvent(_shelfId, _instanceId, "该商品已经不在货架上。"));
                return;
            }

            var cart = this.GetModel<CartModel>();
            cart.AddItem(new CartItem(item.InstanceId, item.Product));

            this.SendEvent(new ShelfChangedEvent(_shelfId));
            this.SendEvent(new CartItemsChangedEvent());
        }
    }
}
