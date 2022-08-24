using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MPTickBar
{
    public unsafe class Chat
    {
        private ProcessChatBoxDelegate ProcessChatBox { get; }

        private delegate void ProcessChatBoxDelegate(UIModule* uiModule, IntPtr message, IntPtr unused, byte a4);

        public Chat(SigScanner sigScanner) => this.ProcessChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(sigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9"));

        public void ExecuteCommand(string command)
        {
            (IntPtr, long) PrepareString(string message)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                var mem = Marshal.AllocHGlobal(bytes.Length + 30);
                Marshal.Copy(bytes, 0, mem, bytes.Length);
                Marshal.WriteByte(mem + bytes.Length, 0);
                return (mem, bytes.Length + 1);
            }

            IntPtr PrepareContainer(IntPtr message, long length)
            {
                var mem = Marshal.AllocHGlobal(400);
                Marshal.WriteInt64(mem, message.ToInt64());
                Marshal.WriteInt64(mem + 0x8, 64);
                Marshal.WriteInt64(mem + 0x10, length);
                Marshal.WriteInt64(mem + 0x18, 0);
                return mem;
            }

            var (text, length) = PrepareString(command);
            var payload = PrepareContainer(text, length);

            this.ProcessChatBox.Invoke(FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule(), payload, IntPtr.Zero, 0);

            Marshal.FreeHGlobal(payload);
            Marshal.FreeHGlobal(text);
        }
    }
}