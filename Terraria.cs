using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace TerrariaPatcher
{
    public class TerrariaDetails
    {
        public bool FixedPrefixes = false;
        public bool PermanentWings = false;
        public bool RemovePotionSickness = false;
        public bool RemoveManaCost = false;
        public bool DisplayTime = false;
        public bool RemoveDiscordBuff = false;
        public bool RemoveAnglerQuestLimit = false;
        public bool RemoveDrowning = false;
        public bool OneHitKill = false;
        public bool DemigodMode = false;
		public bool InfiniteAmmo = false;
        public bool SteamFix = false;
        public bool Plugins = false;
        public bool InfiniteCloudJumps = false;
        public bool MaxCraftingRange = false;
        public float VampiricHealing = 7.5f;
        public float SpectreHealing = 20f;
		public int AccessoryPrefix = 65;
        public int Thorns = 33;
        public int SpawnRateVoodoo = 100;
        public List<int> PermanentBuffs = new List<int>();
    }

    public class Terraria
    {
        /// <summary>
        /// Gets or set the main assembly definition.
        /// </summary>
        private static AssemblyDefinition AsmDefinition { get; set; }

        /// <summary>
        /// Gets or sets the main module definition.
        /// </summary>
        private static ModuleDefinition ModDefinition { get; set; }

        /// <summary>
        /// Entrypoint.
        /// </summary>
        public static void Patch(string original, string target, TerrariaDetails details)
        {
            // Load Terraria and obtain the main module..
            AsmDefinition = AssemblyDefinition.ReadAssembly(original);
            ModDefinition = AsmDefinition.MainModule;

            /**
              * Patch functions.
              */

            if (details.PermanentWings) AddWings();
            if (details.SteamFix) RemoveSteam();
            if (details.PermanentBuffs.Count > 0) AddBuffs(details.PermanentBuffs);
            if (details.InfiniteAmmo) InfiniteAmmo();
            if (details.RemovePotionSickness) RemovePotionSickness();
            if (details.FixedPrefixes) FixPrefixes(details.AccessoryPrefix);
            if (details.RemoveDiscordBuff) RemoveDiscordBuff();
            if (details.MaxCraftingRange) RecipeRange();
            if (details.InfiniteCloudJumps) InfiniteCloudJumps();
            if (details.RemoveManaCost) RemoveManaCost();
            if (details.DemigodMode) DemigodMode();
            if (details.RemoveDrowning) RemoveDrowning();
            if (details.DisplayTime) DisplayTime();
            if (details.OneHitKill) OneHitKill();
            if (details.RemoveAnglerQuestLimit) RemoveAnglerQuestLimit();
            if (details.Plugins) Plugins();
            if (Math.Abs(details.VampiricHealing - 7.5f) > 0.01) ModVampiricKnives(details.VampiricHealing / 100f);
            if (Math.Abs(details.SpectreHealing - 20f) > 0.01) ModSpectreArmor(details.SpectreHealing / 100f);
            if (details.SpawnRateVoodoo != 15) ModSpawnRateVoodooDemon(details.SpawnRateVoodoo / 100f);
            //ModThornsBuff(details.Thorns / 100f);

            /**
              * Save patches.
              */

            AsmDefinition.Write(target);
        }

        private static void ModVampiricKnives(float healingRate) // default is 0.075
        {
            var projectile = IL.GetTypeDefinition(ModDefinition, "Projectile");
            var vampireHeal = IL.GetMethodDefinition(projectile, "vampireHeal");
            
            int spot = IL.ScanForOpcodePattern(vampireHeal, OpCodes.Ldc_R4);
            vampireHeal.Body.Instructions[spot].Operand = healingRate;
        }
        private static void ModSpectreArmor(float healingRate) // default is 0.2
        {
            var projectile = IL.GetTypeDefinition(ModDefinition, "Projectile");
            var ghostHeal = IL.GetMethodDefinition(projectile, "ghostHeal");

            int spot = IL.ScanForOpcodePattern(ghostHeal, OpCodes.Ldc_R4);
            ghostHeal.Body.Instructions[spot].Operand = healingRate;
        }

        private static void InfiniteCloudJumps()
        {
            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var update = IL.GetMethodDefinition(player, "Update");
            var jumpAgain1 = IL.GetFieldDefinition(player, "jumpAgainBlizzard");
            var jumpAgain2 = IL.GetFieldDefinition(player, "jumpAgainCloud");
            var jumpAgain3 = IL.GetFieldDefinition(player, "jumpAgainFart");
            var jumpAgain4 = IL.GetFieldDefinition(player, "jumpAgainSail");
            var jumpAgain5 = IL.GetFieldDefinition(player, "jumpAgainSandstorm");
            var jumpAgain6 = IL.GetFieldDefinition(player, "jumpAgainUnicorn");

            int spot = IL.ScanForOpcodePattern(update,
                                               (i, instruction) =>
                                               {
                                                   var i0 = update.Body.Instructions[i + 1].Operand as FieldReference;
                                                   return i0 != null && i0.Name == "doubleJumpCloud";
                                               },
                                               OpCodes.Ldc_I4_0,
                                               OpCodes.Stfld);

            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain1));
            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain2));
            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain3));
            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain4));
            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain5));
            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain6));
        }
        
        private static void ModThornsBuff(float rate)
        {
            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var updatePlayer = IL.GetMethodDefinition(player, "Update");

            var thornsScaling = new MethodDefinition("ThornsScaling", MethodAttributes.Private, ModDefinition.Import(typeof(int)));
            thornsScaling.Parameters.Add(new ParameterDefinition(ModDefinition.Import(typeof(int))));
            var thornsScalingIL = thornsScaling.Body.GetILProcessor();
            thornsScalingIL.Emit(OpCodes.Ldarg_1);
            thornsScalingIL.Emit(OpCodes.Conv_R4);
            thornsScalingIL.Emit(OpCodes.Ldc_R4, rate);
            thornsScalingIL.Emit(OpCodes.Mul);
            thornsScalingIL.Emit(OpCodes.Conv_I4);
            thornsScalingIL.Emit(OpCodes.Ret);
            player.Methods.Add(thornsScaling);

            int spot = IL.ScanForOpcodePattern(updatePlayer,
                                               (i, instruction) =>
                                               {
                                                   var i0 = updatePlayer.Body.Instructions[i].Operand as FieldReference;
                                                   return i0 != null && i0.Name == "thorns";
                                               },
                                               OpCodes.Ldfld);
            
            int spot2 = IL.ScanForOpcodePattern(updatePlayer,
                                                (i, instruction) => true,
                                                spot,
                                                OpCodes.Ldc_I4_3, 
                                                OpCodes.Div);

            var il = updatePlayer.Body.GetILProcessor();
            var in0 = updatePlayer.Body.Instructions[spot2];
            var in1 = updatePlayer.Body.Instructions[spot2 + 1];
            il.Remove(in0);
            il.InsertBefore(in1, il.Create(OpCodes.Call, ModDefinition.Import(thornsScaling)));
            il.Remove(in1);
        }

        private static void FixPrefixes(int accessoryPrefix)
        {
            var item = IL.GetTypeDefinition(ModDefinition, "Item");
            var prefix = IL.GetMethodDefinition(item, "Prefix");
            var il = prefix.Body.GetILProcessor();

            // Melee - Legendary
            int spot = IL.ScanForOpcodePattern(prefix,
                                                (i, instruction) =>
                                                {
                                                    return (sbyte)prefix.Body.Instructions[i].Operand == (sbyte)40;
                                                },
                                               new[]
                                               {
                                                   OpCodes.Ldc_I4_S,
                                                   OpCodes.Callvirt
                                               });
            prefix.Body.Instructions[spot - 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions[spot].Operand = sbyte.MaxValue;
            prefix.Body.Instructions[spot + 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions.Insert(spot + 3, il.Create(OpCodes.Ldc_I4, 81));
            prefix.Body.Instructions.Insert(spot + 4, il.Create(OpCodes.Stloc_0));

            // Generic - Godly
            int spot2 = IL.ScanForOpcodePattern(prefix,
                                                (i, instruction) =>
                                                {
                                                    return (sbyte)prefix.Body.Instructions[i].Operand == (sbyte)14;
                                                },
                                               new[]
                                               {
                                                   OpCodes.Ldc_I4_S,
                                                   OpCodes.Callvirt
                                               });
            prefix.Body.Instructions[spot2 - 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions[spot2].Operand = sbyte.MaxValue;
            prefix.Body.Instructions[spot2 + 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions.Insert(spot2 + 3, il.Create(OpCodes.Ldc_I4, 59));
            prefix.Body.Instructions.Insert(spot2 + 4, il.Create(OpCodes.Stloc_0));

            // Ranged - Unreal
            int spot3 = IL.ScanForOpcodePattern(prefix,
                                                (i, instruction) =>
                                                {
                                                    return (sbyte)prefix.Body.Instructions[i].Operand == (sbyte)36;
                                                },
                                               new[]
                                               {
                                                   OpCodes.Ldc_I4_S,
                                                   OpCodes.Callvirt
                                               });
            prefix.Body.Instructions[spot3 - 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions[spot3].Operand = sbyte.MaxValue;
            prefix.Body.Instructions[spot3 + 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions.Insert(spot3 + 3, il.Create(OpCodes.Ldc_I4, 82));
            prefix.Body.Instructions.Insert(spot3 + 4, il.Create(OpCodes.Stloc_0));

            // Magical - Mythical
            int spot4 = IL.ScanForOpcodePattern(prefix,
                                                (i, instruction) =>
                                                {
                                                    return (sbyte)prefix.Body.Instructions[i].Operand == (sbyte)36;
                                                },
                                               new[]
                                               {
                                                   OpCodes.Ldc_I4_S,
                                                   OpCodes.Callvirt
                                               });
            prefix.Body.Instructions[spot4 - 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions[spot4].Operand = sbyte.MaxValue;
            prefix.Body.Instructions[spot4 + 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions.Insert(spot4 + 3, il.Create(OpCodes.Ldc_I4, 83));
            prefix.Body.Instructions.Insert(spot4 + 4, il.Create(OpCodes.Stloc_0));

            // Boomerangs - Godly
            int spot5 = IL.ScanForOpcodePattern(prefix,
                                                (i, instruction) =>
                                                {
                                                    return (sbyte)prefix.Body.Instructions[i].Operand == (sbyte)14;
                                                },
                                               new[]
                                               {
                                                   OpCodes.Ldc_I4_S,
                                                   OpCodes.Callvirt
                                               });
            prefix.Body.Instructions[spot5 - 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions[spot5].Operand = sbyte.MaxValue;
            prefix.Body.Instructions[spot5 + 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions.Insert(spot5 + 3, il.Create(OpCodes.Ldc_I4, 59));
            prefix.Body.Instructions.Insert(spot5 + 4, il.Create(OpCodes.Stloc_0));
            
            // Accessory - based on selection of Accessory Prefix combo box
            int spot6 = IL.ScanForOpcodePattern(prefix,
                                                (i, instruction) =>
                                                {
                                                    return (sbyte)prefix.Body.Instructions[i].Operand == (sbyte)62 &&
                                                        (sbyte)prefix.Body.Instructions[i + 1].Operand == (sbyte)81;
                                                },
                                               new[]
                                               {
                                                   OpCodes.Ldc_I4_S,
                                                   OpCodes.Ldc_I4_S,
                                                   OpCodes.Callvirt
                                               });
            prefix.Body.Instructions[spot6 - 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions[spot6].Operand = (sbyte)accessoryPrefix;
            prefix.Body.Instructions[spot6 + 1].OpCode = OpCodes.Nop;
            prefix.Body.Instructions[spot6 + 2].OpCode = OpCodes.Nop;

            // No knockback - Demonic
            int spot7 = IL.ScanForOpcodePattern(prefix,
                                                (i, instruction) =>
                                                {
                                                    var i0 = prefix.Body.Instructions[i].Operand as FieldReference;
                                                    return i0 != null && i0.Name == "knockBack";
                                                },
                                               new[]
                                               {
                                                   OpCodes.Ldfld,
                                                   OpCodes.Ldc_R4,
                                                   OpCodes.Bne_Un_S
                                               });
            prefix.Body.Instructions.Insert(spot7 + 5, il.Create(OpCodes.Ldc_I4_0));
            prefix.Body.Instructions.Insert(spot7 + 6, il.Create(OpCodes.Starg, 1));
            prefix.Body.Instructions[spot7 + 7].OpCode = OpCodes.Ldc_I4_S;
            prefix.Body.Instructions[spot7 + 7].Operand = (sbyte)60;

            // No damage - Rapid
            int spot8 = IL.ScanForOpcodePattern(prefix,
                                                (i, instruction) =>
                                                {
                                                    var i0 = prefix.Body.Instructions[i].Operand as FieldReference;
                                                    return i0 != null && i0.Name == "damage";
                                                },
                                               new[]
                                               {
                                                   OpCodes.Ldfld,
                                                   OpCodes.Conv_R8,
                                                   OpCodes.Bne_Un_S
                                               });
            prefix.Body.Instructions.Insert(spot8 + 5, il.Create(OpCodes.Ldc_I4_0));
            prefix.Body.Instructions.Insert(spot8 + 6, il.Create(OpCodes.Starg, 1));
            prefix.Body.Instructions[spot8 + 7].OpCode = OpCodes.Ldc_I4_S;
            prefix.Body.Instructions[spot8 + 7].Operand = (sbyte)17;

            // No mana - Godly
            int spot9 = IL.ScanForOpcodePattern(prefix,
                                                (i, instruction) =>
                                                {
                                                    var i0 = prefix.Body.Instructions[i].Operand as FieldReference;
                                                    return i0 != null && i0.Name == "mana";
                                                },
                                               new[]
                                               {
                                                   OpCodes.Ldfld,
                                                   OpCodes.Conv_R8,
                                                   OpCodes.Bne_Un_S
                                               });
            prefix.Body.Instructions.Insert(spot9 + 5, il.Create(OpCodes.Ldc_I4_0));
            prefix.Body.Instructions.Insert(spot9 + 6, il.Create(OpCodes.Starg, 1));
            prefix.Body.Instructions[spot9 + 7].OpCode = OpCodes.Ldc_I4_S;
            prefix.Body.Instructions[spot9 + 7].Operand = (sbyte)59;

            // No attack speed - Godly
            int spot10 = IL.ScanForOpcodePattern(prefix,
                                                (i, instruction) =>
                                                {
                                                    var i0 = prefix.Body.Instructions[i].Operand as FieldReference;
                                                    return i0 != null && i0.Name == "useAnimation";
                                                },
                                               new[]
                                               {
                                                   OpCodes.Ldfld,
                                                   OpCodes.Conv_R8,
                                                   OpCodes.Bne_Un_S
                                               });
            prefix.Body.Instructions.Insert(spot10 + 5, il.Create(OpCodes.Ldc_I4_0));
            prefix.Body.Instructions.Insert(spot10 + 6, il.Create(OpCodes.Starg, 1));
            prefix.Body.Instructions[spot10 + 7].OpCode = OpCodes.Ldc_I4_S;
            prefix.Body.Instructions[spot10 + 7].Operand = (sbyte)59;

        }

        private static void OneHitKill()
        {
            var npc = IL.GetTypeDefinition(ModDefinition, "NPC");
            var strikeNPC = IL.GetMethodDefinition(npc, "StrikeNPC");

            int spot = IL.ScanForOpcodePattern(strikeNPC,
                OpCodes.Ldarg_1,
                OpCodes.Conv_R8,
                OpCodes.Stloc_1);
            
            var life = IL.GetFieldDefinition(npc, "life");
            strikeNPC.Body.Instructions[spot].OpCode = OpCodes.Ldarg_0;
            strikeNPC.Body.Instructions.Insert(spot + 1, Instruction.Create(OpCodes.Ldfld, life));

            int spot2 = IL.ScanForOpcodePattern(strikeNPC,
                (i, instruction) =>
                {
                    var i0 = strikeNPC.Body.Instructions[i].Operand as ParameterReference;
                    return i0 != null && i0.Name == "crit";
                },
                spot,
                OpCodes.Ldarg_S,
                OpCodes.Brfalse_S);

            for (int i = spot + 4; i < spot2; i++)
                strikeNPC.Body.Instructions[i].OpCode = OpCodes.Nop;
        }

        private static void DisplayTime()
        {
            var main = IL.GetTypeDefinition(ModDefinition, "Main");
            var drawInfoAccs = IL.GetMethodDefinition(main, "DrawInfoAccs");

            int spot = IL.ScanForOpcodePattern(drawInfoAccs, (i, instruction) =>
            {
                var fieldReference = instruction.Operand as FieldReference;
                return fieldReference != null && fieldReference.Name == "accWatch";
            },
                OpCodes.Ldfld,
                OpCodes.Ldc_I4_0,
                OpCodes.Ble
                );

            drawInfoAccs.Body.Instructions[spot + 2].OpCode = OpCodes.Blt;
        }

        private static void DemigodMode()
        {
            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var killMe = IL.GetMethodDefinition(player, "KillMe");
            killMe.Body.ExceptionHandlers.Clear();
            killMe.Body.Instructions.Clear();
            killMe.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void RemoveDrowning()
        {
            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var checkDrowning = IL.GetMethodDefinition(player, "CheckDrowning");
            checkDrowning.Body.ExceptionHandlers.Clear();
            checkDrowning.Body.Instructions.Clear();
            checkDrowning.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void RemoveAnglerQuestLimit()
        {
            var main = IL.GetTypeDefinition(ModDefinition, "Main");
            var guiChatDrawInner = IL.GetMethodDefinition(main, "GUIChatDrawInner");
            var questSwap = IL.GetMethodDefinition(main, "AnglerQuestSwap");

            int spot = IL.ScanForOpcodePattern(guiChatDrawInner,
                                               (i, instruction) =>
                                               {
                                                   var i3 = guiChatDrawInner.Body.Instructions[i + 3].Operand as FieldReference;
                                                   return   i3 != null && i3.Name == "anglerQuestFinished";
                                               },
                                               OpCodes.Ldloc_S,
                                               OpCodes.Brfalse,
                                               OpCodes.Ldc_I4_1);

            guiChatDrawInner.Body.Instructions[spot + 2] = Instruction.Create(OpCodes.Call, questSwap);
            for (int i = spot + 3; guiChatDrawInner.Body.Instructions[i].OpCode != OpCodes.Ret; i++)
                guiChatDrawInner.Body.Instructions[i].OpCode = OpCodes.Nop;
        }
        
		private static void InfiniteAmmo()
		{
			var player = IL.GetTypeDefinition(ModDefinition, "Player");
			var pickAmmo = IL.GetMethodDefinition(player, "PickAmmo");

            int spot = IL.ScanForOpcodePattern(pickAmmo,
												(i, instruction) =>
												{
                                                    var i1 = instruction.Operand as FieldReference;
													return i1 != null && i1.Name == "stack";
												},
												OpCodes.Ldfld,
												OpCodes.Ldc_I4_1,
												OpCodes.Sub,
												OpCodes.Stfld);

            pickAmmo.Body.Instructions[spot + 1].OpCode = OpCodes.Ldc_I4_0;
		}

		private static void RemovePotionSickness()
        {
            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var quickHeal = IL.GetMethodDefinition(player, "QuickHeal");
            var quickMana = IL.GetMethodDefinition(player, "QuickMana");
            var itemCheck = IL.GetMethodDefinition(player, "ItemCheck"); // regular potion usage

            // quick heal
		    int spot1 = IL.ScanForOpcodePattern(quickHeal,
		        (i, instruction) =>
		        {
		            var i1 = quickHeal.Body.Instructions[i + 1].Operand as FieldReference;
		            return i1 != null && i1.Name == "potion";
		        },
		        OpCodes.Ldloc_1,
		        OpCodes.Ldfld,
		        OpCodes.Brfalse_S);

		    for (int i = 0; i < 2; i++)
		        quickHeal.Body.Instructions[spot1 + i].OpCode = OpCodes.Nop;
            quickHeal.Body.Instructions[spot1 + 2].OpCode = OpCodes.Br_S;

            // quick mana
		    int spot2 = IL.ScanForOpcodePattern(quickMana,
		        (i, instruction) =>
		        {
		            var i4 = quickMana.Body.Instructions[i + 4].Operand as FieldReference;
		            return i4 != null && i4.Name == "potion";
		        },
		        OpCodes.Ldarg_0,
                OpCodes.Ldfld,
                OpCodes.Ldloc_0,
		        OpCodes.Ldelem_Ref,
                OpCodes.Ldfld,
                OpCodes.Brfalse_S);

            for (int i = 0; i < 5; i++)
                quickMana.Body.Instructions[spot2 + i].OpCode = OpCodes.Nop;
            quickMana.Body.Instructions[spot2 + 5].OpCode = OpCodes.Br_S;

            // health/mana
            int spot3 = IL.ScanForOpcodePattern(itemCheck,
                                               (i, instruction) =>
                                               {
                                                   var i2 = itemCheck.Body.Instructions[i + 2].Operand as FieldReference;
                                                   return i2 != null && i2.Name == "potionDelayTime";
                                               },
                                               OpCodes.Ldarg_0,
                                               OpCodes.Ldarg_0,
                                               OpCodes.Ldfld,
                                               OpCodes.Stfld);

            for (int i = 0; i < 10; i++)
                itemCheck.Body.Instructions[spot3 + i].OpCode = OpCodes.Nop;

            // rejuv
            int spot4 = IL.ScanForOpcodePattern(itemCheck,
                                               (i, instruction) =>
                                               {
                                                   var in1 = itemCheck.Body.Instructions[i - 1].OpCode;
                                                   var i0 = itemCheck.Body.Instructions[i + 0].Operand as FieldReference;
                                                   return in1.Name.ToLower().Contains("ldloc") && i0 != null && i0.Name == "potion";
                                               },
                                               OpCodes.Ldfld,
                                               OpCodes.Brfalse_S);

            for (int i = -1; i < 1; i++)
                itemCheck.Body.Instructions[spot4 + i].OpCode = OpCodes.Nop;
            itemCheck.Body.Instructions[spot4 + 1].OpCode = OpCodes.Br_S;
        }

        private static void RemoveManaCost()
        {
            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var itemCheck = IL.GetMethodDefinition(player, "ItemCheck");
            var checkMana = IL.GetMethodDefinition(player, "CheckMana");

            int spot = IL.ScanForOpcodePattern(itemCheck, (i, instruction) =>
            {
                var in1 = itemCheck.Body.Instructions[i - 1].OpCode;
                return in1.Name.ToLower().Contains("ldloc") && itemCheck.Body.Instructions[i + 1].Operand as sbyte? == 127;
            },
                OpCodes.Ldfld,
                OpCodes.Ldc_I4_S,
                OpCodes.Bne_Un_S);

            for (int i = -1; i < 5; i++)
                itemCheck.Body.Instructions[spot + i].OpCode = OpCodes.Nop;
            itemCheck.Body.Instructions[spot + 5].OpCode = OpCodes.Br;

            checkMana.Body.ExceptionHandlers.Clear();
            checkMana.Body.Instructions.Clear();
            checkMana.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            checkMana.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void RemoveDiscordBuff()
        {
            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var itemCheck = IL.GetMethodDefinition(player, "ItemCheck");

            int spot = IL.ScanForOpcodePattern(itemCheck, (i, instruction) =>
            {
                var fieldReference = instruction.Operand as FieldReference;
                return fieldReference != null && fieldReference.Name == "chaosState";
            },
                OpCodes.Ldfld,
                OpCodes.Brfalse);

            var target = itemCheck.Body.Instructions[spot + 1].Operand as Instruction;
            bool done = false;
            for (; !done; target = target.Next)
            {
                if (target.OpCode == OpCodes.Call) done = true;

                target.OpCode = OpCodes.Nop;
                target.Operand = null;
            }

            int spot2 = IL.ScanForOpcodePattern(itemCheck, (i, instruction) =>
            {
                var methodReference = instruction.Operand as MethodReference;
                return methodReference != null && methodReference.Name == "SolidCollision";
            },
                OpCodes.Call,
                OpCodes.Brtrue);

            itemCheck.Body.Instructions[spot2 + 1].OpCode = OpCodes.Pop;
        }

        private static void ModSpawnRateVoodooDemon(float rate)
        {
            var npc = IL.GetTypeDefinition(ModDefinition, "NPC");
            var spawn = IL.GetMethodDefinition(npc, "SpawnNPC");

            int spot = IL.ScanForOpcodePattern(spawn, (i, instruction) =>
                                           {
                                               var instr = spawn.Body.Instructions[i + 11];
                                               return (instr.Operand as sbyte?) == 66;
                                           }, new[]
                                           {
                                               OpCodes.Ldc_I4_7,
                                               OpCodes.Callvirt,
                                               OpCodes.Brtrue_S
                                           });

            spawn.Body.Instructions[spot].OpCode = OpCodes.Ldc_I4;
            spawn.Body.Instructions[spot].Operand = Math.Abs(rate) < 0.001 ? int.MaxValue : (int)Math.Round(1 / rate);
        }

        private static void AddBuffs(IEnumerable<int> buffs)
        {
            var main = IL.GetTypeDefinition(ModDefinition, "Main");
            var update = IL.GetMethodDefinition(main, "Update");
            var playerArr = IL.GetFieldDefinition(main, "player");
            var myPlayer = IL.GetFieldDefinition(main, "myPlayer");

            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var addBuff = IL.GetMethodDefinition(player, "AddBuff");

            foreach (int buff in buffs)
            {
                IL.MethodPrepend(update, new[]
                    {
                        Instruction.Create(OpCodes.Ldsfld, playerArr),
                        Instruction.Create(OpCodes.Ldsfld, myPlayer),
                        Instruction.Create(OpCodes.Ldelem_Ref),
                        Instruction.Create(OpCodes.Ldc_I4, buff),
                        Instruction.Create(OpCodes.Ldc_I4_2),
                        Instruction.Create(OpCodes.Ldc_I4_0),
                        Instruction.Create(OpCodes.Call, addBuff)
                    });
            }
        }
        
        private static void RecipeRange()
        {
            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var adjTiles = IL.GetMethodDefinition(player, "AdjTiles");

            int spot = IL.ScanForOpcodePattern(adjTiles, OpCodes.Ldc_I4_4,
                OpCodes.Stloc_0,
                OpCodes.Ldc_I4_3,
                OpCodes.Stloc_1
                );

            adjTiles.Body.Instructions[spot].OpCode = OpCodes.Ldc_I4;
            adjTiles.Body.Instructions[spot].Operand = 30;
            adjTiles.Body.Instructions[spot + 2].OpCode = OpCodes.Ldc_I4;
            adjTiles.Body.Instructions[spot + 2].Operand = 30;
        }

        private static void AddWings()
        {
            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var updatePlayerEquips = IL.GetMethodDefinition(player, "UpdateEquips");
            var wings = IL.GetFieldDefinition(player, "wings");
            var wingsLogic = IL.GetFieldDefinition(player, "wingsLogic");
            var wingTimeMax = IL.GetFieldDefinition(player, "wingTimeMax");

            IL.MethodAppend(updatePlayerEquips.Body.GetILProcessor(), updatePlayerEquips.Body.Instructions.Count - 1, 1, new[]
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4, 32),
                Instruction.Create(OpCodes.Stfld, wings),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4, 32),
                Instruction.Create(OpCodes.Stfld, wingsLogic),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4, int.MaxValue),
                Instruction.Create(OpCodes.Stfld, wingTimeMax),
                Instruction.Create(OpCodes.Ret)
            });
        }

        private static void Plugins()
        {
            // Loader target methods
            var loader = ModDefinition.Import(typeof(PluginLoader.Loader)).Resolve();
            var onInitialize = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnInitialize"));
            var onDrawInventory = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnDrawInventory"));
            var onUpdate = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnUpdate"));
            var onUpdateTime = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnUpdateTime"));
            var onPlaySound = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnPlaySound"));
            var onPlayerPreUpdate = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnPlayerPreUpdate"));
            var onPlayerUpdate = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnPlayerUpdate"));
            var onPlayerUpdateBuffs = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnPlayerUpdateBuffs"));
            var onPlayerPickAmmo = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnPlayerPickAmmo"));
            var onItemSetDefaults = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnItemSetDefaults"));
            var onProjectileAI = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnProjectileAI001"));
            var onRightClick = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnItemSlotRightClick"));
            var onNetMessage = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnNetMessageSendData"));
            var onGetColor = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnLightingGetColor"));
            var onGetItem = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnPlayerGetItem"));
            var onChestSetupShop = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnChestSetupShop"));
            var onPlayerQuickBuff = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnPlayerQuickBuff"));
            var onNPCLoot = ModDefinition.Import(IL.GetMethodDefinition(loader, "OnNPCLoot"));

            // Types
            var main = IL.GetTypeDefinition(ModDefinition, "Main");
            var player = IL.GetTypeDefinition(ModDefinition, "Player");
            var npc = IL.GetTypeDefinition(ModDefinition, "NPC");
            var item = IL.GetTypeDefinition(ModDefinition, "Item");
            var projectile = IL.GetTypeDefinition(ModDefinition, "Projectile");
            var itemSlot = IL.GetTypeDefinition(ModDefinition, "ItemSlot");
            var netMessage = IL.GetTypeDefinition(ModDefinition, "NetMessage");
            var lighting = IL.GetTypeDefinition(ModDefinition, "Lighting");
            var chest = IL.GetTypeDefinition(ModDefinition, "Chest");

            // Methods
            var initialize = IL.GetMethodDefinition(main, "Initialize");
            var drawInventory = IL.GetMethodDefinition(main, "DrawInventory");
            var update = IL.GetMethodDefinition(main, "Update");
            var updateTime = IL.GetMethodDefinition(main, "UpdateTime");
            var playSound = IL.GetMethodDefinition(main, "PlaySound", 4);
            var updatePlayer = IL.GetMethodDefinition(player, "Update");
            var updatePlayerBuffs = IL.GetMethodDefinition(player, "UpdateBuffs");
            var pickAmmo = IL.GetMethodDefinition(player, "PickAmmo");
            var setDefaults = IL.GetMethodDefinition(item, "SetDefaults", 2);
            var ai = IL.GetMethodDefinition(projectile, "AI_001");
            var rightClick = IL.GetMethodDefinition(itemSlot, "RightClick", 3);
            var sendData = IL.GetMethodDefinition(netMessage, "SendData");
            var getColor = IL.GetMethodDefinition(lighting, "GetColor", 2);
            var getItem = IL.GetMethodDefinition(player, "GetItem");
            var setupShop = IL.GetMethodDefinition(chest, "SetupShop");
            var quickBuff = IL.GetMethodDefinition(player, "QuickBuff");
            var npcLoot = IL.GetMethodDefinition(npc, "NPCLoot");

            {
                // Main.Initialize post hook
                IL.MethodAppend(initialize, initialize.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Call, onInitialize),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Main.DrawInventory post hook
                IL.MethodAppend(drawInventory, drawInventory.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Call, onDrawInventory),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // patch chat to allow on singleplayer
                int spot = IL.ScanForOpcodePattern(update, (i, instruction) =>
                {
                    var f0 = instruction.Operand as FieldReference;
                    var result0 = f0 != null && f0.Name == "netMode";
                    var f33 = update.Body.Instructions[i + 33].Operand as FieldReference;
                    var result33 = f33 != null && f33.Name == "chatMode";
                    return result0 && result33;
                }, OpCodes.Ldsfld,
                    OpCodes.Ldc_I4_1);

                update.Body.Instructions[spot + 0].OpCode = OpCodes.Nop;
                update.Body.Instructions[spot + 1].OpCode = OpCodes.Nop;
                update.Body.Instructions[spot + 2].OpCode = OpCodes.Nop;
            }

            {
                // Main.Update post hook
                IL.MethodAppend(update, update.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Call, onUpdate),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Main.UpdateTime post hook
                IL.MethodAppend(updateTime, updateTime.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Call, onUpdateTime),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Main.PlaySound pre hook
                var firstInstr = playSound.Body.Instructions.FirstOrDefault();
                IL.MethodPrepend(playSound, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0), // type
                    Instruction.Create(OpCodes.Ldarg_1), // x
                    Instruction.Create(OpCodes.Ldarg_2), // y
                    Instruction.Create(OpCodes.Ldarg_3), // style
                    Instruction.Create(OpCodes.Call, onPlaySound),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Player.Update pre hook
                IL.MethodPrepend(updatePlayer, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onPlayerPreUpdate)
                });

                // Player.Update post hook
                IL.MethodAppend(updatePlayer, updatePlayer.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onPlayerUpdate),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Player.UpdateBuffs post hook
                IL.MethodAppend(updatePlayerBuffs, updatePlayerBuffs.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onPlayerUpdateBuffs),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Player.PickAmmo post hook
                IL.MethodAppend(pickAmmo, pickAmmo.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "shoot")),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "speed")),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "canShoot")),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "Damage")),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "KnockBack")),
                    Instruction.Create(OpCodes.Call, onPlayerPickAmmo),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Item.SetDefaults post hook
                IL.MethodAppend(setDefaults, setDefaults.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onItemSetDefaults),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Projectile.AI post hook
                IL.MethodAppend(ai, ai.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onProjectileAI),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // ItemSlot.RightClick pre hook
                var firstInstr = rightClick.Body.Instructions.FirstOrDefault();
                IL.MethodPrepend(rightClick, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Call, onRightClick),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            {
                // NetMessage.SendData pre hook
                var firstInstr = sendData.Body.Instructions.FirstOrDefault();
                IL.MethodPrepend(sendData, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0), // msgType
                    Instruction.Create(OpCodes.Ldarg_1), // remoteClient
                    Instruction.Create(OpCodes.Ldarg_2), // ignoreClient
                    Instruction.Create(OpCodes.Ldarg_3), // text
                    Instruction.Create(OpCodes.Ldarg_S, sendData.Parameters[4]), // number
                    Instruction.Create(OpCodes.Ldarg_S, sendData.Parameters[5]), // number2
                    Instruction.Create(OpCodes.Ldarg_S, sendData.Parameters[6]), // number3
                    Instruction.Create(OpCodes.Ldarg_S, sendData.Parameters[7]), // number4
                    Instruction.Create(OpCodes.Ldarg_S, sendData.Parameters[8]), // number5
                    Instruction.Create(OpCodes.Ldarg_S, sendData.Parameters[9]), // number6
                    Instruction.Create(OpCodes.Ldarg_S, sendData.Parameters[10]), // number7
                    Instruction.Create(OpCodes.Call, onNetMessage),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Lighting.GetColor pre hook
                var firstInstr = getColor.Body.Instructions.FirstOrDefault();
                var varColor = new VariableDefinition("test", IL.GetTypeReference(ModDefinition, "Microsoft.Xna.Framework.Color"));
                getColor.Body.Variables.Add(varColor);
                IL.MethodPrepend(getColor, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldloca_S, varColor),
                    Instruction.Create(OpCodes.Call, onGetColor),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ldloc, varColor),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Player.GetItem pre hook
                var firstInstr = getItem.Body.Instructions.FirstOrDefault();
                var varItem = new VariableDefinition("test", IL.GetTypeDefinition(ModDefinition, "Item"));
                getItem.Body.Variables.Add(varItem);
                IL.MethodPrepend(getItem, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Ldloca_S, varItem),
                    Instruction.Create(OpCodes.Call, onGetItem),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ldloc, varItem),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Chest.SetupShop post hook
                IL.MethodAppend(setupShop, setupShop.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Call, onChestSetupShop),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Player.QuickBuff pre hook
                var firstInstr = quickBuff.Body.Instructions.FirstOrDefault();
                IL.MethodPrepend(quickBuff, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onPlayerQuickBuff),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // NPC.NPCLoot pre hook
                var firstInstr = npcLoot.Body.Instructions.FirstOrDefault();
                IL.MethodPrepend(npcLoot, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onNPCLoot),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            {
                // Make many fields/methods public so we can access freely without using reflection on the fly
                IL.MakeTypePublic(IL.GetTypeDefinition(ModDefinition, "MapHelper"));
                IL.MakeTypePublic(IL.GetTypeDefinition(ModDefinition, "Player"));
                IL.MakeTypePublic(IL.GetTypeDefinition(ModDefinition, "Main"));
                //IL.MakeTypePublic(IL.GetTypeDefinition(ModDefinition, "Lighting"));
            }
        }

        private static void RemoveSteam()
        {
            var socialAPI = IL.GetTypeDefinition(ModDefinition, "SocialAPI");
            var socialAPIInitialize = IL.GetMethodDefinition(socialAPI, "LoadSteam");

            socialAPIInitialize.Body.Instructions.Clear();
            socialAPIInitialize.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
    }
}
