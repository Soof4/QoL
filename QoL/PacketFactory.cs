using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework;

namespace QoL
{
    public class PacketFactory
    {
        private MemoryStream memoryStream;
        public BinaryWriter writer;
        public PacketFactory(bool writeOffset = true, bool writeExtraOffsetForNetModule = false)
        {
            memoryStream = new MemoryStream();
            writer = new BinaryWriter(memoryStream);
            if (writeOffset)
                writer.BaseStream.Position = 3L;
            if (writeExtraOffsetForNetModule)
                writer.BaseStream.Position = 5L;
        }

        public PacketFactory SetPacketType(short type)
        {
            long currentPosition = writer.BaseStream.Position;
            writer.BaseStream.Position = 2L;
            writer.Write(type);
            writer.BaseStream.Position = currentPosition;
            return this;
        }

        public PacketFactory SetNetModuleType(short type)
        {
            long currentPosition = writer.BaseStream.Position;
            writer.BaseStream.Position = 3L;
            writer.Write(type);
            writer.BaseStream.Position = currentPosition;
            return this;
        }

        public PacketFactory PackBool(bool flag)
        {
            writer.Write(flag);
            return this;
        }

        public PacketFactory PackByte(byte num)
        {
            writer.Write(num);
            return this;
        }
        public PacketFactory PackSByte(sbyte num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackInt16(short num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackUInt16(ushort num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackInt32(int num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackUInt32(uint num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackInt64(long num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackUInt64(ulong num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackSingle(float num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackString(string str)
        {
            writer.Write(str);
            return this;
        }

        public PacketFactory PackBuffer(byte[] buffer)
        {
            writer.Write(buffer);
            return this;
        }

        public PacketFactory PackVector2(Vector2 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
            return this;
        }

        public PacketFactory PackPoint16(Point16 p)
        {
            writer.Write(p.X);
            writer.Write(p.Y);
            return this;
        }

        public PacketFactory PackColor(Color color)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
            return this;
        }

        public PacketFactory PackParticleOrchestra(ParticleOrchestraSettings s)
        {
            s.Serialize(writer);
            return this;
        }

        public PacketFactory PackAccessoryVisibility(bool[] hideVisibleAccessory)
        {
            ushort num = 0;
            for (int i = 0; i < hideVisibleAccessory.Length; i++)
            {
                if (hideVisibleAccessory[i])
                {
                    num |= (ushort)(1 << i);
                }
            }
            writer.Write(num);
            return this;
        }

        public PacketFactory PackPlayerDeathReason(PlayerDeathReason p)
        {
            p.WriteSelfTo(writer);
            return this;
        }

        public PacketFactory PackNetworkText(NetworkText t)
        {
            t.Serialize(writer);
            return this;
        }

        private void UpdateLength()
        {
            long currentPosition = writer.BaseStream.Position;
            writer.BaseStream.Position = 0L;
            writer.Write((short)currentPosition);
            writer.BaseStream.Position = currentPosition;
        }

        public byte[] GetByteData()
        {
            UpdateLength();
            return memoryStream.ToArray();
        }

        public byte[] ToArray()
        {
            return memoryStream.ToArray();
        }
    }
}