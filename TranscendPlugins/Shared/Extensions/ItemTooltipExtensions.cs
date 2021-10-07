using Terraria.UI;

namespace TranscendPlugins.Shared.Extensions
{
    public static class ItemTooltipExtensions
    {
        public static void SetValue(this ItemTooltip tooltip, string text)
        {
            tooltip._text.SetValue(text);
            tooltip._validatorKey = 0;
        }
    }
}
