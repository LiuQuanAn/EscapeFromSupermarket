using System;
using QFramework;

namespace EscapeFromSupermarket.Models
{
    public enum UpgradeType { CartCapacity, CartWeightLimit }

    public class MetaProgressModel : AbstractModel
    {
        public BindableProperty<int> Money { get; } = new(0);
        public BindableProperty<int> NavigationProgress { get; } = new(0);
        public BindableProperty<int> CartCapacityLevel { get; } = new(0);
        public BindableProperty<int> CartWeightLimitLevel { get; } = new(0);

        protected override void OnInit()
        {
        }

        public int GetLevel(UpgradeType upgradeType)
        {
            return upgradeType switch
            {
                UpgradeType.CartCapacity => CartCapacityLevel.Value,
                UpgradeType.CartWeightLimit => CartWeightLimitLevel.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, "Unknown upgrade type."),
            };
        }

        public void AddLevel(UpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case UpgradeType.CartCapacity:
                    CartCapacityLevel.Value++;
                    break;
                case UpgradeType.CartWeightLimit:
                    CartWeightLimitLevel.Value++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, "Unknown upgrade type.");
            }
        }
    }
}
