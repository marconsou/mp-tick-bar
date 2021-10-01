using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Linq;

namespace MPTickBar
{
    public static class PlayerHelpers
    {
        private static bool IsEffectActivated(PlayerCharacter currentPlayer, uint statusId)
        {
            if (currentPlayer == null)
                return false;

            return currentPlayer.StatusList.Any(x => x.StatusId == statusId);
        }

        public static bool IsBlackMage(PlayerCharacter currentPlayer)
        {
            if (currentPlayer == null)
                return false;

            return (currentPlayer.ClassJob?.Id == 25);
        }

        public static bool IsLucidDreamingActivated(PlayerCharacter currentPlayer)
        {
            return PlayerHelpers.IsEffectActivated(currentPlayer, 1204);
        }

        public static bool IsCircleOfPowerActivated(PlayerCharacter currentPlayer)
        {
            return PlayerHelpers.IsEffectActivated(currentPlayer, 738);
        }

        public static bool IsManafontOnCooldown()
        {
            unsafe
            {
                return ActionManager.Instance()->IsRecastTimerActive(ActionType.Spell, 158);
            }
        }

        public static bool IsUmbralIceIIIActivated(JobGauges jobGauges)
        {
            return (jobGauges.Get<BLMGauge>().UmbralIceStacks == 3);
        }

        public static float GetFastFireIIICastTime(int level, bool isCircleOfPowerActivated)
        {
            unsafe
            {
                var gcd35 = 3500;
                var astralUmbral = 50;
                var sub = LevelModifier.GetLevelModifierSub(level);
                var div = LevelModifier.GetLevelModifierDiv(level);
                var spellSpeed = UIState.pInstance->PlayerState.Attributes[46];
                return (float)Math.Floor(Math.Floor(Math.Ceiling(Math.Floor(100.0 - (isCircleOfPowerActivated ? 15 : 0)) * 1) * Math.Floor((2000 - Math.Floor(130.0 * (spellSpeed - sub) / div + 1000)) * gcd35 / 1000) / 1000) * astralUmbral / 100) / 100;
            }
        }
    }
}