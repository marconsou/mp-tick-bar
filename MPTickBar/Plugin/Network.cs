using Dalamud.Game.Network;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MPTickBar
{
    public class Network
    {
        public PlayerState PlayerState { get; set; }

        public ushort OpCode { get; private set; }

        private bool WasOpCodeFound => (this.OpCode != 0);

        private List<NetworkData> NetworkDataList { get; } = new();

        private readonly struct NetworkData
        {
            public ushort OpCode { get; init; }

            public int HP { get; init; }

            public int MP { get; init; }
        }

        public bool Update()
        {
            if (!this.WasOpCodeFound)
            {
                var data = new List<NetworkData>(this.NetworkDataList);
                foreach (var item in data)
                {
                    if ((this.PlayerState != null) && this.PlayerState.CheckPlayerStatus(item.HP, item.MP))
                    {
                        this.OpCode = item.OpCode;
                        this.NetworkDataList.Clear();
                        return true;
                    }
                }
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
                if (this.NetworkDataList.Count >= 20)
                    this.NetworkDataList.Clear();

                this.NetworkDataList.Add(new() { OpCode = opCode, HP = Network.GetData(dataPtr, 0, 3), MP = Network.GetData(dataPtr, 4, 2) });
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