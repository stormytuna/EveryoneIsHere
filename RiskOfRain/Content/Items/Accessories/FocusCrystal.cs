using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.Items.Accessories;

[AutoloadEquip(EquipType.Waist)]
public class FocusCrystal : ModItem
{
	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 1;

		base.SetStaticDefaults();
	}

	public override void SetDefaults() {
		Item.width = 22;
		Item.height = 22;
		Item.value = Item.sellPrice(silver: 15);
		Item.rare = ItemRarityID.Green;
		Item.accessory = true;

		base.SetDefaults();
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.GetModPlayer<FocusCrystalPlayer>().FocusCrystal = true;
		player.GetModPlayer<FocusCrystalPlayer>().FocusCrystalVisuals = !hideVisual;

		base.UpdateAccessory(player, hideVisual);
	}
}

public class FocusCrystalGlobalProjectile : GlobalProjectile
{
	public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
		return entity.type == ProjectileID.Geode;
	}

	public override void Kill(Projectile projectile, int timeLeft) {
		if (!Main.rand.NextBool(10)) {
			return;
		}

		int itemIndex = Item.NewItem(projectile.GetSource_Loot(), projectile.getRect(), ModContent.ItemType<FocusCrystal>());
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
		}

		base.Kill(projectile, timeLeft);
	}
}

public class FocusCrystalPlayer : ModPlayer
{
	private const float FocusCrystalRange = 12f * 16f;

	public bool FocusCrystal { private get; set; }
	public bool FocusCrystalVisuals { get; set; }

	public override void ResetEffects() {
		FocusCrystal = false;
		FocusCrystalVisuals = false;

		base.ResetEffects();
	}

	private void TryFocusCrystalDamageIncrease(NPC target, ref NPC.HitModifiers modifiers) {
		if (!Player.WithinRange(target.Center, FocusCrystalRange)) {
			return;
		}

		modifiers.SourceDamage *= 1.2f;

		int numDust = Main.rand.Next(3, 7);
		for (int i = 0; i < numDust; i++) {
			Vector2 dustPositionOffset = Main.rand.NextVector2Circular(5f, 5f);
			Vector2 dustPosition = target.Center + dustPositionOffset;
			Dust newDust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<FocusCrystalAuraDust>());
			newDust.velocity = Main.rand.NextVector2Circular(5f, 5f);
		}
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
		if (FocusCrystal) {
			TryFocusCrystalDamageIncrease(target, ref modifiers);
		}

		base.ModifyHitNPC(target, ref modifiers);
	}

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
		if (!FocusCrystalVisuals) {
			return;
		}

		// Aura dust
		int numDust = Main.rand.Next(3, 8);
		for (int i = 0; i < numDust; i++) {
			Vector2 dustPositionOffset = Main.rand.NextVector2CircularEdge(FocusCrystalRange, FocusCrystalRange);
			Vector2 dustPosition = drawInfo.drawPlayer.MountedCenter + dustPositionOffset;

			Tile tile = Framing.GetTileSafely(dustPosition.ToTileCoordinates());
			if (tile.HasTile && Main.tileSolid[tile.TileType]) {
				continue;
			}

			Dust newDust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<FocusCrystalAuraDust>());

			// 1/2 chance of being edge dust or zoomy dust
			Vector2 dustVelocity = Main.rand.NextBool() ? Vector2.Zero : dustPosition.DirectionTo(drawInfo.drawPlayer.MountedCenter) * Main.rand.NextFloat(0.4f, 1.8f);
			newDust.velocity = dustVelocity;
			newDust.customData = drawInfo.drawPlayer.whoAmI;

			drawInfo.DustCache.Add(newDust.dustIndex);
		}

		// TODO: Dust on waist sprite

		base.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);
	}
}

public class FocusCrystalAuraDust : ModDust
{
	public override string Texture => null;

	public override void OnSpawn(Dust dust) {
		dust.noGravity = true;

		int vanillaDustId = 60;
		int frameX = vanillaDustId * 10 % 1000;
		int frameY = vanillaDustId * 10 / 1000 * 30 + Main.rand.Next(3) * 10; // Wacky stuff from EM, basically just gives our dust a vanilla texture
		dust.frame = new Rectangle(frameX, frameY, 8, 8);

		base.OnSpawn(dust);
	}

	public override bool Update(Dust dust) {
		dust.rotation += 0.1f;
		dust.scale -= 0.03f;
		dust.position += dust.velocity;

		if (dust.scale < 0.25f) {
			dust.active = false;
		}

		if (dust.customData is int owner) {
			Player player = Main.player[owner];
			dust.position += player.position - player.oldPosition;
		}

		return false;
	}
}