using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8EmulatorWPF
{
    /// <summary>
    /// 2016 - David Brown (asapdavid91@gmail.com)
    /// Chip8 WPF Hexadecimal editor
    /// </summary>

    public class DecodedInstruction
    {
        private ushort addrOfInstruc;
        private ushort opcode;
        private string mnemonicInstuc;
        private string description;

        public DecodedInstruction()
        {
           
        }

        public DecodedInstruction(ushort addrOfInstruc, ushort opcode, string mnemonicInstuc, string description)
        {
            this.addrOfInstruc = addrOfInstruc;
            this.opcode = opcode;
            this.mnemonicInstuc = mnemonicInstuc;
            this.description = description;
        }

        public string getAssemblyCode()
        {
            string outputLine = "";
            ushort addr = (ushort)(Chip8.ROM_BASE_ADDR + addrOfInstruc);
            //add address
            outputLine = outputLine + addr.ToString("x2").ToUpper();
            //add spaces
            outputLine += string.Join("", Enumerable.Repeat(' ', 4));
            //add machine code
            outputLine = outputLine + Utli.getHighByte(opcode).ToString("x2").ToUpper() + " " + Utli.getLowerByte(opcode).ToString("x2").ToUpper();
            //add spaces
            outputLine += string.Join("", Enumerable.Repeat(' ', 10));
            //add assembly code
            outputLine = outputLine + mnemonicInstuc;

            return outputLine;
        }

        public string getAddrStringFormat()
        {
            ushort addr = (ushort)(Chip8.ROM_BASE_ADDR + addrOfInstruc);
            //add address
            return addr.ToString("x2").ToUpper();
        }

        public string getOpcodeStringFormat()
        {
            return Utli.getHighByte(opcode).ToString("x2").ToUpper() + " " + Utli.getLowerByte(opcode).ToString("x2").ToUpper();
        }

        public ushort Opcode
        {
            get
            {
                return opcode;
            }

            set
            {
                opcode = value;
            }
        }

        public string MnemonicInstuc
        {
            get
            {
                return mnemonicInstuc;
            }

            set
            {
                mnemonicInstuc = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        public ushort AddrOfInstruc
        {
            get
            {
                return addrOfInstruc;
            }

            set
            {
                addrOfInstruc = value;
            }
        }
    }
}
