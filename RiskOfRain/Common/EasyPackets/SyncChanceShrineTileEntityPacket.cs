using EasyPacketsLib;
using System.IO;

namespace EveryoneIsHere.RiskOfRain.Common.EasyPackets
{
    public readonly struct SyncChanceShrineTileEntityPacket : IEasyPacket<SyncChanceShrineTileEntityPacket>
    {
        public readonly int PositionX;
        public readonly int PositionY;
        public readonly bool IsActive;

        public SyncChanceShrineTileEntityPacket(int posX, int posY, bool isActive) {
            PositionX = posX;
            PositionY = posY;
            IsActive = isActive;
        }

        void IEasyPacket<SyncChanceShrineTileEntityPacket>.Serialise(BinaryWriter writer) {
            writer.Write(PositionX);
            writer.Write(PositionY);
            writer.Write(IsActive);
        }

        SyncChanceShrineTileEntityPacket IEasyPacket<SyncChanceShrineTileEntityPacket>.Deserialise(BinaryReader reader, in SenderInfo sender) {
            return new SyncChanceShrineTileEntityPacket(reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean());
        }
    }
}
