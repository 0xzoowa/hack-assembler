using static System.Text.RegularExpressions.Regex;
using System.IO;
using HackAssembler.Types;

namespace HackAssembler.Modules;

public class Parser
{
    private static readonly SymbolTable SymbolTable = new();
    private readonly StreamReader _sr;
    private string _currentInstruction;
    private int _instructionCount = -1; //initialized to no valid instruction
    private int  memory = 16;

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
            _currentInstruction = line;
            break;
        }
    }

    private InstructionType InstructionType()
    {
        var currentInstruction = GetCurrentInstruction();
       
        if (currentInstruction.StartsWith('@')) //extend to handle symbols
        { 
            var secondHalf = currentInstruction.Substring(1);
            if(IsMatch(secondHalf.Trim(), @"^\d+$"))
            {
                _instructionCount++;
                 return Types.InstructionType.A_INSTRUCTION;
            }
          
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
            _instructionCount++;
            return Types.InstructionType.C_INSTRUCTION;
        }
        return Types.InstructionType.UNKNOWN_INSTRUCTION;

    }
    
    private string Symbol()
    {
        var instruction = GetCurrentInstruction();
        var aout = string.Empty;
        
        var iType = InstructionType();
        
        switch (iType)
        {
            /*
             * 
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

                else if (!SymbolTable.Contains(part) && IsMatch(part, "^[a-z]+$"))
                {
                    SymbolTable.AddEntry(part,memory );
                    memory++;
                    var addr = SymbolTable.GetAddress(part);
                    aout = addr.ToString();
                }
                break;
            
            case Types.InstructionType.L_INSTRUCTION:
                
                var label = instruction.Substring(0, instruction.Length - 2);
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
        
        return instruction.Contains('=') ? instruction.Split('=')[0].Trim() : null;
    }
    
    private string Comp()
    {
        var instruction = GetCurrentInstruction();
        var comPart = instruction;

        if (instruction.Contains('=') )
        {
            comPart =  instruction.Split('=')[1];
            if (comPart.Contains(';'))
            {
                comPart = comPart.Split(';')[0];
            }

            return comPart;
        }
        
        if (instruction.Contains(';') )
        {
            comPart =  instruction.Split(';')[0];

            return comPart;
        }

        return comPart.Trim();
    }
    
    private string? Jump()
    {
        var instruction = GetCurrentInstruction();
        
        return instruction.Contains(';') ? instruction.Split(';')[1].Trim() : null;
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
                    var destBinary = code.Dest(destination!);
                    var compBinary = code.Comp(computation);
                    var jumpBinary = code.Jump(jump!);
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
            var instruction = GetCurrentInstruction();
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

                    SymbolTable.AddEntry(label, _instructionCount+1);
                    break;
                
                
            }
        }
      
    }
}

