using Dalamud.Game;
using System;
using System.Runtime.InteropServices;

namespace MPTickBar
{
    public unsafe class ActionManager
    {
        private IntPtr DataAddress { get; set; }

        public ActionManager(SigScanner sigScanner)
        {
            this.DataAddress = sigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 33 C0 E9 ?? ?? ?? ?? 8B 7D 0C");
            this.GetRecastTimeElapsedPointer = Marshal.GetDelegateForFunctionPointer<GetRecastTimeElapsedDelegate>(sigScanner.ScanText("E8 ?? ?? ?? ?? F3 0F 5C F0 49 8B CD"));
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate float GetRecastTimeElapsedDelegate(void* data, ActionType actionType, uint actionID);
        private readonly GetRecastTimeElapsedDelegate GetRecastTimeElapsedPointer;

        public float GetRecastTimeElapsed(ActionType actionType, uint actionID) => this.GetRecastTimeElapsedPointer((void*)this.DataAddress, actionType, actionID);
    }

    public enum ActionType : byte
    {
        None = 0x00,
        Spell = 0x01,
    }
}