using EveryoneIsHere.RiskOfRain.Common.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EveryoneIsHere.RiskOfRain.Content.Tiles
{
    public class ChanceShrine : ModTile
    {
        private readonly int ChanceShrineCost = 100000; // 10 Gold

        public override void SetStaticDefaults() {
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1000;
            Main.tileFrameImportant[Type] = true;
            Main.tileOreFinderPriority[Type] = 500;
            Main.tileSpelunker[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            DustType = DustID.Stone;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Shrine");
            AddMapEntry(new Color(144, 148, 144), name);
        }

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => !ShrineSystem.IsShrineActive(i, j);

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override void MouseOver(int i, int j) {
            if (!ShrineSystem.IsShrineActive(i, j)) {
                return;
            }

            Player player = Main.LocalPlayer;

            player.cursorItemIconID = -1;
            player.cursorItemIconText = $"[i:{ItemID.GoldCoin}] 10";
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;

            base.MouseOver(i, j);
        }

        public override bool RightClick(int i, int j) {
            if (!ShrineSystem.IsShrineActive(i, j)) {
                return false;
            }

            if (!ShrineSystem.IsShrineActive(i, j)) {
                return false;
            }

            Player player = Main.LocalPlayer;
            if (!player.CanBuyItem(ChanceShrineCost)) {
                // TODO: Play insufficient funds sound
                return false;
            }

            if (Main.rand.NextBool(3)) {
                // Success

                int[] itemTypes = new int[] {
                        ItemID.DirtBlock,
                        ItemID.Zenith,
                        ItemID.ZapinatorGray
                    };

                int newItemIndex = Item.NewItem(new EntitySource_TileInteraction(player, i, j), i * 16, j * 16, 16, 16, Main.rand.Next(itemTypes));
                Main.item[newItemIndex].noGrabDelay = 100;
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItemIndex, 1f);
                }
                ShrineSystem.SetShrineAsInactive(i, j);

                // TODO: Visuals
            } else {
                // Failure

                // TODO: Visuals
            }

            // TODO: Play Shrine Activate sound
            player.BuyItem(ChanceShrineCost);

            return true;
        }
    }
}
