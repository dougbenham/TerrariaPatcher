using System;
using System.Collections.Generic;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class CreativeCrafting : MarshalByRefObject, IPluginUpdate, IPluginChatCommand
    {
        private struct RecipeBackup
        {
            public Recipe.RequiredItemEntry[] RequiredQuickLookup;
            public int RequiredTile;
            public bool NeedWater;
            public bool NeedHoney;
            public bool NeedLava;
            public bool NeedSnowBiome;
            public bool NeedGraveyardBiome;
            public bool NeedMechdusa;
            public bool NeedTorchGodsFavor;
        }

        private readonly Dictionary<int, RecipeBackup> recipeBackups = new Dictionary<int, RecipeBackup>();
        private bool enabled;
        private bool refreshRecipes;
        private bool recipesOverridden;

        public void OnUpdate()
        {
            if (Main.gameMenu || !enabled)
                return;

            EnsurePlayerCanCraftAnywhere();

            if (!recipesOverridden)
            {
                ApplyRecipeOverrides();
            }

            if (refreshRecipes && Main.playerInventory)
            {
                Recipe.UpdateRecipeList();
                refreshRecipes = false;
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "craftmode") return false;

            bool newEnabled;
            if (args.Length == 0 || args[0].Equals("toggle", StringComparison.OrdinalIgnoreCase))
            {
                newEnabled = !enabled;
            }
            else if (args[0].Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                newEnabled = true;
            }
            else if (args[0].Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                newEnabled = false;
            }
            else if (args[0].Equals("status", StringComparison.OrdinalIgnoreCase))
            {
                LocalMessage(enabled ? "Craft without materials is enabled." : "Craft without materials is disabled.");
                return true;
            }
            else
            {
                LocalMessage("Usage:");
                LocalMessage("  /craftmode on");
                LocalMessage("  /craftmode off");
                LocalMessage("  /craftmode status");
                LocalMessage("  /craftmode toggle");
                return true;
            }

            SetEnabled(newEnabled);

            return true;
        }

        private static void EnsurePlayerCanCraftAnywhere()
        {
            var player = Main.player[Main.myPlayer];
            if (player == null) return;

            var adj = player.adjTile;
            if (adj != null)
            {
                for (int i = 0; i < adj.Length; i++)
                    adj[i] = true;
            }

            player.adjWaterSource = true;
            player.adjHoney = true;
            player.adjLava = true;
        }

        private static void LocalMessage(string message)
        {
            if (Main.netMode != 2)
                Main.NewText(message);
        }

        private void SetEnabled(bool newEnabled)
        {
            if (newEnabled == enabled)
            {
                if (enabled)
                    ApplyRecipeOverrides();
                refreshRecipes = true;
                return;
            }

            enabled = newEnabled;

            refreshRecipes = true;

            if (enabled)
            {
                ApplyRecipeOverrides();
                LocalMessage("Craft without materials enabled (recipes only).");
            }
            else
            {
                RestoreRecipeOverrides();
                LocalMessage("Craft without materials disabled.");
            }
        }

        private void ApplyRecipeOverrides()
        {
            recipesOverridden = true;

            var recipes = Main.recipe;
            var max = Math.Min(Recipe.maxRecipes, recipes.Length);
            for (int i = 0; i < max; i++)
            {
                var recipe = recipes[i];
                if (recipe == null) continue;

                if (!recipeBackups.ContainsKey(i))
                {
                    recipeBackups[i] = new RecipeBackup
                    {
                        RequiredQuickLookup = recipe.requiredItemQuickLookup,
                        RequiredTile = recipe.requiredTile,
                        NeedWater = recipe.needWater,
                        NeedHoney = recipe.needHoney,
                        NeedLava = recipe.needLava,
                        NeedSnowBiome = recipe.needSnowBiome,
                        NeedGraveyardBiome = recipe.needGraveyardBiome,
                        NeedMechdusa = recipe.needMechdusa,
                        NeedTorchGodsFavor = recipe.needTorchGodsFavor
                    };
                }

                recipe.requiredItemQuickLookup = CreateEmptyRequirements();
                recipe.requiredTile = -1;
                recipe.needWater = false;
                recipe.needHoney = false;
                recipe.needLava = false;
                recipe.needSnowBiome = false;
                recipe.needGraveyardBiome = false;
                recipe.needMechdusa = false;
                recipe.needTorchGodsFavor = false;
            }
        }

        private void RestoreRecipeOverrides()
        {
            if (!recipesOverridden)
                return;

            var recipes = Main.recipe;
            foreach (var kvp in recipeBackups)
            {
                if (kvp.Key < 0 || kvp.Key >= recipes.Length)
                    continue;

                var recipe = recipes[kvp.Key];
                if (recipe == null) continue;

                var backup = kvp.Value;
                recipe.requiredItemQuickLookup = backup.RequiredQuickLookup;
                recipe.requiredTile = backup.RequiredTile;
                recipe.needWater = backup.NeedWater;
                recipe.needHoney = backup.NeedHoney;
                recipe.needLava = backup.NeedLava;
                recipe.needSnowBiome = backup.NeedSnowBiome;
                recipe.needGraveyardBiome = backup.NeedGraveyardBiome;
                recipe.needMechdusa = backup.NeedMechdusa;
                recipe.needTorchGodsFavor = backup.NeedTorchGodsFavor;
            }

            recipesOverridden = false;
        }

        private static Recipe.RequiredItemEntry[] CreateEmptyRequirements()
        {
            return new Recipe.RequiredItemEntry[Recipe.maxRequirements];
        }
    }
}
