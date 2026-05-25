using System;
using System.Collections.Generic;
using EscapeFromSupermarket.Config;
using QFramework;

namespace EscapeFromSupermarket.Utilities
{
    public enum ProductRarity
    {
        Common,
        Rare,
        HighRare,
    }

    public sealed record Product(
        string ProductTypeId,
        string Name,
        int Value,
        int Slots,
        int Weight,
        string Category,
        string TaskKey = "",
        ProductRarity Rarity = ProductRarity.Common);

    public class ProductCatalog : IUtility
    {
        private readonly List<Product> _products;
        private readonly Dictionary<string, IReadOnlyList<Product>> _byCategory;

        public ProductCatalog()
            : this(PrototypeBalance.Default)
        {
        }

        public ProductCatalog(PrototypeBalance balance)
        {
            _products = new List<Product>(balance.Products);

            var buckets = new Dictionary<string, List<Product>>();
            foreach (var product in _products)
            {
                if (!buckets.TryGetValue(product.Category, out var list))
                {
                    list = new List<Product>();
                    buckets[product.Category] = list;
                }
                list.Add(product);
            }

            var frozen = new Dictionary<string, IReadOnlyList<Product>>(buckets.Count);
            foreach (var kv in buckets)
            {
                frozen[kv.Key] = kv.Value;
            }
            _byCategory = frozen;
        }

        public IReadOnlyList<Product> All => _products;

        public IReadOnlyList<Product> GetByCategory(string category)
        {
            return _byCategory.TryGetValue(category, out var list)
                ? list
                : Array.Empty<Product>();
        }

        public Product FindByTypeId(string productTypeId)
        {
            return _products.Find(x => x.ProductTypeId == productTypeId);
        }
    }
}
