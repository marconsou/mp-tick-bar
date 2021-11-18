using System.ComponentModel;

namespace MPTickBar
{
    public enum MPTickBarVisibility
    {
        Visible,
        [Description("Only during combat")]
        InCombat,
        Hidden,
    }
}