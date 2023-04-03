using System.IO;
using EasyPacketsLib;
using EveryoneIsHere.Helpers;
using EveryoneIsHere.RiskOfRain.Content.Tiles;

namespace EveryoneIsHere.RiskOfRain.Common.EasyPackets;

public readonly struct SyncSacrificeShrineTileEntityPacket : IEasyPacket<SyncSacrificeShrineTileEntityPacket>, IEasyPacketHandler<SyncSacrificeShrineTileEntityPacket>
{
	public readonly int PositionX;
	public readonly int PositionY;
	public readonly bool IsActive;

	public SyncSacrificeShrineTileEntityPacket(int posX, int posY, bool isActive) {
		PositionX = posX;
		PositionY = posY;
		IsActive = isActive;
	}

	void IEasyPacket<SyncSacrificeShrineTileEntityPacket>.Serialise(BinaryWriter writer) {
		writer.Write(PositionX);
		writer.Write(PositionY);
		writer.Write(IsActive);
	}

	SyncSacrificeShrineTileEntityPacket IEasyPacket<SyncSacrificeShrineTileEntityPacket>.Deserialise(BinaryReader reader, in SenderInfo sender) {
		return new SyncSacrificeShrineTileEntityPacket(reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean());
	}

	public void Receive(in SyncSacrificeShrineTileEntityPacket packet, in SenderInfo sender, ref bool handled) {
		if (!TileUtils.TryGetTileEntityAs(packet.PositionX, packet.PositionY, out SacrificeShrine_TileEntity sacrificeShrineEntity)) {
			return;
		}

		sacrificeShrineEntity.Active = packet.IsActive;
		handled = true;
	}
}