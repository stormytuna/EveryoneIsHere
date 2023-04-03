using System.Collections.Generic;
using EveryoneIsHere.Helpers;
using EveryoneIsHere.RiskOfRain.Content.Tiles;
using StructureHelper;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace EveryoneIsHere.RiskOfRain.Common.Systems;

public class WorldGenSystem : ModSystem
{
	public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
		int dungeonIndex = tasks.FindIndex(genpass => genpass.Name == "Dungeon");
		if (dungeonIndex != -1) {
			tasks.Insert(dungeonIndex + 1, new PassLegacy("Warbanners", Warbanners));
		}

		int lifeCrystalsIndex = tasks.FindLastIndex(genpass => genpass.Name == "Life Crystals");
		if (lifeCrystalsIndex != -1) {
			tasks.Insert(lifeCrystalsIndex + 1, new PassLegacy("Shrines", Shrines));
		}

		base.ModifyWorldGenTasks(tasks, ref totalWeight);
	}

	private void Shrines(GenerationProgress progress, GameConfiguration _) {
		progress.Message = "Placing Shrines";

		for (int i = 0; i < Main.maxTilesX * Main.maxTilesY * 4E-05; i++) {
			int x = WorldGen.genRand.Next(40, Main.maxTilesX - 40);
			int y = WorldGen.genRand.Next((int)Main.rockLayer, Main.UnderworldLayer);

			List<Tile> tiles = TileUtils.FindAllSolidTiles(x, x + 20, y + 10, y + 14);
			if (tiles.Count > 35) {
				Generator.GenerateMultistructureRandom("RiskOfRain/Structures/ChanceShrineMultiStructure", new Point16(x, y), Mod);
			}
		}

		for (int i = 0; i < Main.maxTilesX * Main.maxTilesY * 5E-06; i++) {
			int x = WorldGen.genRand.Next(40, Main.maxTilesX);
			int y = WorldGen.genRand.Next(Main.UnderworldLayer, Main.maxTilesY - 60);

			Mod.Logger.Info($"x: {x}, y: {y}");

			List<Tile> tiles = TileUtils.FindAllSolidTiles(x, x + 20, y, y + 20);
			if (tiles.Count > 300) {
				Generator.GenerateMultistructureRandom("RiskOfRain/Structures/SacrificeShrineMultiStructure", new Point16(x, y), Mod);
			}
		}
	}

	private void Warbanners(GenerationProgress progress, GameConfiguration _) {
		progress.Message = "Placing Warbanners in the Dungeon";

		int numBanners = 4;
		if (Main.maxTilesX > 4200) {
			numBanners += 2;
		}

		if (Main.maxTilesX > 6400) {
			numBanners += 2;
		}

		int numPlaced = 0;
		// Code adapted from vanilla placing banners in dungeon
		while (numPlaced < numBanners) {
			int x;
			int y;
			// Reroll x, y coordinates until we find an empty tile in the dungeon
			do {
				x = WorldGen.genRand.Next(GenVars.dMinX, GenVars.dMaxX);
				y = WorldGen.genRand.Next((int)GenVars.rockLayer, Main.maxTilesY);
			} while (!Main.wallDungeon[Main.tile[x, y].WallType] || Main.tile[x, y].HasTile);

			// Move upwards until we hit a solid tile
			while (!WorldGen.SolidTile(x, y) && y > 10) {
				y--;
			}

			// Check that we have room for our banner
			y++;
			if (!Main.wallDungeon[Main.tile[x, y].WallType] || Main.tile[x, y - 1].TileType == 48 || Main.tile[x, y].HasTile || Main.tile[x, y + 1].HasTile || Main.tile[x, y + 2].HasTile ||
			    Main.tile[x, y + 3].HasTile) {
				continue;
			}

			// Check that the surrounding tiles are dungeon tiles
			bool surroundedByDungeonTiles = true;
			for (int j = x - 1; j <= x + 1; j++) {
				for (int k = y; k <= y + 3; k++) {
					if (Main.tile[j, k].HasTile && (Main.tile[j, k].TileType == 10 || Main.tile[j, k].TileType == 11 || Main.tile[j, k].TileType == 91)) {
						surroundedByDungeonTiles = false;
					}
				}
			}

			// Actually place our tile
			if (surroundedByDungeonTiles) {
				WorldGen.PlaceTile(x, y, ModContent.TileType<WarbannerTile>(), true);
				numPlaced++;
			}
		}
	}
}