# hack-assembler

# The Hack Computer: From Basic Components to Program Execution (Assembly)

## Table of Contents

1. [Foundation: Basic Logic Gates](#foundation-basic-logic-gates)
   - [NAND Gate: The Universal Building Block](#nand-gate-the-universal-building-block)
   - [Fundamental Multi-Bit Gates](#fundamental-multi-bit-gates)
2. [Arithmetic Components](#arithmetic-components)
   - [Half Adder](#half-adder)
   - [Full Adder](#full-adder)
   - [ALU (Arithmetic Logic Unit)](#alu-arithmetic-logic-unit)
3. [Sequential Logic](#sequential-logic)
   - [D Flip-Flop (DFF) Deep Dive](#d-flip-flop-dff-deep-dive)
   - [Registers](#registers)
4. [Memory Hierarchy](#memory-hierarchy)
   - [RAM Construction](#ram-construction)
5. [I/O System](#io-system)
   - [Memory-Mapped I/O](#memory-mapped-io)
   - [Screen Interface](#screen-interface)
   - [Keyboard Interface](#keyboard-interface)
6. [System Integration](#system-integration)
   - [Bus Architecture](#bus-architecture)
7. [Instruction Set Architecture (ISA) and CPU Implementation](#instruction-set-architecture-isa-and-cpu-implementation)
   - [ISA Overview](#isa-overview)
   - [CPU Implementation](#cpu-implementation)
8. [The Hack Assembler](#the-hack-assembler)
   - [Core Features](#core-features)
   - [Two-Pass Assembly](#two-pass-assembly)
   - [Assembler Functionality](#assembler-functionality)

### Foundation: Basic Logic Gates

### NAND Gate: The Universal Building Block

The NAND gate serves as the atomic building block for the entire Hack computer architecture. All other logic gates can be constructed from NAND gates:

- NOT gate: Implemented by connecting both NAND inputs together
- AND gate: NAND followed by NOT
- OR gate: Constructed using NAND gates via De Morgan's laws

```
    a  | b | NOT(a) = c | NOT(b) = d | NAND(c,d)
    0  | 0 | 1          | 1          | 0
    0  | 1 | 1          | 0          | 1
    1  | 0 | 0          | 1          | 1
    1  | 1 | 0          | 0          | 1

    Conclusion : NAND(c,d) corresponds to OR output above
    hence the OR CHIP IMPLEMENTATION is

        CHIP Or {
        IN a, b;
        OUT out;

        PARTS:
        Not( in = a, out = notA);
        Not( in = b, out = notB);
        Nand( a = notA, b = notB, out = out);
    }
```

- XOR gate: Built from a combination of NAND, AND, and OR gates

### Fundamental Multi-Bit Gates

Multi-bit gates operate on bus-width inputs (16-bit in Hack):

- Multi-bit NOT: Parallel NOT operations on each bit
- Multi-bit AND: Parallel AND operations
- Multi-bit OR: Parallel OR operations
- Multi-bit Multiplexer (Mux): Selects between two 16-bit inputs

```
s  | a | b            out
--------------------------------
0  | 0 | x             a
0  | 1 | x             a

1  | x | 0             b
1  | x | 1             b

s  | a | b            out
--------------------------------
0  | 0 | 0            a
1  | 0 | 1            b
0  | 1 | 0            a
1  | 1 | 1            b

!(sel).a + sel.b

CHIP Mux {
    IN a, b, sel;
    OUT out;

    PARTS:

    Not( in = sel, out = Notsel);
    And( a = Notsel, b = a, out = NotselAnda);
    And( a = sel, b = b, out = selAndb);
    Or( a = NotselAnda, b = selAndb, out = out);


}
```

- Multi-bit Demultiplexer (DMux): Routes input to one of two 16-bit outputs

```

s                      a | b
--------------------------------
0                     in | 0
1                     0  | in

if 1, the input bit will be directed(channeled) into b : And(a = sel , b = in , out = b);
if 0, the input bit will be directed(channeled) into a : And(a = Notsel , b = in , out = a);

CHIP DMux {
    IN in, sel;
    OUT a, b;

    PARTS:
    Not(in = sel , out = Notsel);
    And(a = Notsel , b = in , out = a);
    And(a = sel , b = in , out = b);

}
```

### Arithmetic Components

### Half Adder

The half adder performs binary addition of two single bits:

Inputs: a, b (single bits)

Outputs:

- sum (a XOR b)
- carry (a AND b)

Implementation:

```
sum = a XOR b
carry = a AND b
```

### Full Adder

Extends the half adder to handle carry-in:

Inputs: a, b, c (carry-in)

Outputs:

- sum ((a XOR b) XOR c)
- carry ((a AND b) OR (c AND (a XOR b)))

Implementation:

Uses two half adders and an OR gate.

### ALU (Arithmetic Logic Unit)

The Hack ALU is a sophisticated component that performs:

**Arithmetic operations:**

- Addition (x + y)
- Subtraction (x - y)
- Negation (-x)

**Logical operations:**

- AND (x & y)
- OR (x | y)
- NOT (!x)

**Control bits:**

- zx: Zero the x input
- nx: Negate the x input
- zy: Zero the y input
- ny: Negate the y input
- f: Function select (1 for add, 0 for AND)
- no: Negate the output

**Output flags:**

- zr: True if output equals zero
- ng: True if output is negative

### Sequential Logic

### D Flip-Flop (DFF) Deep Dive

The DFF is the fundamental sequential building block:

**Clock-synchronized operation:**

- Rising edge triggered
- Setup time: Input must be stable before clock edge
- Hold time: Input must remain stable after clock edge

### Registers

Built upon DFF with additional control:

**Bit Register:**

Components:

- DFF
- Multiplexer
- Feedback path

Control signals:

- load: Controls write enable
- out: Current stored value

**16-bit Register:**

- 16 parallel bit registers
- Shared load control

Applications:

- A-Register: Address/data storage
- D-Register: Data manipulation
- Program Counter: Instruction sequencing

### Memory Hierarchy

### RAM Construction

Hierarchical memory built from registers:

**RAM8:**

- 8 16-bit registers
- 3-bit address space

Components:

- 8 16-bit registers
- 3-to-8 decoder
- 16-bit 8-way multiplexer

**RAM64:**

- 8 RAM8 chips
- 6-bit address space (3 bits RAM8 select, 3 bits register select)

Implementation:

- Address decoding logic
- Register bank selection
- Data multiplexing

**RAM512, RAM4K, RAM16K ...:**

- Each level multiplies capacity by 8
- Additional address bits for bank selection
- Maintained 16-bit data width

### I/O System

### Memory-Mapped I/O

**Address space allocation:**

- 0-16383: Main memory
- 16384-24575: Screen memory
- 24576: Keyboard memory

### Screen Interface

- 256 x 512 pixel display

**Memory organization:**

- 32 x 256 words
- Each word represents 16 pixels
- Sequential mapping

### Keyboard Interface

- Single memory-mapped register
- ASCII code representation
- Interrupt-free polling design

### System Integration

### Bus Architecture

The Hack computer employs a simplified but effective bus architecture:

**Data Bus (16-bit):**

- Carries data between components
- Primary pathways:
  - Memory to CPU
  - CPU to Memory
  - ALU to registers
  - Registers to ALU
- Bidirectional operation

**Address Bus (15-bit):**

- Carries memory addresses
- Address space partitioning:
  - 0-16383 (0x0000-0x3FFF): RAM
  - 16384-24575 (0x4000-0x5FFF): Screen memory
  - 24576 (0x6000): Keyboard memory
- Sources:
  - A-Register (direct addressing)
  - Program Counter (instruction fetch)

**Control Lines:**

- Memory write enable (load)
- Register load signals
- ALU operation select
- Program Counter control

### Instruction Set Architecture (ISA) and CPU Implementation

### ISA Overview

**A-Instructions (Address/Value Instructions):**

Format: 0vvvvvvvvvvvvvvv

```
        │└───────────────┘
        │      15-bit value/address
        └── MSB=0 indicates A-instruction
```

Functions:

- Load constant into A-register
- Set up memory address for subsequent operation
- Set up jump target address

**C-Instructions (Compute Instructions):**

Format:
111accccccdddjjj

```
││││││││││││││││
││└┴┴┴┴┴┘│││└┴┴┘
││c- comp   │││j- jump
││       ││└── j3
││       │└─── j2
││       └──── j1
││d- dest
│└── a-bit
└─── op-code (111)
```

**Components:**

A. Computation Field (cccccc):

- ALU operation specification
- Source operand selection
- Common operations:

```
0: 0      | 1: 1      | -1: -1
D: D      | A: A/M    | !D: NOT D
!A: NOT A | -D: -D    | -A: -A
D+1: D+1  | A+1: A+1  | D-1: D-1
A-1: A-1  | D+A: D+A  | D-A: D-A
A-D: A-D  | D&A: D&A  | D|A: D|A
```

B. Destination Field (ddd):

- d1: write to A-register
- d2: write to D-register
- d3: write to Memory[A]

C. Jump Field (jjj):

- 000: no jump | 100: JLT (jump if < 0)
- 001: JGT (> 0) | 101: JLE (≤ 0)
- 010: JEQ (= 0) | 110: JNE (≠ 0)
- 011: JGE (≥ 0) | 111: JMP (unconditional)

### CPU Implementation

**Control Unit**

- Instruction decoder
- Separates instruction fields
- Generates control signals
- Coordinates data flow

### The Instruction Cycle

Each instruction executes in ONE clock cycle through these phases:

**A. Fetch Phase**

What happens:

- PC (Program Counter) outputs the address of next instruction (PC → ROM address)
- Instruction is read from ROM at this address

**B. Decode Phase**

For A-Instructions (0xxxxxxxxxxxxxxx):

- Simply prepare to load 15-bit value into A register

For C-Instructions (111accccccdddjjj):

- Split instruction into parts:

  - comp: what to compute
  - dest: where to store result
  - jump: whether to jump

**C. Execute Phase**

For A-Instructions:

- Load value into A register

For C-Instructions:

- ALU performs computation
- If accessing memory:

  - Read: Get value from RAM[A]
  - Write: Prepare value for RAM[A]

**D. Write Back Phase**

For A-Instructions:

- A register now holds new value
- PC increments

For C-Instructions:

- Store result based on dest bits:

  - A register (if specified)
  - D register (if specified)
  - Memory[A] (if specified)

- Update PC:
  - Jump: Set PC to A register value
  - No jump: Increment PC

### Example Instruction Execution

**A-Instruction Example: @100**

- Fetch: Get instruction from ROM
- Decode: Recognize as A-instruction
- Execute: Prepare value 100
- Write:
  - Store 100 in A register
  - Increment PC

**C-Instruction Example: D=M+1**

- Fetch: Get instruction from ROM
- Decode:

  - comp: M+1
  - dest: D
  - jump: none

- Execute:

  - Read M (RAM[A])
  - ALU adds 1

- Write:
  - Store result in D register
  - Increment PC

### The Hack Assembler

### Core Features

### **Symbol Management**

- Predefined Symbols:

  - R0-R15: RAM addresses 0-15
  - SCREEN: 16384
  - KBD: 24576
  - SP: 0
  - LCL: 1
  - ARG: 2
  - THIS: 3
  - THAT: 4

- Label Symbols:

  - Marks instruction locations
  - Format: (LABEL)
  - Resolved to ROM addresses

- Variable Symbols:

  - Automatically allocated from RAM[16] onward
  - First use creates allocation

### **Two-Pass Assembly**

- First Pass:

  - Scan for label declarations
  - Build symbol table
  - Count instruction positions

- Second Pass:

  - Process instructions
  - Resolve symbols
  - Generate binary code

### Assembler Functionality

**A-Instruction Translation**

    @value  →  0vvvvvvvvvvvvvvv
    @symbol →  0vvvvvvvvvvvvvvv (where v is symbol's value)

**C-Instruction Translation**

    dest=comp;jump  →  111accccccdddjjj

- Computation Translation:

  D+1 → 011111
  A&D → 000000
  M-1 → 110010

- Destination Translation:

  M → 001
  D → 010
  MD → 011
  A → 100
  AM → 101
  AD → 110
  AMD → 111

- Jump Translation:

  JGT → 001
  JEQ → 010
  JLT → 100
  JNE → 110
  JLE → 110
  JMP → 111
