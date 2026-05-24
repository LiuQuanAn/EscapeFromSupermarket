using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Systems
{
    public class MovementSystem : AbstractSystem
    {
        private PrototypeBalance _balance = PrototypeBalance.Default;
        private MetaProgressModel _metaProgress;
        private float _playerSpeedUpgradeMultiplier = 1.0f;

        public float GetSpeedMultiplier(CartLoadTier tier)
        {
            return _balance.GetCartLoadSpeedMultiplier(tier);
        }

        public float GetPlayerSpeedUpgradeMultiplier()
        {
            return _playerSpeedUpgradeMultiplier;
        }

        protected override void OnInit()
        {
            _balance = this.GetUtility<PrototypeBalance>();
            _metaProgress = this.GetModel<MetaProgressModel>();
            _metaProgress.PlayerSpeedLevel.RegisterWithInitValue(UpdatePlayerSpeedUpgradeMultiplier);
        }

        private void UpdatePlayerSpeedUpgradeMultiplier(int level)
        {
            _playerSpeedUpgradeMultiplier = Mathf.Pow(_balance.PlayerSpeedUpgradeMultiplier, level);
        }
    }
}
