using Microsoft.Xna.Framework;
using Terraria;

namespace PluginLoader
{

    #region General

    public interface IPlugin
    {
    }

    #endregion
    
    #region Main

    public interface IPluginInitialize : IPlugin
    {
        void OnInitialize();
    }
    public interface IPluginDrawInventory : IPlugin
    {
        void OnDrawInventory();
    }
    public interface IPluginUpdate : IPlugin
    {
        void OnUpdate();
    }
    public interface IPluginUpdateTime : IPlugin
    {
        void OnUpdateTime();
    }
    public interface IPluginPlaySound : IPlugin
    {
        bool OnPlaySound(int type, int x, int y, int style);
    }

    #endregion

    #region Player

    public interface IPluginPlayerUpdate : IPlugin
    {
        void OnPlayerUpdate(Player player);
    }
    public interface IPluginPlayerPreUpdate : IPlugin
    {
        void OnPlayerPreUpdate(Player player);
    }
    public interface IPluginPlayerUpdateBuffs : IPlugin
    {
        void OnPlayerUpdateBuffs(Player player);
    }
    public interface IPluginPlayerPickAmmo : IPlugin
    {
        void OnPlayerPickAmmo(Player player, Item weapon, ref int shoot, ref float speed, ref bool canShoot, ref int damage, ref float knockback);
    }
    public interface IPluginPlayerGetItem : IPlugin
    {
        bool OnPlayerGetItem(Player player, Item newItem, out Item resultItem);
    }
    public interface IPluginPlayerQuickBuff : IPlugin
    {
        bool OnPlayerQuickBuff(Player player);
    }

    #endregion

    #region Item

    public interface IPluginItemSetDefaults : IPlugin
    {
        void OnItemSetDefaults(Item item);
    }
    public interface IPluginItemSlotRightClick : IPlugin
    {
        bool OnItemSlotRightClick(Item[] invObj, int context, int slot);
    }

    #endregion

    #region Projectile

    public interface IPluginProjectileAI : IPlugin
    {
        void OnProjectileAI001(Projectile projectile);
    }

    #endregion

    #region NetMessage

    public interface IPluginNetMessageSendData : IPlugin
    {
        bool OnNetMessageSendData(int msgType, int remoteClient, int ignoreClient, string text, int number, float number2, float number3, float number4,
            int number5, int number6, int number7);
    }
    public interface IPluginChatCommand : IPlugin
    {
        bool OnChatCommand(string command, string[] args);
    }

    #endregion

    #region Lighting

    public interface IPluginLightingGetColor : IPlugin
    {
        bool OnLightingGetColor(int x, int y, out Color color);
    }

    #endregion
    
    #region Chest

    public interface IPluginChestSetupShop : IPlugin
    {
        void OnChestSetupShop(Chest chest, int type);
    }

    #endregion

    #region NPC

    public interface IPluginNPCLoot : IPlugin
    {
        bool OnNPCLoot(NPC npc);
    }

    #endregion

}
