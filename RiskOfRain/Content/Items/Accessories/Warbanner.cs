using EveryoneIsHere.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.Items.Accessories;

// [AutoloadEquip(EquipType.Back)] // TODO: Implement this 
public class Warbanner : ModItem
{
	private const float WarbannerRange = 50f * 16f;

	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 1;

		base.SetStaticDefaults();
	}

	public override void SetDefaults() {
		Item.width = 46;
		Item.height = 58;
		Item.value = Item.sellPrice(silver: 54);
		Item.rare = ItemRarityID.Green;
		Item.accessory = true;

		base.SetDefaults();
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
		foreach (Player nearbyPlayer in GeneralUtils.NearbyPlayers(player.MountedCenter, WarbannerRange, false, player.team)) {
			nearbyPlayer.AddBuff(ModContent.BuffType<WarbannerBuff>(), 5 * 60);
		}

		base.UpdateAccessory(player, hideVisual);
	}
}

public class WarbannerBuff : ModBuff
{
	public override void SetStaticDefaults() {
		Main.buffNoTimeDisplay[Type] = true;

		base.SetStaticDefaults();
	}

	public override void Update(Player player, ref int buffIndex) {
		player.GetDamage(DamageClass.Generic) += 0.08f;
		player.moveSpeed += 0.2f;

		base.Update(player, ref buffIndex);
	}
}