using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Utilities;
using QFramework;


namespace EscapeFromSupermarket.Models
{
    public sealed record ShelfItemInstance(int InstanceId, Product Product);

    public class ShelfModel : AbstractModel
    {
        private int _nextInstanceId;
        private ProductCatalog _catalog;
        private PrototypeBalance _balance = PrototypeBalance.Default;

        public Dictionary<int, List<ShelfItemInstance>> Inventories { get; } = new();

        protected override void OnInit()
        {
            _catalog = this.GetUtility<ProductCatalog>();
            _balance = this.GetUtility<PrototypeBalance>();
            RefreshRound();
        }

        public void RefreshRound()
        {
            Inventories.Clear();
            _nextInstanceId = 0;
            if (_catalog == null) return;

            foreach (var shelf in _balance.Shelves)
            {
                var count = Random.Shared.Next(shelf.MinItemCount, shelf.MaxItemCount + 1);
                FillShelf(shelf.ShelfId, _catalog.GetByCategory(shelf.Category), count);
            }

            EnsureProductAppears(PrototypeBalance.RouterTaskKey);
            EnsureHighValueProductAppears();
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
                var product = PickLowRepeatProduct(products, items);
                items.Add(new ShelfItemInstance(Interlocked.Increment(ref _nextInstanceId), product));
            }

            Inventories[shelfId] = items;
        }

        private static Product PickLowRepeatProduct(IReadOnlyList<Product> products, List<ShelfItemInstance> existing)
        {
            if (products.Count == 1) return products[0];

            int attempts = products.Count * 2;
            for (int i = 0; i < attempts; i++)
            {
                var candidate = products[Random.Shared.Next(products.Count)];
                if (existing.TrueForAll(x => x.Product.ProductTypeId != candidate.ProductTypeId))
                {
                    return candidate;
                }
            }

            return products[Random.Shared.Next(products.Count)];
        }

        private void EnsureProductAppears(string productTypeId)
        {
            if (Inventories.Values.Any(items => items.Exists(item => item.Product.ProductTypeId == productTypeId))) return;

            var product = _catalog.FindByTypeId(productTypeId);
            if (product == null) return;
            ReplaceOrAppendProduct(product);
        }

        private void EnsureHighValueProductAppears()
        {
            if (Inventories.Values.Any(items => items.Exists(item => item.Product.Value >= _balance.HighValueProductMinValue))) return;

            var product = _catalog.All
                .Where(x => x.Value >= _balance.HighValueProductMinValue)
                .OrderByDescending(x => x.Value)
                .FirstOrDefault();
            if (product != null) ReplaceOrAppendProduct(product);
        }

        private void ReplaceOrAppendProduct(Product product)
        {
            var rule = _balance.Shelves.FirstOrDefault(x => x.Category == product.Category);
            if (rule == null) return;

            if (!Inventories.TryGetValue(rule.ShelfId, out var items))
            {
                items = new List<ShelfItemInstance>();
                Inventories[rule.ShelfId] = items;
            }

            if (items.Count >= rule.MaxItemCount)
            {
                var replaceIndex = Random.Shared.Next(items.Count);
                items[replaceIndex] = new ShelfItemInstance(Interlocked.Increment(ref _nextInstanceId), product);
            }
            else
            {
                items.Add(new ShelfItemInstance(Interlocked.Increment(ref _nextInstanceId), product));
            }
        }
    }
}
