using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.Items.Accessories;

[AutoloadEquip(EquipType.HandsOff, EquipType.HandsOn)]
public class AdornedGlove : ModItem
{
	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults() {
		Item.width = 22;
		Item.height = 28;
		Item.value = Item.sellPrice(gold: 4, silver: 50);
		Item.rare = ItemRarityID.LightPurple; // 1 above pink used by Power Glove
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.GetModPlayer<FocusCrystalPlayer>().FocusCrystal = true;
		player.GetModPlayer<FocusCrystalPlayer>().FocusCrystalVisuals = !hideVisual;
		player.kbGlove = true;
		player.autoReuseGlove = true;
		player.meleeScaleGlove = true;
		player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient(ItemID.PowerGlove)
			.AddIngredient<FocusCrystal>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}