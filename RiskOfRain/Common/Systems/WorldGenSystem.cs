using EveryoneIsHere.Helpers;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace EveryoneIsHere.RiskOfRain.Common.Systems
{
    public class WorldGenSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
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
                    StructureHelper.Generator.GenerateMultistructureRandom("RiskOfRain/Structures/ChanceShrineMultiStructure", new Point16(x, y), Mod);
                }
            }
        }
    }
}
