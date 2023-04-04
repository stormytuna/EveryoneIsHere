using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Utilities;

namespace EveryoneIsHere.Helpers;

public static class Extensions
{
	/// <summary>
	///     Returns a random Vector2 within the intersection of both rectangles
	/// </summary>
	public static Vector2 NextVector2InRectangleIntersection(this UnifiedRandom rand, Rectangle startRect, Rectangle targetRect) {
		if (!startRect.Intersects(targetRect)) {
			return default;
		}

		Rectangle intersection = Rectangle.Intersect(startRect, targetRect);
		return rand.NextVector2FromRectangle(intersection);
	}

	/// <summary>
	///     Converts a world position to screen coordinates
	/// </summary>
	public static Vector2 ToScreenCoordinates(this Vector2 worldPosition) => worldPosition - Main.screenPosition;
}