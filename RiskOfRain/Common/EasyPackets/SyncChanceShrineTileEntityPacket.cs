using System.IO;
using EasyPacketsLib;
using EveryoneIsHere.Helpers;
using EveryoneIsHere.RiskOfRain.Content.Tiles;

namespace EveryoneIsHere.RiskOfRain.Common.EasyPackets;

public readonly struct SyncChanceShrineTileEntityPacket : IEasyPacket<SyncChanceShrineTileEntityPacket>, IEasyPacketHandler<SyncChanceShrineTileEntityPacket>
{
	public readonly int PositionX;
	public readonly int PositionY;
	public readonly bool IsActive;
	public readonly int Price;

	public SyncChanceShrineTileEntityPacket(int posX, int posY, bool isActive, int price) {
		PositionX = posX;
		PositionY = posY;
		IsActive = isActive;
		Price = price;
	}

	void IEasyPacket<SyncChanceShrineTileEntityPacket>.Serialise(BinaryWriter writer) {
		writer.Write(PositionX);
		writer.Write(PositionY);
		writer.Write(IsActive);
		writer.Write(Price);
	}

	SyncChanceShrineTileEntityPacket IEasyPacket<SyncChanceShrineTileEntityPacket>.Deserialise(BinaryReader reader, in SenderInfo sender) =>
		new SyncChanceShrineTileEntityPacket(reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean(), reader.ReadInt32());

	public void Receive(in SyncChanceShrineTileEntityPacket packet, in SenderInfo sender, ref bool handled) {
		if (!TileUtils.TryGetTileEntityAs(packet.PositionX, packet.PositionY, out ChanceShrine_TileEntity chanceShrineEntity)) {
			return;
		}

		chanceShrineEntity.Active = packet.IsActive;
		chanceShrineEntity.Price = packet.Price;
		handled = true;
	}
}