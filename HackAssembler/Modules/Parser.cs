using static System.Text.RegularExpressions.Regex;
using HackAssembler.Types;

namespace HackAssembler.Modules;

public class Parser
{
    public static readonly SymbolTable SymbolTable = SymbolTable.Instance(ISA.HackPredefinedSymbol);
    private readonly StreamReader _sr;
    private string _currentInstruction;
    private int _instructionCount = -1; //initialized to no valid instruction
    private int  _memory = 16;

    public Parser(string path)
    {
        if (File.Exists(path))
        {
            _sr = new StreamReader(path); // Open input file for reading
        }
        else
        {
            Console.WriteLine("Invalid File");
        }
    }
    
    private string GetCurrentInstruction()
    {
        return _currentInstruction;
    } 
    
    private void Close()
    {
        _sr?.Close();
    }

    private bool HasMoreLines()
    {
        return !_sr.EndOfStream;
    }
    
    private void Advance()
    { 
        var line = string.Empty;

        // Keep reading the next line 
        while ((line = _sr.ReadLine()) != null)
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
            {
                continue;   //skips the current iteration
            }
            var commentIndex = line.IndexOf("//");
            if (commentIndex != -1)
            {
                // line = line[..commentIndex].Trim();
                line = line.Substring(0, commentIndex).Trim();
            }
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            _currentInstruction = line.Trim();
            switch (InstructionType())
            {
                case Types.InstructionType.A_INSTRUCTION:
                case Types.InstructionType.C_INSTRUCTION:
                    _instructionCount++;
                    break;
                case Types.InstructionType.L_INSTRUCTION:
                case Types.InstructionType.UNKNOWN_INSTRUCTION:
                    break;
                    
            }
            break;
        }
    }

    private InstructionType InstructionType()
    {
        var currentInstruction = GetCurrentInstruction();
       
        if (currentInstruction.StartsWith('@')) //extend to handle symbols
        { 
            return Types.InstructionType.A_INSTRUCTION;
            // var secondHalf = currentInstruction.Substring(1);
            // if(IsMatch(secondHalf.Trim(), @"^\d+$"))
            // {
            //     
            // }
          
        }
        if (currentInstruction.StartsWith('(') && currentInstruction.EndsWith(')'))
        {
            var labelContent = currentInstruction.Substring(1, currentInstruction.Length - 2).Trim();
            if (!string.IsNullOrEmpty(labelContent))
            {
                return Types.InstructionType.L_INSTRUCTION;
            }
        }
        if (currentInstruction.Contains('=') || currentInstruction.Contains(';'))
        {
            return Types.InstructionType.C_INSTRUCTION;
        }
        return Types.InstructionType.UNKNOWN_INSTRUCTION;

    }
    
    private string Symbol()
    {
        var instruction = GetCurrentInstruction();
            //Console.WriteLine($"current instruction: {instruction}");
        var aout = string.Empty;
        
        var iType = InstructionType();
        
        switch (iType)
        {
            /*
             * Improvements:
             *    - handle edge cases
             *    - error handling
             *    - memory management
             */
            
            case Types.InstructionType.A_INSTRUCTION:
                
                //@ - 123 => 123
                //@ - KBD  = addr from symbol table
                //@ - LOOP(validate) is in symbol table => ADDR 
                //@ - R1 is in symbol table => ADDR 
                //@ - sum (letters in all lower case: validate) sum in symbol table ? return addr : add entry (sum, 16) RAM then return addr
                
                var part = instruction.Split('@')[1];
                
                if (IsMatch(part, @"^\d+$"))
                {
                    aout = part;
                }

                else if (SymbolTable.Contains(part)) //@R1 @sum @LOOP 
                {
                    var addr = SymbolTable.GetAddress(part);
                    aout = addr.ToString();
                }

                else if (!SymbolTable.Contains(part) )//&& IsMatch(part, "^[a-z]+$" pattern should include all valid variable name formats
                {
                    SymbolTable.AddEntry(part,_memory );
                    _memory++;
                    var addr = SymbolTable.GetAddress(part);
                    aout = addr.ToString();
                }
                break;
            
            case Types.InstructionType.L_INSTRUCTION:
                
                var label = instruction.Substring(1, instruction.Length - 2);
                aout = label;
                break;
            
            case Types.InstructionType.C_INSTRUCTION:
            case Types.InstructionType.UNKNOWN_INSTRUCTION:
                break;
                
        }

        return aout;
        
     
    }
    
    private string? Dest()
    {
        var instruction = GetCurrentInstruction();
        
        return instruction.Contains('=') ? instruction.Split('=')[0] : null;
    }
    
    private string Comp()
    {
        var instruction = GetCurrentInstruction();
        var comPart = instruction; //A=D+1;JGT 

        if (instruction.Contains('=') )
        {
            comPart =  instruction.Split('=')[1];//D+1;JGT 
            if (comPart.Contains(';')) 
            {
                comPart = comPart.Split(';')[0];//D+1
            }

            return comPart;
        }
        
        if (instruction.Contains(';') )//D+1;JGT 
        {
            comPart =  instruction.Split(';')[0];//D+1

            return comPart;
        }

        return comPart.Trim();
    }
    
    private string? Jump()
    {
        var instruction = GetCurrentInstruction();
        
        return instruction.Contains(';') ? instruction.Split(';')[1] : null;
    }

    public IEnumerable<string> ParseInstruction() //second pass
    {
        var binaryRepresentation = new List<string>();
        
        while (HasMoreLines())
        {
            Advance();
            
            var iType = InstructionType();
            switch (iType)
            {
                case Types.InstructionType.A_INSTRUCTION:
                    var a = int.Parse(Symbol()); 
                    var data = Convert.ToString(a, 2).PadLeft(16, '0');;
                    binaryRepresentation.Add(data);
                    break;
                case Types.InstructionType.C_INSTRUCTION:
                    var destination = Dest();
                    var computation = Comp();
                    var jump = Jump();
                    var code = new HackInstructionSet();
                    var destBinary = code.Dest(NormalizeCode(destination));
                    var compBinary = code.Comp(computation);
                    var jumpBinary = code.Jump(jump);
                    var instructionBinary = "111" + compBinary + destBinary + jumpBinary;
                    binaryRepresentation.Add(instructionBinary);
                    break;
            }


        }
        Close();
        return binaryRepresentation;
    }

    public void FirstPass()
    {
        while (HasMoreLines())
        {
            Advance();
            var iType = InstructionType();
            switch (iType)
            {
                case Types.InstructionType.A_INSTRUCTION:
                case Types.InstructionType.C_INSTRUCTION:
                case Types.InstructionType.UNKNOWN_INSTRUCTION:
                    continue;
                
                case Types.InstructionType.L_INSTRUCTION:
                    var label = Symbol(); 
                    if (SymbolTable.Contains(label)) 
                    {
                        continue;
                    }

                    SymbolTable.AddEntry(label, _instructionCount + 1);
                    break;
                
                
            }
        }
        ResetState();
    }
    
    private void ResetState()
    {
        // Reset file stream to the start of the file before calling second pass
        _sr.BaseStream.Seek(0, SeekOrigin.Begin);
        _sr.DiscardBufferedData();

        // Reset instruction counter before calling second pass
        _instructionCount = -1;
    }
    
    private static string? NormalizeCode(string microcode)
    {
        if (microcode == null)
        {
            return null; 
        }

        var chars = microcode.ToCharArray();
        Array.Sort(chars); // Sort characters alphabetically
        //Console.WriteLine($"microcode:{microcode} normalized code : {new string(chars)}");
        return new string(chars);
    }
}

