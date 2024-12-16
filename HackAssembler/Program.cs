using HackAssembler.Modules;
namespace HackAssembler;
using System.IO;


public static class Program
{
    private static readonly SymbolTable symbolTable = Parser.SymbolTable;
    
    static Program()
    {
        Console.WriteLine("from parser");
        symbolTable.GetAllEntries();
    }   


    private static void Main(string[] args)
    {
        var (verified, file) = VerifyFile(args);
        if (!verified) 
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

        Console.WriteLine("from main");
        symbolTable.GetAllEntries();
        
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