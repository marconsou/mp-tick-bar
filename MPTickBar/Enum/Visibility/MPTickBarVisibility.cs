using System.ComponentModel;

namespace MPTickBar
{
    public enum MPTickBarVisibility
    {
        Visible,
        [Description("Only under Umbral Ice")]
        UnderUmbralIce,
        [Description("Only during combat")]
        InCombat,
        Hidden,
    }
}