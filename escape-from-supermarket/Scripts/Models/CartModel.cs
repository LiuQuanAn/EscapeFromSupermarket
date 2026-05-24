using System.Collections.Generic;
using EscapeFromSupermarket.Utilities;
using QFramework;

namespace EscapeFromSupermarket.Models
{
    public enum CartLoadTier { Empty, Mid, Heavy }

    /// <summary>
    /// Single cart entry. InstanceId is the stable identifier minted by
    /// ShelfModel — used by DropProductCommand to locate the exact item.
    /// </summary>
    public sealed record CartItem(int InstanceId, Product Product);

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
        /// PickProductCommand / DropProductCommand via AddItem / TryRemoveItem.
        /// Listeners subscribe via CartItemsChangedEvent, not by polling.
        /// </summary>
        public List<CartItem> Items { get; } = new();

        protected override void OnInit()
        {
        }

        /// <summary>
        /// Append an item to the cart and refresh aggregate counters + tier in
        /// a single atomic batch. Commands must use this instead of mutating
        /// the BindableProperties directly so LoadTier stays in sync with
        /// CurrentWeight without a separate subscription.
        /// </summary>
        public void AddItem(CartItem item)
        {
            Items.Add(item);
            CurrentSlots.Value += item.Product.Slots;
            CurrentWeight.Value += item.Product.Weight;
            CurrentValue.Value += item.Product.Value;
            LoadTier.Value = ComputeTier(CurrentWeight.Value);
        }

        /// <summary>
        /// Remove the item identified by <paramref name="instanceId"/> and
        /// roll back the aggregate counters. Returns false (and leaves the
        /// cart untouched) when the id is unknown — caller may treat that as
        /// a no-op (idempotent drop / duplicate click).
        /// </summary>
        public bool TryRemoveItem(int instanceId, out CartItem removed)
        {
            int index = Items.FindIndex(x => x.InstanceId == instanceId);
            if (index < 0)
            {
                removed = null;
                return false;
            }

            removed = Items[index];
            Items.RemoveAt(index);
            CurrentSlots.Value -= removed.Product.Slots;
            CurrentWeight.Value -= removed.Product.Weight;
            CurrentValue.Value -= removed.Product.Value;
            LoadTier.Value = ComputeTier(CurrentWeight.Value);
            return true;
        }

        private static CartLoadTier ComputeTier(int weight)
        {
            if (weight < 10) return CartLoadTier.Empty;
            if (weight < 22) return CartLoadTier.Mid;
            return CartLoadTier.Heavy;
        }
    }
}
