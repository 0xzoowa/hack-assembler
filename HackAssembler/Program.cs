using HackAssembler.Modules;
namespace HackAssembler;
using System.IO;


public static class Program
{
    private static readonly SymbolTable symbolTable = new();
    private static readonly Dictionary<string, int> PredefinedSymbol = new()
    {
        {"R1" , 1},
        {"R2" , 2},
        {"R3" , 3},
        {"R4" , 4},
        {"R5" , 5},
        {"R6" , 6},
        {"R7" , 7},
        {"R8" , 8},
        {"R9" , 9},
        {"R10" , 10},
        {"R11" , 11},
        {"R12" , 12},
        {"R13" , 12},
        {"R14" , 14},
        {"R15" , 15},
        {"SP" , 0},
        {"LCL" , 1},
        {"ARG" , 2},
        {"THIS" , 3},
        {"THAT" , 4},
        {"SCREEN" , 16384},
        {"KBD" , 24576},
    };
    
    static Program()
    {
        
        symbolTable.GetAllEntries();
    
    }   


    private static void Main(string[] args)
    {
        //SymbolTable.GetAllEntries();
        var (res, file) = VerifyFile(args);
        if (!res) 
        {
            Console.WriteLine("invalid file");
        }
        else
        {
            var parser = new Parser(file);
            
            //first pass
            parser.FirstPass(); //add all labels to symbol table
            
            
            //second pass
            var outPath = OutPath(file);
            var content = parser.ParseInstruction();
            using var sw = new StreamWriter(outPath);
            foreach (var line in content)
            {
                sw.WriteLine(line);
               
            }
            sw.Close();
            Console.WriteLine("done");
        }
        
    }

    private static (bool, string) VerifyFile(IEnumerable<string> args)
    {
        //if multiple valid files are supplied, file defaults to first encountered file
        try
        {
            var files = args.Where(t => Path.GetExtension(t) == ".asm").ToList();
            return files.Count > 0 ? (true, files[0]) : (false, "");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (false, "");
        }
        
    }

    private static string OutPath(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        
        return string.IsNullOrWhiteSpace(directory) ? "Program.hack" : Path.Combine(directory, "Program.hack");
    }
}