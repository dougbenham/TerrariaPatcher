using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class InfiniteSundial : MarshalByRefObject, IPluginUpdate
    {
        public void OnUpdate()
        {
            Main.sundialCooldown = 0;
        }
    }
}
