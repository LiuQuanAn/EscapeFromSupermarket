using System;
using System.Collections.Generic;
using System.Threading;
using EscapeFromSupermarket.Utilities;
using QFramework;


namespace EscapeFromSupermarket.Models
{
    public sealed record ShelfItemInstance(int InstanceId, Product Product);

    public class ShelfModel : AbstractModel
    {
        private int _nextInstanceId;

        public Dictionary<int, List<ShelfItemInstance>> Inventories { get; } = new();

        protected override void OnInit()
        {
            var catalog = this.GetUtility<ProductCatalog>();
            if (catalog == null) return;

            FillShelf(1, catalog.GetByCategory("零食"), 4);
            FillShelf(2, catalog.GetByCategory("零食"), 4);
            FillShelf(3, catalog.GetByCategory("日用品"), 4);
            FillShelf(4, catalog.GetByCategory("家电"), 3);
        }

        public ShelfItemInstance FindItem(int shelfId, int instanceId)
        {
            if (!Inventories.TryGetValue(shelfId, out var items)) return null;
            return items.Find(i => i.InstanceId == instanceId);
        }

        public bool RemoveItem(int shelfId, int instanceId, out ShelfItemInstance item)
        {
            item = null;
            if (!Inventories.TryGetValue(shelfId, out var items)) return false;

            int index = items.FindIndex(i => i.InstanceId == instanceId);
            if (index < 0) return false;

            item = items[index];
            items.RemoveAt(index);
            return true;
        }

        private void FillShelf(int shelfId, IReadOnlyList<Product> products, int count)
        {
            var items = new List<ShelfItemInstance>();
            if (products.Count == 0)
            {
                Inventories[shelfId] = items;
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var product = products[Random.Shared.Next(products.Count)];
                items.Add(new ShelfItemInstance(Interlocked.Increment(ref _nextInstanceId), product));
            }

            Inventories[shelfId] = items;
        }
    }
}
