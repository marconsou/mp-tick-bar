using System.ComponentModel;

namespace MPTickBar
{
    public enum ProgressBarUI
    {
        Default,
        [Description("Material UI Discord")]
        MaterialUIDiscord,
        [Description("Material UI Black")]
        MaterialUIBlack,
        [Description("Material UI Silver")]
        MaterialUISilver,
        [Description("Solid Bar")]
        SolidBar
    }
}