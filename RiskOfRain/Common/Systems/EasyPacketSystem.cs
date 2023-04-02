using EasyPacketsLib;
using EveryoneIsHere.Helpers;
using EveryoneIsHere.RiskOfRain.Common.EasyPackets;
using EveryoneIsHere.RiskOfRain.Content.Tiles;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Common.Systems;

public class EasyPacketSystem : ModSystem
{
	public override void Load() {
		Mod.AddPacketHandler<SyncChanceShrineTileEntityPacket>(OnSyncChanceShrineTileEntityPacketReceived);
		Mod.AddPacketHandler<SyncSacrificeShrineTileEntityPacket>(OnSyncSacrificeShrineTileEntityPacketReceived);

		base.Load();
	}

	public override void Unload() {
		Mod.RemovePacketHandler<SyncChanceShrineTileEntityPacket>(OnSyncChanceShrineTileEntityPacketReceived);
		Mod.RemovePacketHandler<SyncSacrificeShrineTileEntityPacket>(OnSyncSacrificeShrineTileEntityPacketReceived);


		base.Unload();
	}

	private void OnSyncChanceShrineTileEntityPacketReceived(in SyncChanceShrineTileEntityPacket packet, in SenderInfo sender, ref bool handled) {
		if (TileUtils.TryGetTileEntityAs(packet.PositionX, packet.PositionY, out ChanceShrine_TileEntity chanceShrineEntity)) {
			chanceShrineEntity.Active = packet.IsActive;
			chanceShrineEntity.Price = packet.Price;
			handled = true;
		}
	}

	private void OnSyncSacrificeShrineTileEntityPacketReceived(in SyncSacrificeShrineTileEntityPacket packet, in SenderInfo sender, ref bool handled) {
		if (TileUtils.TryGetTileEntityAs(packet.PositionX, packet.PositionY, out SacrificeShrine_TileEntity sacrificeShrineEntity)) {
			sacrificeShrineEntity.Active = packet.IsActive;
			handled = true;
		}
	}
}