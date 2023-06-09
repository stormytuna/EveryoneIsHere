﻿using System.IO;
using EveryoneIsHere.RiskOfRain.Content.NPCs.AlphaConstruct;
using EveryoneIsHere.RiskOfRain.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.NPCs.XiConstruct;

// WIP, might never be finished :shrug:

public class XiConstruct : ModNPC
{
	public override bool IsLoadingEnabled(Mod mod) => false;

	private const float MaxTargetRange = 200f * 16f;

	private const int Barrage_StartupTime = 50;
	private const int Barrage_StartupForSweep = Barrage_StartupTime - 20;
	private const int Barrage_NumShots = 8;
	private const int Barrage_TimeBetweenShots = 10;
	private const int Barrage_NumBarrages = 4;
	private const int Barrage_TimeBetweenBarrages = 30;
	private const float Barrage_SweepAngle = 0.261799f; // 15 degrees

	private float Barrage_TotalActiveTimeForOneBarrage => Barrage_NumShots * Barrage_TimeBetweenShots;

	private const int LaserBurst_StartupTime = 90;
	private const int LaserBurst_StartupTimeForTelegraph = 10;
	private const int LaserBurst_TargetChaseCutoff = 75;
	private const int LaserBurst_LaserActiveTime = 40;
	private const int LaserBurst_NumLasers = 4;

	private const int LaserCannon_StartupTime = 150;
	private const int LaserCannon_StartupTimeForTelegraph = 10;
	private const int LaserCannon_TargetChaseCutoff = 110;
	private const int LaserCannon_LaserActiveTime = 360;
	private const float LaserCannon_LaserRotationStrength = 0.001f;

	private float RotationStrength => State switch {
		AIState.Barrage or AIState.LaserBurst => 0.15f,
		_ => 0.08f
	};

	private static Asset<Texture2D> PanelTexture => ModContent.Request<Texture2D>("EveryoneIsHere/RiskOfRain/Content/NPCs/XiConstruct/XiConstruct_Panel");

	private XiConstructPanel backLeftPanel;
	private XiConstructPanel backRightPanel;
	private XiConstructPanel frontLeftPanel;
	private XiConstructPanel frontRightPanel;

	private Player Target => Main.player[NPC.target];

	private AIState State {
		get => (AIState)NPC.ai[0];
		set {
			NPC.ai[0] = (float)value;
			timer = 0f;
		}
	}

	private float timer;

	private ref float Barrage_BarrageTimer => ref NPC.ai[1];

	private ref float Barrage_BarrageCounter => ref NPC.ai[2];

	private Projectile LaserBarrage_Laser {
		get => Main.projectile[(int)NPC.ai[1]];
		set => NPC.ai[1] = value.whoAmI;
	}

	private ref float LaserBarrage_LaserTimer => ref NPC.ai[2];
	private ref float LaserBarrage_LaserCounter => ref NPC.ai[3];

	private Projectile LaserCannon_Laser {
		get => Main.projectile[(int)NPC.ai[1]];
		set => NPC.ai[1] = value.whoAmI;
	}

	private ref float LaserCannon_LaserTimer => ref NPC.ai[2];

	public override void SetStaticDefaults() {
		NPCID.Sets.DontDoHardmodeScaling[Type] = true;
		NPCID.Sets.CantTakeLunchMoney[Type] = true;
		NPCID.Sets.BossBestiaryPriority.Add(Type);

		NPCDebuffImmunityData debuffData = new() {
			ImmuneToAllBuffsThatAreNotWhips = true
		};
		NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);
	}

	public override void SetDefaults() {
		// TODO: Make these more appropriate values
		// Base properties
		NPC.width = 100;
		NPC.height = 100;
		NPC.aiStyle = -1;
		NPC.value = 10f;
		NPC.noGravity = true;

		// Enemy properties
		NPC.damage = 10;
		NPC.defense = 80;
		NPC.lifeMax = 100000;
		NPC.knockBackResist = 0f;

		// Boss properties
		NPC.boss = true;
		NPC.netAlways = true;
		NPC.npcSlots = 10f;
		if (!Main.dedServ) {
			Music = MusicID.Boss1;
		}
	}

	public override void OnSpawn(IEntitySource source) {
		State = AIState.NoTarget;
	}

	private void TryInitialisePanels() {
		backLeftPanel ??= new XiConstructPanel(new Vector2(-32, -32), 0f);
		backRightPanel ??= new XiConstructPanel(new Vector2(-32, 32), 3 * MathHelper.PiOver2);
		frontLeftPanel ??= new XiConstructPanel(new Vector2(32, -32), MathHelper.PiOver2);
		frontRightPanel ??= new XiConstructPanel(new Vector2(32, 32), MathHelper.Pi);
	}

	private bool ValidateTarget() {
		bool withinRange = NPC.WithinRange(Target.MountedCenter, MaxTargetRange);
		if (withinRange && NPC.HasValidTarget) {
			return true;
		}

		State = AIState.NoTarget;
		return false;
	}

	private void FaceTarget(Vector2 targetPos) {
		float targetRotation = NPC.AngleTo(targetPos);
		float oldRotation = NPC.rotation;
		NPC.rotation = NPC.rotation.AngleLerp(targetRotation, RotationStrength);
		float rotateChildrenBy = NPC.rotation - oldRotation;

		frontLeftPanel.RotateAroundParent(rotateChildrenBy);
		frontRightPanel.RotateAroundParent(rotateChildrenBy);
		backLeftPanel.RotateAroundParent(rotateChildrenBy);
		backRightPanel.RotateAroundParent(rotateChildrenBy);
	}

	private void AI_NoTarget() {
		Main.NewText("No Target!");

		NPC.TargetClosest();
		if (NPC.HasValidTarget) {
			State = AIState.Idle;
			return;
		}

		// TODO: Fix despawn code
		NPC.EncourageDespawn(30);
	}

	private void AI_Idle() {
		Main.NewText("Idle!");

		if (!ValidateTarget()) {
			return;
		}

		FaceTarget(Target.MountedCenter);

		timer++;
		if (timer >= 180f) {
			State = AIState.LaserCannon;
		}

		frontLeftPanel.ReturnToIdle();
		frontRightPanel.ReturnToIdle();
		backLeftPanel.ReturnToIdle();
		backRightPanel.ReturnToIdle();

		//Dust.QuickDustLine(NPC.Center, NPC.Center + NPC.rotation.ToRotationVector2() * 100f, 10f, Color.White);
	}

	private void AI_Barrage() {
		Main.NewText("Barrage!");

		if (!ValidateTarget()) {
			return;
		}

		if (timer == 0f) {
			Barrage_BarrageTimer = 0f;
			Barrage_BarrageCounter = 0f;
		}

		// Sets our rotation target, this funky stuff controls the rotational sweep across our target while firing
		float rotation = 0f;
		if (Barrage_BarrageTimer > Barrage_StartupForSweep) {
			bool isSweepFlipped = Barrage_BarrageCounter % 2 == 0;
			float sweepStartAngle = isSweepFlipped ? Barrage_SweepAngle : -Barrage_SweepAngle;
			float sweepEndAngle = isSweepFlipped ? -Barrage_SweepAngle : Barrage_SweepAngle;
			float lerpAmount = (Barrage_BarrageTimer - Barrage_StartupForSweep) / Barrage_TotalActiveTimeForOneBarrage;
			rotation = MathHelper.Lerp(sweepStartAngle, sweepEndAngle, lerpAmount);
			rotation = MathHelper.Clamp(rotation, -Barrage_SweepAngle, Barrage_SweepAngle);
		}

		Vector2 toTarget = Target.MountedCenter - NPC.Center;
		Vector2 target = NPC.Center + toTarget.RotatedBy(rotation);
		FaceTarget(target);

		// This controls actually firing, we only want to fire during our 'active' time, between our startup and wind down, and also only on frames we're actually allowed to shoot
		if (Barrage_BarrageTimer >= Barrage_StartupTime && Barrage_BarrageTimer - Barrage_StartupTime <= Barrage_TotalActiveTimeForOneBarrage &&
		    Barrage_BarrageTimer % Barrage_TimeBetweenShots == 0f) {
			Vector2 velocity = NPC.DirectionTo(target) * 10f;
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ModContent.ProjectileType<AlphaConstructProjectile>(), 10, 3f, Main.myPlayer);
			frontLeftPanel.AddRecoil(velocity * -0.5f, 0f);
			frontRightPanel.AddRecoil(velocity * -0.5f, 0f);
			backLeftPanel.AddRecoil(velocity * -0.5f, 0f);
			backRightPanel.AddRecoil(velocity * -0.5f, 0f);
		}

		// This controls what happens when we've finished any given barrage, either start our next or move onto our next state
		if (Barrage_BarrageTimer > Barrage_TotalActiveTimeForOneBarrage + Barrage_TimeBetweenBarrages) {
			Barrage_BarrageCounter++;
			Barrage_BarrageTimer = Barrage_StartupForSweep;
			if (Barrage_BarrageCounter >= Barrage_NumBarrages) {
				State = AIState.Idle;
				timer = 0f;
				Barrage_BarrageTimer = 0f;
				Barrage_BarrageCounter = 0f;
			}
		}

		frontLeftPanel.SetNextAnimationTarget(new Vector2(5f, -10f), -0.15f);
		frontRightPanel.SetNextAnimationTarget(new Vector2(5f, 10f), 0.15f);
		backLeftPanel.SetNextAnimationTarget(new Vector2(0f, -8f), -0.3f);
		backRightPanel.SetNextAnimationTarget(new Vector2(0f, 8f), 0.3f);

		timer++;
		Barrage_BarrageTimer++;

		Dust.QuickDust(target, Color.Red);
	}

	private void AI_LaserBurst() {
		Main.NewText("Laser Burst!");

		if (!ValidateTarget()) {
			return;
		}

		if (timer < LaserBurst_TargetChaseCutoff) {
			FaceTarget(Target.MountedCenter);
		}

		LaserBarrage_Laser.rotation = NPC.rotation;

		if (timer == LaserBurst_StartupTimeForTelegraph) {
			LaserBarrage_Laser = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<XiConstructLaserBurst>(), 10, 2f, Main.myPlayer,
				NPC.whoAmI);
		}

		if (timer == LaserBurst_StartupTime) {
			// Our laser uses ai[2] to know whether or not to be a telegraph line or laser line
			LaserBarrage_Laser.ai[2] = 1f;
		}

		if (timer == LaserBurst_StartupTime + LaserBurst_LaserActiveTime) {
			// Our laser uses ai[2] to know whether or not to be a telegraph line or laser line
			timer = LaserBurst_StartupTimeForTelegraph;
			LaserBarrage_Laser.ai[2] = 0f;
			LaserBarrage_LaserCounter++;

			// TODO: Maybe explosion

			if (LaserBarrage_LaserCounter >= LaserBurst_NumLasers) {
				timer = 0f;
				LaserBarrage_Laser.Kill();
				NPC.ai[1] = 0f; // Can't exactly assign Projectile as float :)
				LaserBarrage_LaserCounter = 0f;
				LaserBarrage_LaserTimer = 0f;
				State = AIState.Idle;
			}
		}

		frontLeftPanel.SetNextAnimationTarget(new Vector2(10f, -10f), 0f);
		frontRightPanel.SetNextAnimationTarget(new Vector2(10f, 10f), 0f);
		backLeftPanel.SetNextAnimationTarget(new Vector2(-10f, -10f), 0f);
		backRightPanel.SetNextAnimationTarget(new Vector2(-10f, 10f), 0f);

		timer++;
	}

	private void AI_LaserCannon() {
		Main.NewText("Laser Cannon!");

		if (!ValidateTarget()) {
			return;
		}

		if (timer < LaserCannon_TargetChaseCutoff) {
			FaceTarget(Target.MountedCenter);
		}

		LaserCannon_Laser.rotation = NPC.rotation;

		if (timer == LaserCannon_StartupTimeForTelegraph) {
			LaserCannon_Laser = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<XiConstructLaserBurst>(), 10, 2f, Main.myPlayer,
				NPC.whoAmI);
		}

		if (timer == LaserCannon_StartupTime) {
			// Our laser uses ai[2] to know whether or not to be a telegraph line or laser line
			LaserCannon_Laser.ai[2] = 1f;
		}

		if (timer >= LaserCannon_StartupTime) {
			// TODO: Slightly rotate towards the player 
		}

		if (timer == LaserCannon_StartupTime + LaserCannon_LaserActiveTime) {
			timer = 0f;
			LaserCannon_Laser.Kill();
			NPC.ai[1] = 0f; // Can't exactly assign Projectile as float :)
			LaserBarrage_LaserCounter = 0f;
			LaserBarrage_LaserTimer = 0f;
			State = AIState.Idle;
		}

		frontLeftPanel.SetNextAnimationTarget(new Vector2(15f, -15f), 0f);
		frontRightPanel.SetNextAnimationTarget(new Vector2(15f, 15f), 0f);
		backLeftPanel.SetNextAnimationTarget(new Vector2(-15f, -15f), 0f);
		backRightPanel.SetNextAnimationTarget(new Vector2(-15f, 15f), 0f);

		timer++;
	}

	public override void AI() {
		TryInitialisePanels();

		switch (State) {
			case AIState.NoTarget:
				AI_NoTarget();
				break;
			case AIState.Idle:
				AI_Idle();
				break;
			case AIState.Barrage:
				AI_Barrage();
				break;
			case AIState.LaserBurst:
				AI_LaserBurst();
				break;
			case AIState.LaserCannon:
				AI_LaserCannon();
				break;
		}

		backLeftPanel.Animate();
		backRightPanel.Animate();
		frontLeftPanel.Animate();
		frontRightPanel.Animate();
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		Main.instance.LoadNPC(Type);
		Texture2D orbTexture = TextureAssets.Npc[Type].Value;
		Vector2 orbDrawPosition = NPC.Center - screenPos;
		Rectangle orbSourceRect = new(0, 0, orbTexture.Width, orbTexture.Height);
		Vector2 orbOrigin = orbSourceRect.Size() / 2f;

		spriteBatch.Draw(orbTexture, orbDrawPosition, orbSourceRect, drawColor, 0f, orbOrigin, 1f, SpriteEffects.None, 0);

		backLeftPanel?.Draw(spriteBatch, orbDrawPosition, drawColor);
		backRightPanel?.Draw(spriteBatch, orbDrawPosition, drawColor);
		frontLeftPanel?.Draw(spriteBatch, orbDrawPosition, drawColor);
		frontRightPanel?.Draw(spriteBatch, orbDrawPosition, drawColor);

		return false;

		Main.instance.LoadNPC(Type);
		Texture2D texture = TextureAssets.Npc[Type].Value;
		Vector2 drawPosition = NPC.Center - Main.screenPosition;
		Rectangle sourceRect = texture.Frame();
		float rotation = NPC.rotation;
		Vector2 origin = texture.Size() / 2f;
		float scale = NPC.scale;
		SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		spriteBatch.Draw(texture, drawPosition, sourceRect, drawColor, rotation, origin, scale, effects, 0);
	}

	private class XiConstructPanel
	{
		private Vector2 idlePosition;
		private readonly float idleRotation;

		private Vector2 curPositionOffset;
		private Vector2 nextPositionOffset;

		private float curRotationOffset;
		private float nextRotationOffset;

		private float rotationAroundParent;

		public XiConstructPanel(Vector2 idleOffsetFromParent, float startRotationOffset) {
			idlePosition = idleOffsetFromParent;
			idleRotation = startRotationOffset;

			curPositionOffset = Vector2.Zero;
			nextPositionOffset = Vector2.Zero;
			curRotationOffset = 0f;
			nextRotationOffset = 0f;

			rotationAroundParent = 0f;
		}

		public void SetNextAnimationTarget(Vector2 nextPosition, float nextRotation) {
			nextPositionOffset = nextPosition;
			nextRotationOffset = nextRotation;
		}

		public void ReturnToIdle() => SetNextAnimationTarget(Vector2.Zero, 0f);

		public void AddRecoil(Vector2 velocity, float rotation) {
			curPositionOffset += velocity;
			curRotationOffset += rotation;
		}

		public void RotateAroundParent(float amount) {
			idlePosition = idlePosition.RotatedBy(amount);
			rotationAroundParent += amount;
		}

		public void Animate() {
			curPositionOffset = Vector2.Lerp(curPositionOffset, nextPositionOffset, 0.05f);
			curRotationOffset = curRotationOffset.AngleLerp(nextRotationOffset, 0.05f);
		}

		public void Draw(SpriteBatch spriteBatch, Vector2 parentScreenPosition, Color drawColor) {
			Texture2D texture = PanelTexture.Value;
			Vector2 drawPosition = parentScreenPosition + idlePosition + curPositionOffset.RotatedBy(rotationAroundParent);
			Rectangle sourceRect = new(0, 0, texture.Width, texture.Height);
			float rotation = idleRotation + curRotationOffset + rotationAroundParent;
			Vector2 origin = sourceRect.Size() / 2f;

			spriteBatch.Draw(texture, drawPosition, sourceRect, drawColor, rotation, origin, 1f, SpriteEffects.None, 0);
		}
	}

	public override void SendExtraAI(BinaryWriter writer) {
		writer.Write(timer);
	}

	public override void ReceiveExtraAI(BinaryReader reader) {
		timer = reader.ReadSingle();
	}

	private enum AIState
	{
		NoTarget,
		Idle,
		CreateBarrier,
		LaserCannon,
		LaserBurst,
		Barrage
	}
}