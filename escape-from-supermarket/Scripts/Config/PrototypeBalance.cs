using System;
using System.Collections.Generic;
using EscapeFromSupermarket.Models;
using EscapeFromSupermarket.Utilities;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Config
{
	// PrototypeBalance：原型阶段唯一调参入口。
	// 只在需要改变真实地图位置或实例路线时，才去改场景节点 Inspector / Transform。
	public sealed class PrototypeBalance : IUtility
	{
		// ===== 全局与任务 =====

		// Default / 默认配置实例：少数构造期默认值需要在 Architecture 初始化前读取。
		public static PrototypeBalance Default { get; } = new();

		// RouterTaskKey / 路由器任务标记：Product.TaskKey 等于该值时，算作“带出路由器”目标商品。
		public const string RouterTaskKey = "router";

		// ===== 回合与撤离 =====

		// RoundSeconds / 回合时长：每轮倒计时初始秒数。
		public float RoundSeconds => 240.0f;

		// FrontDoorExtractionSeconds / 正门撤离时间：正门撤离区需要读条的秒数。
		public float FrontDoorExtractionSeconds => 7.0f;

		// StaffDoorExtractionSeconds / 员工门撤离时间：持有钥匙卡后从员工门撤离需要读条的秒数。
		public float StaffDoorExtractionSeconds => 2.0f;

		// ===== 购物车容量与载重 =====

		// CartCapacity / 购物车基础容量：未购买升级时可用格子数。
		public int CartCapacity => 5;

		// CartCapacityUpgradeBonus / 容量升级增量：每级购物车容量升级增加的格子数。
		public int CartCapacityUpgradeBonus => 2;

		// CartWeightLimit / 购物车重量上限：超过该重量时不能继续拾取商品。
		public int CartWeightLimit => 15;

		// CartWeightLimitUpgradeBonus / 载重升级增量：每级购物车载重升级增加的重量上限。
		public int CartWeightLimitUpgradeBonus => 5;

		// MidLoadMinWeightPercent / 中载重起点百分比：以 CartWeightLimit 为基数，当前重量达到 50% 后进入 Mid 载重档。
		public float MidLoadMinWeightPercent => 0.50f;

		// HeavyLoadMinWeightPercent / 重载重起点百分比：以 CartWeightLimit 为基数，当前重量达到 80% 后进入 Heavy 载重档。
		public float HeavyLoadMinWeightPercent => 0.80f;

		// EmptyLoadSpeedMultiplier / 空载速度倍率：空载档位下玩家速度乘数。
		public float EmptyLoadSpeedMultiplier => 1.00f;

		// MidLoadSpeedMultiplier / 中载速度倍率：中载档位下玩家速度乘数。
		public float MidLoadSpeedMultiplier => 0.5f;

		// HeavyLoadSpeedMultiplier / 重载速度倍率：重载档位下玩家速度乘数。
		public float HeavyLoadSpeedMultiplier => 0.2f;

		// CartCollisionLocalOffset / 购物车碰撞体本地偏移：让购物车碰撞盒保持在玩家身前。
		public Vector3 CartCollisionLocalOffset => new(0.0f, -0.2f, -0.9f);

		// ===== 玩家移动 =====

		// PlayerBaseSpeed / 玩家基础速度：无载重惩罚时的移动速度。
		public float PlayerBaseSpeed => 4.5f;

		// PlayerTurnSpeed / 玩家转向速度：角色视觉朝移动方向旋转的速度。
		public float PlayerTurnSpeed => 12.0f;

		// CustomerSlowMultiplier / 顾客碰撞减速倍率：玩家碰到顾客后短时间内的速度乘数。
		public float CustomerSlowMultiplier => 0.75f;

		// CustomerSlowSeconds / 顾客碰撞减速时长：玩家碰到顾客后减速持续秒数。
		public float CustomerSlowSeconds => 0.6f;

		// ===== 保安 AI =====

		// GuardPatrolSpeed / 保安巡逻速度：普通巡逻状态下的移动速度。
		public float GuardPatrolSpeed => 2.0f;

		// GuardChaseSpeed / 保安追逐速度：警戒值满后追玩家时的移动速度。
		public float GuardChaseSpeed => 4f;

		// GuardTurnSpeed / 保安转向速度：保安朝移动方向旋转的速度。
		public float GuardTurnSpeed => 10.0f;

		// GuardViewDistance / 保安视野距离：超过该距离不会看到玩家。
		public float GuardViewDistance => 7.0f;

		// GuardViewAngleDegrees / 保安视野角度：保安前方检测扇形的总角度。
		public float GuardViewAngleDegrees => 70.0f;

		// GuardAlertRaiseRate / 警戒增长速度：保安看到带货玩家时每秒增加的警戒值。
		public float GuardAlertRaiseRate => 10f;

		// GuardAlertDecayRate / 警戒衰减速度：保安没看到玩家时每秒降低的警戒值。
		public float GuardAlertDecayRate => 0.35f;

		// GuardCatchDistance / 抓捕距离：追逐状态下保安距离玩家小于该值时判定失败。
		public float GuardCatchDistance => 1.05f;

		// GuardPatrolArrivalDistance / 巡逻点到达距离：保安靠近当前巡逻点到该距离内后切换到下一个点。
		public float GuardPatrolArrivalDistance => 0.35f;

		// ===== 局外成长与奖励 =====

		// HighValueProductMinValue / 高价值商品阈值：每轮货架刷新后至少保证出现一个价值不低于该值的商品。
		public int HighValueProductMinValue => 45;

		// CapacityUpgradeBasePrice / 容量升级基础价格：购买 0 级到 1 级购物车容量升级的价格。
		public int CapacityUpgradeBasePrice => 30;

		// CapacityUpgradePriceStep / 容量升级价格增量：每已有一级容量升级，下一次购买价格增加该值。
		public int CapacityUpgradePriceStep => 20;

		// WeightLimitUpgradeBasePrice / 载重升级基础价格：购买 0 级到 1 级购物车载重升级的价格。
		public int WeightLimitUpgradeBasePrice => 50;

		// WeightLimitUpgradePriceStep / 载重升级价格增量：每已有一级载重升级，下一次购买价格增加该值。
		public int WeightLimitUpgradePriceStep => 30;

		// ===== 顾客 NPC =====

		// CustomerSpeed / 顾客移动速度：顾客沿 PointA/PointB 往返移动的速度。
		public float CustomerSpeed => 1.35f;

		// CustomerTurnSpeed / 顾客转向速度：顾客朝移动方向旋转的速度。
		public float CustomerTurnSpeed => 8.0f;

		// CustomerArriveDistance / 顾客到达距离：顾客靠近目标点到该距离内后切换到另一端路线点。
		public float CustomerArriveDistance => 0.35f;

		// CustomerPushStrength / 顾客被推力度：玩家撞到顾客时顾客获得的短暂推开速度。
		public float CustomerPushStrength => 2.0f;

		// CustomerPushDecay / 顾客推开衰减：顾客被推开后的额外速度每秒衰减速度。
		public float CustomerPushDecay => 6.0f;

		// ===== 货架识别 =====

		// CommonIdentificationSeconds / 普通物品识别时间：普通商品从未知变为已识别需要的秒数。
		public float CommonIdentificationSeconds => 1.0f;

		// RareIdentificationSeconds / 稀有物品识别时间：稀有商品从未知变为已识别需要的秒数。
		public float RareIdentificationSeconds => 2.0f;

		// HighRareIdentificationSeconds / 高稀有物品识别时间：高稀有商品从未知变为已识别需要的秒数。
		public float HighRareIdentificationSeconds => 5.0f;

		// TaskItemIdentificationSeconds / 任务物品识别时间：带任务标记商品从未知变为已识别需要的秒数。
		public float TaskItemIdentificationSeconds => 3.0f;

		// ===== 商品池 =====

		// Products / 商品表：定义所有可随机上架、可拾取、可结算的商品。
		// Product 参数顺序：
		// Id / 商品ID：代码和存档识别用；DisplayName / 中文名：UI 显示用；
		// Value / 价值：撤离成功后的结算金额；Slots / 格子：占用购物车容量；
		// Weight / 重量：影响购物车重量和玩家速度；Category / 分类：匹配货架刷新池；
		// TaskKey / 任务标记：非空时参与本轮目标判断；Rarity / 稀有度：决定非任务商品识别时间。
		public IReadOnlyList<Product> Products { get; } = new[]
		{
			new Product("plastic_cup", "塑料杯", 3, 1, 1, "日用品"),
			new Product("toy_shark", "玩具鲨鱼", 4, 1, 1, "玩具"),
			new Product("discountrise", "临期米饭", 2, 1, 1, "零食"),
			// chips / 薯片：价值 6，占 1 格，重 1，零食类；作用：低价值轻量商品。
			new Product("chips", "薯片", 6, 1, 1, "零食"),
			// canned_soup / 罐头汤：价值 9，占 1 格，重 3，零食类；作用：稍重的低价值商品。
			new Product("canned_soup", "罐头汤", 9, 1, 3, "零食"),
			// toothpaste / 牙膏：价值 12，占 1 格，重 1，日用品类；作用：轻量日用品。
			new Product("toothpaste", "牙膏", 12, 1, 1, "日用品"),
			// detergent / 洗衣液：价值 18，占 2 格，重 5，日用品类；作用：中等容量和重量压力。
			new Product("detergent", "洗衣液", 18, 2, 5, "日用品"),
			// microwave / 微波炉：价值 45，占 4 格，重 12，家电类；作用：高价值重物，制造载重取舍。
			new Product("microwave", "微波炉", 45, 4, 12, "家电", Rarity: ProductRarity.Rare),
			// television / 电视：价值 70，占 5 格，重 16，家电类；作用：最高普通价值商品，制造容量和重量压力。
			new Product("television", "电视", 70, 5, 16, "家电", Rarity: ProductRarity.HighRare),
			// router / 路由器：价值 80，占 5 格，重 15，家电类，任务标记 router；作用：V0.2 目标商品。
			new Product("router", "路由器", 80, 5, 15, "家电", RouterTaskKey, ProductRarity.HighRare),
		};

		// ===== 派生计算 =====

		// GetExtractionSeconds / 获取撤离读条时间：根据出口类型返回正门或员工门所需秒数。
		public float GetExtractionSeconds(ExtractionExitType exitType)
		{
			return exitType == ExtractionExitType.StaffDoor
				? StaffDoorExtractionSeconds
				: FrontDoorExtractionSeconds;
		}

		// GetUpgradePrice / 获取升级价格：根据升级类型和当前等级计算下一次购买费用。
		public int GetUpgradePrice(UpgradeType upgradeType, int currentLevel)
		{
			return upgradeType switch
			{
				UpgradeType.CartCapacity => CapacityUpgradeBasePrice + CapacityUpgradePriceStep * currentLevel,
				UpgradeType.CartWeightLimit => WeightLimitUpgradeBasePrice + WeightLimitUpgradePriceStep * currentLevel,
				_ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, "Unknown upgrade type."),
			};
		}

		// GetIdentificationSeconds / 获取识别时间：任务商品优先用任务时间，否则按稀有度返回读条秒数。
		public float GetIdentificationSeconds(Product product)
		{
			if (!string.IsNullOrEmpty(product.TaskKey)) return TaskItemIdentificationSeconds;

			return product.Rarity switch
			{
				ProductRarity.Common => CommonIdentificationSeconds,
				ProductRarity.Rare => RareIdentificationSeconds,
				ProductRarity.HighRare => HighRareIdentificationSeconds,
				_ => throw new ArgumentOutOfRangeException(nameof(product.Rarity), product.Rarity, "Unknown product rarity."),
			};
		}

		// GetCartLoadTier / 获取购物车载重档位：根据当前重量判断 Empty、Mid、Heavy。
		public CartLoadTier GetCartLoadTier(int weight)
		{
			float midLoadMinWeight = CartWeightLimit * MidLoadMinWeightPercent;
			float heavyLoadMinWeight = CartWeightLimit * HeavyLoadMinWeightPercent;

			if (weight < midLoadMinWeight) return CartLoadTier.Empty;
			if (weight < heavyLoadMinWeight) return CartLoadTier.Mid;
			return CartLoadTier.Heavy;
		}

		// GetCartLoadSpeedMultiplier / 获取载重速度倍率：根据载重档位返回玩家移动速度乘数。
		public float GetCartLoadSpeedMultiplier(CartLoadTier tier) => tier switch
		{
			CartLoadTier.Empty => EmptyLoadSpeedMultiplier,
			CartLoadTier.Mid => MidLoadSpeedMultiplier,
			CartLoadTier.Heavy => HeavyLoadSpeedMultiplier,
			_ => EmptyLoadSpeedMultiplier,
		};
	}
}
