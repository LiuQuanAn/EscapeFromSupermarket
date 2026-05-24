using System.Collections.Generic;
using EscapeFromSupermarket.Models;
using EscapeFromSupermarket.Utilities;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Config
{
	public sealed record ShelfSpawnRule(int ShelfId, string Category, int MinItemCount, int MaxItemCount);

	/// <summary>
	/// Single prototype balance entry point. Step 9 tuning should start here,
	/// then only move into scene transforms when changing physical layout.
	/// </summary>
	public sealed class PrototypeBalance : IUtility
	{
		public static PrototypeBalance Default { get; } = new();
		public const string RouterTaskKey = "router";

		public float RoundSeconds => 240.0f;
		public float FrontDoorExtractionSeconds => 3.0f;
		public float StaffDoorExtractionSeconds => 2.0f;

		public int CartCapacity => 10;
		public int CartCapacityUpgradeBonus => 2;
		public int CartWeightLimit => 30;
		public int MidLoadMinWeight => 10;
		public int HeavyLoadMinWeight => 22;

		public float EmptyLoadSpeedMultiplier => 1.00f;
		public float MidLoadSpeedMultiplier => 0.85f;
		public float HeavyLoadSpeedMultiplier => 0.65f;

		public float PlayerBaseSpeed => 4.5f;
		public float PlayerTurnSpeed => 12.0f;
		public float PlayerSpeedUpgradeMultiplier => 1.05f;
		public float CustomerSlowMultiplier => 0.75f;
		public float CustomerSlowSeconds => 0.6f;
		public Vector3 CartCollisionLocalOffset => new(0.0f, -0.2f, -0.9f);

		public float GuardPatrolSpeed => 2.0f;
		public float GuardChaseSpeed => 3.5f;
		public float GuardTurnSpeed => 10.0f;
		public float GuardViewDistance => 7.0f;
		public float GuardViewAngleDegrees => 70.0f;
		public float GuardAlertRaiseRate => 0.55f;
		public float GuardAlertDecayRate => 0.35f;
		public float GuardCatchDistance => 1.05f;
		public float GuardPatrolArrivalDistance => 0.35f;

		public int HighValueProductMinValue => 45;
		public int CapacityUpgradeBasePrice => 120;
		public int CapacityUpgradePriceStep => 80;
		public int SpeedUpgradeBasePrice => 150;
		public int SpeedUpgradePriceStep => 100;

		public float CustomerSpeed => 1.35f;
		public float CustomerTurnSpeed => 8.0f;
		public float CustomerArriveDistance => 0.35f;
		public float CustomerPushStrength => 2.0f;
		public float CustomerPushDecay => 6.0f;
		public float InteractionRange => 2.8f;

		public IReadOnlyList<Product> Products { get; } = new[]
		{
			new Product("chips", "薯片", 6, 1, 1, "零食"),
			new Product("canned_soup", "罐头汤", 9, 1, 3, "零食"),
			new Product("toothpaste", "牙膏", 12, 1, 1, "日用品"),
			new Product("detergent", "洗衣液", 18, 2, 5, "日用品"),
			new Product("microwave", "微波炉", 45, 4, 12, "家电"),
			new Product("television", "电视", 70, 5, 16, "家电"),
			new Product("router", "路由器", 80, 2, 2, "家电", RouterTaskKey),
		};

		public IReadOnlyList<ShelfSpawnRule> Shelves { get; } = new[]
		{
			new ShelfSpawnRule(1, "零食", 2, 4),
			new ShelfSpawnRule(2, "零食", 2, 4),
			new ShelfSpawnRule(3, "日用品", 2, 4),
			new ShelfSpawnRule(4, "家电", 2, 4),
		};

		public IReadOnlyList<Vector3> GuardPatrolPath { get; } = new[]
		{
			new Vector3(0, 0.8f, 10),
			new Vector3(0, 0.8f, -10),
			new Vector3(7, 0.8f, -10),
			new Vector3(7, 0.8f, 10),
		};

		public float GetExtractionSeconds(ExtractionExitType exitType)
		{
			return exitType == ExtractionExitType.StaffDoor
				? StaffDoorExtractionSeconds
				: FrontDoorExtractionSeconds;
		}

		public int GetUpgradePrice(UpgradeType upgradeType, int currentLevel)
		{
			return upgradeType == UpgradeType.CartCapacity
				? CapacityUpgradeBasePrice + CapacityUpgradePriceStep * currentLevel
				: SpeedUpgradeBasePrice + SpeedUpgradePriceStep * currentLevel;
		}

		public CartLoadTier GetCartLoadTier(int weight)
		{
			if (weight < MidLoadMinWeight) return CartLoadTier.Empty;
			if (weight < HeavyLoadMinWeight) return CartLoadTier.Mid;
			return CartLoadTier.Heavy;
		}

		public float GetCartLoadSpeedMultiplier(CartLoadTier tier) => tier switch
		{
			CartLoadTier.Empty => EmptyLoadSpeedMultiplier,
			CartLoadTier.Mid => MidLoadSpeedMultiplier,
			CartLoadTier.Heavy => HeavyLoadSpeedMultiplier,
			_ => EmptyLoadSpeedMultiplier,
		};
	}
}
