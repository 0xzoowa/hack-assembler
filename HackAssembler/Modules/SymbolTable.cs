namespace HackAssembler.Modules;

public class SymbolTable
{
    private readonly Dictionary<string,int> symbolTable;
    public SymbolTable()
    {
        symbolTable = new Dictionary<string, int>();

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