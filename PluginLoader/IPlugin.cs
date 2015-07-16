using Terraria;

namespace PluginLoader
{
    public interface IPlugin
    {
    }
    public interface IPluginUpdate : IPlugin
    {
        void OnUpdate();
    }
    public interface IPluginPlayerUpdate : IPlugin
    {
        void OnPlayerUpdate(Player player);
    }
    public interface IPluginPlayerUpdateBuffs : IPlugin
    {
        void OnPlayerUpdateBuffs(Player player);
    }
    public interface IPluginPlayerPickAmmo : IPlugin
    {
        void OnPlayerPickAmmo(Player player, Item weapon, ref int shoot, ref float speed, ref bool canShoot, ref int damage, ref float knockback);
    }
    public interface IPluginItemSetDefaults : IPlugin
    {
        void OnItemSetDefaults(Item item);
    }
    public interface IPluginProjectileAI : IPlugin
    {
        void OnProjectileAI001(Projectile projectile);
    }
    public interface IPluginItemSlotRightClick : IPlugin
    {
        bool OnItemSlotRightClick(Item[] invObj, int context, int slot);
    }
    public interface IPluginNetMessageSendData : IPlugin
    {
        bool OnNetMessageSendData(int msgType, int remoteClient, int ignoreClient, string text, int number, float number2, float number3, float number4,
            int number5, int number6, int number7);
    }
    public interface IPluginChatCommand : IPlugin
    {
        bool OnChatCommand(string command, string[] args);
    }
}
