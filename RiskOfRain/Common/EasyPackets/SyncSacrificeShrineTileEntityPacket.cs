using System.IO;
using EasyPacketsLib;

namespace EveryoneIsHere.RiskOfRain.Common.EasyPackets;

public readonly struct SyncSacrificeShrineTileEntityPacket : IEasyPacket<SyncSacrificeShrineTileEntityPacket>
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
}