using EasyPacketsLib;
using EveryoneIsHere.Helpers;
using EveryoneIsHere.RiskOfRain.Common.EasyPackets;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EveryoneIsHere.RiskOfRain.Content.Tiles
{
    public class ChanceShrine : ModTile
    {
        private readonly int ChanceShrineCost = 100000; // 10 Gold

        private readonly int[] ChanceShrineItems = new int[] {
            // TODO: Add modded items here
            ItemID.CloudinaBottle,
            ItemID.BandofRegeneration,
            ItemID.HermesBoots
        };

        private static bool IsShrineActive(int i, int j) {
            if (TileUtils.TryGetTileEntityAs(i, j, out ChanceShrine_TileEntity chanceShrineEntity)) {
                return chanceShrineEntity.Active;
            }

            return false;
        }

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
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ChanceShrine_TileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);

            DustType = DustID.Stone;

            LocalizedText shrineName = CreateMapEntryName();
            AddMapEntry(new Color(144, 148, 144));
        }

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => !IsShrineActive(i, j);

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Point16 origin = TileUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<ChanceShrine_TileEntity>().Kill(origin.X, origin.Y);

            base.KillMultiTile(i, j, frameX, frameY);
        }

        public override void MouseOver(int i, int j) {
            if (!IsShrineActive(i, j)) {
                return;
            }

            Player player = Main.LocalPlayer;

            player.cursorItemIconID = -1;
            player.cursorItemIconText = $"[i:{ItemID.GoldCoin}]10";
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;

            base.MouseOver(i, j);
        }

        public override bool RightClick(int i, int j) {
            if (!IsShrineActive(i, j)) {
                return false;
            }

            Player player = Main.LocalPlayer;
            if (!player.CanBuyItem(ChanceShrineCost)) {
                SoundEngine.PlaySound(EveryoneIsHereSounds.ShrineInsufficientFunds);
                return false;
            }

            if (Main.rand.NextBool(2)) {
                // Success

                int newItemIndex = Item.NewItem(new EntitySource_TileInteraction(player, i, j), i * 16, j * 16, 16, 16, Main.rand.Next(ChanceShrineItems));
                Main.item[newItemIndex].noGrabDelay = 100;
                if (TileUtils.TryGetTileEntityAs(i, j, out ChanceShrine_TileEntity chanceShrineEntity)) {
                    chanceShrineEntity.Active = false;

                    if (Main.netMode == NetmodeID.MultiplayerClient) {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItemIndex, 1f);
                        Mod.SendPacket(new SyncChanceShrineTileEntityPacket(chanceShrineEntity.Position.X, chanceShrineEntity.Position.Y, false), forward: true);
                    }
                }

                // TODO: Visuals
            } else {
                // Failure

                // TODO: Visuals
            }

            SoundEngine.PlaySound(EveryoneIsHereSounds.ShrineActivate);

            player.BuyItem(ChanceShrineCost);

            return true;
        }
    }

    public class ChanceShrine_TileEntity : ModTileEntity
    {
        public bool Active { get; set; } = true;

        public override bool IsTileValidForEntity(int x, int y) {
            return Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<ChanceShrine>();
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
            TileObjectData tileData = TileObjectData.GetTileData(type, style, alternate);
            int topLeftX = i - tileData.Origin.X;
            int topLeftY = j - tileData.Origin.Y;

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, tileData.Width, tileData.Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: type);
                return -1;
            }

            return Place(topLeftX, topLeftY);
        }

        public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);

        public override void NetSend(BinaryWriter writer) {
            writer.Write(Active);

            base.NetSend(writer);
        }

        public override void NetReceive(BinaryReader reader) {
            Active = reader.ReadBoolean();

            base.NetReceive(reader);
        }
    }
}
