﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.NPCs.XiConstruct;

public class XiConstruct : ModNPC
{
	private const float MaxTargetRange = 200f * 16f;
	private const float RotationStrength = 0.08f;

	public static Asset<Texture2D> PanelTexture => ModContent.Request<Texture2D>("EveryoneIsHere/RiskOfRain/Content/NPCs/XiConstruct/XiConstruct_Panel");

	private XiConstructPanel backLeftPanel;
	private XiConstructPanel backRightPanel;
	private XiConstructPanel frontLeftPanel;
	private XiConstructPanel frontRightPanel;

	private Player Target => Main.player[NPC.target];

	private AIState State {
		get => (AIState)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

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

		float targetRotation = NPC.AngleTo(Target.MountedCenter);
		float oldRotation = NPC.rotation;
		NPC.rotation = NPC.rotation.AngleLerp(targetRotation, RotationStrength);
		float rotateChildrenBy = NPC.rotation - oldRotation;
		frontLeftPanel.RotateAroundParent(rotateChildrenBy);
		frontRightPanel.RotateAroundParent(rotateChildrenBy);
		backLeftPanel.RotateAroundParent(rotateChildrenBy);
		backRightPanel.RotateAroundParent(rotateChildrenBy);

		Dust.QuickDustLine(NPC.Center, NPC.Center + NPC.rotation.ToRotationVector2() * 100f, 10f, Color.White);
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
		}

		if (NPC.localAI[0] % 600 == 0) {
			frontLeftPanel.SetNextAnimationTarget(new Vector2(20f, -20f), -0.2f);
			frontRightPanel.SetNextAnimationTarget(new Vector2(20f, 20f), 0.2f);
			backLeftPanel.SetNextAnimationTarget(new Vector2(-10f, -16f), -0.5f);
			backRightPanel.SetNextAnimationTarget(new Vector2(-10f, 16f), 0.5f);
		}

		if (NPC.localAI[0] % 600 == 300) {
			frontLeftPanel.ReturnToIdle();
			frontRightPanel.ReturnToIdle();
			backLeftPanel.ReturnToIdle();
			backRightPanel.ReturnToIdle();
		}

		NPC.localAI[0]++;

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
	}

	private class XiConstructPanel
	{
		private Vector2 idlePosition;
		private readonly float idleRotation;

		private Vector2 curPositionOffset;
		private Vector2 nextPositionOffset;

		public float curRotationOffset;
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

	private enum AIState
	{
		NoTarget,
		Idle,
		ResummonAlphaConstruct,
		CreateBarrier,
		LaserCannon,
		LaserBurst,
		Barrage
	}
}