using System.IO;
using EasyPacketsLib;

namespace EveryoneIsHere.RiskOfRain.Common.EasyPackets;

public readonly struct SyncChanceShrineTileEntityPacket : IEasyPacket<SyncChanceShrineTileEntityPacket>
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

	SyncChanceShrineTileEntityPacket IEasyPacket<SyncChanceShrineTileEntityPacket>.Deserialise(BinaryReader reader, in SenderInfo sender) {
		return new SyncChanceShrineTileEntityPacket(reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean(), reader.ReadInt32());
	}
}