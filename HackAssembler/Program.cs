using HackAssembler.Modules;
namespace HackAssembler;
using System.IO;

/*
 * Entry point: Program class, call Parser service and pass in path
 * In Parser Class:
 * initialize count variable
 * Create Out file : Prog.hack
 * Read each line from file (enters loop)
 * determine if it's a valid instruction: ignore comments and white spaces
 * determine type of instruction : A/C/L
 * if A OR L instruction : symbol()
 * if C instruction : dest(Instruction), comp(Instruction), jmp(Instruction)
 * Call Code Service: dest/comp/jmp(return value from Parser dest, comp or jump method) - translates mnemonic to binary code
 * write each binary representation into the out file
 */

public static class Program
{
    private static readonly SymbolTable SymbolTable = new();
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
        
        foreach (var entry in PredefinedSymbol)
        {
            SymbolTable.AddEntry(entry.Key, entry.Value);
        } 
    
    }   


    private static void Main(string[] args)
    {
        SymbolTable.GetAllEntries();
        var (res, file) = VerifyFile(args);
        Console.WriteLine(!res ? "invalid file" : file);
    }

    private static (bool, string) VerifyFile(IEnumerable<string> args)
    {
        //if multiple valid files are supplied, file defaults to first encountered file
        try
        {
            var files = args.Where(t => Path.GetExtension(t) == ".txt").ToList();
            return files.Count > 0 ? (true, files[0]) : (false, "");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (false, "");
        }
        
    }
}