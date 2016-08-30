using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;
namespace PoiCompiler
{
    enum Instruction
    {
        INCREMENT = 0x01,
        DECREMENT = 0x02,
        OUTPUT = 0x03,
        READ = 0x04
    }
    class Program
    {
        public const string token_read = "POIPOIPOIPOI";
        public const string token_output = "POIPOIPOI";
        public const string token_increment = "POI";
        public const string token_decrement = "POIPOI";
        static void Main(string[] args)
        {
            int current_val = 0;
            Console.WriteLine("Reading input file: " + args[0]);
            List<string> file_lines = File.ReadAllLines(args[0]).ToList();
            List<Instruction> instructions = new List<Instruction>();
            string assembly_name = "UntitledAssembly";
            string output_fname = Path.ChangeExtension(args[0], "exe");
            Console.WriteLine("Parsing input file...");
            foreach (var line in file_lines)
            {
                if (line.StartsWith("#"))
                {
                    string metaLine = line.Substring(1);
                    string[] parts = metaLine.Split(':');
                    string prop_name = parts[0];
                    switch (prop_name.ToUpper())
                    {
                        case "NAME":
                            assembly_name = parts[1];
                            break;
                    }
                }
                else
                {
                    switch (line)
                    {
                        case token_increment:
                            instructions.Add(Instruction.INCREMENT);
                            break;
                        case token_decrement:
                            instructions.Add(Instruction.DECREMENT);
                            break;
                        case token_output:
                            instructions.Add(Instruction.OUTPUT);
                            break;
                        case token_read:
                            instructions.Add(Instruction.READ);
                            break;
                    }
                }
            }

            Console.WriteLine("Generating code...");
            AssemblyName an = new AssemblyName();
            an.Name = assembly_name;
            AppDomain appd = AppDomain.CurrentDomain;
            AssemblyBuilder builder = appd.DefineDynamicAssembly(an, AssemblyBuilderAccess.Save);
            ModuleBuilder mb = builder.DefineDynamicModule(output_fname);
            TypeBuilder programBuilder = mb.DefineType(assembly_name + ".Program", TypeAttributes.Public | TypeAttributes.Class);
            MethodBuilder mainMeth = programBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(int), new Type[] { typeof(string[]) });
            ILGenerator ilg = mainMeth.GetILGenerator();
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Stloc_0);
            ilg.DefineLabel();
            ilg.DeclareLocal(typeof(int));
            foreach (Instruction inst in instructions)
            {
                switch (inst)
                {
                    case Instruction.INCREMENT:
                        ilg.Emit(OpCodes.Ldloc_0);
                        ilg.Emit(OpCodes.Ldc_I4_1);
                        ilg.Emit(OpCodes.Add);
                        ilg.Emit(OpCodes.Stloc_0);
                        break;
                    case Instruction.DECREMENT:
                        ilg.Emit(OpCodes.Ldloc_0);
                        ilg.Emit(OpCodes.Ldc_I4_1);
                        ilg.Emit(OpCodes.Sub);
                        ilg.Emit(OpCodes.Stloc_0);
                        break;
                    case Instruction.OUTPUT:
                        ilg.Emit(OpCodes.Ldloc_0);
                        ilg.Emit(OpCodes.Conv_U2);
                        ilg.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(char) }));
                        break;
                    case Instruction.READ:
                        //ilg.Emit(OpCodes.Call, typeof(Console).GetMethod("Read"));
                        //ilg.Emit(OpCodes.Stloc_0);
                        throw new NotImplementedException();
                        break;
                }
            }
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Ret);
            Type programType = programBuilder.CreateType();
            builder.SetEntryPoint(mainMeth, PEFileKinds.ConsoleApplication);
            builder.Save(output_fname);
            Console.WriteLine("Success! Assembly \"" + assembly_name + "\" has been saved to " + output_fname);
            Console.ReadKey();
        }
    }
}
