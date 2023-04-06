using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.Items.Accessories;

public class BerserkersPauldron : ModItem
{
	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults() {
		// Base properties
		Item.width = 26;
		Item.height = 28;
		Item.value = Item.sellPrice(gold: 2);
		Item.rare = ItemRarityID.Pink;

		// Other properties
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.GetModPlayer<BerserkersPauldronPlayer>().BerserkersPauldron = true;
	}
}

public class BerserkersPauldronDropRule : GlobalNPC
{
	private static readonly int[] AllowedNpcTypes = { NPCID.GoblinPeon, NPCID.GoblinSorcerer, NPCID.GoblinThief, NPCID.GoblinWarrior, NPCID.GoblinArcher, NPCID.GoblinSummoner };

	public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => AllowedNpcTypes.Contains(entity.type);

	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
		int dropChanceDenominator = npc.type == NPCID.GoblinSummoner ? 25 : 200;
		IItemDropRule dropRule = ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<BerserkersPauldron>(), dropChanceDenominator);
		npcLoot.Add(dropRule);
	}
}

public class BerserkersPauldronPlayer : ModPlayer
{
	private const int EffectTimeframeMax = 5 * 60;
	private const int NumEnemiesToKillForEffect = 3;
	private const int BerserkBuffTime = 6 * 60;

	private int effectTimeframe;
	private int numKilledEnemies;

	public bool BerserkersPauldron { private get; set; }

	public override void ResetEffects() {
		BerserkersPauldron = false;
	}

	public override void PostUpdateEquips() {
		effectTimeframe--;
		if (effectTimeframe <= 0) {
			numKilledEnemies = 0;
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
		if (target.active || !BerserkersPauldron) {
			return;
		}

		effectTimeframe = EffectTimeframeMax;
		numKilledEnemies++;

		if (numKilledEnemies < NumEnemiesToKillForEffect) {
			return;
		}

		effectTimeframe = 0;
		numKilledEnemies = 0;
		Player.AddBuff(ModContent.BuffType<BerserkersPauldronBuff>(), BerserkBuffTime);
	}
}

public class BerserkersPauldronBuff : ModBuff
{
	public override string Texture => "EveryoneIsHere/RiskOfRain/Content/Items/Accessories/BerserkersPauldron_Buff";

	public override void Update(Player player, ref int buffIndex) {
		player.GetDamage(DamageClass.Generic) += 25f;
	}
}