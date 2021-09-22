using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using System.Linq;
using System.Reflection;

namespace MPTickBar
{
    public static class PlayerHelpers
    {
        private static bool IsEffectActivated(PlayerCharacter currentPlayer, short effectId)
        {
            if (currentPlayer == null)
                return false;

            return currentPlayer.StatusEffects.Any(x => x.EffectId == effectId);
        }

        public static bool IsBlackMage(PlayerCharacter currentPlayer)
        {
            if (currentPlayer == null)
                return false;

            var jobBLMId = 25;
            return (currentPlayer.ClassJob?.Id == jobBLMId);
        }

        public static bool IsLucidDreamingActivated(PlayerCharacter currentPlayer)
        {
            return PlayerHelpers.IsEffectActivated(currentPlayer, 1204);
        }

        public static bool IsCircleOfPowerActivated(PlayerCharacter currentPlayer)
        {
            return PlayerHelpers.IsEffectActivated(currentPlayer, 738);
        }

        public static int GetUmbralIceStacks(ClientState clientState)
        {
            if (clientState == null)
                return 0;

            var BLMGaugeData = clientState.JobGauges.Get<BLMGauge>();
            var fieldInfo = typeof(BLMGauge).GetField("elementStance", BindingFlags.NonPublic | BindingFlags.Instance);
            var stacks = 256 - (byte)fieldInfo.GetValue(BLMGaugeData);

            if (Enumerable.Range(1, 3).Contains(stacks))
                return stacks;
            return 0;
        }

        public static bool IsUmbralIceIIIActivated(ClientState clientState)
        {
            return (PlayerHelpers.GetUmbralIceStacks(clientState) == 3);
        }

        public static float CalculatedFireIIICastTime(float fireIIICastTime, bool isUmbralIceIIIActivated, bool isCircleOfPowerActivated)
        {
            /*unsafe
            {
                var fire3CastTime = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetAdjustedCastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, 152);
                Dalamud.Plugin.PluginLog.Information($"{fire3CastTime}s");
                return fire3CastTime;
            }*/

            var circleOfPowerModifier = 0.85f;
            return (fireIIICastTime * (isCircleOfPowerActivated ? circleOfPowerModifier : 1.0f)) / (isUmbralIceIIIActivated ? 2.0f : 1.0f);
        }
    }
}
