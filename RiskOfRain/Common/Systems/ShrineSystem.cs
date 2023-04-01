using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EveryoneIsHere.RiskOfRain.Common.Systems
{
    public class ShrineSystem : ModSystem
    {
        public static ShrineSystem Instance => ModContent.GetInstance<ShrineSystem>();

        private Dictionary<(int, int), byte> shrineStates = new();

        private static void GetShrineTopLeft(ref int i, ref int j) => WorldGen.GetTopLeftAndStyles(ref i, ref j, 3, 4, 16, 16);

        public static void RegisterShrinePlacedByWorld(int i, int j) {
            GetShrineTopLeft(ref i, ref j);
            Instance.shrineStates.TryAdd((i, j), (byte)ShrineState.Active);
        }

        public static bool IsShrineActive(int i, int j) {
            GetShrineTopLeft(ref i, ref j);
            if (Instance.shrineStates.TryGetValue((i, j), out byte shrineData)) {
                return (ShrineState)shrineData == ShrineState.Active;
            }

            return false;
        }

        public static void SetShrineAsInactive(int i, int j) {
            GetShrineTopLeft(ref i, ref j);
            if (Instance.shrineStates.ContainsKey((i, j))) {
                Instance.shrineStates[(i, j)] = (byte)ShrineState.Inactive;
            }
        }

        public override void SaveWorldData(TagCompound tag) {
            shrineStates ??= new();
            List<TagCompound> tagCompounds = new();

            foreach (var kvp in shrineStates) {
                TagCompound shrineStateEntry = new TagCompound() {
                    { "TileI", kvp.Key.Item1 },
                    { "TileJ", kvp.Key.Item2 },
                    { "ShrineState", kvp.Value }
                };
                tagCompounds.Add(shrineStateEntry);
            }

            tag["ShrineStates"] = tagCompounds;

            base.SaveWorldData(tag);
        }

        public override void LoadWorldData(TagCompound tag) {
            shrineStates = new();
            var shrineStatesList = tag.GetList<TagCompound>("ShrineStates");

            foreach (var entry in shrineStatesList) {
                shrineStates.Add((entry.GetInt("TileI"), entry.GetInt("TileJ")), entry.GetByte("ShrineState"));
            }

            base.LoadWorldData(tag);
        }

        public enum ShrineState
        {
            Active,
            Inactive
        }
    }
}
