using QFramework;

namespace EscapeFromSupermarket.Models
{
    public enum UpgradeType { CartCapacity, PlayerSpeed }

    public class MetaProgressModel : AbstractModel
    {
        public BindableProperty<int> Money { get; } = new(0);
        public BindableProperty<int> NavigationProgress { get; } = new(0);
        public BindableProperty<int> CartCapacityLevel { get; } = new(0);
        public BindableProperty<int> PlayerSpeedLevel { get; } = new(0);

        protected override void OnInit()
        {
        }

        public int GetLevel(UpgradeType upgradeType)
        {
            return upgradeType == UpgradeType.CartCapacity
                ? CartCapacityLevel.Value
                : PlayerSpeedLevel.Value;
        }

        public void AddLevel(UpgradeType upgradeType)
        {
            if (upgradeType == UpgradeType.CartCapacity)
            {
                CartCapacityLevel.Value++;
            }
            else
            {
                PlayerSpeedLevel.Value++;
            }
        }
    }
}
