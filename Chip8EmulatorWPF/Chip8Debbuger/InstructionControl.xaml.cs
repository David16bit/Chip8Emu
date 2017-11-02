using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chip8EmulatorWPF.Chip8DebbugerUICtrls
{
    /// <summary>
    /// 2016 - David Brown (asapdavid91@gmail.com)
    /// Chip8 WPF Hexadecimal editor
    /// </summary>
    public partial class InstructionControl : UserControl
    {
        private ushort addr;
        private string descVal;
        public delegate void BrkPtHandler(ushort addr, Image breakIcon);
        public BrkPtHandler brkPtHandler;

        public String binaryVal
        {
            set
            {
                Binary.Content = value;
            }
        }

        public String opcodeVal
        {
            set { Opcode.Content = value; }
        }

        public ushort Addr
        {
            get
            {
                return addr;
            }

            set
            {
                addr = value;
            }
        }

        public InstructionControl(String binary, String opcode, string desc, ushort address)
        {
            InitializeComponent();
            Binary.Content = binary;
            Opcode.Content = opcode;

            if(desc.Length >= 75)
            {
                Description.Text = desc.Substring(0, 75) + "...";
            }
            else
            {
                Description.Text = desc;
            }
            descVal = desc;
            BodyToolTip.Text = desc;
            Addr = address;
            
        }

        public void selectLine(bool val)
        {
            if (val)
                Selection.Content = ">";
            else
                Selection.Content = "";
        }

        private void breakPoint_Click(object sender, RoutedEventArgs e)
        {
            brkPtHandler(addr, breakIcon);
        }
    }
}
