using EasyPacketsLib;
using EveryoneIsHere.Helpers;
using EveryoneIsHere.RiskOfRain.Common.EasyPackets;
using EveryoneIsHere.RiskOfRain.Content.Tiles;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Common.Systems
{
    public class EasyPacketSystem : ModSystem
    {
        public override void Load() {
            Mod.AddPacketHandler<SyncChanceShrineTileEntityPacket>(OnSyncChanceShrineTileEntityPacketReceived);

            base.Load();
        }

        public override void Unload() {
            Mod.RemovePacketHandler<SyncChanceShrineTileEntityPacket>(OnSyncChanceShrineTileEntityPacketReceived);

            base.Unload();
        }

        private void OnSyncChanceShrineTileEntityPacketReceived(in SyncChanceShrineTileEntityPacket packet, in SenderInfo sender, ref bool handled) {
            if (TileUtils.TryGetTileEntityAs(packet.PositionX, packet.PositionY, out ChanceShrine_TileEntity chanceShrineEntity)) {
                chanceShrineEntity.Active = packet.IsActive;
                chanceShrineEntity.Price = packet.Price;
                handled = true;
            }
        }
    }
}
