using HackAssembler.Types;

namespace HackAssembler.Modules;

public class SymbolTable
{
    private static SymbolTable _instance;
    private readonly Dictionary<string,int> symbolTable;

    private SymbolTable(Dictionary<string, int>? custom = null) 
    {
        symbolTable = new Dictionary<string, int>();
        if (custom != null)
        {
            foreach (var entry in custom)
            {
                AddEntry(entry.Key, entry.Value); //ISA.HackPredefinedSymbol is custom
            }
        }
    }
    
    public static SymbolTable Instance(Dictionary<string, int>? custom = null)
    {
        // Check if the _instance is null (meaning no instance has been created yet)
        if (_instance == null)
        {
            _instance = new SymbolTable(custom);
        }
        return _instance;
    }


    public void AddEntry(string symbol, int address)
    {
        symbolTable[symbol] = address;
    }

    public bool Contains(string symbol)
    {
        return symbolTable.ContainsKey(symbol);
    }

    public int GetAddress(string symbol)
    {
        return symbolTable.GetValueOrDefault(symbol, 0);
        //symbolTable.TryGetValue(symbol, out int value) ? value : 0;
    }

    public IEnumerable<string> Keys()
    {
        return symbolTable.Keys;
    }
    
    public IEnumerable<int> Values()
    {
        return symbolTable.Values;
    }
    
    public void GetAllEntries()
    {
        foreach (var entry in symbolTable)
        {
            Console.WriteLine($"{entry.Key} : {entry.Value}");
        }
        
        //return new Dictionary<string, int>(symbolTable).ToString();
    }
}