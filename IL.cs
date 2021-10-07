using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace TerrariaPatcher
{
    static class IL
    {
        public static void MakeLargeAddressAware(string file)
        {
            using (var stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite))
            {
                const int IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x20;

                var br = new BinaryReader(stream);
                var bw = new BinaryWriter(stream);

                if (br.ReadInt16() != 0x5A4D)       //No MZ Header
                    return;

                br.BaseStream.Position = 0x3C;
                var peloc = br.ReadInt32();         //Get the PE header location.

                br.BaseStream.Position = peloc;
                if (br.ReadInt32() != 0x4550)       //No PE header
                    return;

                br.BaseStream.Position += 0x12;
                
                var position = br.BaseStream.Position;
                var flags = br.ReadInt16();
                bool isLAA = (flags & IMAGE_FILE_LARGE_ADDRESS_AWARE) == IMAGE_FILE_LARGE_ADDRESS_AWARE;
                if (isLAA)                          //Already Large Address Aware
                    return;

                flags |= IMAGE_FILE_LARGE_ADDRESS_AWARE;

                bw.Seek((int)position, SeekOrigin.Begin);
                bw.Write(flags);
                bw.Flush();
            }
        }
        
        public static Version GetAssemblyVersion(string path)
        {
            using (var asm = AssemblyDefinition.ReadAssembly(path))
                return asm.Name.Version;
        }

        public static void MethodPrepend(MethodDefinition method, IEnumerable<Instruction> instructions)
        {
            MethodPrepend(method, method.Body.Instructions.FirstOrDefault(), instructions);
        }

        public static void MethodPrepend(MethodDefinition method, Instruction first, IEnumerable<Instruction> instructions)
        {
            MethodPrepend(method.Body.GetILProcessor(), first, instructions);
        }

        public static void MethodPrepend(ILProcessor il, Instruction first, IEnumerable<Instruction> instructions)
        {
            foreach (var instr in instructions) il.InsertBefore(first, instr);
        }

        public static void MethodAppend(MethodDefinition method, IEnumerable<Instruction> instructions)
        {
            var il = method.Body.GetILProcessor();
            MethodAppend(il, il.Body.Instructions.Count, 0, instructions);
        }

        public static void MethodAppend(MethodDefinition definition, int index, int overwrite, IEnumerable<Instruction> instructions)
        {
            MethodAppend(definition.Body.GetILProcessor(), index, overwrite, instructions);
        }

        public static void MethodAppend(ILProcessor il, int index, int overwrite, IEnumerable<Instruction> instructions)
        {
            var list = instructions.ToList();
            while (overwrite-- > 0)
            {
                var existing = il.Body.Instructions[index++];
                var instr = list[0];
                existing.OpCode = instr.OpCode;
                existing.Operand = instr.Operand;
                list.RemoveAt(0);
            }
            foreach (var instr in list) il.Body.Instructions.Insert(index++, instr);
        }

        public static FieldDefinition AddStaticField(ModuleDefinition moduleDefinition, TypeDefinition classType, string field, Type type, object value = null)
        {
            return AddStaticField(classType, field, moduleDefinition.Import(type), value);
        }

        public static FieldDefinition AddStaticField(TypeDefinition classType, string field, TypeReference type, object value = null)
        {
            var classStaticConstructor = GetMethodDefinition(classType, ".cctor");
            if (classStaticConstructor == null) return null;

            var fld = new FieldDefinition(field, FieldAttributes.Static | FieldAttributes.Public, type);
            classType.Fields.Add(fld);

            if (value != null)
            {
                var il = classStaticConstructor.Body.GetILProcessor();
                var first = il.Body.Instructions[0];

                if (type.Name == "String")
                    il.InsertBefore(first, il.Create(OpCodes.Ldstr, (string)value));
                else if (type.Name == "Int32")
                    il.InsertBefore(first, il.Create(OpCodes.Ldc_I4, (int)value));
                else if (type.Name == "Boolean")
                    il.InsertBefore(first, il.Create(OpCodes.Ldc_I4, (bool)value ? 1 : 0));
                else if (type.Name == "Single")
                    il.InsertBefore(first, il.Create(OpCodes.Ldc_R4, (Single)value));
                else if (value is Instruction)
                    il.InsertBefore(first, (Instruction)value);
                else
                    Program.ShowErrorMessage(string.Format("AddStaticField(): Unrecognized type '{0}'!", type.FullName));

                il.InsertBefore(first, il.Create(OpCodes.Stsfld, fld));
            }

            return fld;
        }

        public static void ModifyStaticField(TypeDefinition classType, string field, object newValue)
        {
            var classStaticConstructor = GetMethodDefinition(classType, ".cctor");
            if (classStaticConstructor == null) return;

            if (newValue is string)
            {
                ModifyStaticField(classStaticConstructor, field, instr =>
                {
                    instr.OpCode = OpCodes.Ldstr;
                    instr.Operand = newValue;
                });
            }
            else if (newValue is int || newValue is bool)
            {
                ModifyStaticField(classStaticConstructor, field, instr =>
                {
                    instr.OpCode = OpCodes.Ldc_I4;
                    instr.Operand = newValue;
                });
            }
            else if (newValue is float)
            {
                ModifyStaticField(classStaticConstructor, field, instr =>
                {
                    instr.OpCode = OpCodes.Ldc_R4;
                    instr.Operand = newValue;
                });
            }
            else
                Program.ShowErrorMessage(string.Format("ModifyStaticField(): Unrecognized type '{0}'!", newValue.GetType().FullName));
        }

        public static void ModifyStaticField(MethodDefinition classStaticConstructor, string field, Action<Instruction> set)
        {
            Instruction last = null;
            foreach (var instr in classStaticConstructor.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Stsfld && instr.Operand != null && instr.Operand.ToString().ToLower().Contains(field.ToLower()))
                {
                    set(last);
                    return;
                }
                last = instr;
            }
        }

        public static bool ScanAndNop(MethodDefinition m, int offsetStart, int offsetInstructionWithOperand, params OpCode[] pattern)
        {
            var index = ScanForOpcodePattern(m, pattern);
            if (index < 0) return false;

            var end = m.Body.Instructions[index + offsetInstructionWithOperand].Operand;
            for (var x = index + offsetStart; end != m.Body.Instructions[x]; x++)
            {
                m.Body.Instructions[x].OpCode = OpCodes.Nop;
                m.Body.Instructions[x].Operand = null;
            }
            return true;
        }

        /// <summary>
        /// Returns a property definition.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static FieldDefinition GetFieldDefinition(TypeDefinition t, string fieldName, bool verbose = true)
        {
            var result = (from FieldDefinition f in t.Fields
                          where f.Name == fieldName
                          select f).FirstOrDefault();

            if (result == null && verbose)
                Program.ShowErrorMessage(string.Format("Failed to locate {0}.{1} field!", t.FullName, fieldName));

            return result;
        }

        /// <summary>
        /// Returns a method definition.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static MethodDefinition GetMethodDefinition(TypeDefinition t, string methodName, int parameterCount = -1, bool verbose = true)
        {
            var result = (from MethodDefinition m in t.Methods
                          where m.Name == methodName && (parameterCount == -1 || m.Parameters.Count + m.GenericParameters.Count == parameterCount)
                          select m).FirstOrDefault();

            if (result == null && verbose)
                Program.ShowErrorMessage(string.Format("Failed to locate {0}.{1}() method!", t.FullName, methodName));

            return result;
        }

		/// <summary>
		/// Returns a method definition.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static ModuleDefinition GetModuleDefinition(AssemblyDefinition definition, string fullyQualifiedName, bool verbose = true)
		{
			ModuleDefinition module = definition.Modules.FirstOrDefault(p => p.FullyQualifiedName == fullyQualifiedName);

			if (module == null && verbose)
			{
				Program.ShowErrorMessage(string.Format("Failed to locate {0} reference!", fullyQualifiedName));
				module = definition.MainModule;
			}

			return module;
		}

        /// <summary>
        /// Returns a property definition.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static PropertyDefinition GetPropertyDefinition(TypeDefinition t, string propName, bool verbose = true)
        {
            var result = (from PropertyDefinition p in t.Properties
                          where p.Name == propName
                          select p).FirstOrDefault();

            if (result == null && verbose)
                Program.ShowErrorMessage(string.Format("Failed to locate {0}.{1} property!", t.FullName, propName));

            return result;
        }

        /// <summary>
        /// Returns a type definition.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static TypeDefinition GetTypeDefinition(ModuleDefinition moduleDefinition, string typeName, bool verbose = true)
        {
            var result = (from TypeDefinition t in moduleDefinition.Types
                          where t.Name == typeName
                          select t).FirstOrDefault();

            if (result == null && verbose)
                Program.ShowErrorMessage(string.Format("Failed to locate {0} type!", typeName));

            return result;
        }

        /// <summary>
        /// Only use this instead of GetTypeDefinition() when the type is not within the Terraria module (eg. an XNA type).
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static TypeReference GetTypeReference(ModuleDefinition moduleDefinition, string fullName, bool verbose = true)
        {
            TypeReference reference;

            if (!moduleDefinition.TryGetTypeReference(fullName, out reference) && verbose)
                Program.ShowErrorMessage(string.Format("Failed to locate {0} type!", fullName));

            return reference;
        }

        /// <summary>
        /// Changes all fields/methods/nested types to use public modifiers.
        /// </summary>
        /// <param name="typeDefinition"></param>
        /// <param name="includeNestedTypes"></param>
        public static void MakeTypePublic(TypeDefinition typeDefinition, bool includeNestedTypes = true)
        {
            if (includeNestedTypes)
            {
                typeDefinition.NestedTypes.ToList().ForEach(definition =>
                {
                    if (definition.FullName != typeDefinition.FullName) // avoid infinite recursion
                        MakeTypePublic(definition);
                });
            }
            if (typeDefinition.IsNested)
                typeDefinition.IsNestedPublic = true;
            else
                typeDefinition.IsPublic = true;
            foreach (var field in typeDefinition.Fields)
                field.IsPublic = true;
            foreach (var method in typeDefinition.Methods)
            {
                if (!method.IsSpecialName)
                    method.IsPublic = true;
            }
        }

        /// <summary>
        /// Scans a method for a pattern of opcodes.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="inst"></param>
        /// <returns></returns>
        public static int ScanForOpcodePattern(MethodDefinition m, params OpCode[] inst)
        {
            return ScanForOpcodePattern(m, (v, i) => true, 0, inst);
        }

        /// <summary>
        /// Scans a method for a pattern of opcodes.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="inst"></param>
        /// <returns></returns>
        public static int ScanForOpcodePattern(MethodDefinition m, Func<int, Instruction, bool> check, params OpCode[] inst)
        {
            return ScanForOpcodePattern(m, check, 0, inst);
        }

        /// <summary>
        /// Scans a method for a pattern of opcodes.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="nStartOffset"></param>
        /// <param name="inst"></param>
        /// <returns></returns>
        public static int ScanForOpcodePattern(MethodDefinition m, Func<int, Instruction, bool> check, int nStartOffset, params OpCode[] inst)
        {
            var il = m.Body.GetILProcessor();

            for (var x = nStartOffset; x < il.Body.Instructions.Count - inst.Count(); x++)
            {
                if (il.Body.Instructions[x].OpCode != inst[0])
                    continue;

                for (var y = 0; y < inst.Count(); y++)
                {
                    if (il.Body.Instructions[x + y].OpCode != inst[y])
                        break;
                    if (y == inst.Count() - 1 && check(x, il.Body.Instructions[x]))
                        return x;
                }
            }

            return -1;
        }

		/// <summary>
		/// Creates a copy of the instruction.
		/// </summary>
		/// <param name="original"></param>
		/// <returns></returns>
		public static Instruction CopyInstruction(Instruction original)
		{
			if (original.Operand is byte)
				return Instruction.Create(original.OpCode, (byte)original.Operand);
			else if (original.Operand is CallSite)
				return Instruction.Create(original.OpCode, original.Operand as CallSite);
			else if (original.Operand is double)
				return Instruction.Create(original.OpCode, (double)original.Operand);
			else if (original.Operand is FieldReference)
				return Instruction.Create(original.OpCode, original.Operand as FieldReference);
			else if (original.Operand is float)
				return Instruction.Create(original.OpCode, (float)original.Operand);
			else if (original.Operand is Instruction)
				return Instruction.Create(original.OpCode, IL.CopyInstruction(original.Operand as Instruction));
			else if (original.Operand is Instruction[])
			{
				Instruction[] set = new Instruction[(original.Operand as Instruction[]).Length];

				for (int i = 0; i < set.Length; i++)
					set[i] = IL.CopyInstruction((original.Operand as Instruction[])[i]);

				return Instruction.Create(original.OpCode, set);
			}
			else if (original.Operand is int)
				return Instruction.Create(original.OpCode, (int)original.Operand);
			else if (original.Operand is long)
				return Instruction.Create(original.OpCode, (long)original.Operand);
			else if (original.Operand is MethodReference)
				return Instruction.Create(original.OpCode, original.Operand as MethodReference);
			else if (original.Operand is ParameterDefinition)
				return Instruction.Create(original.OpCode, original.Operand as ParameterDefinition);
			else if (original.Operand is sbyte)
				return Instruction.Create(original.OpCode, (sbyte)original.Operand);
			else if (original.Operand is String)
				return Instruction.Create(original.OpCode, original.Operand as String);
			else if (original.Operand is TypeReference)
				return Instruction.Create(original.OpCode, original.Operand as TypeReference);
			else if (original.Operand is VariableDefinition)
				return Instruction.Create(original.OpCode, original.Operand as VariableDefinition);
			else
				return Instruction.Create(original.OpCode);
		}

        class JumpFixHelper : IDisposable
        {
            private readonly MethodDefinition _method;

            public JumpFixHelper(MethodDefinition method)
            {
                _method = method;
            }

            public void Dispose()
            {
                _method.Body.SimplifyMacros();
                _method.Body.OptimizeMacros();
            }
        }

        public static IDisposable JumpFix(this MethodDefinition method)
        {
            return new JumpFixHelper(method);
        }
    }
}
