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
using System.Windows.Shapes;
using Chip8EmulatorWPF.Chip8DebbugerUICtrls;

namespace Chip8EmulatorWPF
{
    /// <summary>
    /// 2016 - David Brown (asapdavid91@gmail.com)
    /// Chip8 WPF Hexadecimal editor
    /// </summary>
    /// 
    public partial class DebuggerWindow : Window
    {
        MainWindow gameWindow;
        private bool openWin;
        private int currSelectedCode;// represents the code that is selected in the decoded window for execution
        private Chip8 chip8;
        bool breakPointTrigger;//If set to true, process executes the instruction which has a
                               //breakpoint assigned to it, instead of halting the process at the breakpoint instruction


        #region Miscellaneous property/methods
        private void bringMainWinToFront()
        {
            //bring main window to forefront
            if (!gameWindow.IsVisible)
            {
                gameWindow.Show();
            }

            if (gameWindow.WindowState == WindowState.Minimized)
            {
                gameWindow.WindowState = WindowState.Normal;
            }

            gameWindow.Activate();
            gameWindow.Topmost = true;  // important
            gameWindow.Topmost = false; // important
            gameWindow.Focus();         // important
        }

        public bool OpenWin
        {
            get { return openWin; }
            set { openWin = value; }
        }

        public MainWindow GameWindow
        {
            get { return gameWindow; }
            set { gameWindow = value; }
        }

        internal Chip8 Chip8
        {
            set
            {
                chip8 = value;
            }
        }

        public DebuggerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            initRoutine();
        }

        public void resetInterface()
        {
            initRoutine();
        }

        private void loadHexEditor()
        {
            HexEditor.Chip8 = chip8;
            HexEditor.updateHexEditor();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (chip8.IsOn())
            {
                chip8.brkPtAddresses.Clear();
                if (!chip8.isCpuRunning())
                {
                    //pause state
                    chip8.runCpuCycle();
                }
            }
            chip8.DebugMode = false;
            openWin = false;
        }
        #endregion Miscellaneous property/methods


        public void initRoutine()
        {
            currSelectedCode = -1;
            chip8.DebugMode = true;
            if (chip8.IsOn() && chip8.isCpuRunning())
            {
                chip8.setDebuggerInterface(this);
                //end chip 8 continous cycle thread
                chip8.haltContinousCpuCycle();
                initCodeWindow();
                updateDebugger(false);

                pauseBtn.IsEnabled = false;
                runBtn.IsEnabled = true;
                stepBtn.IsEnabled = true;
                rebootBtn.IsEnabled = true;

                chip8.brkPtAddresses.Clear();
                breakPtList.Items.Clear();
            }
            else if(chip8.isRomLoaded())
            {
                chip8.PowerStatus = true;
                chip8.setDebuggerInterface(this);
                initCodeWindow();
                updateDebugger(false);

                pauseBtn.IsEnabled = false;
                runBtn.IsEnabled = true;
                stepBtn.IsEnabled = true;
                rebootBtn.IsEnabled = true;

                chip8.brkPtAddresses.Clear();
                breakPtList.Items.Clear();
            }
            else
            {
                pauseBtn.IsEnabled = false;
                runBtn.IsEnabled = false;
                stepBtn.IsEnabled = false;
                rebootBtn.IsEnabled = false;
            }
        }


       
        private void initCodeWindow()
        {
            CodeSection.Children.Clear();
            List<DecodedInstruction> code = chip8.decodedRom();
            for (int i = 0; i < code.Count; i++)
            {
                InstructionControl x = new InstructionControl(code[i].getAddrStringFormat() + ": "
                    + code[i].getOpcodeStringFormat(),
                    code[i].MnemonicInstuc, code[i].Description,
                    (ushort)(Chip8.ROM_BASE_ADDR + code[i].AddrOfInstruc));
                x.brkPtHandler = DelegateBrkPtMethod;
                CodeSection.Children.Add(x);
            }
        }

        private void updateDebugger(bool breakPointFlag)
        {
            //load Vreg values into interface
            V0InputBox.Text = "0x" + (chip8.VRegs[0].ToString("x2")).ToUpper();
            V1InputBox.Text = "0x" + (chip8.VRegs[1].ToString("x2")).ToUpper();
            V2InputBox.Text = "0x" + (chip8.VRegs[2].ToString("x2")).ToUpper();
            V3InputBox.Text = "0x" + (chip8.VRegs[3].ToString("x2")).ToUpper();
            V4InputBox.Text = "0x" + (chip8.VRegs[4].ToString("x2")).ToUpper();
            V5InputBox.Text = "0x" + (chip8.VRegs[5].ToString("x2")).ToUpper();
            V6InputBox.Text = "0x" + (chip8.VRegs[6].ToString("x2")).ToUpper();
            V7InputBox.Text = "0x" + (chip8.VRegs[7].ToString("x2")).ToUpper();

            V8InputBox.Text = "0x" + (chip8.VRegs[8].ToString("x2")).ToUpper();
            V9InputBox.Text = "0x" + (chip8.VRegs[9].ToString("x2")).ToUpper();
            VAInputBox.Text = "0x" + (chip8.VRegs[0xA].ToString("x2")).ToUpper();
            VBInputBox.Text = "0x" + (chip8.VRegs[0xB].ToString("x2")).ToUpper();
            VCInputBox.Text = "0x" + (chip8.VRegs[0xC].ToString("x2")).ToUpper();
            VDInputBox.Text = "0x" + (chip8.VRegs[0xD].ToString("x2")).ToUpper();
            VEInputBox.Text = "0x" + (chip8.VRegs[0xE].ToString("x2")).ToUpper();
            VFInputBox.Text = "0x" + (chip8.VRegs[0xF].ToString("x2")).ToUpper();

            //load specialized registers
            IndexInputBox.Text = "0x" + (chip8.Ireg1.ToString("x2")).ToUpper();
            pcInputBox.Text = "0x" + (chip8.Pc.ToString("x2")).ToUpper();
            DelayTimerInputBox.Text = "0x" + (chip8.DelayTimer.ToString("x2")).ToUpper();
            SoundTimerInputBox.Text = "0x" + (chip8.SoundTimer.ToString("x2")).ToUpper();

            //select instruction in codeview
            for(int i = 0; i < CodeSection.Children.Count; i++)
            {
                InstructionControl code = (InstructionControl)CodeSection.Children[i];
                if (code.Addr == chip8.Pc)
                {
                    if (currSelectedCode > -1)
                    {
                        InstructionControl previousInstruc = (InstructionControl)CodeSection.Children[currSelectedCode];
                        previousInstruc.selectLine(false);
                    }
                    code.selectLine(true);
                    code.BringIntoView();
                    currSelectedCode = i;

                    if(breakPointFlag)
                    {
                       //code.Background = Brushes.Red;
                        breakPointTrigger = true;
                    }
                }
            }

          
            //load stack 
            byte[] stack = chip8.Stack;
            string stackOutput = "";
            if (stack != null)
            {
                for (int j = 0; j < stack.Length; j++)
                {
                    if(stack[j] != 0)
                        stackOutput = stackOutput + "0x" + ((Chip8.STACK_BASE_ADDR - 1) - j).ToString("x2").ToUpper() + ": 0x" + stack[j].ToString("x2").ToUpper();
                    else
                        stackOutput = stackOutput + "0x" + ((Chip8.STACK_BASE_ADDR - 1) - j).ToString("x2").ToUpper() + ": 0x00";

                    if (j != (stack.Length - 1))
                    {
                        stackOutput = stackOutput + "\n";
                    }
                }
                //stackOutput = stackOutput.Substring(0, (stackOutput.Length - 1));
                stackBox.Text = stackOutput;
            }

            //updateHexEditor
            loadHexEditor();
        }

        #region Control Button handlers property/methods

        private void stepBtn_Click(object sender, RoutedEventArgs e)
        {
            updateDebugger(chip8.debugStepRoutine());
            bringMainWinToFront();
        }


        private void reset_Click(object sender, RoutedEventArgs e)
        {
            chip8.brkPtAddresses.Clear();
            breakPtList.Items.Clear();
            stackBox.Text = "";

            if (!chip8.isCpuRunning())
            {
                chip8.runCpuCycle();
            }
            chip8.powerReset();
            chip8.clearDisplay();
            bringMainWinToFront();
        }

        private void run_Click(object sender, RoutedEventArgs e)
        {
            if(breakPointTrigger)
            {
                chip8.debugStepRoutine();
                breakPointTrigger = false;
            }

            pauseBtn.IsEnabled = true;
            runBtn.IsEnabled = false;
            stepBtn.IsEnabled = false;

            chip8.runCpuCycle();
            bringMainWinToFront();
            
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            pauseBtn.IsEnabled = false;
            runBtn.IsEnabled = true;
            stepBtn.IsEnabled = true;
            pauseRoutine();
        }

        public void pauseRoutine()
        {
            chip8.haltContinousCpuCycle();
            updateDebugger(false);
        }

        #endregion Control Button handlers property/methods

        #region Breakpoint property/methods

        public bool BreakPointTrigger
        {
            get { return breakPointTrigger; }
            set { breakPointTrigger = value; }
        }

        public void breakPointPause()
        {
            pauseBtn.IsEnabled = false;
            runBtn.IsEnabled = true;
            stepBtn.IsEnabled = true;
            updateDebugger(true);
        }

     
        public void DelegateBrkPtMethod(ushort addr, Image breakIcon)
        {
            if (breakIcon.IsVisible)
            {
                String addrVal = addr.ToString("x2").ToUpper();
                ushort value = (ushort) Convert.ToInt32(addrVal, 16);
                breakIcon.Visibility = Visibility.Hidden;

                //remove address from listbox
                for (int i = 0; i < breakPtList.Items.Count; i++)
                {
                    String textVal = ((ListBoxItem)breakPtList.Items[i]).Content.ToString().Substring(8);
                    if (addrVal.Equals(textVal))
                    {
                        breakPtList.Items.RemoveAt(i);
                    }
                }

                //remove address from Chip8's list of addresses to break on
                for (int j = 0; j < chip8.brkPtAddresses.Count(); j++)
                {
                    if (value == chip8.brkPtAddresses[j])
                    {
                        chip8.brkPtAddresses.RemoveAt(j);
                    }
                }
            }
            else
            {
                breakIcon.Visibility = Visibility.Visible;
                ListBoxItem itm = new ListBoxItem();
                itm.Content = "Addr: " + "0x" + (addr.ToString("x2")).ToUpper();
                breakPtList.Items.Add(itm);
                chip8.brkPtAddresses.Add(addr);
            }
        }

      

        private void breakPtList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (breakPtList.SelectedItem != null)
            {
                //MessageBox.Show(breakPtList.SelectedItem.ToString());
                String textVal = ((ListBoxItem)breakPtList.SelectedItem).Content.ToString().Substring(8);
                ushort selectedVal = (ushort) Convert.ToInt32(textVal, 16);

                //select instruction in codeview
                for (int i = 0; i < CodeSection.Children.Count; i++)
                {
                    InstructionControl code = (InstructionControl)CodeSection.Children[i];
                    if (code.Addr == selectedVal)
                    {
                        code.BringIntoView();
                    }
                }
            }
        }
        #endregion Breakpoint property/methods
    }
}
