using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using System.Linq;

namespace MPTickBar
{
    public static class PlayerHelpers
    {
        private static bool IsEffectActivated(PlayerCharacter currentPlayer, short statusId)
        {
            if (currentPlayer == null)
                return false;

            return currentPlayer.StatusList.Any(x => x.StatusId == statusId);
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

        public static bool IsUmbralIceIIIActivated(JobGauges jobGauges)
        {
            return (jobGauges.Get<BLMGauge>().UmbralIceStacks == 3);
        }

        public static float CalculatedFireIIICastTime(float fireIIICastTime, bool isUmbralIceIIIActivated, bool isCircleOfPowerActivated)
        {
            //unsafe
            //{
                //var fireIIIId = 152u;
                //ActionManager.Instance()->GetAdjustedCastTime(ActionType.Spell, fireIIIId);
            //}
            var circleOfPowerModifier = 0.85f;
            return (fireIIICastTime * (isCircleOfPowerActivated ? circleOfPowerModifier : 1.0f)) / (isUmbralIceIIIActivated ? 2.0f : 1.0f);
        }
    }
}