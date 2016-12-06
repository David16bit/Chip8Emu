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
            screen.Source = Utli.loadBitmap(chip8.getScreenBitmap());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //use dialog obeject to get rom file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                chip8.reset();
                chip8.loadRom(openFileDialog.FileName);

                if (worker != null)
                {
                    worker.CancelAsync();
                }
                else
                {
                    //activate chip8 cpu cycle thread here on first run.
                    runChip8Thread();
                }
            }

        }


        void workerDoWork(object sender, DoWorkEventArgs e)
        {


            bool run = true;
            while (run)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                else
                chip8FrameCycle(sender);
            }

        }


        void workerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //update screen
            screen.Source = Utli.loadBitmap(chip8.getScreenBitmap());
        }


        void workerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                runChip8Thread();
            }
        }

        private void runChip8Thread()
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += workerDoWork;
            worker.ProgressChanged += workerProgressChanged;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerCompleted += workerRunWorkerCompleted;
            worker.RunWorkerAsync();
        }


        void chip8FrameCycle(object sender)
        {
            long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            //run frame cycle
            for (int i = 0; i < Chip8.INSTRUCTION_PER_FRAME; i++)
            {
                chip8.cpuStep();
            }
            //update screen and timer
            chip8.updateTimers();
            (sender as BackgroundWorker).ReportProgress(0);

            //sleep until milli seconds per frame(17millis) has passed.
            long endTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            int delay = (int)(Chip8.MILLI_SEC_PER_FRAME - (endTime - startTime));
            if (delay > 0)
            {
                System.Threading.Thread.Sleep(delay);
            }
        }

     


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (chip8 != null)
            {
                if (e.Key == Key.NumPad0)
                {
                    chip8.keys[0] = 1;
                }
                else if (e.Key == Key.NumPad1)
                {
                    chip8.keys[1] = 1;
                }
                else if (e.Key == Key.NumPad2)
                {
                    chip8.keys[2] = 1;
                }
                else if (e.Key == Key.NumPad3)
                {
                    chip8.keys[3] = 1;
                }
                else if (e.Key == Key.NumPad4)
                {
                    chip8.keys[4] = 1;
                }
                else if (e.Key == Key.NumPad5)
                {
                    chip8.keys[5] = 1;
                }
                else if (e.Key == Key.NumPad6)
                {
                    chip8.keys[6] = 1;
                }
                else if (e.Key == Key.NumPad7)
                {
                    chip8.keys[7] = 1;
                }
                else if (e.Key == Key.NumPad8)
                {
                    chip8.keys[8] = 1;
                }
                else if (e.Key == Key.NumPad9)
                {
                    chip8.keys[9] = 1;
                }
                else if (e.Key == Key.A)
                {
                    chip8.keys[0x0A] = 1;
                }
                else if (e.Key == Key.B)
                {
                    chip8.keys[0x0B] = 1;
                }
                else if (e.Key == Key.C)
                {
                    chip8.keys[0x0C] = 1;
                }
                else if (e.Key == Key.D)
                {
                    chip8.keys[0x0D] = 1;
                }
                else if (e.Key == Key.E)
                {
                    chip8.keys[0x0E] = 1;
                }
                else if (e.Key == Key.F)
                {
                    chip8.keys[0x0F] = 1;
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (chip8 != null)
            {
                if (e.Key == Key.NumPad0)
                {
                    chip8.keys[0] = 0;
                }
                else if (e.Key == Key.NumPad1)
                {
                    chip8.keys[1] = 0;
                }
                else if (e.Key == Key.NumPad2)
                {
                    chip8.keys[2] = 0;
                }
                else if (e.Key == Key.NumPad3)
                {
                    chip8.keys[3] = 0;
                }
                else if (e.Key == Key.NumPad4)
                {
                    chip8.keys[4] = 0;
                }
                else if (e.Key == Key.NumPad5)
                {
                    chip8.keys[5] = 0;
                }
                else if (e.Key == Key.NumPad6)
                {
                    chip8.keys[6] = 0;
                }
                else if (e.Key == Key.NumPad7)
                {
                    chip8.keys[7] = 0;
                }
                else if (e.Key == Key.NumPad8)
                {
                    chip8.keys[8] = 0;
                }
                else if (e.Key == Key.NumPad9)
                {
                    chip8.keys[9] = 0;
                }
                else if (e.Key == Key.A)
                {
                    chip8.keys[0x0A] = 0;
                }
                else if (e.Key == Key.B)
                {
                    chip8.keys[0x0B] = 0;
                }
                else if (e.Key == Key.C)
                {
                    chip8.keys[0x0C] = 0;
                }
                else if (e.Key == Key.D)
                {
                    chip8.keys[0x0D] = 0;
                }
                else if (e.Key == Key.E)
                {
                    chip8.keys[0x0E] = 0;
                }
                else if (e.Key == Key.F)
                {
                    chip8.keys[0x0F] = 0;
                }
            }
        }
    }
}
