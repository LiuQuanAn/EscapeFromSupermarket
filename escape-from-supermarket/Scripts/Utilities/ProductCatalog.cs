using System;
using System.Collections.Generic;
using QFramework;

namespace EscapeFromSupermarket.Utilities
{
    public sealed record Product(string ProductTypeId, string Name, int Value, int Slots, int Weight, string Category);

    public class ProductCatalog : IUtility
    {
        private readonly List<Product> _products = new()
        {
            new Product("chips", "薯片", 6, 1, 1, "零食"),
            new Product("canned_soup", "罐头汤", 9, 1, 3, "零食"),
            new Product("toothpaste", "牙膏", 12, 1, 1, "日用品"),
            new Product("detergent", "洗衣液", 18, 2, 5, "日用品"),
            new Product("microwave", "微波炉", 45, 4, 12, "家电"),
            new Product("television", "电视", 70, 5, 16, "家电"),
        };

        private readonly Dictionary<string, IReadOnlyList<Product>> _byCategory;

        public ProductCatalog()
        {
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
    }
}
