using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EveryoneIsHere.Helpers;

public class ProjectileHelpers { }

/// <summary>
///     This is a base class for 'aura' effects using projectiles
/// </summary>
public abstract class AuraProjectile : ModProjectile
{
	public Color AuraDrawColor { private get; set; } = Color.White;
	public float AuraDrawColorMultiplier { private get; set; } = 1f;

	public Player Owner => Main.player[Projectile.owner];

	public abstract bool ShouldKillProjectile();
	public abstract void SafeSetDefaults();

	public override void SetDefaults() {
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.hide = true;
		SafeSetDefaults();
	}

	public override void AI() {
		Projectile.velocity = Vector2.Zero;
		Projectile.Center = Owner.MountedCenter;
		Projectile.timeLeft = 2;

		if (ShouldKillProjectile()) {
			Projectile.Kill();
		}
	}

	public override bool PreDraw(ref Color lightColor) {
		Main.instance.LoadProjectile(Type);
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		Vector2 drawPosition = Projectile.Center.ToScreenCoordinates();
		Rectangle sourceRect = new(0, 0, texture.Width, texture.Height);
		Vector2 origin = sourceRect.Size() / 2f;
		Color drawColor = AuraDrawColor * GeneralUtils.GetBrightness(Projectile.Center) * AuraDrawColorMultiplier;
		Main.spriteBatch.Draw(texture, drawPosition, sourceRect, drawColor, 0f, origin, 1f, SpriteEffects.None, 0f);

		return false;
	}

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
		behindNPCsAndTiles.Add(index);
	}
}