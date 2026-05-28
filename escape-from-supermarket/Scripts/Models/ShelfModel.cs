using System;
using System.Collections.Generic;
using System.Threading;
using EscapeFromSupermarket.Utilities;
using QFramework;


namespace EscapeFromSupermarket.Models
{
    public sealed record ShelfItemInstance(int InstanceId, Product Product);
    public sealed record ShelfSpawnOption(string ProductTypeId, int Weight);

    internal sealed record ShelfSpawnConfig(
        int ShelfId,
        int MinItemCount,
        int MaxItemCount,
        IReadOnlyList<ResolvedShelfSpawnOption> Options);

    internal sealed record ResolvedShelfSpawnOption(Product Product, int Weight);

    public class ShelfModel : AbstractModel
    {
        private int _nextInstanceId;
        private ProductCatalog _catalog;
        private readonly Dictionary<int, ShelfSpawnConfig> _configs = new();
        private readonly HashSet<int> _identifiedInstanceIds = new();

        public Dictionary<int, List<ShelfItemInstance>> Inventories { get; } = new();

        protected override void OnInit()
        {
            _catalog = this.GetUtility<ProductCatalog>();
        }

        public void RegisterShelfConfig(
            int shelfId,
            int minItemCount,
            int maxItemCount,
            IReadOnlyList<ShelfSpawnOption> options)
        {
            if (shelfId <= 0) throw new InvalidOperationException($"ShelfId must be > 0; got {shelfId}.");
            if (minItemCount < 0) throw new InvalidOperationException($"Shelf {shelfId} MinItemCount must be >= 0.");
            if (maxItemCount < minItemCount) throw new InvalidOperationException($"Shelf {shelfId} MaxItemCount must be >= MinItemCount.");
            if (options.Count == 0) throw new InvalidOperationException($"Shelf {shelfId} requires at least one spawn option.");

            var resolved = new List<ResolvedShelfSpawnOption>(options.Count);
            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                var product = _catalog.FindByTypeId(option.ProductTypeId)
                    ?? throw new InvalidOperationException($"Shelf {shelfId} references unknown ProductTypeId '{option.ProductTypeId}'.");
                if (option.Weight <= 0) throw new InvalidOperationException($"Shelf {shelfId} product '{option.ProductTypeId}' weight must be > 0.");
                resolved.Add(new ResolvedShelfSpawnOption(product, option.Weight));
            }

            var config = new ShelfSpawnConfig(shelfId, minItemCount, maxItemCount, resolved);
            _configs[shelfId] = config;

            if (!Inventories.ContainsKey(shelfId))
            {
                FillShelf(config);
            }
        }

        public void RefreshRound()
        {
            Inventories.Clear();
            _identifiedInstanceIds.Clear();
            _nextInstanceId = 0;

            foreach (var config in _configs.Values)
            {
                FillShelf(config);
            }
        }

        public ShelfItemInstance FindItem(int shelfId, int instanceId)
        {
            if (!Inventories.TryGetValue(shelfId, out var items)) return null;
            return items.Find(i => i.InstanceId == instanceId);
        }

        public bool IsItemIdentified(int instanceId)
        {
            return _identifiedInstanceIds.Contains(instanceId);
        }

        public void MarkItemIdentified(int shelfId, int instanceId)
        {
            if (FindItem(shelfId, instanceId) == null)
            {
                throw new InvalidOperationException($"Cannot identify missing shelf item: shelf={shelfId}, instance={instanceId}");
            }

            _identifiedInstanceIds.Add(instanceId);
        }

        public ShelfItemInstance FindNextUnidentifiedItem(int shelfId)
        {
            if (!Inventories.TryGetValue(shelfId, out var items)) return null;
            return items.Find(item => !_identifiedInstanceIds.Contains(item.InstanceId));
        }

        public bool RemoveItem(int shelfId, int instanceId, out ShelfItemInstance item)
        {
            item = null;
            if (!Inventories.TryGetValue(shelfId, out var items)) return false;

            int index = items.FindIndex(i => i.InstanceId == instanceId);
            if (index < 0) return false;

            item = items[index];
            items.RemoveAt(index);
            _identifiedInstanceIds.Remove(instanceId);
            return true;
        }

        private void FillShelf(ShelfSpawnConfig config)
        {
            int count = Random.Shared.Next(config.MinItemCount, config.MaxItemCount + 1);
            var items = new List<ShelfItemInstance>(count);

            for (int i = 0; i < count; i++)
            {
                var product = PickWeightedProduct(config.Options);
                items.Add(new ShelfItemInstance(Interlocked.Increment(ref _nextInstanceId), product));
            }

            Inventories[config.ShelfId] = items;
        }

        private static Product PickWeightedProduct(IReadOnlyList<ResolvedShelfSpawnOption> options)
        {
            int totalWeight = 0;
            for (int i = 0; i < options.Count; i++)
            {
                totalWeight += options[i].Weight;
            }

            if (totalWeight <= 0)
            {
                throw new InvalidOperationException("Shelf spawn total weight must be > 0.");
            }

            int roll = Random.Shared.Next(1, totalWeight + 1);
            int cumulative = 0;
            for (int i = 0; i < options.Count; i++)
            {
                cumulative += options[i].Weight;
                if (roll <= cumulative) return options[i].Product;
            }

            throw new InvalidOperationException("Weighted shelf product selection failed.");
        }
    }
}
