using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace EveryoneIsHere.Helpers;

public static class GeneralUtils
{
	/// <summary>Converts an integer coin value to a string</summary>
	/// <param name="coinValue">The coin value</param>
	/// <param name="useIcons">Whether the string should use coin icons or coin names</param>
	/// <param name="useColors">Whether the string should colour the number of coins</param>
	/// <returns>Returns a string representing the coin value</returns>
	public static string CoinValueToString(int coinValue, bool useIcons = true, bool useColors = true) {
		int platinumValue = 1000000;
		int goldValue = 10000;
		int silverValue = 100;
		int copperValue = 1;

		int platinumCoins = 0;
		int goldCoins = 0;
		int silverCoins = 0;
		int copperCoins = 0;
		while (coinValue > 0) {
			if (coinValue >= platinumValue) {
				coinValue -= platinumValue;
				platinumCoins++;
			} else if (coinValue >= goldValue) {
				coinValue -= goldValue;
				goldCoins++;
			} else if (coinValue >= silverValue) {
				coinValue -= silverValue;
				silverCoins++;
			} else if (coinValue >= copperValue) {
				coinValue -= copperValue;
				copperCoins++;
			}
		}

		string text = "";
		if (platinumCoins > 0) {
			string coinsAmount = useColors ? $"[c/DCDCC6:{platinumCoins}]" : $"{platinumCoins}";
			string coinsRepresentation = useIcons ? $"[i:{ItemID.PlatinumCoin}] " : $" {Language.GetTextValue("Currency.Platinum")} ";
			text += coinsAmount + coinsRepresentation;
		}

		if (goldCoins > 0) {
			string coinsAmount = useColors ? $"[c/E0C95C:{goldCoins}]" : $"{goldCoins}";
			string coinsRepresentation = useIcons ? $"[i:{ItemID.GoldCoin}] " : $" {Language.GetTextValue("Currency.Gold")} ";
			text += coinsAmount + coinsRepresentation;
		}

		if (silverCoins > 0) {
			string coinsAmount = useColors ? $"[c/B5C0C1:{silverCoins}]" : $"{silverCoins}";
			string coinsRepresentation = useIcons ? $"[i:{ItemID.SilverCoin}] " : $" {Language.GetTextValue("Currency.Silver")} ";
			text += coinsAmount + coinsRepresentation;
		}

		if (copperCoins > 0) {
			string coinsAmount = useColors ? $"[c/F68A60:{copperCoins}]" : $"{copperCoins}";
			string coinsRepresentation = useIcons ? $"[i:{ItemID.CopperCoin}] " : $" {Language.GetTextValue("Currency.Copper")} ";
			text += coinsAmount + coinsRepresentation;
		}

		return text;
	}

	/// <summary>
	///     Creates a new item at the given tile coordinates in a 16x16 area
	/// </summary>
	public static Item NewItemFromTile(IEntitySource source, int tileX, int tileY, int type) {
		int newItemIndex = Item.NewItem(source, tileX * 16, tileY * 16, 16, 16, type);
		return Main.item[newItemIndex];
	}

	/// <summary>
	///     Ease-out interpolation between start and end
	/// </summary>
	public static float EaseOutInterpolation(float start, float end, float lerpAmount, int exponent) {
		float flipped = 1 - lerpAmount;
		float exp = MathF.Pow(flipped, exponent);
		float reFlipped = 1 - exp;
		return MathHelper.Lerp(start, end, reFlipped);
	}
}