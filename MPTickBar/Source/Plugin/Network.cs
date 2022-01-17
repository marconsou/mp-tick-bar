using Dalamud.Game.Network;
using System;
using System.Runtime.InteropServices;

namespace MPTickBar
{
    public class Network
    {
        public PlayerState PlayerState { get; set; }

        private int HP { get; set; }

        private int MP { get; set; }

        private ushort OpCode { get; set; }

        private bool WasOpCodeFound { get; set; }

        public ushort GetOpCode() => (ushort)(this.WasOpCodeFound ? this.OpCode : 0);

        public bool Update()
        {
            if (!this.WasOpCodeFound && (this.OpCode != 0) && (this.PlayerState != null) && this.PlayerState.CheckPlayerStatus(this.HP, this.MP))
            {
                this.WasOpCodeFound = true;
                return true;
            }
            return false;
        }

        public bool NetworkMessage(IntPtr dataPtr, ushort opCode, uint targetActorId, NetworkMessageDirection direction)
        {
            bool CheckNetworkPlayerStatus() => (direction == NetworkMessageDirection.ZoneDown) && (this.PlayerState != null) && this.PlayerState.CheckPlayerId(targetActorId);

            if (this.OpCode == opCode && this.WasOpCodeFound && CheckNetworkPlayerStatus())
            {
                return true;
            }
            else if (!this.WasOpCodeFound && CheckNetworkPlayerStatus())
            {
                this.HP = Network.GetData(dataPtr, 0, 3);
                this.MP = Network.GetData(dataPtr, 4, 2);
                this.OpCode = opCode;
            }
            return false;
        }

        private static int GetData(IntPtr dataPtr, int offset, int size)
        {
            var bytes = new byte[4];
            Marshal.Copy(dataPtr + offset, bytes, 0, size);
            return BitConverter.ToInt32(bytes);
        }
    }
}