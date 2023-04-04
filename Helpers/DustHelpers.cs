using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EveryoneIsHere.Helpers;

public class DustHelpers
{
	/// <summary>
	///     Frames dust properly based on the passed vanilla dust type. Automatically randomly picks between the 3 available dust sprites.
	///     Usage: dust.frame = DustHelpers.GetDustFrameFromType(DustID.Dirt)
	/// </summary>
	/// <param name="type">The vanilla dust type for the given dust sprite</param>
	/// <returns>A Rectangle framing </returns>
	public static Rectangle GetDustFrameFromType(int type) {
		int frameX = type * 10 % 1000;
		int frameY = type * 10 / 1000 * 30 + Main.rand.Next(3) * 10;
		return new Rectangle(frameX, frameY, 8, 8);
	}
}

/// <summary>
///     A helper class that adds a base implementation of Update that makes the dust act as though it's parented to the player. Make sure to call base when overriding Update.
/// </summary>
public abstract class PlayerParentedDust : ModDust
{
	public override bool Update(Dust dust) {
		if (dust.customData is not int owner) {
			return false;
		}

		Player player = Main.player[owner];
		dust.position += player.position - player.oldPosition;

		return base.Update(dust);
	}
}