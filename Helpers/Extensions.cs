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

	public static Vector2 ClampLength(this Vector2 vector, float minLength, float maxLength) {
		float minLengthSquared = minLength * minLength;
		float maxLengthSquared = maxLength * maxLength;

		if (vector.LengthSquared() < minLengthSquared) {
			return vector.SafeNormalize(Vector2.Zero) * minLength;
		}

		if (vector.LengthSquared() > maxLengthSquared) {
			return vector.SafeNormalize(Vector2.Zero) * maxLength;
		}

		return vector;
	}
}