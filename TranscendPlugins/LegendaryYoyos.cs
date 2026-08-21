using PluginLoader;
using Terraria.GameContent.Prefixes;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("Lets every yoyo roll the special Legendary prefix that normally only the Terrarian can have.")]
    public class LegendaryYoyos : PluginBase, IPluginInitialize
    {
        public void OnInitialize()
        {
            for (var i = 0; i < ItemID.Sets.Yoyo.Length; i++)
            {
                if (!ItemID.Sets.Yoyo[i]) continue;

                PrefixLegacy.ItemSets.BoomerangsChakrams[i] = false;
                PrefixLegacy.ItemSets.ItemsThatCanHaveLegendary2[i] = true;
            }
        }
    }
}
