using EveryoneIsHere.Helpers;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.NPCs.AlphaConstruct;

public class AlphaConstructProjectile : ModProjectile
{
	private SlotId loopedSoundSlot;

	public override void SetDefaults() {
		// Base properties
		Projectile.width = 12;
		Projectile.height = 12;

		// Weapon properties
		Projectile.hostile = true;
		Projectile.penetrate = 1;
	}

	public override void AI() {
		// Point where we going
		Projectile.rotation = Projectile.velocity.ToRotation();

		// Dust trail
		for (int i = 0; i < 3; i++) {
			Dust newDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SandstormInABottle);
			newDust.velocity = Main.rand.NextVector2Circular(1f, 1f);
			newDust.noGravity = true;
		}

		Lighting.AddLight(Projectile.Center, AlphaConstruct.LightColor);

		if (SoundEngine.TryGetActiveSound(loopedSoundSlot, out ActiveSound sound)) {
			sound.Position = Projectile.Center;
		} else {
			loopedSoundSlot = SoundEngine.PlaySound(EveryoneIsHereSounds.AlphaConstruct_ProjectileLoop, Projectile.Center);
		}
	}

	public override void Kill(int timeLeft) {
		// Dust explosion
		for (int i = 0; i < 16; i++) {
			Dust newDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SandstormInABottle);
			newDust.velocity = Main.rand.NextVector2Circular(5f, 5f);
			newDust.noGravity = true;
		}

		if (SoundEngine.TryGetActiveSound(loopedSoundSlot, out ActiveSound sound)) {
			sound.Stop();
		}

		SoundEngine.PlaySound(EveryoneIsHereSounds.AlphaConstruct_ProjectileExplode, Projectile.Center);
	}
}