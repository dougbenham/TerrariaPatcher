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
        public bool BossBagsDropAllLoot = false;
        public List<int> PermanentBuffs = new List<int>();
    }

    public class Terraria
    {
        private class BossBagDrop
        {
            public int Item;
            public int Stack;
        }

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
                {
                    if (details.PermanentBuffs.Contains(147))
                        EnableAllBannerBuffs();
                    AddBuffs(details.PermanentBuffs);
                }
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
                if (details.BossBagsDropAllLoot)
                    TreasureBagsDropAll();
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
            var npcChatTextDoAnglerQuest = IL.GetMethodDefinition(main, "NPCChatText_DoAnglerQuest");
            var questSwap = IL.GetMethodDefinition(main, "AnglerQuestSwap");

            int spot = IL.ScanForOpcodePattern(npcChatTextDoAnglerQuest,
                                               (i, instruction) =>
                                               {
                                                   var i3 = npcChatTextDoAnglerQuest.Body.Instructions[i + 3].Operand as FieldReference;
                                                   return   i3 != null && i3.Name == "anglerQuestFinished";
                                               },
                                               OpCodes.Ldloc_0,
                                               OpCodes.Brfalse_S,
                                               OpCodes.Ldc_I4_1);

            npcChatTextDoAnglerQuest.Body.Instructions[spot + 2] = Instruction.Create(OpCodes.Call, questSwap);
            for (int i = spot + 3; npcChatTextDoAnglerQuest.Body.Instructions[i].OpCode != OpCodes.Ret; i++)
                npcChatTextDoAnglerQuest.Body.Instructions[i].OpCode = OpCodes.Nop;
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
		            var i5 = applyLifeAndOrMana.Body.Instructions[i + 4].Operand as MethodReference;
		            return i2 != null && i2.Name == "manaSickTime" &&
		                   i5 != null && i5.Name == "AddBuff";
		        },
		        OpCodes.Ldarg_0,
		        OpCodes.Ldc_I4_S,
                OpCodes.Ldsfld,
		        OpCodes.Ldc_I4_0,
		        OpCodes.Call);

            for (int i = 0; i < 5; i++)
	            applyLifeAndOrMana.Body.Instructions[spot2 + i].OpCode = OpCodes.Nop;

            // health/mana
            applyPotionDelay.Body.ExceptionHandlers.Clear();
            applyPotionDelay.Body.Instructions.Clear();
            applyPotionDelay.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void RemoveManaCost()
        {
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var skipManaUse = IL.GetMethodDefinition(player, "ItemCheck_PayMana_ShouldSkipManaUse");
            
            skipManaUse.Body.ExceptionHandlers.Clear();
            skipManaUse.Body.Instructions.Clear();
            skipManaUse.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            skipManaUse.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
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
            var spawner = npc.NestedTypes.First(t => t.Name == "Spawner");
            var spawn = IL.GetMethodDefinition(spawner, "SpawnAnNPC");

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

        private static void EnableAllBannerBuffs()
        {
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var hasNpcBannerBuff = IL.GetMethodDefinition(player, "HasNPCBannerBuff");

            hasNpcBannerBuff.Body.ExceptionHandlers.Clear();
            hasNpcBannerBuff.Body.Instructions.Clear();

            var il = hasNpcBannerBuff.Body.GetILProcessor();
            il.Emit(OpCodes.Ldc_I4_1); // true
            il.Emit(OpCodes.Ret);
        }
        
        private static void RecipeRange()
        {
	        var tileReachCheckSettings = IL.GetTypeDefinition(_mainModule, "TileReachCheckSettings");
	        var getRanges = IL.GetMethodDefinition(tileReachCheckSettings, "GetRanges");
	        var tileReachLimit = IL.GetFieldDefinition(tileReachCheckSettings, "TileReachLimit");

	        using (getRanges.JumpFix())
	        {
		        IL.MethodPrepend(getRanges, new[]
		        {
			        Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldflda, tileReachLimit),
			        Instruction.Create(OpCodes.Initobj, _mainModule.ImportReference(typeof(int?)))
		        });
	        }
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
            var setDefaults = IL.GetMethodDefinition(item, "SetDefaults", 2);
            var rollAPrefix = IL.GetMethodDefinition(item, "RollAPrefix");
            var ai = IL.GetMethodDefinition(projectile, "AI_001");
            var rightClick = IL.GetMethodDefinition(itemSlot, "RightClick", 3);
            var doUpdateHandleChat = IL.GetMethodDefinition(main, "DoUpdate_HandleChat");
            var getColor = (from MethodDefinition m in lighting.Methods
	            where m.Name == "GetColor" && m.Parameters.Count == 2
	                                       && m.Parameters[0].ParameterType.FullName == "System.Int32"
	                                       && m.Parameters[1].ParameterType.FullName == "System.Int32"
	            select m).FirstOrDefault();
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
                    Instruction.Create(OpCodes.Ldarg_1),
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

        private static void TreasureBagsDropAll()
        {
            var player = IL.GetTypeDefinition(_mainModule, "Player");
            var openBossBag = IL.GetMethodDefinition(player, "OpenBossBag");
            if (openBossBag == null) return;

            if (openBossBag.Body.Variables.Count < 2) return;
            var entitySourceType = openBossBag.Body.Variables[1].VariableType;

            var quickSpawn = player.Methods.FirstOrDefault(m =>
                m.Name == "QuickSpawnItem" &&
                m.Parameters.Count == 3 &&
                m.Parameters[0].ParameterType.FullName.Contains("IEntitySource"));
            var getItemSource = IL.GetMethodDefinition(player, "GetItemSource_OpenItem");
            if (quickSpawn == null || getItemSource == null) return;

            var instructions = openBossBag.Body.Instructions;
            var indexMap = instructions.Select((ins, idx) => new { ins, idx }).ToDictionary(x => x.ins, x => x.idx);
            var loot = new Dictionary<int, List<BossBagDrop>>();
            var coinIds = new HashSet<int> { 71, 72, 73, 74 }; // copper, silver, gold, platinum coins
            var killSegments = new List<List<Instruction>>();
            var listAdds = new Dictionary<int, List<int>>(); // bagId -> constants added to List<int> before weapon selection
            var arrayAdds = new Dictionary<int, List<int>>(); // bagId -> constants stored into new int[] pools
            var arrayFieldRefs = new Dictionary<int, List<FieldReference>>(); // bagId -> static int[] fields referenced

            // Identify bag-specific code regions by branching pattern
            for (int i = 0; i < instructions.Count - 2; i++)
            {
                if (instructions[i].OpCode != OpCodes.Ldarg_1 || instructions[i + 1].OpCode != OpCodes.Ldc_I4)
                    continue;

                int bagId;
                if (!int.TryParse(instructions[i + 1].Operand.ToString(), out bagId))
                    continue;

                var branch = instructions[i + 2];
                if (branch == null || branch.Operand == null || !indexMap.ContainsKey(branch.Operand as Instruction))
                    continue;

                int start = i + 3;
                int end = indexMap[(Instruction)branch.Operand] - 1;
                if (end < start) continue;

                // track list variable -> elements for this bag
                var listLocalMap = new Dictionary<int, List<int>>();
                _currentBagListAdds = listLocalMap;

                // collect List<int> constants (e.g., Moon Lord weapons)
                for (int j = start; j <= end - 1; j++)
                {
                    if (instructions[j].OpCode == OpCodes.Newobj &&
                        instructions[j].Operand is MethodReference ctor &&
                        ctor.DeclaringType.FullName.Contains("System.Collections.Generic.List`1<System.Int32>"))
                    {
                        var added = new List<int>();
                        int k = j + 1;
                        while (k < end - 1 &&
                               instructions[k].OpCode == OpCodes.Dup &&
                               instructions[k + 1].OpCode == OpCodes.Ldc_I4)
                        {
                            added.Add((int)instructions[k + 1].Operand);
                            k += 3; // dup, ldc.i4, callvirt Add
                        }
                        if (added.Count > 0)
                        {
                            // map to bag-level listAdds (bagId) and local variable if stored
                            if (!listAdds.ContainsKey(bagId)) listAdds[bagId] = new List<int>();
                            foreach (var c in added)
                                if (!listAdds[bagId].Contains(c)) listAdds[bagId].Add(c);

                            // find where list is stored (next stloc)
                            int st = j + 1;
                            while (st <= end && !IsStoreToAnyLocal(instructions[st])) st++;
                            if (st <= end && IsStoreToAnyLocal(instructions[st]))
                            {
                                int lidx = GetStoreLocalIndex(instructions[st]);
                                if (lidx >= 0)
                                {
                                    listLocalMap[lidx] = new List<int>(added);
                                }
                            }
                        }
                    }
                }

                // collect int[] constants (e.g., emblem pools) newarr + stelem
                for (int j = start; j <= end - 3; j++)
                {
                    if (instructions[j].OpCode == OpCodes.Newarr &&
                        instructions[j].Operand is TypeReference tr &&
                        tr.FullName == "System.Int32")
                    {
                        var added = new List<int>();
                        int k = j + 1;
                        while (k <= end - 3 &&
                               instructions[k].OpCode == OpCodes.Dup &&
                               IsAnyConstant(instructions[k + 1]) &&
                               IsAnyConstant(instructions[k + 2]) &&
                               instructions[k + 3].OpCode == OpCodes.Stelem_I4)
                        {
                            int val;
                            if (TryGetConstantInt(instructions[k + 2], out val))
                                added.Add(val);
                            k += 4; // dup, idx, val, stelem
                        }
                        if (added.Count > 0)
                        {
                            if (!arrayAdds.ContainsKey(bagId)) arrayAdds[bagId] = new List<int>();
                            foreach (var c in added)
                                if (!arrayAdds[bagId].Contains(c)) arrayAdds[bagId].Add(c);
                        }
                    }
                }

                // fallback: any stelem.i4 with constant within the bag block (covers non-sequential writes)
                for (int j = start + 2; j <= end; j++)
                {
                    if (instructions[j].OpCode == OpCodes.Stelem_I4 &&
                        IsAnyConstant(instructions[j - 1]))
                    {
                        int val;
                        if (TryGetConstantInt(instructions[j - 1], out val))
                        {
                            if (!arrayAdds.ContainsKey(bagId)) arrayAdds[bagId] = new List<int>();
                            if (!arrayAdds[bagId].Contains(val)) arrayAdds[bagId].Add(val);
                        }
                    }
                }

                for (int j = start; j <= end; j++)
                {
                    var instr = instructions[j];
                    if (instr.OpCode != OpCodes.Call) continue;
                    var mr = instr.Operand as MethodReference;
                    if (mr == null || mr.Name != "QuickSpawnItem" || mr.Parameters.Count != 3) continue;
                    if (j < 2) continue;

                    var itemCandidates = ResolvePossibleInts(instructions, j - 2);
                    var stackCandidates = ResolvePossibleInts(instructions, j - 1);
                    if (stackCandidates.Count == 0) stackCandidates.Add(1);
                    if (stackCandidates.Count > 5) // e.g., large ranges like luminite 90-110
                    {
                        stackCandidates = new List<int> { stackCandidates.Max() };
                    }

                    // Fallback for switch-assigned locals (e.g., Moon Lord weapons)
                    if (itemCandidates.Count == 0 && IsLoadLocal(instructions[j - 2]))
                    {
                        int localIdx = GetLocalIndex(instructions[j - 2]);
                        if (localIdx >= 0)
                        {
                            var constants = CollectConstAssignments(instructions, localIdx);
                            foreach (var c in constants) if (!itemCandidates.Contains(c)) itemCandidates.Add(c);
                        }
                    }

                    // Detect ldelem from static int[] field (e.g., emblem arrays)
                    if (itemCandidates.Count == 0 &&
                        instructions[j - 2].OpCode.Code == Code.Ldelem_I4)
                    {
                        for (int back = j - 3; back >= j - 8 && back >= start; back--)
                        {
                            if (instructions[back].OpCode == OpCodes.Ldsfld &&
                                instructions[back].Operand is FieldReference fr &&
                                fr.FieldType.FullName == "System.Int32[]")
                            {
                                if (!arrayFieldRefs.ContainsKey(bagId)) arrayFieldRefs[bagId] = new List<FieldReference>();
                                if (!arrayFieldRefs[bagId].Any(x => x.FullName == fr.FullName))
                                    arrayFieldRefs[bagId].Add(fr);
                                break;
                            }
                        }
                    }

                    foreach (var item in itemCandidates)
                    {
                        if (coinIds.Contains(item)) continue; // let vanilla handle coin amounts
                        foreach (var stack in stackCandidates)
                        {
                            if (!loot.ContainsKey(bagId))
                                loot[bagId] = new List<BossBagDrop>();
                            if (!loot[bagId].Any(x => x.Item == item))
                                loot[bagId].Add(new BossBagDrop { Item = item, Stack = stack });
                        }
                    }

                    // If this exact call is spawning non-coin items, mark to remove to avoid duplicates; keep coins/dev armor vanilla
                    if (itemCandidates.Count > 0 && itemCandidates.All(x => !coinIds.Contains(x)))
                    {
                        // Expect pattern: ldarg.0, ldloc|ldloc.s source, ldc item, ldc stack, call QuickSpawnItem
                        int callStart = j - 4;
                        if (callStart >= 0 &&
                            IsLoadPlayer(instructions[callStart]) &&
                            IsLoadLocal(instructions[callStart + 1]) &&
                            IsAnyConstantOrLocal(instructions[callStart + 2]) && // allow ldloc for Moon Lord weapons
                            IsAnyConstant(instructions[callStart + 3]))
                        {
                            var segment = new List<Instruction>();
                            for (int k = callStart; k <= j; k++) segment.Add(instructions[k]);
                            killSegments.Add(segment);
                        }
                    }
                }

                _currentBagListAdds = null;
            }

            // Second pass: if vanilla QuickSpawnItem would drop an item we already inject, NOP it to prevent dupes.
            for (int i = 0; i < instructions.Count - 2; i++)
            {
                if (instructions[i].OpCode != OpCodes.Ldarg_1 || instructions[i + 1].OpCode != OpCodes.Ldc_I4)
                    continue;
                int bagId;
                if (!int.TryParse(instructions[i + 1].Operand.ToString(), out bagId)) continue;
                if (!loot.ContainsKey(bagId)) continue;

                var branch = instructions[i + 2] as Instruction;
                if (branch == null || !indexMap.ContainsKey(branch)) continue;
                int start = i + 3;
                int end = indexMap[branch] - 1;
                if (end < start) continue;

                for (int j = start; j <= end; j++)
                {
                    var instr = instructions[j];
                    if (instr.OpCode != OpCodes.Call) continue;
                    var mr = instr.Operand as MethodReference;
                    if (mr == null || mr.Name != "QuickSpawnItem" || mr.Parameters.Count != 3) continue;
                    if (j < 2) continue;
                    var items = ResolvePossibleInts(instructions, j - 2);
                    if (items.Count == 0 && IsLoadLocal(instructions[j - 2]))
                    {
                        int li = GetLocalIndex(instructions[j - 2]);
                        if (li >= 0) items = CollectConstAssignments(instructions, li);
                    }
                    if (items.Any(it => !coinIds.Contains(it) && loot[bagId].Any(x => x.Item == it)))
                    {
                        int callStart = j - 4;
                        if (callStart >= 0)
                        {
                            var segment = new List<Instruction>();
                            for (int k = callStart; k <= j; k++) segment.Add(instructions[k]);
                            killSegments.Add(segment);
                        }
                    }
                }
            }

            var helper = new MethodDefinition("ForceBossBagAllLoot", MethodAttributes.Private, _mainModule.TypeSystem.Boolean);
            helper.Parameters.Add(new ParameterDefinition(_mainModule.TypeSystem.Int32));      // bag type
            helper.Parameters.Add(new ParameterDefinition(entitySourceType));                  // entity source
            helper.Body.InitLocals = true;
            player.Methods.Add(helper);

            // union of all bag ids seen anywhere
            var allBags = new HashSet<int>(loot.Keys);
            foreach (var k in listAdds.Keys) allBags.Add(k);
            foreach (var k in arrayAdds.Keys) allBags.Add(k);
            foreach (var k in arrayFieldRefs.Keys) allBags.Add(k);

            var hIL = helper.Body.GetILProcessor();
            foreach (var bagId in allBags)
            {
                if (!loot.ContainsKey(bagId))
                    loot[bagId] = new List<BossBagDrop>();

                if (listAdds.TryGetValue(bagId, out var extras))
                {
                    foreach (var ex in extras)
                        if (!loot[bagId].Any(x => x.Item == ex))
                            loot[bagId].Add(new BossBagDrop { Item = ex, Stack = 1 });
                }
                if (arrayAdds.TryGetValue(bagId, out var aextras))
                {
                    foreach (var ex in aextras)
                        if (!loot[bagId].Any(x => x.Item == ex))
                            loot[bagId].Add(new BossBagDrop { Item = ex, Stack = 1 });
                }

                var skip = Instruction.Create(OpCodes.Nop);
                hIL.Emit(OpCodes.Ldarg_1);
                hIL.Emit(OpCodes.Ldc_I4, bagId);
                hIL.Emit(OpCodes.Bne_Un, skip);

                // spawn loot list
                foreach (var drop in loot[bagId])
                {
                    hIL.Emit(OpCodes.Ldarg_0);
                    hIL.Emit(OpCodes.Ldarg_2);
                    hIL.Emit(OpCodes.Ldc_I4, drop.Item);
                    hIL.Emit(OpCodes.Ldc_I4, drop.Stack);
                    hIL.Emit(OpCodes.Call, quickSpawn);
                }

                // spawn any int[] static pools
                if (arrayFieldRefs.TryGetValue(bagId, out var fields))
                {
                    foreach (var fr in fields)
                    {
                        var arrVar = new VariableDefinition(_mainModule.ImportReference(typeof(int[])));
                        var idxVar = new VariableDefinition(_mainModule.TypeSystem.Int32);
                        helper.Body.Variables.Add(arrVar);
                        helper.Body.Variables.Add(idxVar);

                        var loopStart = Instruction.Create(OpCodes.Ldloc, idxVar);
                        var loopEnd = Instruction.Create(OpCodes.Nop);

                        hIL.Emit(OpCodes.Ldsfld, fr);
                        hIL.Emit(OpCodes.Stloc, arrVar);
                        hIL.Emit(OpCodes.Ldc_I4_0);
                        hIL.Emit(OpCodes.Stloc, idxVar);

                        hIL.Append(loopStart);
                        hIL.Emit(OpCodes.Ldloc, idxVar);
                        hIL.Emit(OpCodes.Ldloc, arrVar);
                        hIL.Emit(OpCodes.Ldlen);
                        hIL.Emit(OpCodes.Conv_I4);
                        hIL.Emit(OpCodes.Bge, loopEnd);

                        hIL.Emit(OpCodes.Ldarg_0);
                        hIL.Emit(OpCodes.Ldarg_2);
                        hIL.Emit(OpCodes.Ldloc, arrVar);
                        hIL.Emit(OpCodes.Ldloc, idxVar);
                        hIL.Emit(OpCodes.Ldelem_I4);
                        hIL.Emit(OpCodes.Ldc_I4_1);
                        hIL.Emit(OpCodes.Call, quickSpawn);

                        hIL.Emit(OpCodes.Ldloc, idxVar);
                        hIL.Emit(OpCodes.Ldc_I4_1);
                        hIL.Emit(OpCodes.Add);
                        hIL.Emit(OpCodes.Stloc, idxVar);

                        hIL.Emit(OpCodes.Br, loopStart);
                        hIL.Append(loopEnd);
                    }
                }

                hIL.Emit(OpCodes.Ldc_I4_1);
                hIL.Emit(OpCodes.Ret);
                hIL.Append(skip);
            }
            hIL.Emit(OpCodes.Ldc_I4_0);
            hIL.Emit(OpCodes.Ret);

            var newSourceVar = new VariableDefinition(entitySourceType);
            openBossBag.Body.Variables.Add(newSourceVar);

            var first = openBossBag.Body.Instructions.First();
            var prepend = new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Call, getItemSource),
                Instruction.Create(OpCodes.Stloc, newSourceVar),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Ldloc, newSourceVar),
                Instruction.Create(OpCodes.Call, helper),
                Instruction.Create(OpCodes.Pop) // allow vanilla to continue (coins, dev armor)
            };
            IL.MethodPrepend(openBossBag, first, prepend);

            // Remove original QuickSpawnItem calls for non-coin loot we already spawned (by reference, safe after prepend)
            foreach (var segment in killSegments)
            {
                foreach (var instr in segment)
                {
                    instr.OpCode = OpCodes.Nop;
                    instr.Operand = null;
                }
            }
            killSegments.Clear();
        }

        private static List<int> ResolvePossibleInts(IList<Instruction> instrs, int index)
        {
            var result = new List<int>();
            if (index < 0 || index >= instrs.Count) return result;

            var instr = instrs[index];
            int constant;
            if (TryGetConstantInt(instr, out constant))
            {
                result.Add(constant);
                return result;
            }

            // Handle locals coming from UnifiedRandom.Next
            if (instr.OpCode.Code == Code.Ldloc || instr.OpCode.Code == Code.Ldloc_S ||
                instr.OpCode.Code == Code.Ldloc_0 || instr.OpCode.Code == Code.Ldloc_1 ||
                instr.OpCode.Code == Code.Ldloc_2 || instr.OpCode.Code == Code.Ldloc_3)
            {
                int localIndex = GetLocalIndex(instr);
                for (int i = index - 1; i >= 0; i--)
                {
                    var prev = instrs[i];
                    if (!IsStoreToLocal(prev, localIndex)) continue;

                    // Expect callvirt UnifiedRandom::Next
                    if (i > 0 && instrs[i - 1].OpCode == OpCodes.Callvirt &&
                        instrs[i - 1].Operand is MethodReference mr &&
                        mr.Name == "Next" &&
                        mr.DeclaringType.FullName.Contains("UnifiedRandom"))
                    {
                        // Next(int max) or Next(int min, int max)
                        if (mr.Parameters.Count == 1)
                        {
                            int max;
                            if (TryGetConstantInt(instrs[i - 2], out max))
                            {
                                for (int v = 0; v < max; v++) result.Add(v);
                            }
                        }
                        else if (mr.Parameters.Count == 2)
                        {
                            int min, max;
                            if (TryGetConstantInt(instrs[i - 3], out min) && TryGetConstantInt(instrs[i - 2], out max))
                            {
                                for (int v = min; v < max; v++) result.Add(v);
                            }
                        }
                    }
                    break;
                }
            }

            return result;
        }

        private static bool TryGetConstantInt(Instruction instr, out int value)
        {
            value = 0;
            if (instr == null) return false;
            switch (instr.OpCode.Code)
            {
                case Code.Ldc_I4:
                    value = (int)instr.Operand; return true;
                case Code.Ldc_I4_S:
                    value = (sbyte)instr.Operand; return true;
                case Code.Ldc_I4_M1:
                    value = -1; return true;
                case Code.Ldc_I4_0:
                    value = 0; return true;
                case Code.Ldc_I4_1:
                    value = 1; return true;
                case Code.Ldc_I4_2:
                    value = 2; return true;
                case Code.Ldc_I4_3:
                    value = 3; return true;
                case Code.Ldc_I4_4:
                    value = 4; return true;
                case Code.Ldc_I4_5:
                    value = 5; return true;
                case Code.Ldc_I4_6:
                    value = 6; return true;
                case Code.Ldc_I4_7:
                    value = 7; return true;
                case Code.Ldc_I4_8:
                    value = 8; return true;
                default:
                    return false;
            }
        }

        private static bool IsAnyConstant(Instruction instr)
        {
            int _;
            return TryGetConstantInt(instr, out _);
        }

        private static bool IsAnyConstantOrLocal(Instruction instr)
        {
            return IsAnyConstant(instr) || IsLoadLocal(instr);
        }

        private static bool IsLoadPlayer(Instruction instr)
        {
            return instr != null && (instr.OpCode == OpCodes.Ldarg_0 || instr.OpCode == OpCodes.Ldarg || instr.OpCode == OpCodes.Ldarg_S);
        }

        private static bool IsLoadLocal(Instruction instr)
        {
            if (instr == null) return false;
            switch (instr.OpCode.Code)
            {
                case Code.Ldloc:
                case Code.Ldloc_S:
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                    return true;
                default:
                    return false;
            }
        }

        private static List<int> CollectConstAssignments(IList<Instruction> instrs, int localIndex)
        {
            var result = new List<int>();
            for (int i = 0; i < instrs.Count - 1; i++)
            {
                if (!IsStoreToLocal(instrs[i], localIndex)) continue;
                int c;
                // direct constant
                if (TryGetConstantInt(instrs[i - 1], out c))
                {
                    if (!result.Contains(c)) result.Add(c);
                }
                // list[int] pattern: ldloc listVar; ldloc idx; callvirt get_Item
                else if (i >= 3 &&
                         instrs[i - 1].OpCode.Code == Code.Callvirt &&
                         (instrs[i - 1].Operand as MethodReference)?.Name == "get_Item" &&
                         IsLoadLocal(instrs[i - 2]) &&
                         IsLoadLocal(instrs[i - 3]))
                {
                    var listVar = GetLocalIndex(instrs[i - 3]);
                    if (listVar >= 0 && _currentBagListAdds != null && _currentBagListAdds.TryGetValue(listVar, out var elems))
                    {
                        foreach (var e in elems) if (!result.Contains(e)) result.Add(e);
                    }
                }
            }
            return result;
        }

        // bag-scoped map of list variable index -> contents (set at traversal)
        private static Dictionary<int, List<int>> _currentBagListAdds;

        private static bool IsStoreToLocal(Instruction instr, int localIndex)
        {
            if (instr == null) return false;
            switch (instr.OpCode.Code)
            {
                case Code.Stloc_0: return localIndex == 0;
                case Code.Stloc_1: return localIndex == 1;
                case Code.Stloc_2: return localIndex == 2;
                case Code.Stloc_3: return localIndex == 3;
                case Code.Stloc:
                case Code.Stloc_S:
                    return instr.Operand is VariableDefinition v && v.Index == localIndex;
                default:
                    return false;
            }
        }

        private static bool IsStoreToAnyLocal(Instruction instr)
        {
            if (instr == null) return false;
            switch (instr.OpCode.Code)
            {
                case Code.Stloc:
                case Code.Stloc_S:
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                    return true;
                default:
                    return false;
            }
        }

        private static int GetStoreLocalIndex(Instruction instr)
        {
            if (instr == null) return -1;
            switch (instr.OpCode.Code)
            {
                case Code.Stloc_0: return 0;
                case Code.Stloc_1: return 1;
                case Code.Stloc_2: return 2;
                case Code.Stloc_3: return 3;
                case Code.Stloc:
                case Code.Stloc_S:
                    return (instr.Operand as VariableDefinition)?.Index ?? -1;
                default:
                    return -1;
            }
        }

        private static int GetLocalIndex(Instruction instr)
        {
            switch (instr.OpCode.Code)
            {
                case Code.Ldloc_0: return 0;
                case Code.Ldloc_1: return 1;
                case Code.Ldloc_2: return 2;
                case Code.Ldloc_3: return 3;
                case Code.Ldloc:
                case Code.Ldloc_S:
                    var v = instr.Operand as VariableDefinition;
                    return v != null ? v.Index : -1;
                default:
                    return -1;
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
