using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class BuyUpgradeCommand : AbstractCommand
    {
        private readonly UpgradeType _upgradeType;

        public BuyUpgradeCommand(UpgradeType upgradeType)
        {
            _upgradeType = upgradeType;
        }

        protected override void OnExecute()
        {
            var balance = this.GetUtility<PrototypeBalance>();
            var meta = this.GetModel<MetaProgressModel>();
            int price = balance.GetUpgradePrice(_upgradeType, meta.GetLevel(_upgradeType));
            if (meta.Money.Value < price) return;

            meta.Money.Value -= price;
            meta.AddLevel(_upgradeType);
        }
    }
}
