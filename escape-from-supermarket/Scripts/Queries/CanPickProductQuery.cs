using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Queries
{
    public sealed record CanPickProductResult(bool Ok, string Reason);

    public class CanPickProductQuery : AbstractQuery<CanPickProductResult>
    {
        private readonly int _shelfId;
        private readonly int _instanceId;

        public CanPickProductQuery(int shelfId, int instanceId)
        {
            _shelfId = shelfId;
            _instanceId = instanceId;
        }

        protected override CanPickProductResult OnDo()
        {
            var shelf = this.GetModel<ShelfModel>();
            var cart = this.GetModel<CartModel>();
            var meta = this.GetModel<MetaProgressModel>();
            var item = shelf?.FindItem(_shelfId, _instanceId);
            if (item == null) return new CanPickProductResult(false, "该商品已经不在货架上。");

            int capacity = cart.GetCapacity(meta?.CartCapacityLevel.Value ?? 0);
            if (cart.CurrentSlots.Value + item.Product.Slots > capacity)
            {
                return new CanPickProductResult(false, "购物车格子不够。");
            }

            if (cart.CurrentWeight.Value + item.Product.Weight > cart.WeightLimit)
            {
                return new CanPickProductResult(false, "购物车太重。");
            }

            return new CanPickProductResult(true, string.Empty);
        }
    }
}
