using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Systems
{
    public class MovementSystem : AbstractSystem
    {
        public float GetSpeedMultiplier(CartLoadTier tier) => tier switch
        {
            CartLoadTier.Empty => 1.00f,
            CartLoadTier.Mid   => 0.85f,
            CartLoadTier.Heavy => 0.65f,
            _ => 1.00f,
        };

        protected override void OnInit()
        {
        }
    }
}
