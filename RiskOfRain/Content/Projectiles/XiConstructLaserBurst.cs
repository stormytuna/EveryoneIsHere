using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.Projectiles;

public class XiConstructLaserBurst : ModProjectile
{
	private const float MaxBeamLength = 2400f;
	private const float BeamTileCollisionWidth = 1f;
	private const float BeamHitboxCollisionWidth = 24f;
	private const int NumSamplePoints = 3;
	private const float BeamLengthChangeFactor = 0.75f;

	private const float VisualEffectThreshold = 0.8f;

	private const float OuterBeamOpacityMultiplier = 0.75f;
	private const float InnerBeamOpacityMultiplier = 0.15f;

	private const float BeamLightBrightness = 0.8f;
	private const float BeamColorHue = 0.11f;
	private const float BeamColorSaturation = 0.8f;
	private const float BeamColorLightness = 0.53f;

	private NPC Parent => Main.npc[(int)Projectile.ai[0]];

	private ref float BeamLength => ref Projectile.ai[1];

	// 0f == telegraph line
	// 1f == actual laser line
	private ref float State => ref Projectile.ai[2];

	private Color GetOuterBeamColor() {
		Color c = Main.hslToRgb(BeamColorHue, BeamColorSaturation, BeamColorLightness);
		c.A = 150; // Manually reduce alpha so beams can overlap seamlessly
		return c;
	}

	private Color GetInnerBeamColor() => Color.White;

	public override void SetDefaults() {
		// Base properties
		Projectile.width = 18;
		Projectile.height = 18;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.alpha = 255;
		Projectile.scale = 0f;

		// Enemy properties
		Projectile.hostile = true;
	}

	private float PerformBeamHitscan() {
		Vector2 samplingPoint = Projectile.Center;

		float[] laserScanResults = new float[NumSamplePoints];
		Collision.LaserScan(samplingPoint, Projectile.velocity, BeamTileCollisionWidth * Projectile.scale, MaxBeamLength, laserScanResults);
		float averageLengthSample = 0f;
		for (int i = 0; i < laserScanResults.Length; ++i) {
			averageLengthSample += laserScanResults[i];
		}

		averageLengthSample /= NumSamplePoints;

		return averageLengthSample;
	}

	private void ProduceBeamDust(Color beamColor) {
		if (Projectile.scale < VisualEffectThreshold) {
			return;
		}

		const int type = 76;
		Vector2 startPosition = Projectile.Center;
		Vector2 endPosition = Projectile.Center + Projectile.velocity * (BeamLength - 14.5f * Projectile.scale);

		// Dust along the laser line
		Vector2 laserLine = endPosition - startPosition;
		int numLaserDust = (int)laserLine.Length() / 200;
		for (int i = 0; i < numLaserDust; i++) {
			Vector2 dustPosition = startPosition + laserLine * Main.rand.NextFloat();
			float angle = Projectile.rotation + (Main.rand.NextBool() ? 1f : -1f) * MathHelper.PiOver2;
			float startDistance = Main.rand.NextFloat(1f, 1.8f);
			float scale = Main.rand.NextFloat(0.7f, 1.1f);
			Vector2 velocity = angle.ToRotationVector2() * startDistance;
			Dust dust = Dust.NewDustPerfect(dustPosition, type, velocity, 0, beamColor, scale);
			dust.velocity = velocity;
			dust.color = beamColor;
			dust.noGravity = true;
		}

		int numEndDust = 1;
		for (int i = 0; i < numEndDust; i++) {
			// Main.rand.NextBool is used to give a 50/50 chance for the angle to point to the left or right.
			// This gives the dust a 50/50 chance to fly off on either side of the beam.
			float angle = Projectile.rotation + (Main.rand.NextBool() ? 1f : -1f) * MathHelper.PiOver2;
			float startDistance = Main.rand.NextFloat(1f, 1.8f);
			float scale = Main.rand.NextFloat(0.7f, 1.1f);
			Vector2 velocity = angle.ToRotationVector2() * startDistance;
			Dust dust = Dust.NewDustPerfect(endPosition, type, velocity, 0, beamColor, scale);
			dust.velocity = velocity;
			dust.color = beamColor;
			dust.noGravity = true;

			// If the beam is currently large, make the dust faster and larger to match.
			if (Projectile.scale > 1f) {
				dust.velocity *= Projectile.scale;
				dust.scale *= Projectile.scale;
			}
		}
	}

	private void ProduceWaterRipples(Vector2 beamDims) {
		WaterShaderData shaderData = (WaterShaderData)Filters.Scene["WaterDistortion"].GetShader();

		// A universal time-based sinusoid which updates extremely rapidly.
		float waveSine = 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f);
		Vector2 ripplePos = Projectile.position + new Vector2(beamDims.X * 0.5f, 0f).RotatedBy(Projectile.rotation);

		// WaveData is encoded as a Color. Not really sure why.
		Color waveData = new Color(0.5f, 0.1f * Math.Sign(waveSine) + 0.5f, 0f, 1f) * Math.Abs(waveSine);
		shaderData.QueueRipple(ripplePos, waveData, beamDims, RippleShape.Square, Projectile.rotation);
	}

	public override void AI() {
		Projectile.Center = Parent.Center;

		Projectile.velocity = Projectile.rotation.ToRotationVector2();
		float hitscanBeamLength = PerformBeamHitscan();
		BeamLength = MathHelper.Lerp(BeamLength, hitscanBeamLength, BeamLengthChangeFactor);

		Vector2 beamDims = new(Projectile.velocity.Length() * BeamLength, Projectile.width * Projectile.scale);

		Color beamColor = GetOuterBeamColor();

		if (Projectile.scale > VisualEffectThreshold) {
			ProduceBeamDust(beamColor);
			if (Main.netMode != NetmodeID.Server) {
				ProduceWaterRipples(beamDims);
			}
		}

		Projectile.alpha -= 10;
		if (Projectile.alpha < 0) {
			Projectile.alpha = 0;
		}

		float targetScale = State == 0f ? 0.2f : 1.4f;
		Projectile.scale = MathHelper.Lerp(Projectile.scale, targetScale, 0.2f);

		DelegateMethods.v3_1 = beamColor.ToVector3() * BeamLightBrightness;
		Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * BeamLength, beamDims.Y, DelegateMethods.CastLight);
	}

	public override bool CanHitPlayer(Player target) => base.CanHitPlayer(target) && State == 1f;

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
		if (projHitbox.Intersects(targetHitbox)) {
			return true;
		}

		float _ = float.NaN;
		Vector2 beamEndPos = Projectile.Center + Projectile.velocity * BeamLength;
		return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, beamEndPos, BeamHitboxCollisionWidth * Projectile.scale, ref _);
	}

	private void DrawBeam(SpriteBatch spriteBatch, Texture2D texture, Vector2 startPosition, Vector2 endPosition, Vector2 drawScale, Color beamColor) {
		Utils.LaserLineFraming lineFraming = DelegateMethods.RainbowLaserDraw;

		// c_1 is an unnamed decompiled variable which is the render color of the beam drawn by DelegateMethods.RainbowLaserDraw.
		DelegateMethods.c_1 = beamColor;
		Utils.DrawLaser(spriteBatch, texture, startPosition, endPosition, drawScale, lineFraming);
	}

	public override bool PreDraw(ref Color lightColor) {
		if (Projectile.velocity == Vector2.Zero) {
			return false;
		}

		Texture2D texture = TextureAssets.Projectile[Type].Value;
		Vector2 centerFloored = Projectile.Center.Floor() + Projectile.velocity * Projectile.scale * 10.5f;
		Vector2 drawScale = new(Projectile.scale);

		float visualBeamLength = BeamLength - 14.5f * Projectile.scale * Projectile.scale;

		DelegateMethods.f_1 = 1f;
		Vector2 startPosition = centerFloored - Main.screenPosition;
		Vector2 endPosition = startPosition + Projectile.velocity * visualBeamLength;

		// Draw outer beam
		DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, GetOuterBeamColor() * OuterBeamOpacityMultiplier * Projectile.Opacity);

		// Draw inner beam
		drawScale *= 0.5f;
		DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, GetInnerBeamColor() * InnerBeamOpacityMultiplier * Projectile.Opacity);

		return false;
	}

	// Automatically iterates through every tile the laser is overlapping to cut grass at all those locations.
	public override void CutTiles() {
		// tilecut_0 is an unnamed decompiled variable which tells CutTiles how the tiles are being cut (in this case, via a projectile).
		DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
		Utils.TileActionAttempt cut = DelegateMethods.CutTiles;
		Vector2 beamStartPos = Projectile.Center;
		Vector2 beamEndPos = beamStartPos + Projectile.velocity * BeamLength;

		// PlotTileLine is a function which performs the specified action to all tiles along a drawn line, with a specified width.
		// In this case, it is cutting all tiles which can be destroyed by projectiles, for example grass or pots.
		Utils.PlotTileLine(beamStartPos, beamEndPos, Projectile.width * Projectile.scale, cut);
	}
}