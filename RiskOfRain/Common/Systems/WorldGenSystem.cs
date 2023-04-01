using EveryoneIsHere.RiskOfRain.Content.Tiles;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace EveryoneIsHere.RiskOfRain.Common.Systems
{
    public class WorldGenSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
            int lifeCrystalsIndex = tasks.FindLastIndex(genpass => genpass.Name == "Life Crystals");
            if (lifeCrystalsIndex != -1) {
                tasks.Insert(lifeCrystalsIndex + 1, new PassLegacy("Shrines", Shrines));
            }

            base.ModifyWorldGenTasks(tasks, ref totalWeight);
        }

        private void Shrines(GenerationProgress progress, GameConfiguration _) {
            progress.Message = "Placing Shrines";

            for (int i = 0; i < Main.maxTilesX * Main.maxTilesY * 1E-01; i++) {
                int x = WorldGen.genRand.Next(40, Main.maxTilesX - 40);
                int y = WorldGen.genRand.Next((int)Main.rockLayer, Main.UnderworldLayer);

                for (int k = y; k < Main.UnderworldLayer; k++) {
                    Tile leftTile = Main.tile[x - 1, k - 1];
                    Tile middleTile = Main.tile[x, k - 1];
                    Tile rightTile = Main.tile[x + 1, k - 1];

                    bool tilesAreWet = leftTile.LiquidAmount > 0 || middleTile.LiquidAmount > 0 || rightTile.LiquidAmount > 0;
                    bool allEmptyTiles = WorldGen.EmptyTileCheck(x - 1, x + 1, k - 4, k - 1);
                    bool inDungeon = Main.wallDungeon[middleTile.WallType];
                    if (tilesAreWet || !allEmptyTiles || inDungeon) {
                        continue;
                    }

                    WorldGen.Place3x4(x, k, (ushort)ModContent.TileType<ChanceShrine>(), 0);
                    ShrineSystem.RegisterShrinePlacedByWorld(x, k);
                    break;
                }
            }
        }
    }
}
