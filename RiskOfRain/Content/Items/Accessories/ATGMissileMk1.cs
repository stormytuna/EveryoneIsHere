using System;
using System.Linq;
using EveryoneIsHere.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.Items.Accessories;

public class ATGMissileMk1 : ModItem
{
	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults() {
		// Base properties
		Item.width = 22;
		Item.height = 22;
		Item.value = Item.buyPrice(gold: 18);
		Item.rare = ItemRarityID.LightRed;

		// Other properties
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.GetModPlayer<ATGPlayer>().ATG = true;
		player.GetModPlayer<ATGPlayer>().ATGVisuals = !hideVisual;
	}
}

public class ATGGlobalNPC : GlobalNPC
{
	private static readonly int[] VeryRareItems = {
		ItemID.BambooLeaf, ItemID.BedazzledNectar, ItemID.BlueEgg, ItemID.ExoticEasternChewToy, ItemID.BirdieRattle, ItemID.AntiPortalBlock, ItemID.CompanionCube, ItemID.SittingDucksFishingRod,
		ItemID.HunterCloak, ItemID.WinterCape, ItemID.RedCape, ItemID.CrimsonCloak, ItemID.DiamondRing, ItemID.CelestialMagnet, ItemID.WaterGun, ItemID.PulseBow, ItemID.YellowCounterweight
	};

	public override void SetupTravelShop(int[] shop, ref int nextSlot) {
		int veryRareItemIndex = shop.ToList().FindIndex(itemType => VeryRareItems.Contains(itemType));
		if (veryRareItemIndex > -1 && Main.rand.NextBool(VeryRareItems.Length)) {
			shop[veryRareItemIndex] = ModContent.ItemType<ATGMissileMk1>();
		}
	}
}

public class ATGPlayer : ModPlayer
{
	private const float ATGMissileChance = 0.08f;
	public const float ATGTargetRange = 50f * 16f;

	public bool ATG { private get; set; }
	public bool ATGVisuals { get; set; }

	public override void ResetEffects() {
		ATG = false;
		ATGVisuals = false;
	}

	private void TryFireATGMissile(NPC target, int damage) {
		if (ATG && Main.rand.NextFloat() > ATGMissileChance) {
			return;
		}

		NPC closestNPC = NPCHelpers.FindClosestNPC(Player.MountedCenter, ATGTargetRange, true);
		if (closestNPC is null) {
			return;
		}

		IEntitySource source = Player.GetSource_OnHit(target);
		Vector2 position = Player.MountedCenter + new Vector2(4f * Player.direction, -18f);
		Vector2 velocity = Player.DirectionTo(closestNPC.Center) * ATGMissile.Speed;
		Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ATGMissile>(), (int)(damage * 1.8f), 4f, Player.whoAmI, ai1: closestNPC.whoAmI);
	}

	public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
		TryFireATGMissile(target, damageDone);
	}

	public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
		// guh!!!
		if (proj.type != ModContent.ProjectileType<ATGMissile>()) {
			TryFireATGMissile(target, damageDone);
		}
	}
}

public class ATGShoulderDrawLayer : PlayerDrawLayer
{
	private readonly Asset<Texture2D> frameTexture = ModContent.Request<Texture2D>("EveryoneIsHere/RiskOfRain/Content/Items/Accessories/ATGMissileMk1_BodySpriteFrame");
	private readonly Asset<Texture2D> launcherTexture = ModContent.Request<Texture2D>("EveryoneIsHere/RiskOfRain/Content/Items/Accessories/ATGMissileMk1_BodySpriteLauncher");

	public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Torso);

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.GetModPlayer<ATGPlayer>().ATGVisuals;

	protected override void Draw(ref PlayerDrawSet drawInfo) {
		Vector2 frameDrawPosition = drawInfo.Center.ToScreenCoordinates().Floor();
		Rectangle frameSourceRect = drawInfo.drawPlayer.bodyFrame;
		Color drawColor = drawInfo.colorArmorBody;
		float frameRotation = drawInfo.drawPlayer.bodyRotation;
		Vector2 frameOrigin = drawInfo.bodyVect;

		drawInfo.DrawDataCache.Add(new DrawData(frameTexture.Value, frameDrawPosition, frameSourceRect, drawColor, frameRotation, frameOrigin, 1f, drawInfo.playerEffect));

		Vector2 launcherWorldPosition = drawInfo.Center + new Vector2(4f * drawInfo.drawPlayer.direction, -15f);
		Vector2 launcherDrawPosition = launcherWorldPosition.ToScreenCoordinates().Floor();
		Rectangle launcherSourceRect = new(0, 0, launcherTexture.Width(), launcherTexture.Height());
		Vector2 launcherOrigin = launcherSourceRect.Size() / 2f;

		NPC closestNPC = NPCHelpers.FindClosestNPC(drawInfo.drawPlayer.MountedCenter, ATGPlayer.ATGTargetRange, true);
		float launcherRotation = drawInfo.drawPlayer.direction == 1 ? 0f : MathHelper.Pi;
		if (closestNPC is not null) {
			launcherRotation = launcherWorldPosition.AngleTo(closestNPC.Center);
		}

		SpriteEffects launcherEffects = Math.Sign(launcherRotation.ToRotationVector2().X) > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

		drawInfo.DrawDataCache.Add(new DrawData(launcherTexture.Value, launcherDrawPosition, launcherSourceRect, drawColor, launcherRotation, launcherOrigin, 1f, launcherEffects));
	}
}

public class ATGMissile : ModProjectile
{
	public const float Speed = 15f;
	private const float HomingStrength = 5f;

	private AI_State State {
		get => (AI_State)Projectile.ai[0];
		set => Projectile.ai[0] = (float)value;
	}

	private NPC Target => Main.npc[(int)Projectile.ai[1]];

	public override string Texture => "EveryoneIsHere/RiskOfRain/Content/Items/Accessories/ATGMissileMk1_Missile";

	public override void SetStaticDefaults() {
		ProjectileID.Sets.CultistIsResistantTo[Type] = true;
	}

	public override void SetDefaults() {
		// Base properties
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.timeLeft = 120;

		// Weapon properties
		Projectile.penetrate = 1;
		Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Generic;
	}

	public override void OnSpawn(IEntitySource source) {
		State = AI_State.Homing;
	}

	private void AI_NoTarget() { }

	private void AI_Homing() {
		// Validate our target
		if (!Target.CanBeChasedBy()) {
			State = AI_State.NoTarget;
			Projectile.timeLeft = 120;
			return;
		}

		// Home in on the target
		Vector2 toTarget = Projectile.DirectionTo(Target.Center);
		Projectile.velocity += toTarget * HomingStrength;
		Projectile.velocity = Projectile.velocity.ClampLength(0f, Speed);

		Projectile.timeLeft = 2;
	}

	public override void AI() {
		switch (State) {
			case AI_State.Homing:
				AI_Homing();
				break;
			case AI_State.NoTarget:
				AI_NoTarget();
				break;
		}

		Projectile.rotation = Projectile.velocity.ToRotation();

		// Fire dust trail
		Dust fireDust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch);
		fireDust.velocity = Main.rand.NextVector2Circular(1f, 1f);
		fireDust.noGravity = true;

		// Smoke dust trail
		if (Main.rand.NextBool()) {
			Dust smokeDust = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke);
			smokeDust.velocity = Main.rand.NextVector2Circular(1f, 1f);
			smokeDust.noGravity = true;
		}
	}

	public override void Kill(int timeLeft) {
		// Dust explosion
		for (int i = 0; i < 13; i++) {
			Dust fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
			fireDust.velocity = Main.rand.NextVector2Circular(5f, 5f);
			fireDust.noGravity = true;
		}

		// Smoke explosion
		for (int i = 0; i < 10; i++) {
			Dust fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
			fireDust.velocity = Main.rand.NextVector2Circular(5f, 5f);
			fireDust.noGravity = true;
		}

		SoundEngine.PlaySound(SoundID.Item13 /* Rocket explosion */, Projectile.Center);
	}

	private enum AI_State
	{
		Homing,
		NoTarget
	}
}