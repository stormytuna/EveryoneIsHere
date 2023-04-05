using System;
using System.Collections.Generic;
using EveryoneIsHere.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.Items.Accessories;

public class BrilliantBehemoth : ModItem
{
	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults() {
		// Base properties
		Item.width = 54;
		Item.height = 52;
		Item.value = Item.buyPrice(gold: 22);
		Item.rare = ItemRarityID.Lime;

		// Other properties
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.GetModPlayer<BrilliantBehemothPlayer>().BrilliantBehemoth = true;
	}
}

public class BrilliantBehemothGlobalNPC : GlobalNPC
{
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Demolitionist;

	public override void ModifyShop(NPCShop shop) {
		shop.Add<BrilliantBehemoth>(new Condition("Mods.EveryoneIsHere.Conditions.DownedGolem", () => NPC.downedGolemBoss));
	}
}

public class BrilliantBehemothPlayer : ModPlayer
{
	private const float ExplosionRadius = 8f * 16f;
	private const float ExplosionDamageMult = 0.2f;
	private const float ExplosionKnockbackMult = 0.5f;

	public bool BrilliantBehemoth { private get; set; }

	public override void ResetEffects() {
		BrilliantBehemoth = false;
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
		if (!BrilliantBehemoth) {
			return;
		}

		NPC.HitInfo brilliantBehemothExplosionHit = hit;
		brilliantBehemothExplosionHit.DamageType = DamageClass.Generic;
		brilliantBehemothExplosionHit.Knockback *= ExplosionKnockbackMult;
		brilliantBehemothExplosionHit.Damage = (int)(brilliantBehemothExplosionHit.Damage * ExplosionDamageMult);

		List<int> excludedNPCs = new() {
			target.whoAmI
		};
		foreach (NPC closeNPC in NPCHelpers.FindNearbyNPCs(target.Center, ExplosionRadius, true, excludedNPCs)) {
			brilliantBehemothExplosionHit.HitDirection = Math.Sign(target.DirectionTo(closeNPC.Center).X);
			closeNPC.StrikeNPC(brilliantBehemothExplosionHit);
		}

		// Fiery dust explosion
		for (int i = 0; i < 20; i++) {
			Dust fireDust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Torch);
			fireDust.velocity = Main.rand.NextVector2Circular(10f, 10f);
			fireDust.scale = Main.rand.NextFloat(1.3f, 2f);
			fireDust.noGravity = true;
		}

		// Smoke explosion
		for (int i = 0; i < 13; i++) {
			Dust fireDust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Smoke);
			fireDust.velocity = Main.rand.NextVector2Circular(10f, 10f);
			fireDust.noGravity = true;
		}

		// Fiery dust on the enemy
		for (int i = 0; i < 4; i++) {
			Dust fireDust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Torch);
			fireDust.scale = Main.rand.NextFloat(2.3f, 3f);
			fireDust.noGravity = true;
			fireDust.alpha = 120;
		}
	}
}