using System.IO;
using System.Net;

namespace TNet
{
    public class TcpPlayer : TcpProtocol
    {
        public Channel channel;

        public IPEndPoint udpEndPoint;

        public bool udpIsUsable;

        public void FinishJoiningChannel()
        {
            Buffer buffer = Buffer.Create();
            BinaryWriter binaryWriter = buffer.BeginPacket(Packet.ResponseJoiningChannel);
            binaryWriter.Write(channel.id);
            binaryWriter.Write((short)channel.players.size);
            for (int i = 0; i < channel.players.size; i++)
            {
                TcpPlayer tcpPlayer = channel.players[i];
                binaryWriter.Write(tcpPlayer.id);
                binaryWriter.Write((!string.IsNullOrEmpty(tcpPlayer.name)) ? tcpPlayer.name : "Guest");
                binaryWriter.WriteObject(tcpPlayer.data);
            }
            int startOffset = buffer.EndPacket();
            if (channel.host == null)
            {
                channel.host = this;
            }
            buffer.BeginPacket(Packet.ResponseSetHost, startOffset);
            binaryWriter.Write(channel.host.id);
            startOffset = buffer.EndTcpPacketStartingAt(startOffset);
            if (!string.IsNullOrEmpty(channel.data))
            {
                buffer.BeginPacket(Packet.ResponseSetChannelData, startOffset);
                binaryWriter.Write(channel.data);
                startOffset = buffer.EndTcpPacketStartingAt(startOffset);
            }
            buffer.BeginPacket(Packet.ResponseLoadLevel, startOffset);
            binaryWriter.Write((!string.IsNullOrEmpty(channel.level)) ? channel.level : string.Empty);
            startOffset = buffer.EndTcpPacketStartingAt(startOffset);
            for (int j = 0; j < channel.created.size; j++)
            {
                Channel.CreatedObject createdObject = channel.created.buffer[j];
                buffer.BeginPacket(Packet.ResponseCreate, startOffset);
                binaryWriter.Write(createdObject.playerID);
                binaryWriter.Write(createdObject.objectID);
                binaryWriter.Write(createdObject.uniqueID);
                binaryWriter.Write(createdObject.buffer.buffer, createdObject.buffer.position, createdObject.buffer.size);
                startOffset = buffer.EndTcpPacketStartingAt(startOffset);
            }
            if (channel.destroyed.size != 0)
            {
                buffer.BeginPacket(Packet.ResponseDestroy, startOffset);
                binaryWriter.Write((ushort)channel.destroyed.size);
                for (int k = 0; k < channel.destroyed.size; k++)
                {
                    binaryWriter.Write(channel.destroyed.buffer[k]);
                }
                startOffset = buffer.EndTcpPacketStartingAt(startOffset);
            }
            for (int l = 0; l < channel.rfcs.size; l++)
            {
                Buffer buffer2 = channel.rfcs[l].buffer;
                buffer2.BeginReading();
                buffer.BeginWriting(startOffset);
                binaryWriter.Write(buffer2.buffer, buffer2.position, buffer2.size);
                startOffset = buffer.EndWriting();
            }
            buffer.BeginPacket(Packet.ResponseJoinChannel, startOffset);
            binaryWriter.Write(value: true);
            startOffset = buffer.EndTcpPacketStartingAt(startOffset);
            SendTcpPacket(buffer);
            buffer.Recycle();
        }
    }
}
