using System.ComponentModel;

namespace MPTickBar
{
    public enum NumberVisibility
    {
        Visible,
        [Description("While in progress")]
        WhileInProgress,
        Hidden,
    }
}