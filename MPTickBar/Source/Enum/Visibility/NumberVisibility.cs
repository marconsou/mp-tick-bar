using System.ComponentModel;

namespace MPTickBar
{
    public enum NumberVisibility
    {
        Visible,
        [Description("Only under Umbral Ice")]
        UnderUmbralIce,
        [Description("While in progress")]
        InProgress,
        Hidden,
    }
}