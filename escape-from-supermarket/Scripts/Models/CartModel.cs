using System.Collections.Generic;
using QFramework;

namespace EscapeFromSupermarket.Models
{
    public enum CartLoadTier { Empty, Mid, Heavy }

    /// <summary>
    /// Single cart entry. ProductTypeId references ProductCatalog (Step 3 onward);
    /// for Step 1 the list is empty. InstanceId is the stable identifier minted
    /// by ShelfModel — used by DropProductCommand to locate the exact item.
    /// </summary>
    public sealed record CartItem(int InstanceId, string ProductTypeId, int Slots, int Weight, int Value);

    public class CartModel : AbstractModel
    {
        public const int Capacity = 10;
        public const int WeightLimit = 30;

        public BindableProperty<int> CurrentSlots { get; } = new(0);
        public BindableProperty<int> CurrentWeight { get; } = new(0);
        public BindableProperty<int> CurrentValue { get; } = new(0);
        public BindableProperty<CartLoadTier> LoadTier { get; } = new(CartLoadTier.Empty);

        /// <summary>
        /// Stable record of items currently in the cart. Mutated only by
        /// PickProductCommand / DropProductCommand. Listeners subscribe via
        /// CartItemsChangedEvent (Step 3+), not by polling this list.
        /// </summary>
        public List<CartItem> Items { get; } = new();

        protected override void OnInit()
        {
            CurrentWeight.Register(w => LoadTier.Value = ComputeTier(w));
        }

        private static CartLoadTier ComputeTier(int weight)
        {
            if (weight < 10) return CartLoadTier.Empty;
            if (weight < 22) return CartLoadTier.Mid;
            return CartLoadTier.Heavy;
        }
    }
}
