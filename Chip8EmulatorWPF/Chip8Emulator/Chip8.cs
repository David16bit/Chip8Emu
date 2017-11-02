using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using Chip8EmulatorWPF.Chip8Emulator;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Chip8EmulatorWPF
{
    /// <summary>
    /// 2016 - David Brown (asapdavid91@gmail.com)
    /// Chip8 WPF Hexadecimal editor
    /// </summary>

    class Chip8
    {
        //constants
        private const ushort FONT_BASE_ADDR = 0x000;
        private const ushort FONT_BASE_SIZE = 0x50; //80 bytes

        public const ushort STACK_BASE_ADDR = 0xEA0;
        public const byte INSTRUCTION_PER_FRAME = 16;
        public const long MILLI_SEC_PER_FRAME = 17;
        public const ushort ROM_BASE_ADDR = 0x200;
        public const int SCREEN_WIDTH = 64;
        public const int SCREEN_HEIGHT = 32;
        public const ushort STACK_SIZE = 0x20; //32 bytes
        public const ushort MAIN_MEMOY_SIZE = 0x1000;


        //font information
        private byte[] chip8FontSet = { 0xF0, 0x90, 0x90, 0x90, 0xF0,
                                0x20, 0x60, 0x20, 0x20, 0x70,
                                0xF0, 0x10, 0xF0, 0x80, 0xF0,
                                0xF0, 0x10, 0xF0, 0x10, 0xF0,
                                0x90, 0x90, 0xF0, 0x10, 0x10,
                                0xF0, 0x80, 0xF0, 0x10, 0xF0,
                                0xF0, 0x80, 0xF0, 0x90, 0xF0,
                                0xF0, 0x10, 0x20, 0x40, 0x40,
                                0xF0, 0x90, 0xF0, 0x90, 0xF0,
                                0xF0, 0x90, 0xF0, 0x10, 0xF0,
                                0xF0, 0x90, 0xF0, 0x90, 0x90,
                                0xE0, 0x90, 0xE0, 0x90, 0xE0,
                                0xF0, 0x80, 0x80, 0x80, 0xF0,
                                0xE0, 0x90, 0x90, 0x90, 0xE0,
                                0xF0, 0x80, 0xF0, 0x80, 0xF0,
                                0xF0, 0x80, 0xF0, 0x80, 0x80 };
        //pallete data
        Color[] pallete = new Color[] {
            Color.FromArgb(00,00,33), Color.FromArgb(102,170,51) };

        //CPU context
        private byte[] mainMemory;
        private byte[] vRegs;
        private ushort Ireg;
        private ushort pc;
        private ushort stackPtr;
        private byte delayTimer;
        private byte soundTimer;
        private BackgroundWorker worker;
       
        // input & output
        private byte[] screenBuffer;
        private byte[] blankScreenBuffer = new byte[SCREEN_HEIGHT * SCREEN_WIDTH];
        public int[] gamePad; // state of inputs
        private System.Windows.Controls.Image display;
      

        //rom
        private Game game;
        private string gameToBeLoaded;
        private bool swapGameFlag;
     
        //chip8 flags
        private bool powerStatus = false;
        private bool cpuHalt = false;
        private bool resetFlag = false;
        private bool debugMode = false;
        private bool breakPointFlag = false;

        //breakpoint addresses
        public List<ushort> brkPtAddresses = new List<ushort>();

        //debugger gui interface
        private DebuggerWindow debuggerWindow;

        public Chip8()
        {
            init();
        }

        public bool IsOn()
        {
            return powerStatus;
        }

        public void connectDisplay(System.Windows.Controls.Image display)
        {
            this.display = display;
            if(display != null)
                display.Source = Utli.loadBitmap(getScreenBitmap(screenBuffer));
        }

        public void clearDisplay()
        {
            if(display != null)
                display.Source = Utli.loadBitmap(getScreenBitmap(blankScreenBuffer));
        }
        public bool PowerStatus
        {
            get
            {return powerStatus;}

            set
            {powerStatus = value;}
        }


        public void powerOn()
        {
            if (powerStatus == false)
            {
                powerStatus = true;
                cpuCycle();
            }
        }

        public void powerOff()
        {
            if (powerStatus)
            {
                powerStatus = false;
                worker.CancelAsync();
            }
        }

        
        public void powerReset()
        {
            resetFlag = true;
            powerOff();
        }

        public void swapGame(string game)
        {
            swapGameFlag = true;
            gameToBeLoaded = game;
            worker.CancelAsync();
        }

        public bool isRomLoaded()
        {
            if (!(game.getRom() == null))
                return true;
            else
                return false;
        }

        private void init()
        {
            //init cpu context
            initCpuContext();

            //init i/o
            screenBuffer = new byte[SCREEN_HEIGHT * SCREEN_WIDTH];
            gamePad = new int[0x10]; // state of inputs

            //load font data into memory
            for (int i = FONT_BASE_ADDR; i < FONT_BASE_SIZE; i++)
            {
                mainMemory[i] = chip8FontSet[i];
            }

            game = new Game();
        }

        public List<DecodedInstruction> decodedRom()
        {
            return game.getDecodeRom();
        }

        private void initCpuContext()
        {
            mainMemory = new byte[MAIN_MEMOY_SIZE];
            vRegs = new byte[0x10];
            Ireg = 0;
            pc = ROM_BASE_ADDR;
            stackPtr = STACK_BASE_ADDR;
            delayTimer = 0;
            soundTimer = 0;
        }

        public void loadGame(string romFile)
        {
            try
            {
                game.loadRom(romFile);
                byte[] romBuffer = game.getRom();
                
                for (int i = 0; i < romBuffer.Length; i++)
                {
                    mainMemory[ROM_BASE_ADDR + i] = romBuffer[i];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception information: {0}", ex);
            }
        }

        private void cpuCycle()
        {
            //activate chip8 cpu cycle thread here on first run.
            runChip8Thread();
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
            if(display != null)
                display.Source = Utli.loadBitmap(getScreenBitmap(screenBuffer));
        }


        void workerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                //cpuHalt flag controls the state of the Chip8 when the Cpu loop thread ends
                //If cpuHalt is true, the thread that is currently carrying out the cpu loop will end without altering 
                //the current state of the chip8, thus, if desired, a new Cpu thread can be invoked and start
                //executing from the last state processed by the previous cpu loop thread
                //If false, the chip8 properties are re-initilizing to the state it was in prior to the excution of the rom
                if (!cpuHalt) 
                {
                    String path = game.Path;
                    init();
                    if (swapGameFlag)
                    {
                        swapGameFlag = false;
                        loadGame(gameToBeLoaded);
                        worker = null;
                        cpuCycle();
                    }
                    else
                    {
                        loadGame(path);
                    }
    
                    //exectued if reset() is called to end chip8 simulation
                    if (resetFlag)
                    {
                        resetFlag = false;
                        worker = null;
                        powerOn();
                        debuggerWindow.initRoutine();
                    }
                   
                }

               
                //Set by a breakpoint being flagged which can only occur if the chip8 debugMode variable is set to true
                if(breakPointFlag)
                {
                    breakPointFlag = false;
                    debuggerWindow.breakPointPause();
                    debuggerWindow.BreakPointTrigger = true;
                }

            }
        }

        private bool breakPointCheck()
        {
            //dependent condtion - debugger window open, cpu is being simulated
            if (debugMode && (!cpuHalt))
            {
                for (int j = 0; j < brkPtAddresses.Count(); j++)
                {
                    if (pc == brkPtAddresses[j])
                    {
                        breakPointFlag = true;
                        haltContinousCpuCycle();
                        return true;
                    }
                }
            }
            return false;
        }

        void chip8FrameCycle(object sender)
        {
            long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            //run frame cycle
            for (int i = 0; i < Chip8.INSTRUCTION_PER_FRAME; i++)
            {
                //break point check
                if (breakPointCheck())
                    return;

                cpuStep();
            }

            //update screen and timer
             updateTimers();
             (sender as BackgroundWorker).ReportProgress(0);

            //sleep until milli seconds per frame(17millis) has passed.
            long endTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            int delay = (int)(Chip8.MILLI_SEC_PER_FRAME - (endTime - startTime));
            if (delay > 0)
            {
                System.Threading.Thread.Sleep(delay);
            }
        }


        public void cpuStep()
        {

            //fetch instruction
            ushort opcode = (ushort)((mainMemory[pc] << 8) | mainMemory[pc + 1]);
            pc += 2;

            //decode and execute instruction
            switch (Utli.getUpperByteHighNibble(opcode))
            {
                case 0x0:
                    {
                        if (Utli.getLowerByteHighNibble(opcode) == 0xE)
                        {
                            switch (Utli.getLowerByteLowNibble(opcode))
                            {
                                case 0x0://	Clears the screen.
                                    Array.Clear(screenBuffer, 0, screenBuffer.Length);
                                    //drawflag = true;
                                    break;
                                case 0xE://Returns from a subroutine.
                                    pc = mainMemory[stackPtr];
                                    stackPtr++;
                                    pc = (ushort)((mainMemory[stackPtr] << 8) | pc);
                                    stackPtr++;
                                    break;
                                default:
                                    Console.WriteLine("Unknown Opcode - " + "0x" + opcode.ToString("x2"));
                                    break;
                            }
                        }
                        else
                        {
                            if ((opcode & 0x0FFF) == 0)
                                Console.WriteLine("0NNN - Calls RCA 1802 program at address NNN. Not necessary for most ROMs.");
                            else
                                Console.WriteLine("Unknown Opcode - " + "0x" + opcode.ToString("x2"));

                            pc += 2;
                        }
                    }
                    break;

                case 0x1://Jumps to address NNN.
                    pc = (ushort)(opcode & 0x0FFF);
                    break;

                case 0x2://Calls subroutine at NNN.
                    stackPtr--;
                    mainMemory[stackPtr] = Utli.getHighByte(pc);
                    stackPtr--;
                    mainMemory[stackPtr] = Utli.getLowerByte(pc);

                    pc = (ushort)(opcode & 0x0FFF);
                    break;

                case 0x3://3XNN	Skips the next instruction if VX equals NN.
                    if (vRegs[Utli.getUpperByteLowNibble(opcode)] == Utli.getLowerByte(opcode))
                        pc += 2;
                    break;

                case 0x4://4XNN	Skips the next instruction if VX doesn't equal NN.
                    if (vRegs[Utli.getUpperByteLowNibble(opcode)] != Utli.getLowerByte(opcode))
                        pc += 2;
                    break;

                case 0x5://5XY0	Skips the next instruction if VX equals VY.
                    if (vRegs[Utli.getUpperByteLowNibble(opcode)] == vRegs[Utli.getLowerByteHighNibble(opcode)])
                        pc += 2;
                    break;

                case 0x6://6XNN	Sets VX to NN.
                    vRegs[Utli.getUpperByteLowNibble(opcode)] = Utli.getLowerByte(opcode);
                    break;

                case 0x7://7XNN	Adds NN to VX.
                    vRegs[Utli.getUpperByteLowNibble(opcode)] += Utli.getLowerByte(opcode);
                    break;

                case 0x8:
                    {
                        switch (Utli.getLowerByteLowNibble(opcode))
                        {

                            case 0x0://	8XY0 Sets VX to the value of VY.
                                vRegs[Utli.getUpperByteLowNibble(opcode)] = vRegs[Utli.getLowerByteHighNibble(opcode)];
                                break;

                            case 0x1://	8XY1 Sets VX to VX or VY.
                                vRegs[Utli.getUpperByteLowNibble(opcode)] |= vRegs[Utli.getLowerByteHighNibble(opcode)];
                                break;

                            case 0x2://	8XY2 Sets VX to VX and VY.
                                vRegs[Utli.getUpperByteLowNibble(opcode)] &= vRegs[Utli.getLowerByteHighNibble(opcode)];
                                break;

                            case 0x3://	8XY3 Sets VX to VX xor VY.
                                vRegs[Utli.getUpperByteLowNibble(opcode)] ^= vRegs[Utli.getLowerByteHighNibble(opcode)];
                                break;

                            case 0x4://	8XY4 Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                                {
                                    int carryCheck = vRegs[Utli.getUpperByteLowNibble(opcode)] + vRegs[Utli.getLowerByteHighNibble(opcode)];
                                    if (carryCheck > 0xFF)
                                        vRegs[0xF] = 1;
                                    else
                                        vRegs[0xF] = 0;

                                    vRegs[Utli.getUpperByteLowNibble(opcode)] += vRegs[Utli.getLowerByteHighNibble(opcode)];
                                }
                                break;

                            case 0x5://8XY5	VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                                {
                                    if (vRegs[Utli.getLowerByteHighNibble(opcode)] > vRegs[Utli.getUpperByteLowNibble(opcode)])
                                        vRegs[0xF] = 0;
                                    else
                                        vRegs[0xF] = 1;
                                    vRegs[Utli.getUpperByteLowNibble(opcode)] -= vRegs[Utli.getLowerByteHighNibble(opcode)];
                                }
                                break;

                            case 0x6://8XY6 Shifts VX right by one.
                                     // VF is set to the value of the least significant bit of VX before the shift.
                                {
                                    vRegs[0xF] = (byte)(0x1 & vRegs[Utli.getUpperByteLowNibble(opcode)]);
                                    vRegs[Utli.getUpperByteLowNibble(opcode)] >>= 1;
                                }
                                break;

                            case 0x7://8XY7	Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                                {
                                    if (vRegs[Utli.getUpperByteLowNibble(opcode)] > vRegs[Utli.getLowerByteHighNibble(opcode)])
                                        vRegs[0xF] = 0;
                                    else
                                        vRegs[0xF] = 1;
                                    vRegs[Utli.getUpperByteLowNibble(opcode)] = (byte)(vRegs[Utli.getLowerByteHighNibble(opcode)] - vRegs[Utli.getUpperByteLowNibble(opcode)]);
                                }
                                break;

                            case 0xE://8XYE	Shifts VX left by one. VF is set to the value of the
                                     // most significant bit of VX before the shift.
                                {
                                    vRegs[0xF] = (byte)(vRegs[Utli.getUpperByteLowNibble(opcode)] >> 7);
                                    vRegs[Utli.getUpperByteLowNibble(opcode)] <<= 1;
                                }
                                break;

                            default:
                                Console.WriteLine("Unknown Opcode - " + "0x" + opcode.ToString("x2"));
                                break;
                        }
                    }
                    break;

                case 0x9://9XY0	Skips the next instruction if VX doesn't equal VY..
                    if (vRegs[Utli.getUpperByteLowNibble(opcode)] != vRegs[Utli.getLowerByteHighNibble(opcode)])
                        pc += 2;
                    break;

                case 0xA://ANNN	Sets Ireg to the address NNN.
                    Ireg = (ushort)(opcode & 0x0FFF);
                    break;

                case 0xB://BNNN	Jumps to the address NNN plus V0.
                    pc = (ushort)((opcode & 0x0FFF) + vRegs[0x0]);
                    break;

                case 0xC://CXNN	Sets VX to the result of a bitwise and operation on a random number and NN.
                    Random rand = new Random();
                    byte random = (byte)rand.Next(0x00, 0xFF);////0xFF is the maximum and the 0 is our minimum
                    vRegs[Utli.getUpperByteLowNibble(opcode)] = (byte)(random & Utli.getLowerByte(opcode));
                    break;

                case 0xD:
                    {
                        //DXYN	Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels.
                        // Each row of 8 pixels is read as bit-coded starting from memory location Ireg;
                        // Ireg value doesn’t change after the execution of this instruction.
                        // As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn,
                        // and to 0 if that doesn’t happen
                        int xCor = vRegs[Utli.getUpperByteLowNibble(opcode)];
                        int yCor = vRegs[Utli.getLowerByteHighNibble(opcode)];
                        int nHeight = Utli.getLowerByteLowNibble(opcode);
                        vRegs[0xF] = 0;

                        for (int i = yCor; i < (yCor + nHeight); i++)
                        {
                            Byte pixel = mainMemory[Ireg + (i - yCor)];
                            for (int j = xCor; j < (xCor + 8); j++)
                            {
                                int pixelVal = (0x1 & (pixel >> ((xCor + 7) - j)));

                                if (pixelVal == 0x1)
                                {
                                    if (screenBuffer[(i * SCREEN_WIDTH) + j] == 1)
                                    {
                                        vRegs[0xF] = 1;
                                    }

                                    screenBuffer[(i * SCREEN_WIDTH) + j] ^= 1;
                                }
                            }
                        }

                        //drawflag = true;
                    }
                    break;

                case 0xE:
                    {
                        switch (Utli.getLowerByte(opcode))
                        {
                            case 0x9E://EX9E Skips the next instruction if the key stored in VX is pressed.
                                if (gamePad[vRegs[Utli.getUpperByteLowNibble(opcode)]] == 1)
                                    pc += 2;
                                break;

                            case 0xA1://EXA1 Skips the next instruction if the key stored in VX isn't pressed.
                                if (gamePad[vRegs[Utli.getUpperByteLowNibble(opcode)]] != 1)
                                    pc += 2;
                                break;

                            default:
                                Console.WriteLine("Unknown Opcode - " + "0x" + opcode.ToString("x2"));
                                break;
                        }
                    }
                    break;

                case 0xF:
                    {
                        switch (Utli.getLowerByte(opcode))
                        {
                            case 0x07://FX07	Sets VX to the value of the delay timer.
                                vRegs[Utli.getUpperByteLowNibble(opcode)] = delayTimer;
                                break;

                            case 0x0A://FX0A    A key press is awaited, and then stored in VX.
                                {
                                    bool keyPressed = false;

                                    for (int i = 0; i < gamePad.Length; i++)
                                    {
                                        if (gamePad[i] == 1)
                                        {
                                            keyPressed = true;
                                            i = gamePad.Length + 1;//end loop
                                        }
                                    }

                                    if (!keyPressed)
                                    {
                                        pc -= 2;
                                    }
                                }
                                break;

                            case 0x15://	FX15 Sets the delay timer to VX.
                                delayTimer = vRegs[Utli.getUpperByteLowNibble(opcode)];
                                break;

                            case 0x18://	FX18 Sets the sound timer to VX.
                                soundTimer = vRegs[Utli.getUpperByteLowNibble(opcode)];
                                break;

                            case 0x1E: //FX1E Adds VX to Ireg.
                                Ireg += vRegs[Utli.getUpperByteLowNibble(opcode)];
                                break;

                            case 0x29://FX29 Sets Ireg to the location of the sprite for the character in VX.
                                      // Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                                Ireg = (ushort)(vRegs[Utli.getUpperByteLowNibble(opcode)] * 5);
                                break;

                            case 0x33://FX33 Stores the binary-coded decimal representation of VX, 
                                      //with the most significant of three digits at the address in I, 
                                      //the middle digit at I plus 1, and the least significant digit at I plus 2.
                                mainMemory[Ireg] = (byte)(vRegs[Utli.getUpperByteLowNibble(opcode)] / 100);
                                mainMemory[Ireg + 1] = (byte)((vRegs[Utli.getUpperByteLowNibble(opcode)] / 10) % 10);
                                mainMemory[Ireg + 2] = (byte)((vRegs[Utli.getUpperByteLowNibble(opcode)] % 100) % 10);
                                break;

                            case 0x55://FX55 Stores V0 to VX (including VX) in memory starting at address Ireg.
                                      //I is set to I + X + 1 after operation
                                for (int i = 0; i <= Utli.getUpperByteLowNibble(opcode); i++)
                                {
                                    mainMemory[Ireg + i] = vRegs[i];
                                }
                                Ireg = (ushort)(Ireg + Utli.getUpperByteLowNibble(opcode) + 1);
                                break;

                            case 0x65://FX65 Fills V0 to VX (including VX) with values from memory starting at address Ireg.
                                for (int i = 0; i <= Utli.getUpperByteLowNibble(opcode); i++)
                                {
                                    vRegs[i] = mainMemory[Ireg + i];
                                }
                                Ireg = (ushort)(Ireg + Utli.getUpperByteLowNibble(opcode) + 1);
                                break;
                            default:
                                Console.WriteLine("Unknown Opcode - " + "0x" + opcode.ToString("x2"));
                                break;
                        }
                    }
                    break;

                default:
                    Console.WriteLine("Unimplemented Opcode - " + "0x" + opcode.ToString("x2"));
                    break;
            }
        }

        public void haltContinousCpuCycle()
        {
            cpuHalt = true;
            worker.CancelAsync();
        }

        public void runCpuCycle()
        {
            cpuHalt = false;
            cpuCycle();
        }

        public bool debugStepRoutine()
        {
            cpuStep();
            updateTimers();
            display.Source = Utli.loadBitmap(getScreenBitmap(screenBuffer));

            //break point check
            if (debugMode && (!cpuHalt))
            {
                for (int j = 0; j < brkPtAddresses.Count(); j++)
                {
                    if (pc == brkPtAddresses[j])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool isCpuRunning()
        {
            return !cpuHalt;
        }


        public void setDebuggerInterface(DebuggerWindow w)
        {
            debuggerWindow = w;
        }


        public void saveState()
        {
            //TODO
        }

        public void loadState()
        {
            //TODO
        }


        //TODO
        public void updateTimers()
        {
            // Update timers
            if (delayTimer > 0)
                --delayTimer;

            //Sound timer: This timer is used for sound effects. When its value is nonzero, a beeping sound is made.
            if (soundTimer > 0)
            {
                --soundTimer;
                //Console.WriteLine("BEEP!");
            }
        }




        private Bitmap getScreenBitmap(byte[] _screenBuffer)
        {
            Bitmap bmp = new Bitmap(SCREEN_WIDTH * 2, SCREEN_HEIGHT * 2);
            for (int y = 0; y < SCREEN_HEIGHT; y++)
            {
                for (int x = 0; x < SCREEN_WIDTH; x++)
                {
                    bmp.SetPixel(x * 2, y * 2, pallete[_screenBuffer[(y * SCREEN_WIDTH) + x]]);
                    bmp.SetPixel(x * 2, y * 2 + 1, pallete[_screenBuffer[(y * SCREEN_WIDTH) + x]]);
                    bmp.SetPixel(x * 2 + 1, y * 2, pallete[_screenBuffer[(y * SCREEN_WIDTH) + x]]);
                    bmp.SetPixel(x * 2 + 1, y * 2 + 1, pallete[_screenBuffer[(y * SCREEN_WIDTH) + x]]);
                }

            }

            return bmp;
        }


        #region getter&setter property/methods
        public bool DebugMode
        {
            get { return debugMode; }
            set { debugMode = value; }
        }


        public byte[] Stack
        {
            get
            {
                return getStackValues();
            }
        }

        private byte[] getStackValues()
        {
           if(stackPtr != STACK_BASE_ADDR)
            {
                
                byte[] stackValues = new byte[STACK_BASE_ADDR - stackPtr];
                int j = 0;
                for(int i = (STACK_BASE_ADDR - 1); i >= stackPtr; i--)
                {
                    stackValues[j] = mainMemory[i];
                    j++;
                }
                return stackValues;
            }
            return null;
        }


        public byte[] MainMemory
        {
            get
            {
                return mainMemory;
            }

            set
            {
                mainMemory = value;
            }
        }

        public byte[] VRegs
        {
            get
            {
                return vRegs;
            }

            set
            {
                vRegs = value;
            }
        }

        public ushort Ireg1
        {
            get
            {
                return Ireg;
            }

            set
            {
                Ireg = value;
            }
        }

        public ushort Pc
        {
            get
            {
                return pc;
            }

            set
            {
                pc = value;
            }
        }

        public ushort StackPtr
        {
            get
            {
                return stackPtr;
            }

            set
            {
                stackPtr = value;
            }
        }

        public byte DelayTimer
        {
            get
            {
                return delayTimer;
            }

            set
            {
                delayTimer = value;
            }
        }

        public byte SoundTimer
        {
            get
            {
                return soundTimer;
            }

            set
            {
                soundTimer = value;
            }
        }
        #endregion getter&setter property/methods

    }
}
