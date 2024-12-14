using static System.Text.RegularExpressions.Regex;
using System.IO;

namespace HackAssembler.Modules;

public class Parser
{
    private readonly StreamReader _sr;
    private string _currentInstruction;

    public Parser(string path)
    {
        if (File.Exists(path))
        {
            /* initializes necessary variables */
            _sr = new StreamReader(path); // Open input file for reading
            // if (!HasMoreLines())
            // {
            //     Close();
            // }
            // Advance();
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

    private string InstructionType()
    {
        var currentInstruction = GetCurrentInstruction();
       
        if (currentInstruction.StartsWith('@')) //extend to handle symbols
        { 
            var secondHalf = currentInstruction.Substring(1);
            if(IsMatch(secondHalf.Trim(), @"^\d+$"))
            {
                   return "A_INSTRUCTION";
            }
        }
        if (currentInstruction.StartsWith('(') && currentInstruction.EndsWith(')'))
        {
            var labelContent = currentInstruction.Substring(1, currentInstruction.Length - 2).Trim();
            if (!string.IsNullOrEmpty(labelContent))
            {
                return "L_INSTRUCTION";
            }
        }
        if (currentInstruction.Contains('=') || currentInstruction.Contains(';')) 
        {
            return "C_INSTRUCTION";
        }
        return "UNKNOWN_INSTRUCTION";

    }
    
    private string? Symbol()
    { 
        //extend to handle symbols
        var instruction = GetCurrentInstruction();
        return instruction.StartsWith('@') ? instruction.Split('@')[1] : null;
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
        }
        
        if (instruction.Contains(';') )
        {
            comPart =  instruction.Split(';')[0];
        }

        return comPart.Trim();
    }
    
    private string? Jump()
    {
        var instruction = GetCurrentInstruction();
        
        return instruction.Contains(';') ? instruction.Split(';')[1] : null;
    }

    public IEnumerable<string> ParseInstruction()
    {
        var binaryRepresentation = new List<string>();
        
        while (HasMoreLines())
        {
            Advance();
            
            var iType = InstructionType();
            switch (iType)
            {
                case "A_INSTRUCTION":
                case "L_INSTRUCTION":
                    var a = int.Parse(Symbol() ?? string.Empty); // does swapping, does not return any values
                    var data = Convert.ToString(a, 2).PadLeft(16, '0');;
                    binaryRepresentation.Add(data);
                    break;
                case "C_INSTRUCTION":
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
    
}

