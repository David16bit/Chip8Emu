using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Chip8EmulatorWPF.Chip8Emulator
{
    /// <summary>
    /// 2016 - David Brown (asapdavid91@gmail.com)
    /// Chip8 WPF Hexadecimal editor
    /// </summary>

    class Game
    {
        private String path;
        private byte[] rom;
        private bool[] addrDecodeCheck;
        private List<DecodedInstruction> decodedRom;

        public string Path
        {
            get{return path;}

            set{path = value;}
        }

        public Game()
        {
           
        }

        public Game(String path)
        {
            loadRom(path);
        }

        public void loadRom(String path)
        {
            this.path = path;

            rom = File.ReadAllBytes(path);

            decodedRom = new List<DecodedInstruction>();
            addrDecodeCheck = new bool[rom.Length];

            recursiveTranversal(0, rom);
            decodedRom = decodedRom.OrderBy(o => o.AddrOfInstruc).ToList();
        }

        public List<DecodedInstruction> getDecodeRom()
        {
            return decodedRom;
        }

        public byte[] getRom()
        {
            return rom;
        }

        private void recursiveTranversal(ushort addr, byte[] rom)
        {
            while (addr >= 0 && addr <= rom.Length)
            {
                if (!addrDecodeCheck[addr])
                {
                    DecodedInstruction instruct = dissaembleInstruc((ushort)((rom[addr] << 8) | rom[addr + 1]), addr);
                    decodedRom.Add(instruct);
                    addrDecodeCheck[addr] = true;

                    byte opcode = Utli.getUpperByteHighNibble(instruct.Opcode);

                    //branch instruction check
                    if (opcode == 1) //non conditional jump
                    {
                        recursiveTranversal((ushort)((instruct.Opcode & 0x0FFF) - Chip8.ROM_BASE_ADDR), rom);
                        return;
                    }
                    else if (opcode == 2)// subroutine call,
                    {
                        //2 brances. first branch is the call, second is the return branch
                        recursiveTranversal((ushort)((instruct.Opcode & 0x0FFF) - Chip8.ROM_BASE_ADDR), rom);
                        recursiveTranversal((ushort)((instruct.AddrOfInstruc + 2)), rom);
                        return;
                    }
                    else if (instruct.Opcode == 0x00EE) //end subroutine
                    {
                        return;
                    }//3,5,4,9
                    else if (opcode == 3 || opcode == 5 || opcode == 4 || opcode == 9 || opcode == 0xE) //primitive conditional branching
                    {
                        recursiveTranversal((ushort)((instruct.AddrOfInstruc + 2)), rom);
                        recursiveTranversal((ushort)((instruct.AddrOfInstruc + 4)), rom);
                        return;
                    }
                    else
                    {
                        addr = (ushort)(addr + 2);
                    }
                }
                else
                    return;
            }
        }





        public DecodedInstruction dissaembleInstruc(ushort opcode, ushort addr)
        {
            DecodedInstruction instruction = new DecodedInstruction();

            //disassemble instruction params

            string nnn = "0x" + ((opcode & 0x0FFF)).ToString("x2").ToUpper();// address - 0nnn
            string nn = "0x" + (Utli.getLowerByte(opcode)).ToString("x2").ToUpper();// constant - 00nn
            string n = "0x" + (Utli.getLowerByteLowNibble(opcode)).ToString("x2").ToUpper();// constant - 000n
            string x = (Utli.getUpperByteLowNibble(opcode)).ToString("x2").ToUpper();// register # - 0x00
            string y = (Utli.getLowerByteHighNibble(opcode)).ToString("x2").ToUpper(); // register # - 00y0

            if (x.Substring(0, 1).Equals("0"))
                x = x.Substring(1);
            if (y.Substring(0, 1).Equals("0"))
                y = y.Substring(1);

            //decode
            switch (Utli.getUpperByteHighNibble(opcode))
            {
                case 0x0:
                    {
                        if (Utli.getLowerByteHighNibble(opcode) == 0xE)
                        {
                            switch (Utli.getLowerByteLowNibble(opcode))
                            {
                                case 0x0://	Clears the screen.
                                    instruction.AddrOfInstruc = addr;
                                    instruction.Opcode = opcode;
                                    instruction.MnemonicInstuc = "CLS";
                                    instruction.Description = "Clears the screen.";
                                    break;
                                case 0xE://Returns from a subroutine.
                                    instruction.AddrOfInstruc = addr;
                                    instruction.Opcode = opcode;
                                    instruction.MnemonicInstuc = "RET";
                                    instruction.Description = "Returns from a subroutine.";
                                    break;
                                default:
                                    Console.WriteLine("Unknown Opcode - " + "0x" + opcode.ToString("x2"));
                                    break;
                            }
                        }
                        else
                        {
                            if ((opcode & 0x0FFF) == 0)
                            {
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "SYS";
                                instruction.Description = "Calls RCA 1802 program at address " + addr + " Not necessary for most ROMs.";
                            }
                            else
                                instruction.AddrOfInstruc = addr;
                            instruction.Opcode = opcode;
                            instruction.MnemonicInstuc = "UNKNOWN";
                            instruction.Description = "Unimplemented Opcode - " + "0x" + opcode.ToString("x2");
                        }
                    }
                    break;

                case 0x1://Jumps to address NNN.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "JP " + nnn;
                    instruction.Description = "Jumps to address " + nnn;
                    break;

                case 0x2://Calls subroutine at NNN.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "CALL " + nnn;
                    instruction.Description = "Calls subroutine at " + nnn;
                    break;

                case 0x3://3XNN	Skips the next instruction if VX equals NN.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "SE V" + x + ", " + nn;
                    instruction.Description = "Skips the next instruction if V" + x + " equals " + nn;
                    break;

                case 0x4://4XNN	Skips the next instruction if VX doesn't equal NN.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "SNE V" + x + ", " + nn;
                    instruction.Description = "Skips the next instruction if V" + x + " doesn't equal " + nn;
                    break;

                case 0x5://5XY0	Skips the next instruction if VX equals VY.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "SE V" + x + ", " + "V" + y;
                    instruction.Description = "Skips the next instruction if V" + x + " equals " + "V" + y;
                    break;

                case 0x6://6XNN Sets VX to NNX to NN.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "LD V" + x + ", " + nn;
                    instruction.Description = "Sets V" + x + " to " + nn;
                    break;

                case 0x7://7XNN	Adds NN to VX.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "ADD V" + x + ", " + nn;
                    instruction.Description = "Adds " + nn + " to " + "V" + x;
                    break;

                case 0x8:
                    {
                        switch (Utli.getLowerByteLowNibble(opcode))
                        {

                            case 0x0://	8XY0 Sets VX to the value of VY.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "LD V" + x + ", " + "V" + y;
                                instruction.Description = "Sets V" + x + " to the value " + "V" + y;
                                break;

                            case 0x1://	8XY1 Sets VX to VX or VY.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "OR V" + x + ", " + "V" + y;
                                instruction.Description = "Sets V" + x + " to " + "V" + x + " or " + "V" + y;
                                break;

                            case 0x2://	8XY2 Sets VX to VX and VY.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "AND V" + x + ", " + "V" + y;
                                instruction.Description = "Sets V" + x + " to " + "V" + x + " and " + "V" + y;
                                break;

                            case 0x3://	8XY3 Sets VX to VX xor VY.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "XOR V" + x + ", " + "V" + y;
                                instruction.Description = "Sets V" + x + " to " + "V" + x + " xor " + "V" + y;
                                break;

                            case 0x4://	8XY4 Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                                {
                                    instruction.AddrOfInstruc = addr;
                                    instruction.Opcode = opcode;
                                    instruction.MnemonicInstuc = "ADD V" + x + ", " + "V" + y;
                                    instruction.Description = "Adds V" + y + " to " + "V" + x;
                                }
                                break;

                            case 0x5://8XY5	VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                                {
                                    instruction.AddrOfInstruc = addr;
                                    instruction.Opcode = opcode;
                                    instruction.MnemonicInstuc = "SUB V" + x + ", " + "V" + y;
                                    instruction.Description = "V" + y + " is subtracted from " + "V" + x;
                                }
                                break;

                            case 0x6://8XY6 Shifts VX right by one.
                                     // VF is set to the value of the least significant bit of VX before the shift.
                                {
                                    instruction.AddrOfInstruc = addr;
                                    instruction.Opcode = opcode;
                                    instruction.MnemonicInstuc = "SHR V" + x + "{, " + "V" + y + "}";
                                    instruction.Description = "Shifts " + "V" + x + "right by one. " + " VF is set to the value of the least significant bit of "
                                        + "V" + x + " before the shift";
                                }
                                break;

                            case 0x7://8XY7	Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                                {
                                    instruction.AddrOfInstruc = addr;
                                    instruction.Opcode = opcode;
                                    instruction.MnemonicInstuc = "SUBN V" + x + ", " + "V" + y;
                                    instruction.Description = "Sets V" + x + " to " + "V" + y + " minus " + "V" + x;
                                }
                                break;

                            case 0xE://8XYE	Shifts VX left by one. VF is set to the value of the
                                     // most significant bit of VX before the shift.
                                {
                                    instruction.AddrOfInstruc = addr;
                                    instruction.Opcode = opcode;
                                    instruction.MnemonicInstuc = "SHL V" + x + "{, " + "V" + y + "}";
                                    instruction.Description = "Shifts " + "V" + x + "left by one. " + " VF is set to the value of the most significant bit of "
                                        + "V" + x + " before the shift";
                                }
                                break;

                            default:
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "UNKNOWN";
                                instruction.Description = "Unimplemented Opcode - " + "0x" + opcode.ToString("x2");
                                break;
                        }
                    }
                    break;

                case 0x9://9XY0	Skips the next instruction if VX doesn't equal VY.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "SNE V" + x + ", " + "V" + y;
                    instruction.Description = "Skips the next instruction if V" + x + " dosen't equal " + "V" + y;
                    break;

                case 0xA://ANNN	Sets Ireg to the address NNN.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "LD I, " + nnn;
                    instruction.Description = "Sets Ireg to the address " + nnn;
                    break;

                case 0xB://BNNN	Jumps to the address NNN plus V0.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "JP V0, " + nnn;
                    instruction.Description = "Jumps to the address " + nnn + " plus V0";
                    break;

                case 0xC://CXNN	Sets VX to the result of a bitwise and operation on a random number and NN.
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "RND V" + x + ", " + nn;
                    instruction.Description = "Sets V" + x + " to the result of a bitwise and operation on a random number and " + nn;
                    break;

                case 0xD:
                    {
                        //DXYN	Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels.
                        // Each row of 8 pixels is read as bit-coded starting from memory location Ireg;
                        // Ireg value doesn’t change after the execution of this instruction.
                        // As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn,
                        // and to 0 if that doesn’t happen
                        instruction.AddrOfInstruc = addr;
                        instruction.Opcode = opcode;
                        instruction.MnemonicInstuc = "DRW V" + x + ", " + "V" + y + ", " + n;
                        instruction.Description = "Draws a spirte at (V" + x + ", V" + y + ")" + " that has a width of 8 pixels and a height of " + n + "pixels";
                    }
                    break;

                case 0xE:
                    {
                        switch (Utli.getLowerByte(opcode))
                        {
                            case 0x9E://EX9E Skips the next instruction if the key stored in VX is pressed.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "SKP V" + x;
                                instruction.Description = "Skips the next instruction if the key stored in V" + x + " is pressed";
                                break;

                            case 0xA1://EXA1 Skips the next instruction if the key stored in VX isn't pressed.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "SKNP V" + x;
                                instruction.Description = "Skips the next instruction if the key stored in V" + x + " isn't pressed";
                                break;

                            default:
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "UNKNOWN";
                                instruction.Description = "Unimplemented Opcode - " + "0x" + opcode.ToString("x2");
                                break;
                        }
                    }
                    break;

                case 0xF:
                    {
                        switch (Utli.getLowerByte(opcode))
                        {
                            case 0x07://FX07  Sets VX to the value of the delay timer.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "LD V" + x + ", DT";
                                instruction.Description = "Sets V" + x + " to the value of the delay timer.";
                                break;

                            case 0x0A://FX0A  A key press is awaited, and then stored in VX.
                                {
                                    instruction.AddrOfInstruc = addr;
                                    instruction.Opcode = opcode;
                                    instruction.MnemonicInstuc = "LD V" + x + ", K";
                                    instruction.Description = "All execution stops until a key is pressed, then the value of that key is stored in V" + x;

                                }
                                break;

                            case 0x15://	FX15 Sets the delay timer to VX.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "LD DT, V" + x;
                                instruction.Description = "Sets the delay timer to V" + x;
                                break;

                            case 0x18://	FX18 Sets the sound timer to VX.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "LD ST, V" + x;
                                instruction.Description = "Sets the sound timer to V" + x;
                                break;

                            case 0x1E: //FX1E Adds VX to Ireg.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "ADD I, V" + x;
                                instruction.Description = "Adds V" + x + " to Ireg.";
                                break;

                            case 0x29://FX29 Sets Ireg to the location of the sprite for the character in VX.
                                      // Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "LD F, V" + x;
                                instruction.Description = "Sets Ireg to the location of the sprite for character in V" + x;
                                break;

                            case 0x33://FX33 Stores the binary-coded decimal representation of VX, 
                                      //with the most significant of three digits at the address in I, 
                                      //the middle digit at I plus 1, and the least significant digit at I plus 2.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "LD B, V" + x;
                                instruction.Description = "Stores the binary-coded decimal representation of V" + x + "(I, I+1, I+2)";
                                break;

                            case 0x55://FX55 Stores V0 to VX (including VX) in memory starting at address Ireg.
                                      //I is set to I + X + 1 after operation
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "LD [I], V" + x;
                                instruction.Description = "Stores V0 to V" + x + " (including V" + x + ") in memory starting at address Ireg";
                                break;

                            case 0x65://FX65 Fills V0 to VX (including VX) with values from memory starting at address Ireg.
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "LD V" + x + ", [I]";
                                instruction.Description = "Fills V0 to V" + x + " (including V" + x + ") with values from memory starting at address Ireg";
                                break;
                            default:
                                instruction.AddrOfInstruc = addr;
                                instruction.Opcode = opcode;
                                instruction.MnemonicInstuc = "UNKNOWN";
                                instruction.Description = "Unimplemented Opcode - " + "0x" + opcode.ToString("x2");
                                break;
                        }
                    }
                    break;

                default:
                    instruction.AddrOfInstruc = addr;
                    instruction.Opcode = opcode;
                    instruction.MnemonicInstuc = "UNKNOWN";
                    instruction.Description = "Unimplemented Opcode - " + "0x" + opcode.ToString("x2");
                    break;
            }

            return instruction;
        }
    }
}
