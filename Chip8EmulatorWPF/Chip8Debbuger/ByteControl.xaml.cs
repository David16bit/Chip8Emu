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

namespace Chip8EmulatorWPF
{
    /// <summary>
    /// 2016 - David Brown (asapdavid91@gmail.com)
    /// Chip8 WPF Hexadecimal editor
    /// </summary>
    public partial class ByteControl : UserControl
    {
        int controlState; // 0 = default, 1 = highlighted, 2 = selected

        public int getControlState()
        {
            return controlState;
        }

        public ByteControl()
        {
            InitializeComponent();
            controlState = 0;
        }

        public void TextColor(Brush color)
        {
            LowerNibble.Foreground = color;
            UpperNibble.Foreground = color;
        }

        public void highlightedState()
        {
            Background = Brushes.LightBlue;
            TextColor(Brushes.Black);
            controlState = 1;

        }

        public void defaultState()
        {
            Background = Brushes.White;
            TextColor(Brushes.Black);
            controlState = 0;
        }

        public void selectedState()
        {
            Background = Brushes.Blue;
            TextColor(Brushes.White);
            controlState = 2;
        }

        public byte ByteVal
        {
            set
            {
                UpperNibble.Content = (value >> 4).ToString("X");
                LowerNibble.Content = (value & 0x0F).ToString("X");

            }
        }

       
    }
}
