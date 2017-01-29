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
using System.ComponentModel;
using System.Drawing;
using Microsoft.Win32;


namespace Chip8EmulatorWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Chip8 chip8;
        BackgroundWorker worker;

        public MainWindow()
        {
            InitializeComponent();
            chip8 = new Chip8();
            chip8.connectDisplay(screen);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //use dialog obeject to get rom file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (chip8.IsOn())
                {
                    chip8.powerOff();
                }

                chip8.insertGame(openFileDialog.FileName);
                chip8.powerOn();
            }
        }


        
     


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (chip8 != null)
            {
                if (e.Key == Key.NumPad0)
                {
                    chip8.gamePad[0] = 1;
                }
                else if (e.Key == Key.NumPad1)
                {
                    chip8.gamePad[1] = 1;
                }
                else if (e.Key == Key.NumPad2)
                {
                    chip8.gamePad[2] = 1;
                }
                else if (e.Key == Key.NumPad3)
                {
                    chip8.gamePad[3] = 1;
                }
                else if (e.Key == Key.NumPad4)
                {
                    chip8.gamePad[4] = 1;
                }
                else if (e.Key == Key.NumPad5)
                {
                    chip8.gamePad[5] = 1;
                }
                else if (e.Key == Key.NumPad6)
                {
                    chip8.gamePad[6] = 1;
                }
                else if (e.Key == Key.NumPad7)
                {
                    chip8.gamePad[7] = 1;
                }
                else if (e.Key == Key.NumPad8)
                {
                    chip8.gamePad[8] = 1;
                }
                else if (e.Key == Key.NumPad9)
                {
                    chip8.gamePad[9] = 1;
                }
                else if (e.Key == Key.A)
                {
                    chip8.gamePad[0x0A] = 1;
                }
                else if (e.Key == Key.B)
                {
                    chip8.gamePad[0x0B] = 1;
                }
                else if (e.Key == Key.C)
                {
                    chip8.gamePad[0x0C] = 1;
                }
                else if (e.Key == Key.D)
                {
                    chip8.gamePad[0x0D] = 1;
                }
                else if (e.Key == Key.E)
                {
                    chip8.gamePad[0x0E] = 1;
                }
                else if (e.Key == Key.F)
                {
                    chip8.gamePad[0x0F] = 1;
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (chip8 != null)
            {
                if (e.Key == Key.NumPad0)
                {
                    chip8.gamePad[0] = 0;
                }
                else if (e.Key == Key.NumPad1)
                {
                    chip8.gamePad[1] = 0;
                }
                else if (e.Key == Key.NumPad2)
                {
                    chip8.gamePad[2] = 0;
                }
                else if (e.Key == Key.NumPad3)
                {
                    chip8.gamePad[3] = 0;
                }
                else if (e.Key == Key.NumPad4)
                {
                    chip8.gamePad[4] = 0;
                }
                else if (e.Key == Key.NumPad5)
                {
                    chip8.gamePad[5] = 0;
                }
                else if (e.Key == Key.NumPad6)
                {
                    chip8.gamePad[6] = 0;
                }
                else if (e.Key == Key.NumPad7)
                {
                    chip8.gamePad[7] = 0;
                }
                else if (e.Key == Key.NumPad8)
                {
                    chip8.gamePad[8] = 0;
                }
                else if (e.Key == Key.NumPad9)
                {
                    chip8.gamePad[9] = 0;
                }
                else if (e.Key == Key.A)
                {
                    chip8.gamePad[0x0A] = 0;
                }
                else if (e.Key == Key.B)
                {
                    chip8.gamePad[0x0B] = 0;
                }
                else if (e.Key == Key.C)
                {
                    chip8.gamePad[0x0C] = 0;
                }
                else if (e.Key == Key.D)
                {
                    chip8.gamePad[0x0D] = 0;
                }
                else if (e.Key == Key.E)
                {
                    chip8.gamePad[0x0E] = 0;
                }
                else if (e.Key == Key.F)
                {
                    chip8.gamePad[0x0F] = 0;
                }
            }
        }
    }
}
