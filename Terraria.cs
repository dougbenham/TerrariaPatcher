using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace TerrariaPatcher
{
    public class TerrariaDetails
    {
        public bool PermanentWings = false;
        public bool RemovePotionSickness = false;
        public bool RemoveManaCost = false;
        public bool DisplayTime = false;
        public bool RemoveDiscordBuff = false;
        public bool RemoveAnglerQuestLimit = false;
        public bool RemoveDrowning = false;
        public bool OneHitKill = false;
		public bool InfiniteAmmo = false;
        public bool SteamFix = false;
        public bool Plugins = false;
        public bool InfiniteCloudJumps = false;
        public bool FunctionalSocialSlots = false;
        public bool MaxCraftingRange = false;
        public float VampiricHealing = 7.5f;
        public float SpectreHealing = 20f;
        public int Thorns = 33;
        public int SpawnRateVoodoo = 100;
        public List<int> PermanentBuffs = new List<int>();
    }

    public class Terraria
    {
        /// <summary>
        /// Gets or sets the main module definition.
        /// </summary>
        private static ModuleDefinition _mainModule;

        private static bool IsModLoader => IL.GetTypeDefinition(_mainModule, "Mod", false) != null;

        /// <summary>
        /// Entrypoint.
        /// </summary>
        public static void Patch(string original, string target, TerrariaDetails details)
        {
            using (var asm = AssemblyDefinition.ReadAssembly(original, new ReaderParameters()
            {
                AssemblyResolver = new MyAssemblyResolver(Path.GetDirectoryName(original))
            }))
            {
                _mainModule = asm.MainModule;
                
                if (details.PermanentWings)
                    AddWings();
                if (details.SteamFix)
                    RemoveSteam();
                if (details.PermanentBuffs.Count > 0)
                    AddBuffs(details.PermanentBuffs);
                if (details.InfiniteAmmo)
                    InfiniteAmmo();
                if (details.RemovePotionSickness)
                    RemovePotionSickness();
                if (details.RemoveDiscordBuff)
                    RemoveDiscordBuff();
                if (details.MaxCraftingRange)
                    RecipeRange();
                if (details.FunctionalSocialSlots)
                    FunctionalSocialSlots();
                if (details.InfiniteCloudJumps)
                    InfiniteCloudJumps();
                if (details.RemoveManaCost)
                    RemoveManaCost();
                if (details.RemoveDrowning)
                    RemoveDrowning();
                if (details.DisplayTime)
                    DisplayTime();
                if (details.OneHitKill)
                    OneHitKill();
                if (details.RemoveAnglerQuestLimit)
                    RemoveAnglerQuestLimit();
                if (details.Plugins)
                    Plugins();
                if (Math.Abs(details.VampiricHealing - 7.5f) > 0.01)
                    ModVampiricKnives(details.VampiricHealing / 100f);
                if (Math.Abs(details.SpectreHealing - 20f) > 0.01)
                    ModSpectreArmor(details.SpectreHealing / 100f);
                if (details.SpawnRateVoodoo != 10)
                    ModSpawnRateVoodooDemon(details.SpawnRateVoodoo / 100f);
                //ModThornsBuff(details.Thorns / 100f);

                asm.Write(target + ".tmp");
            }

            if (File.Exists(target))
                File.Delete(target);
            File.Move(target + ".tmp", target);
            IL.MakeLargeAddressAware(target);
        }

        private static void FunctionalSocialSlots()
        {
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var ctor = IL.GetMethodDefinition(player, ".ctor");
            var updateEquips = IL.GetMethodDefinition(player, "UpdateEquips");
            
            int spot0 = IL.ScanForOpcodePattern(ctor, (i, instruction) =>
            {
                var i3 = ctor.Body.Instructions[i + 3].Operand as FieldReference;
                return i3 != null && i3.Name == "hideVisibleAccessory";
            }, OpCodes.Ldarg_0, OpCodes.Ldc_I4_S, OpCodes.Newarr, OpCodes.Stfld);
            ctor.Body.Instructions[spot0 + 1].Operand = (sbyte) 20;

            int spot = 0;
            while (true)
            {
                spot = IL.ScanForOpcodePattern(updateEquips, (i, instruction) =>
                {
                    return (sbyte) updateEquips.Body.Instructions[i].Operand == (sbyte) 10 &&
                           (updateEquips.Body.Instructions[i + 1].OpCode == OpCodes.Blt ||
                            updateEquips.Body.Instructions[i + 1].OpCode == OpCodes.Blt_S);
                }, spot, OpCodes.Ldc_I4_S);
                if (spot < 0)
                    break;
                updateEquips.Body.Instructions[spot].Operand = (sbyte) 20;
                spot++;
            }

            var isItemSlotUnlockedAndUsable = IL.GetMethodDefinition(player, "IsItemSlotUnlockedAndUsable");
            isItemSlotUnlockedAndUsable.Body.ExceptionHandlers.Clear();
            isItemSlotUnlockedAndUsable.Body.Instructions.Clear();
            isItemSlotUnlockedAndUsable.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            isItemSlotUnlockedAndUsable.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void ModVampiricKnives(float healingRate) // default is 0.075
        {
            var projectile = IL.GetTypeDefinition(_mainModule, "Projectile");
            var vampireHeal = IL.GetMethodDefinition(projectile, "vampireHeal");
            
            int spot = IL.ScanForOpcodePattern(vampireHeal, OpCodes.Ldc_R4);
            vampireHeal.Body.Instructions[spot].Operand = healingRate;
        }
        private static void ModSpectreArmor(float healingRate) // default is 0.2
        {
            var projectile = IL.GetTypeDefinition(_mainModule, "Projectile");
            var ghostHeal = IL.GetMethodDefinition(projectile, "ghostHeal");

            int spot = IL.ScanForOpcodePattern(ghostHeal, OpCodes.Ldc_R4);
            ghostHeal.Body.Instructions[spot].Operand = healingRate;
        }

        private static void InfiniteCloudJumps()
        {
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var update = IL.GetMethodDefinition(player, "Update");

            using (update.JumpFix())
            {
	            var names = new HashSet<string>
	            {
		            "canJumpAgain_Basilisk", "canJumpAgain_Blizzard", "canJumpAgain_Cloud", "canJumpAgain_Fart", "canJumpAgain_Sail",
		            "canJumpAgain_Sandstorm", "canJumpAgain_Santank", "canJumpAgain_Unicorn", "canJumpAgain_WallOfFleshGoat"
	            };
                
	            for (int i = 1; i < update.Body.Instructions.Count; i++)
	            {
		            if (update.Body.Instructions[i].OpCode == OpCodes.Stfld &&
		                update.Body.Instructions[i].Operand is FieldReference f &&
		                names.Contains(f.Name) &&
		                update.Body.Instructions[i - 1].OpCode == OpCodes.Ldc_I4_0)
		            {
			            update.Body.Instructions[i - 1].OpCode = OpCodes.Ldc_I4_1;
		            }
	            }
            }
        }
        
        private static void ModThornsBuff(float rate)
        {
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var updatePlayer = IL.GetMethodDefinition(player, "Update");

            var thornsScaling = new MethodDefinition("ThornsScaling", MethodAttributes.Private, _mainModule.Import(typeof(int)));
            thornsScaling.Parameters.Add(new ParameterDefinition(_mainModule.Import(typeof(int))));
            var thornsScalingIL = thornsScaling.Body.GetILProcessor();
            thornsScalingIL.Emit(OpCodes.Ldarg_1);
            thornsScalingIL.Emit(OpCodes.Conv_R4);
            thornsScalingIL.Emit(OpCodes.Ldc_R4, rate);
            thornsScalingIL.Emit(OpCodes.Mul);
            thornsScalingIL.Emit(OpCodes.Conv_I4);
            thornsScalingIL.Emit(OpCodes.Ret);
            player.Methods.Add(thornsScaling);

            using (updatePlayer.JumpFix())
            {
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
                il.InsertBefore(in1, il.Create(OpCodes.Call, _mainModule.Import(thornsScaling)));
                il.Remove(in1);
            }
        }

        private static void OneHitKill()
        {
            var npc = IL.GetTypeDefinition(_mainModule, "NPC");
            var strikeNPC = IL.GetMethodDefinition(npc, "StrikeNPC");

            using (strikeNPC.JumpFix())
            {
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
        }

        private static void DisplayTime()
        {
            var main = IL.GetTypeDefinition(_mainModule, "Main");
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

        private static void RemoveDrowning()
        {
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var checkDrowning = IL.GetMethodDefinition(player, "CheckDrowning");
            checkDrowning.Body.ExceptionHandlers.Clear();
            checkDrowning.Body.Instructions.Clear();
            checkDrowning.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void RemoveAnglerQuestLimit()
        {
            var main = IL.GetTypeDefinition(_mainModule, "Main");
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
			var player = IL.GetTypeDefinition(_mainModule, "Player");
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
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var quickHeal = IL.GetMethodDefinition(player, "QuickHeal");
            var applyLifeAndOrMana = IL.GetMethodDefinition(player, "ApplyLifeAndOrMana");
            var applyPotionDelay = IL.GetMethodDefinition(player, "ApplyPotionDelay");

            // quick heal
		    int spot1 = IL.ScanForOpcodePattern(quickHeal,
		        (i, instruction) =>
		        {
		            var i1 = quickHeal.Body.Instructions[i + 1].Operand as FieldReference;
		            return i1 != null && i1.Name == "buffType";
		        },
		        OpCodes.Ldloc_0,
		        OpCodes.Ldfld,
		        OpCodes.Ldc_I4_0,
		        OpCodes.Ble_S);

		    for (int i = 0; i < 3; i++)
		        quickHeal.Body.Instructions[spot1 + i].OpCode = OpCodes.Nop;
            quickHeal.Body.Instructions[spot1 + 3].OpCode = OpCodes.Br_S;

            // apply life / mana
		    int spot2 = IL.ScanForOpcodePattern(applyLifeAndOrMana,
		        (i, instruction) =>
		        {
			        var i2 = applyLifeAndOrMana.Body.Instructions[i + 2].Operand as FieldReference;
		            var i5 = applyLifeAndOrMana.Body.Instructions[i + 5].Operand as MethodReference;
		            return i2 != null && i2.Name == "manaSickTime" &&
		                   i5 != null && i5.Name == "AddBuff";
		        },
		        OpCodes.Ldarg_0,
		        OpCodes.Ldc_I4_S,
                OpCodes.Ldsfld,
		        OpCodes.Ldc_I4_1,
		        OpCodes.Ldc_I4_0,
		        OpCodes.Call);

            for (int i = 0; i < 6; i++)
	            applyLifeAndOrMana.Body.Instructions[spot2 + i].OpCode = OpCodes.Nop;

            // health/mana
            applyPotionDelay.Body.ExceptionHandlers.Clear();
            applyPotionDelay.Body.Instructions.Clear();
            applyPotionDelay.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void RemoveManaCost()
        {
            var player = IL.GetTypeDefinition(_mainModule, "Player");
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

            if (spot >= 0)
            {
                for (int i = -1; i < 5; i++)
                    itemCheck.Body.Instructions[spot + i].OpCode = OpCodes.Nop;
                itemCheck.Body.Instructions[spot + 5].OpCode = OpCodes.Br;
            }

            checkMana.Body.ExceptionHandlers.Clear();
            checkMana.Body.Instructions.Clear();
            checkMana.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            checkMana.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void RemoveDiscordBuff()
        {
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var itemCheckUseRodOfDiscord = IL.GetMethodDefinition(player, "ItemCheck_UseTeleportRod");

            int spot = IL.ScanForOpcodePattern(itemCheckUseRodOfDiscord, (i, instruction) =>
                {
                    var fieldReference = instruction.Operand as FieldReference;
                    return fieldReference != null && fieldReference.Name == "chaosState";
                },
                OpCodes.Ldfld,
                OpCodes.Brfalse_S) - 1;

            var target = itemCheckUseRodOfDiscord.Body.Instructions[spot];
            bool done = false;
            for (; !done; target = target.Next)
            {
                if (target.OpCode == OpCodes.Call && (target.Operand as MethodReference)?.Name == "AddBuff")
                    done = true;

                target.OpCode = OpCodes.Nop;
                target.Operand = null;
            }

            int spot2 = IL.ScanForOpcodePattern(itemCheckUseRodOfDiscord, (i, instruction) =>
                {
                    var methodReference = instruction.Operand as MethodReference;
                    return methodReference != null && methodReference.Name == "SolidCollision";
                },
                OpCodes.Call,
                OpCodes.Brtrue);

            itemCheckUseRodOfDiscord.Body.Instructions[spot2 + 1].OpCode = OpCodes.Pop;
        }

        private static void ModSpawnRateVoodooDemon(float rate)
        {
            var npc = IL.GetTypeDefinition(_mainModule, "NPC");
            var spawn = IL.GetMethodDefinition(npc, "SpawnNPC");

            int spot = IL.ScanForOpcodePattern(spawn, (i, instruction) =>
                                           {
                                               var instr = spawn.Body.Instructions[i + 12];
                                               return (instr.Operand as sbyte?) == 66;
                                           }, new[]
                                           {
                                               OpCodes.Ldc_I4_S,
                                               OpCodes.Callvirt,
                                               OpCodes.Brtrue_S
                                           });

            spawn.Body.Instructions[spot].OpCode = OpCodes.Ldc_I4_S;
            spawn.Body.Instructions[spot].Operand = Math.Abs(rate) < 0.001 ? sbyte.MaxValue : (sbyte) Math.Round(1 / rate);
        }

        private static void AddBuffs(IEnumerable<int> buffs)
        {
            var main = IL.GetTypeDefinition(_mainModule, "Main");
            var update = IL.GetMethodDefinition(main, "DoUpdate");
            var playerArr = IL.GetFieldDefinition(main, "player");
            var myPlayer = IL.GetFieldDefinition(main, "myPlayer");
            var gameMenu = IL.GetFieldDefinition(main, "gameMenu");

            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var addBuff = IL.GetMethodDefinition(player, "AddBuff");
            
            using (update.JumpFix())
            {
                var first = update.Body.Instructions.First();

                foreach (int buff in buffs)
                {
                    IL.MethodPrepend(update, new[]
                    {
                        Instruction.Create(OpCodes.Ldsfld, playerArr),
                        Instruction.Create(OpCodes.Ldsfld, myPlayer),
                        Instruction.Create(OpCodes.Ldelem_Ref),
                        Instruction.Create(OpCodes.Ldc_I4, buff),
                        Instruction.Create(OpCodes.Ldc_I4_2),
                        Instruction.Create(OpCodes.Ldc_I4_1),
                        Instruction.Create(OpCodes.Ldc_I4_0),
                        Instruction.Create(OpCodes.Call, addBuff)
                    });
                }

                IL.MethodPrepend(update, new[]
                {
                    Instruction.Create(OpCodes.Ldsfld, gameMenu),
                    Instruction.Create(OpCodes.Brtrue, first)
                });
            }
        }
        
        private static void RecipeRange()
        {
            var player = IL.GetTypeDefinition(_mainModule, "Player");
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
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var updatePlayerEquips = IL.GetMethodDefinition(player, "UpdateEquips");
            var wings = IL.GetFieldDefinition(player, "wings");
            var wingsLogic = IL.GetFieldDefinition(player, "wingsLogic");
            var wingTimeMax = IL.GetFieldDefinition(player, "wingTimeMax");

            using (updatePlayerEquips.JumpFix())
            {
                IL.MethodAppend(updatePlayerEquips, updatePlayerEquips.Body.Instructions.Count - 1, 1, new[]
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
        }

        private static void Plugins()
        {
            // Loader target methods
            if (_mainModule.AssemblyReferences.Any(r => r.Name == "FNA"))
                throw new NotSupportedException();
            
            TypeDefinition loader;
            var loaderFileName = "PluginLoader.XNA.dll";
            using (var fna = AssemblyDefinition.ReadAssembly(loaderFileName))
                loader = _mainModule.ImportReference(new TypeReference("PluginLoader", "Loader", fna.MainModule, fna.MainModule)).Resolve();
            var onInitialize = _mainModule.Import(IL.GetMethodDefinition(loader, "OnInitialize"));
            var onDrawSplash = _mainModule.Import(IL.GetMethodDefinition(loader, "OnDrawSplash"));
            var onDrawInterface = _mainModule.Import(IL.GetMethodDefinition(loader, "OnDrawInterface"));
            var onDrawInventory = _mainModule.Import(IL.GetMethodDefinition(loader, "OnDrawInventory"));
            var onPreUpdate = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPreUpdate"));
            var onUpdate = _mainModule.Import(IL.GetMethodDefinition(loader, "OnUpdate"));
            var onUpdateTime = _mainModule.Import(IL.GetMethodDefinition(loader, "OnUpdateTime"));
            var onCheckXmas = _mainModule.Import(IL.GetMethodDefinition(loader, "OnCheckXmas"));
            var onCheckHalloween = _mainModule.Import(IL.GetMethodDefinition(loader, "OnCheckHalloween"));
            var onPlaySound = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlaySound"));
            var onPlayerPreSpawn = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerPreSpawn"));
            var onPlayerSpawn = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerSpawn"));
            var onPlayerLoad = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerLoad"));
            var onPlayerSave = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerSave"));
            var onPlayerPreUpdate = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerPreUpdate"));
            var onPlayerUpdate = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerUpdate"));
            var onPlayerUpdateBuffs = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerUpdateBuffs"));
            var onPlayerUpdateEquips = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerUpdateEquips"));
            var onPlayerUpdateArmorSets = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerUpdateArmorSets"));
            var onPlayerKillMe = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerKillMe"));
            var onPlayerHurt = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerHurt"));
            var onPlayerPickAmmo = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerPickAmmo"));
            var onItemSetDefaults = _mainModule.Import(IL.GetMethodDefinition(loader, "OnItemSetDefaults"));
            var onProjectileAI = _mainModule.Import(IL.GetMethodDefinition(loader, "OnProjectileAI001"));
            var onRightClick = _mainModule.Import(IL.GetMethodDefinition(loader, "OnItemSlotRightClick"));
            var onItemRollAPrefix = _mainModule.Import(IL.GetMethodDefinition(loader, "OnItemRollAPrefix"));
            var onSendChatMessageFromClient = _mainModule.Import(IL.GetMethodDefinition(loader, "OnSendChatMessageFromClient"));
            var onGetColor = _mainModule.Import(IL.GetMethodDefinition(loader, "OnLightingGetColor"));
            var onGetItem = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerGetItem"));
            var onChestSetupShop = _mainModule.Import(IL.GetMethodDefinition(loader, "OnChestSetupShop"));
            var onPlayerQuickBuff = _mainModule.Import(IL.GetMethodDefinition(loader, "OnPlayerQuickBuff"));
            var onNPCLoot = _mainModule.Import(IL.GetMethodDefinition(loader, "OnNPCLoot"));

            // Types
            var main = IL.GetTypeDefinition(_mainModule, "Main");
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var npc = IL.GetTypeDefinition(_mainModule, "NPC");
            var item = IL.GetTypeDefinition(_mainModule, "Item");
            var projectile = IL.GetTypeDefinition(_mainModule, "Projectile");
            var itemSlot = IL.GetTypeDefinition(_mainModule, "ItemSlot");
            var lighting = IL.GetTypeDefinition(_mainModule, "Lighting");
            var chest = IL.GetTypeDefinition(_mainModule, "Chest");
            var soundEngine = IL.GetTypeDefinition(_mainModule, "SoundEngine");

            // Methods
            var initialize = IL.GetMethodDefinition(main, "Initialize_AlmostEverything");
            var drawSplash = IL.GetMethodDefinition(main, "DrawSplash");
            var drawInterface = IL.GetMethodDefinition(main, "DrawInterface");
            var drawInventory = IL.GetMethodDefinition(main, "DrawInventory");
            var update = IL.GetMethodDefinition(main, "DoUpdate");
            var updateEnterToggleChat = IL.GetMethodDefinition(main, "DoUpdate_Enter_ToggleChat");
            var updateTime = IL.GetMethodDefinition(main, "UpdateTime");
            var checkXmas = IL.GetMethodDefinition(main, "checkXMas");
            var checkHalloween = IL.GetMethodDefinition(main, "checkHalloween");
            var playSound = IL.GetMethodDefinition(soundEngine, "PlaySound", 6);
            var spawn = IL.GetMethodDefinition(player, "Spawn");
            var loadPlayer = IL.GetMethodDefinition(player, "Deserialize");
            var savePlayer = IL.GetMethodDefinition(player, "Serialize");
            var updatePlayer = IL.GetMethodDefinition(player, "Update");
            var updatePlayerBuffs = IL.GetMethodDefinition(player, "UpdateBuffs");
            var updatePlayerEquips = IL.GetMethodDefinition(player, "UpdateEquips");
            var updatePlayerArmorSets = IL.GetMethodDefinition(player, "UpdateArmorSets");
            var killMe = IL.GetMethodDefinition(player, "KillMe");
            var hurt = IL.GetMethodDefinition(player, "Hurt");
            var pickAmmo = IL.GetMethodDefinition(player, "PickAmmo");
            var setDefaults = IL.GetMethodDefinition(item, "SetDefaults", 3);
            var rollAPrefix = IL.GetMethodDefinition(item, "RollAPrefix");
            var ai = IL.GetMethodDefinition(projectile, "AI_001");
            var rightClick = IL.GetMethodDefinition(itemSlot, "RightClick", 3);
            var doUpdateHandleChat = IL.GetMethodDefinition(main, "DoUpdate_HandleChat");
            var getColor = IL.GetMethodDefinition(lighting, "GetColor", 2);
            var getItem = IL.GetMethodDefinition(player, "GetItem");
            var setupShop = IL.GetMethodDefinition(chest, "SetupShop");
            var quickBuff = IL.GetMethodDefinition(player, "QuickBuff");
            var npcLoot = IL.GetMethodDefinition(npc, "NPCLoot");
            
            using (initialize.JumpFix())
            {
                // Main.Initialize post hook
                IL.MethodAppend(initialize, initialize.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Call, onInitialize),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (drawSplash.JumpFix())
            {
                // Main.DrawSplash pre hook
                IL.MethodPrepend(drawSplash, new[]
                {
                    Instruction.Create(OpCodes.Call, onDrawSplash)
                });
            }
            
            using (drawInventory.JumpFix())
            {
                // Main.DrawInventory post hook
                IL.MethodAppend(drawInventory, drawInventory.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Call, onDrawInventory),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (drawInterface.JumpFix())
            {
                // Main.DrawInterface post hook
                IL.MethodAppend(drawInterface, drawInterface.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Call, onDrawInterface),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            /*if (!IsModLoader)
            {
                // patch chat to allow on singleplayer
                int spot = IL.ScanForOpcodePattern(updateEnterToggleChat, (i, instruction) =>
                    {
                        var f0 = instruction.Operand as FieldReference;
                        var result0 = f0 != null && f0.Name == "netMode";
                        var f33 = updateEnterToggleChat.Body.Instructions[i + 3].Operand as FieldReference;
                        var result33 = f33 != null && f33.Name == "keyState";
                        return result0 && result33;
                    }, OpCodes.Ldsfld,
                    OpCodes.Ldc_I4_1,
                    OpCodes.Bne_Un);

                updateEnterToggleChat.Body.Instructions[spot + 0].OpCode = OpCodes.Nop;
                updateEnterToggleChat.Body.Instructions[spot + 1].OpCode = OpCodes.Nop;
                updateEnterToggleChat.Body.Instructions[spot + 2].OpCode = OpCodes.Nop;
            }*/
            
            using (update.JumpFix())
            {
                // Main.Update pre hook
                IL.MethodPrepend(update, new[]
                {
                    Instruction.Create(OpCodes.Call, onPreUpdate)
                });

                // Main.Update post hook
                IL.MethodAppend(update, update.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Call, onUpdate),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (updateTime.JumpFix())
            {
                // Main.UpdateTime post hook
                IL.MethodAppend(updateTime, updateTime.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Call, onUpdateTime),
                    Instruction.Create(OpCodes.Ret)
                });
            }

            using (checkXmas.JumpFix())
            {
                // Main.checkXMas pre hook
                var firstInstr = checkXmas.Body.Instructions.FirstOrDefault();
                IL.MethodPrepend(checkXmas, new[]
                {
                    Instruction.Create(OpCodes.Call, onCheckXmas),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (checkHalloween.JumpFix())
            {
                // Main.checkHalloween pre hook
                var firstInstr = checkHalloween.Body.Instructions.FirstOrDefault();
                IL.MethodPrepend(checkHalloween, new[]
                {
                    Instruction.Create(OpCodes.Call, onCheckHalloween),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (playSound.JumpFix())
            {
                // SoundEngine.PlaySound pre hook
                var firstInstr = playSound.Body.Instructions.FirstOrDefault();
                IL.MethodPrepend(playSound, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0), // type
                    Instruction.Create(OpCodes.Ldarg_1), // x
                    Instruction.Create(OpCodes.Ldarg_2), // y
                    Instruction.Create(OpCodes.Ldarg_3), // style
                    Instruction.Create(OpCodes.Call, onPlaySound),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ldnull),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (loadPlayer.JumpFix())
            {
                // Player.Deserialize inline hook
                
                var instr = Instruction.Create(OpCodes.Ldarg_0);
                int spot = IL.ScanForOpcodePattern(loadPlayer, (i, instruction) => (instruction.Operand as MethodReference)?.Name == "LoadPlayer_LastMinuteFixes", OpCodes.Call);

                foreach (var instruction in loadPlayer.Body.Instructions)
                {
                    if (instruction.Operand == loadPlayer.Body.Instructions[spot - 1])
                        instruction.Operand = instr;
                }

                IL.MethodPrepend(loadPlayer, loadPlayer.Body.Instructions[spot - 1], new[]
                {
                    instr, 
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Call, onPlayerLoad)
                });
            }
            
            using (savePlayer.JumpFix())
            {
                // Player.Serialize post hook
                IL.MethodAppend(savePlayer, savePlayer.Body.Instructions.Count - 1, 1, new[]
                {
	                Instruction.Create(OpCodes.Ldarg_0), // playerFile
	                Instruction.Create(OpCodes.Ldarg_1), // player
	                Instruction.Create(OpCodes.Ldarg_2), // binaryWriter
	                Instruction.Create(OpCodes.Call, onPlayerSave),
	                Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (spawn.JumpFix())
            {
                // Player.Spawn pre hook
                IL.MethodPrepend(spawn, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onPlayerPreSpawn)
                });

                // Player.Spawn post hook
                IL.MethodAppend(spawn, spawn.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onPlayerSpawn),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (updatePlayer.JumpFix())
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
            
            using (updatePlayerBuffs.JumpFix())
            {
                // Player.UpdateBuffs post hook
                IL.MethodAppend(updatePlayerBuffs, updatePlayerBuffs.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onPlayerUpdateBuffs),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (updatePlayerEquips.JumpFix())
            {
                // Player.UpdateEquips post hook
                IL.MethodAppend(updatePlayerEquips, updatePlayerEquips.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onPlayerUpdateEquips),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (updatePlayerArmorSets.JumpFix())
            {
                // Player.UpdateArmorSets post hook
                IL.MethodAppend(updatePlayerArmorSets, updatePlayerArmorSets.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onPlayerUpdateArmorSets),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (killMe.JumpFix())
            {
                // Player.KillMe pre hook
                var firstInstr = killMe.Body.Instructions.FirstOrDefault();
                IL.MethodPrepend(killMe, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Ldarg_3),
                    Instruction.Create(OpCodes.Ldarg_S, killMe.Parameters.FirstOrDefault(def => def.Name == "pvp")),
                    Instruction.Create(OpCodes.Call, onPlayerKillMe),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (hurt.JumpFix())
            {
                // Player.Hurt pre hook
                var firstInstr = hurt.Body.Instructions.FirstOrDefault();
                var varDbl = new VariableDefinition(_mainModule.Import(typeof(double)));
                hurt.Body.Variables.Add(varDbl);
                IL.MethodPrepend(hurt, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0), // this
                    Instruction.Create(OpCodes.Ldarg_1), // damageSource
                    Instruction.Create(OpCodes.Ldarg_2), // Damage
                    Instruction.Create(OpCodes.Ldarg_3), // hitDirection
                    Instruction.Create(OpCodes.Ldarg_S, hurt.Parameters.FirstOrDefault(def => def.Name == "pvp")),
                    Instruction.Create(OpCodes.Ldarg_S, hurt.Parameters.FirstOrDefault(def => def.Name == "quiet")),
                    Instruction.Create(OpCodes.Ldarg_S, hurt.Parameters.FirstOrDefault(def => def.Name == "Crit")),
                    Instruction.Create(OpCodes.Ldarg_S, hurt.Parameters.FirstOrDefault(def => def.Name == "cooldownCounter")),
                    Instruction.Create(OpCodes.Ldarg_S, hurt.Parameters.FirstOrDefault(def => def.Name == "dodgeable")),
                    Instruction.Create(OpCodes.Ldloca_S, varDbl),
                    Instruction.Create(OpCodes.Call, onPlayerHurt),
                    Instruction.Create(OpCodes.Brfalse_S, firstInstr),
                    Instruction.Create(OpCodes.Ldloc, varDbl),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (pickAmmo.JumpFix())
            {
                // Player.PickAmmo post hook
                IL.MethodAppend(pickAmmo, pickAmmo.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "projToShoot")),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "speed")),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "canShoot")),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "Damage")),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "KnockBack")),
                    Instruction.Create(OpCodes.Ldarga_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "usedAmmoItemId")),
                    Instruction.Create(OpCodes.Ldarg_S, pickAmmo.Parameters.FirstOrDefault(def => def.Name == "dontConsume")),
                    Instruction.Create(OpCodes.Call, onPlayerPickAmmo),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (setDefaults.JumpFix())
            {
                // Item.SetDefaults post hook
                IL.MethodAppend(setDefaults, setDefaults.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onItemSetDefaults),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (ai.JumpFix())
            {
                // Projectile.AI post hook
                IL.MethodAppend(ai, ai.Body.Instructions.Count - 1, 1, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, onProjectileAI),
                    Instruction.Create(OpCodes.Ret)
                });
            }
            
            using (rightClick.JumpFix())
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
            
            using (rollAPrefix.JumpFix())
            {
	            // Item.RollAPrefix pre hook
	            var varResult = new VariableDefinition(_mainModule.Import(typeof(bool)));
	            rollAPrefix.Body.Variables.Add(varResult);
	            var firstInstr = rollAPrefix.Body.Instructions.FirstOrDefault();
	            IL.MethodPrepend(rollAPrefix, new[]
	            {
		            Instruction.Create(OpCodes.Ldarg_0),
		            Instruction.Create(OpCodes.Ldarg_1),
		            Instruction.Create(OpCodes.Ldarg_2),
		            Instruction.Create(OpCodes.Ldloca_S, varResult),
		            Instruction.Create(OpCodes.Call, onItemRollAPrefix),
		            Instruction.Create(OpCodes.Brfalse_S, firstInstr),
		            Instruction.Create(OpCodes.Ldloc, varResult),
		            Instruction.Create(OpCodes.Ret)
	            });
            }

            using (doUpdateHandleChat.JumpFix())
            {
                // Main.DoUpdate_HandleChat hook
                int spot = IL.ScanForOpcodePattern(doUpdateHandleChat, (i, instruction) =>
                    {
                        var fieldReference = instruction.Operand as FieldReference;
                        return fieldReference != null && fieldReference.Name == "netMode";
                    },
                    OpCodes.Ldsfld,
                    OpCodes.Ldc_I4_1,
                    OpCodes.Bne_Un_S);
                int spot2 = IL.ScanForOpcodePattern(doUpdateHandleChat, (i, instruction) => instruction.Operand as string == "", spot, OpCodes.Ldstr);
                var chatMessage = doUpdateHandleChat.Body.Variables.FirstOrDefault(definition => definition.VariableType.Name == "ChatMessage");

                var skip = doUpdateHandleChat.Body.Instructions[spot2];
                IL.MethodPrepend(doUpdateHandleChat, doUpdateHandleChat.Body.Instructions[spot], new[]
                {
                    Instruction.Create(OpCodes.Ldloc_S, chatMessage), // text
                    Instruction.Create(OpCodes.Call, onSendChatMessageFromClient),
                    Instruction.Create(OpCodes.Brtrue_S, skip)
                });
            }
            
            using (getColor.JumpFix())
            {
                // Lighting.GetColor pre hook
                var firstInstr = getColor.Body.Instructions.FirstOrDefault();
                var varColor = new VariableDefinition(IL.GetTypeReference(_mainModule, "Microsoft.Xna.Framework.Color"));
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
            
            using (getItem.JumpFix())
            {
                // Player.GetItem pre hook
                var firstInstr = getItem.Body.Instructions.FirstOrDefault();
                var varItem = new VariableDefinition(IL.GetTypeDefinition(_mainModule, "Item"));
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
            
            using (setupShop.JumpFix())
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
            
            using (quickBuff.JumpFix())
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
            
            using (npcLoot.JumpFix())
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
                IL.MakeTypePublic(IL.GetTypeDefinition(_mainModule, "MapHelper"));
                IL.MakeTypePublic(IL.GetTypeDefinition(_mainModule, "Player"));
                IL.MakeTypePublic(IL.GetTypeDefinition(_mainModule, "Main"));
                IL.MakeTypePublic(IL.GetTypeDefinition(_mainModule, "Lang"));
                IL.MakeTypePublic(IL.GetTypeDefinition(_mainModule, "LocalizedText"));
                IL.MakeTypePublic(IL.GetTypeDefinition(_mainModule, "ItemTooltip"));
                IL.MakeTypePublic(IL.GetTypeDefinition(_mainModule, "Item"));
            }
        }

        private static void RemoveSteam()
        {
            var socialAPI = IL.GetTypeDefinition(_mainModule, "SocialAPI");
            var socialAPIInitialize = IL.GetMethodDefinition(socialAPI, "LoadSteam");

            socialAPIInitialize.Body.Instructions.Clear();
            socialAPIInitialize.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
    }
}
