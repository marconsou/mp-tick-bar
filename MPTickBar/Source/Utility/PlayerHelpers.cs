using Dalamud.Game.ClientState.Actors.Types;
using System.Linq;

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
            return (currentPlayer?.ClassJob?.Id == 25);
        }

        public static bool IsLucidDreamingActivated(PlayerCharacter currentPlayer)
        {
            return PlayerHelpers.IsEffectActivated(currentPlayer, 1204);
        }

        public static bool IsCircleOfPowerActivated(PlayerCharacter currentPlayer)
        {
            return PlayerHelpers.IsEffectActivated(currentPlayer, 738);
        }

        public static bool IsManafontOnCooldown(ActionManager actionManager)
        {
            return actionManager?.GetRecastTimeElapsed(ActionType.Spell, 158) != 0.0f;
        }

        public static float CalculatedFireIIICastTime(float fireIIICastTime, bool isCircleOfPowerActivated)
        {
            var circleOfPowerModifier = 0.85f;
            return fireIIICastTime * (isCircleOfPowerActivated ? circleOfPowerModifier : 1.0f) / 2.0f;
        }
    }
}