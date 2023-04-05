using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.Items.Accessories;

public class LaserScope : ModItem
{
	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults() {
		// Base properties
		Item.width = 22;
		Item.height = 26;
		Item.value = Item.sellPrice(gold: 6);
		Item.rare = ItemRarityID.Lime;

		// Other properties
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.GetModPlayer<LaserScopePlayer>().LaserScope = true;
	}
}

public class LaserScopePlayer : ModPlayer
{
	private const float CritDamageMult = 0.5f;

	public bool LaserScope { private get; set; }

	public override void ResetEffects() {
		LaserScope = false;
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
		if (!LaserScope) {
			return;
		}

		modifiers.CritDamage += CritDamageMult;
		modifiers.HideCombatText();
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
		if (!LaserScope || !hit.Crit) {
			CombatText.NewText(target.getRect(), CombatText.DamagedHostile, damageDone);
			hit.HideCombatText = false;
			return;
		}

		Color laserScopeCritColor = new(255, 51, 0);
		CombatText.NewText(target.getRect(), laserScopeCritColor, damageDone, true);
		hit.HideCombatText = false;
	}
}