using System;
using EveryoneIsHere.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.Items.Accessories;

[AutoloadEquip(EquipType.Neck)]
public class OddlyShapedOpal : ModItem
{
	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults() {
		// Base properties
		Item.width = 28;
		Item.height = 28;
		Item.value = Item.sellPrice(silver: 40);
		Item.rare = ItemRarityID.Green;

		// Other properties
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.GetModPlayer<OpalPlayer>().Opal = true;
		player.GetModPlayer<OpalPlayer>().OpalVisuals = !hideVisual;
	}
}

public class OpalDropRule : GlobalNPC
{
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Tim;

	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
		npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<OddlyShapedOpal>()));
	}
}

public class OpalPlayer : ModPlayer
{
	private const int OpalCounterMax = 10 * 60;

	private Vector2 OpalDustLocation {
		get {
			Vector2 position = Player.MountedCenter;
			if (Player.direction == -1) {
				position += new Vector2(-8f, 0f);
			}

			if (Player.gravDir == -1) {
				position += new Vector2(0f, -13f);
			}

			return position;
		}
	}

	public bool Opal { get; set; }
	public bool OpalVisuals { get; set; }
	public int OpalCounter { get; private set; }
	public bool OpalShownAndActive => OpalVisuals && OpalCounter <= 0 && Opal;

	public override void ResetEffects() {
		Opal = false;
		OpalVisuals = false;
	}

	public override void PostUpdate() {
		OpalCounter--;


		if (!OpalShownAndActive) {
			return;
		}

		// Spawn aura projectiles
		if (Player.ownedProjectileCounts[ModContent.ProjectileType<OpalAuraInner>()] <= 0) {
			Projectile.NewProjectile(Player.GetSource_Misc("-1"), Player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<OpalAuraInner>(), 0, 0f, Player.whoAmI);
		}

		if (Player.ownedProjectileCounts[ModContent.ProjectileType<OpalAuraOuter>()] <= 0) {
			Projectile.NewProjectile(Player.GetSource_Misc("-1"), Player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<OpalAuraOuter>(), 0, 0f, Player.whoAmI);
		}

		bool sometimesDust = !Main.rand.NextBool(5);
		bool playerIsUnmounted = Player.mount.Type != MountID.None;
		if (sometimesDust || playerIsUnmounted) {
			return;
		}

		// Make dust shooting outwards from opal necklace
		Vector2 dustPosition = OpalDustLocation;
		Dust newDust = Dust.NewDustDirect(dustPosition, 6, 8, DustID.Clentaminator_Purple);
		newDust.velocity *= 0.2f;
		newDust.noLight = true;
		newDust.scale = Main.rand.NextFloat(0.2f, 0.4f);
	}

	private void TryOpalDamageReduction(ref Player.HurtModifiers modifiers) {
		bool doOpalDamageReduction = OpalCounter < 0 && Opal;
		OpalCounter = OpalCounterMax;
		if (doOpalDamageReduction) {
			modifiers.IncomingDamageMultiplier *= 0.5f;
		}

		// TODO: Fancy dust effect, being absorbed by necklace
	}

	public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) {
		TryOpalDamageReduction(ref modifiers);
	}

	public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers) {
		TryOpalDamageReduction(ref modifiers);
	}

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
		if (!OpalShownAndActive || Main.rand.NextBool()) {
			return;
		}

		// Make dust shooting from aura edge towards player
		Vector2 dustPositionOffset = Main.rand.NextVector2CircularEdge(38f, 38f);
		Vector2 dustPosition = drawInfo.drawPlayer.MountedCenter + dustPositionOffset;
		Point dustTileCoordinates = dustPosition.ToTileCoordinates();

		Tile tile = Framing.GetTileSafely(dustTileCoordinates);
		if (tile.HasTile && WorldGen.SolidTile(dustTileCoordinates.X, dustTileCoordinates.Y)) {
			return;
		}

		Dust newDust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<OpalDust>());

		// 1/2 chance of being edge dust or zoomy dust
		Vector2 dustVelocity = Main.rand.NextBool() ? Vector2.Zero : dustPosition.DirectionTo(drawInfo.drawPlayer.MountedCenter) * Main.rand.NextFloat(0.3f, 0.8f);
		newDust.velocity = dustVelocity;
		newDust.scale = Main.rand.NextFloat(0.3f, 0.6f);
		newDust.customData = drawInfo.drawPlayer.whoAmI;

		drawInfo.DustCache.Add(newDust.dustIndex);
	}
}

public class OpalPlayerDrawLayer : PlayerDrawLayer
{
	private static readonly Lazy<Asset<Texture2D>> OpalNecklace = new(() => ModContent.Request<Texture2D>("EveryoneIsHere/RiskOfRain/Content/Items/Accessories/OddlyShapedOpal_Neck"));
	private static readonly Lazy<Asset<Texture2D>> OpalShine = new(() => ModContent.Request<Texture2D>("EveryoneIsHere/RiskOfRain/Content/Items/Accessories/OddlyShapedOpal_Shine"));

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
		bool opalShownAndActive = drawInfo.drawPlayer.GetModPlayer<OpalPlayer>().OpalShownAndActive;
		bool opalIsShownNecklaceAccessory = drawInfo.drawPlayer.neck == EquipLoader.GetEquipSlot(Mod, "OddlyShapedOpal", EquipType.Neck);
		return opalShownAndActive && opalIsShownNecklaceAccessory;
	}

	public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.NeckAcc);

	protected override void Draw(ref PlayerDrawSet drawInfo) {
		Player drawPlayer = drawInfo.drawPlayer;
		OpalPlayer modPlayer = drawPlayer.GetModPlayer<OpalPlayer>();

		// Draw actual necklace
		Texture2D necklaceTexture = OpalNecklace.Value.Value;
		Vector2 drawPosition = drawInfo.Center.ToScreenCoordinates().Floor();
		Rectangle sourceRect = drawPlayer.bodyFrame;
		Color drawColor = drawInfo.colorArmorBody;
		float rotation = drawPlayer.bodyRotation;
		Vector2 origin = drawInfo.bodyVect;
		float scale = 1f;
		SpriteEffects effects = drawInfo.playerEffect;

		drawInfo.DrawDataCache.Add(new DrawData(necklaceTexture, drawPosition, sourceRect, drawColor, rotation, origin, scale, effects));

		if (modPlayer.OpalCounter > 0) {
			return;
		}

		// Draw opal shine if it's currently active
		Texture2D shineTexture = OpalShine.Value.Value;
		float shineScale = MathF.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 2f) * 0.3f + 1.1f;
		Color shineColor = drawColor * 0.1f * shineScale;
		for (float fl = 0f; fl < 1f; fl += 355f / (678f * MathHelper.Pi)) {
			Vector2 shineDrawPosition = drawPosition + (MathHelper.TwoPi * fl).ToRotationVector2() * 2f;
			drawInfo.DrawDataCache.Add(new DrawData(shineTexture, shineDrawPosition, sourceRect, shineColor, 0f, origin, 1f, effects));
		}
	}
}

public class OpalDust : PlayerParentedDust
{
	public override string Texture => null;

	public override void OnSpawn(Dust dust) {
		dust.alpha = 160;
		dust.frame = DustHelpers.GetDustFrameFromType(62);
		dust.noGravity = true;
	}

	public override bool Update(Dust dust) {
		base.Update(dust);

		dust.rotation += 0.1f;
		dust.scale -= 0.01f;
		dust.position += dust.velocity;

		if (dust.scale < 0.25f) {
			dust.active = false;
		}

		return false;
	}
}

public class OpalAuraOuter : AuraProjectile
{
	public override string Texture => "EveryoneIsHere/RiskOfRain/Content/Items/Accessories/OddlyShapedOpal_AuraOuter";

	public override bool ShouldKillProjectile() => !Owner.GetModPlayer<OpalPlayer>().OpalShownAndActive;

	public override void SafeSetDefaults() {
		AuraDrawColor = Color.MediumPurple;
		AuraDrawColorMultiplier = 0.4f;
	}
}

public class OpalAuraInner : AuraProjectile
{
	public override string Texture => "EveryoneIsHere/RiskOfRain/Content/Items/Accessories/OddlyShapedOpal_AuraInner";

	public override bool ShouldKillProjectile() => !Owner.GetModPlayer<OpalPlayer>().OpalShownAndActive;

	public override void SafeSetDefaults() {
		AuraDrawColor = Color.Purple;
		AuraDrawColorMultiplier = 0.5f;
	}
}