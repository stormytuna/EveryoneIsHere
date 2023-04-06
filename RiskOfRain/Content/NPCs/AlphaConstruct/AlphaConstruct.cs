using System;
using System.Collections.Generic;
using EveryoneIsHere.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace EveryoneIsHere.RiskOfRain.Content.NPCs.AlphaConstruct;

public class AlphaConstruct : ModNPC
{
	public enum AIState
	{
		Hiding,
		Attacking
	}

	private const float TargetRange = 30f * 16f;
	private const float ShootDelayMax = 5f * 60f;
	private const float ProjShootSpeed = 8f;
	private const int ProjDamage = 60;
	private const float ProjKnockback = 2f;
	public static readonly Vector3 LightColor = new(254f / 255f, 254f / 255f, 118f / 255f);

	private readonly Asset<Texture2D> glowTexture = ModContent.Request<Texture2D>("EveryoneIsHere/RiskOfRain/Content/NPCs/AlphaConstruct/AlphaConstruct_Glow");

	private Player Target => Main.player[NPC.target];

	private AIState State {
		get => (AIState)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float ShootDelay => ref NPC.ai[1];

	private Projectile InnerBarrier {
		get => Main.projectile[(int)NPC.ai[2]];
		set => NPC.ai[2] = value.whoAmI;
	}

	private Projectile OuterBarrier {
		get => Main.projectile[(int)NPC.ai[3]];
		set => NPC.ai[3] = value.whoAmI;
	}

	public override void SetStaticDefaults() {
		Main.npcFrameCount[Type] = 2;
		NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new(0) {
			Frame = (int)AnimationFrame.Exposed
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
	}

	public override void SetDefaults() {
		// Base properties
		NPC.width = 38;
		NPC.height = 44;
		NPC.aiStyle = -1;
		NPC.value = 800f /* 8 silver */;

		// Enemy properties
		NPC.damage = 40;
		NPC.defense = 55;
		NPC.lifeMax = 550;
		NPC.knockBackResist = 0f;

		// Other properties
		NPC.behindTiles = true;
	}

	public override void OnSpawn(IEntitySource source) {
		State = AIState.Hiding;
		NPC.ai[2] = Main.maxProjectiles;
		NPC.ai[3] = Main.maxProjectiles;
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo) {
		bool isInSurfaceSnow = Main.LocalPlayer.ZoneSnow && Main.LocalPlayer.ZoneOverworldHeight;
		if (!isInSurfaceSnow || !Main.hardMode) {
			return 0f;
		}

		return SpawnCondition.OverworldDaySlime.Chance;
	}

	private void AI_NoTarget() {
		NPC.TargetClosest();
		if (!NPC.HasValidTarget) {
			return;
		}

		if (NPC.WithinRange(Target.Center, TargetRange)) {
			ShootDelay = ShootDelayMax;
			State = AIState.Attacking;
			SoundEngine.PlaySound(EveryoneIsHereSounds.AlphaConstruct_Open, NPC.Center);
		}

		NPC.immortal = true;

		Lighting.AddLight(NPC.Center, LightColor * 0.4f);

		if (!InnerBarrier.active) {
			InnerBarrier = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<AlphaConstructAuraInner>(), 0, 0f, Main.myPlayer, NPC.whoAmI);
		}

		if (!OuterBarrier.active) {
			OuterBarrier = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<AlphaConstructAuraOuter>(), 0, 0f, Main.myPlayer, NPC.whoAmI);
		}
	}

	private void AI_Attacking() {
		if (!NPC.HasValidTarget || !NPC.WithinRange(Target.Center, TargetRange * 1.3f)) {
			NPC.TargetClosest();
			if (!NPC.HasValidTarget || !NPC.WithinRange(Target.Center, TargetRange)) {
				State = AIState.Hiding;
				SoundEngine.PlaySound(EveryoneIsHereSounds.AlphaConstruct_Hide, NPC.Center);
				return;
			}
		}

		NPC.immortal = false;

		ShootDelay--;

		if (ShootDelay == 60f) {
			SoundEngine.PlaySound(EveryoneIsHereSounds.AlphaConstruct_ChargeUp, NPC.Center);
		}

		if (ShootDelay <= 60f) {
			for (int i = 0; i < 3; i++) {
				if (!(Main.rand.NextFloat() < i / 60f)) {
					continue;
				}

				Dust chargeDust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.SandstormInABottle);
				chargeDust.scale = Main.rand.NextFloat(0.6f, 1f);
				chargeDust.velocity = Vector2.Zero;
			}
		}

		if (ShootDelay <= 0f) {
			ShootDelay = ShootDelayMax;
			Vector2 velocity = NPC.DirectionTo(Target.Center) * ProjShootSpeed;
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0f, 8f), velocity, ModContent.ProjectileType<AlphaConstructProjectile>(), ProjDamage, ProjKnockback,
				Main.myPlayer);
			SoundEngine.PlaySound(EveryoneIsHereSounds.AlphaConstruct_Shoot, NPC.Center);
		}

		Lighting.AddLight(NPC.Center, LightColor * 0.7f);
	}

	public override void AI() {
		switch (State) {
			case AIState.Hiding:
				AI_NoTarget();
				break;
			case AIState.Attacking:
				AI_Attacking();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public override void FindFrame(int frameHeight) {
		switch (State) {
			case AIState.Hiding:
				NPC.frame.Y = (int)AnimationFrame.Hiding * frameHeight;
				break;
			case AIState.Attacking:
				NPC.frame.Y = (int)AnimationFrame.Exposed * frameHeight;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public override bool? CanBeHitByItem(Player player, Item item) => State == AIState.Hiding ? false : base.CanBeHitByItem(player, item);

	public override bool? CanBeHitByProjectile(Projectile projectile) => State == AIState.Hiding ? false : base.CanBeHitByProjectile(projectile);

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		Vector2 drawPosition = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY + 4f);
		Vector2 origin = NPC.frame.Size() / 2f;
		spriteBatch.Draw(glowTexture.Value, drawPosition, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0);
	}

	public override void HitEffect(NPC.HitInfo hit) {
		if (NPC.life > 0) {
			return;
		}

		SoundEngine.PlaySound(EveryoneIsHereSounds.AlphaConstruct_Death, NPC.Center);

		if (Main.netMode == NetmodeID.Server) {
			return;
		}

		Gore.NewGore(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(6f, 6f), Mod.Find<ModGore>("AlphaConstruct_Gore01").Type);
		Gore.NewGore(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(6f, 6f), Mod.Find<ModGore>("AlphaConstruct_Gore02").Type);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
		bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
			new FlavorTextBestiaryInfoElement("Mods.EveryoneIsHere.NPCs.AlphaConstruct.BestiaryFlavorText"),
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow
		});
	}

	private enum AnimationFrame
	{
		Exposed,
		Hiding
	}
}

public class AlphaConstructAuraInner : AuraProjectile
{
	public override string Texture => "EveryoneIsHere/RiskOfRain/Content/NPCs/AlphaConstruct/AlphaConstruct_AuraInner";

	private NPC Parent => Main.npc[(int)Projectile.ai[0]];

	public override bool ShouldKillProjectile() => !Parent.active || Parent.ai[0] != (float)AlphaConstruct.AIState.Hiding;

	public override Vector2 GetCenter() => Main.npc[(int)Projectile.ai[0]].Center + new Vector2(0f, 8f);

	public override void SafeSetDefaults() {
		AuraDrawColor = new Color(254, 191, 16);
		AuraDrawColorMultiplier = 1f;
	}
}

public class AlphaConstructAuraOuter : AuraProjectile
{
	public override string Texture => "EveryoneIsHere/RiskOfRain/Content/NPCs/AlphaConstruct/AlphaConstruct_AuraOuter";

	private NPC Parent => Main.npc[(int)Projectile.ai[0]];

	public override bool ShouldKillProjectile() => !Parent.active || Parent.ai[0] != (float)AlphaConstruct.AIState.Hiding;

	public override Vector2 GetCenter() => Main.npc[(int)Projectile.ai[0]].Center + new Vector2(0f, 8f);

	public override void SafeSetDefaults() {
		AuraDrawColor = new Color(251, 254, 47);
		AuraDrawColorMultiplier = 1f;
	}
}