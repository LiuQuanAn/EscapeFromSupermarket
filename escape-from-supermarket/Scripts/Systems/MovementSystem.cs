using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Systems
{
    public class MovementSystem : AbstractSystem
    {
        private PrototypeBalance _balance = PrototypeBalance.Default;

        public float GetSpeedMultiplier(CartLoadTier tier)
        {
            return _balance.GetCartLoadSpeedMultiplier(tier);
        }

        protected override void OnInit()
        {
            _balance = this.GetUtility<PrototypeBalance>();
        }
    }
}
