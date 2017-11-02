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

    public partial class HexEditor : UserControl
    {
        private const double BYTE_PER_LINE = 16;
        private const double ROW_HEIGHT = 22;
        private const double MAX_LINE = Chip8.MAIN_MEMOY_SIZE / 16;
        private double SCROLL_LARGE_CHANGE = 25;

        //tells which range of rows are in focus within the hexeditor
        private int pos1;
        private int pos2;

        private int startSelection;
        private int endSelection;
        private bool selectionFlag;
        private bool[] selectedHexItems;

        private Chip8 chip8;
        internal Chip8 Chip8
        {
            set
            {
                chip8 = value;
            }
        }

        public HexEditor()
        {
            InitializeComponent();
            chip8 = new Chip8();

            this.Measure(new Size(Width, Height));
            this.Arrange(new Rect(0, 0, this.DesiredSize.Width, this.DesiredSize.Height));

            pos1 = 0;
            pos2 = (int)getMaxVisibleLine();

            initVeticalScrollBar();
            initializeHexEditor();

            startSelection = -1;
            endSelection = -1;
            selectionFlag = false;
            selectedHexItems = new bool[Chip8.MAIN_MEMOY_SIZE];
        }

        #region Scrolling&Line selection property/methods

        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrollBarUpdate();
            //selectedHexItems = new bool[Chip8.MAIN_MEMOY_SIZE];
            updateByteSelectionUI();
        }

        private void scrollBarUpdate()
        {
            pos1 = (int)VerticalScrollBar.Value;
            updateHexEditor();
        }

        private long getMaxVisibleLine()
        {
           return ((long)(StringDataStackPanel.ActualHeight / ROW_HEIGHT) - 4);
        }

        private int getBaseVisibleIndex()
        {
            return pos1 * (int)BYTE_PER_LINE;
        }

        public void initVeticalScrollBar()
        {

            VerticalScrollBar.Maximum = MAX_LINE - getMaxVisibleLine();
            VerticalScrollBar.SmallChange = 1;
            VerticalScrollBar.LargeChange = SCROLL_LARGE_CHANGE; //25

        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            // If the mouse wheel delta is positive, move the box up.
            if (e.Delta > 0)
            {
                VerticalScrollBar.Value--;
            }

            // If the mouse wheel delta is negative, move the box down.
            if (e.Delta < 0)
            {
                VerticalScrollBar.Value++;

            }
            scrollBarUpdate();
        }

        #endregion Scrolling&Line selection property/methods

        #region Hex/Ascii Character tables property/methods

        /// <summary>
        /// Initialize Hex Editor controls and tables
        /// </summary>

        private void initializeHexEditor()
        {
            loadHexHeader();
            loadData();
            loadStringData();
            loadDataOffset();
        }

        public void loadStringData()
        {
            StringDataStackPanel.Children.Clear();

            for (int i = 0; i < (int)getMaxVisibleLine(); i++)
            {
                StackPanel dataLineStack = new StackPanel();
                dataLineStack.Height = ROW_HEIGHT;
                dataLineStack.Orientation = Orientation.Horizontal;

                for (int j = 0; j < BYTE_PER_LINE; j++)
                {
                    //
                    AsciiControl asciiVal = new AsciiControl();
                    asciiVal.Tag = (i * 16) + j;
                    asciiVal.MouseRightButtonDown += new MouseButtonEventHandler(mouseRightDownHandler);
                    asciiVal.MouseWheel += new MouseWheelEventHandler(MouseWheelHandler);
                    asciiVal.AsciiVal = chip8.MainMemory[(i * 16) + j];
                    asciiVal.MouseEnter += new MouseEventHandler(mouseEnterEventHandler);
                    asciiVal.MouseLeave += new MouseEventHandler(mouseLeaveEventHandler);
                    asciiVal.MouseLeftButtonDown += new MouseButtonEventHandler(mouseLeftDownHandler);
                    asciiVal.MouseLeftButtonUp += new MouseButtonEventHandler(mouseLeftUpHandler);
                    asciiVal.HexCopy.Click += new RoutedEventHandler(hexCopyHandler);
                    asciiVal.AsciiCopy.Click += new RoutedEventHandler(AsciiCopyHandler);
                    dataLineStack.Children.Add(asciiVal);
                }

                StringDataStackPanel.Children.Add(dataLineStack);
            }
        }

        public void loadDataOffset()
        {
            RamIndexStackPanel.Children.Clear();

            for (int i = 0; i < MAX_LINE; i++)
            {
                //Create control
                long max = getMaxVisibleLine();
                Label offsetVal = new Label();
                offsetVal.Height = ROW_HEIGHT;
                offsetVal.MouseWheel += new MouseWheelEventHandler(MouseWheelHandler);
                offsetVal.Padding = new Thickness(0, 0, 10, 0);
                offsetVal.Foreground = Brushes.Gray;
                offsetVal.HorizontalContentAlignment = HorizontalAlignment.Right;
                offsetVal.VerticalContentAlignment = VerticalAlignment.Center;
                offsetVal.Content = "0x" + (i * 16).ToString("X").PadLeft(8, '0');
                RamIndexStackPanel.Children.Add(offsetVal);
            }

        }

      

        public void loadHexHeader()
        {
            HexHeaderStackPanel.Children.Clear();

            for (int i = 0; i < BYTE_PER_LINE; i++)
            {
                //Create control
                Label byteColumns = new Label();
                byteColumns.Height = ROW_HEIGHT;
                byteColumns.Padding = new Thickness(0, 0, 10, 0);
                byteColumns.Foreground = Brushes.Gray;
                byteColumns.Width = 25;
                byteColumns.HorizontalContentAlignment = HorizontalAlignment.Right;
                byteColumns.VerticalContentAlignment = VerticalAlignment.Center;
                byteColumns.Content = "0" + i.ToString("X");
                byteColumns.ToolTip = $"Column : {i.ToString()}";

                HexHeaderStackPanel.Children.Add(byteColumns);
            }
        }

        public void loadData()
        {
            DataStackPanel.Children.Clear();

            for(int i = 0; i < (int)getMaxVisibleLine(); i++)
            {
                StackPanel dataLineStack = new StackPanel();
                dataLineStack.Height = ROW_HEIGHT;
                dataLineStack.Orientation = Orientation.Horizontal;

                for (int j = 0; j < BYTE_PER_LINE; j++)
                {
                    ByteControl byteVal = new ByteControl();
                    byteVal.Tag = (i * 16) + j;
                   // byteVal.Address = 
                    byteVal.ByteVal = chip8.MainMemory[(i * 16) + j];
                    byteVal.MouseWheel += new MouseWheelEventHandler(MouseWheelHandler);
                    //byteVal.FocusableChanged += new DependencyPropertyChangedEventHandler(focusableChanged);
                    byteVal.MouseEnter += new MouseEventHandler(mouseEnterEventHandler);
                    byteVal.MouseLeave += new MouseEventHandler(mouseLeaveEventHandler);
                    byteVal.MouseRightButtonDown += new MouseButtonEventHandler(mouseRightDownHandler);
                    byteVal.MouseLeftButtonDown += new MouseButtonEventHandler(mouseLeftDownHandler);
                    byteVal.MouseLeftButtonUp += new MouseButtonEventHandler(mouseLeftUpHandler);
                    byteVal.HexCopy.Click += new RoutedEventHandler(hexCopyHandler);
                    byteVal.AsciiCopy.Click += new RoutedEventHandler(AsciiCopyHandler);
                    dataLineStack.Children.Add(byteVal);
                }

                DataStackPanel.Children.Add(dataLineStack);
            }
        }

        /// <summary>
        /// Update Hex Editor Ascii, Hex, and Offset tables
        /// </summary>

        public void updateHexEditor()
        {
            //initializeHexEditor();
            for (int i = 0; i < StringDataStackPanel.Children.Count; i++)
            {
                StackPanel dataLineStack = (StackPanel)StringDataStackPanel.Children[i];
                for (int j = 0; j < dataLineStack.Children.Count; j++)
                {
                    AsciiControl asciiVal = (AsciiControl)dataLineStack.Children[j];
                    asciiVal.AsciiVal = chip8.MainMemory[j + ((i + pos1) * (int)BYTE_PER_LINE)];
                }
            }

            for (int i = 0; i < DataStackPanel.Children.Count; i++)
            {
                StackPanel dataLineStack = (StackPanel)DataStackPanel.Children[i];
                for (int j = 0; j < dataLineStack.Children.Count; j++)
                {
                    ByteControl val = (ByteControl)dataLineStack.Children[j];
                    val.ByteVal = chip8.MainMemory[j + ((i + pos1) * (int)BYTE_PER_LINE)];
                }
            }

            for (int i = 0; i < RamIndexStackPanel.Children.Count; i++)
            {
                Label val = (Label)RamIndexStackPanel.Children[i];
                int index = (pos1 + i) * (int)BYTE_PER_LINE;
                val.Content = "0x" + (index).ToString("X").PadLeft(8, '0');
            }
        }



        private ByteControl getByteControl(int index)
        {
            for (int i = 0; i < DataStackPanel.Children.Count; i++)
            {
                StackPanel dataLineStack = (StackPanel)DataStackPanel.Children[i];
                for (int j = 0; j < dataLineStack.Children.Count; j++)
                {
                    ByteControl item = (ByteControl)dataLineStack.Children[j];
                    if((int)item.Tag == index)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        private AsciiControl getAsciiControl(int index)
        {
            for (int i = 0; i < StringDataStackPanel.Children.Count; i++)
            {
                StackPanel dataLineStack = (StackPanel)StringDataStackPanel.Children[i];
                for (int j = 0; j < dataLineStack.Children.Count; j++)
                {
                    AsciiControl item = (AsciiControl) dataLineStack.Children[j];
                    if ((int)item.Tag == index)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        #endregion Hex/Ascii Character tables property/methods

        #region Hex Editor copy property/methods

        private void mouseRightDownHandler(object sender, MouseEventArgs e)
        {
            ByteControl byteItem = sender as ByteControl;
            AsciiControl asciiItem = sender as AsciiControl;

            if (byteItem != null)
            {
                if (byteItem.getControlState() == 2)
                {
                    byteItem.HexCopy.IsEnabled = true;
                    byteItem.AsciiCopy.IsEnabled = true;
                }
            }
            else
            {
                if (asciiItem.getControlState() == 2)
                {
                    asciiItem.HexCopy.IsEnabled = true;
                    asciiItem.AsciiCopy.IsEnabled = true;
                }
            }
        }

        private void hexCopyHandler(object sender, EventArgs e)
        {
            copyRoutine(false);
        }

        private void AsciiCopyHandler(object sender, EventArgs e)
        {
            copyRoutine(true);
        }

        private void copyRoutine(bool formatFlag)
        {
            string val = "";

            if (startSelection < endSelection)
                val = getStringVal(startSelection, endSelection, formatFlag);
            else
                val = getStringVal(endSelection, startSelection, formatFlag);

            Clipboard.SetText(val);
        }

        private string getStringVal(int start, int end, bool formatFlag)
        {
            string val = "";
            for (int i = 0; i < selectedHexItems.Length; i++)
            {
                if (i >= start && i <= end)
                    if(formatFlag)
                        val = val + asciiFormatting((char)chip8.MainMemory[i]);
                    else
                        val = val + chip8.MainMemory[i].ToString("x2").ToUpper();
            }
            return val;
        }

        public static char asciiFormatting(char val)
        {
            if (val >= 33 && val < 126)
            {
                return (char)val;
            }
            else
            {
                return '.';
            }
        }
        #endregion Hex Editor copy property/methods

        #region Hex editor selection

        private void mouseLeftDownHandler(object sender, MouseEventArgs e)
        {
            selectedHexItems = new bool[Chip8.MAIN_MEMOY_SIZE];
            selectionFlag = true;
            ByteControl byteItem = sender as ByteControl;
            AsciiControl asciiItem = sender as AsciiControl;

            if (asciiItem != null)
            {
                //asciiItem.Background = background;
                //asciiItem.TextColor(foreground);
                startSelection = getBaseVisibleIndex() + (int)asciiItem.Tag;
                selectedHexItems[startSelection] = true;
                updateByteSelectionUI();
            }
            else
            {
                startSelection = getBaseVisibleIndex() + (int)byteItem.Tag;
                selectedHexItems[startSelection] = true;
                updateByteSelectionUI();

            }
        }

        private void mouseLeftUpHandler(object sender, MouseEventArgs e)
        {
            selectionFlag = false;
        }

        private void mouseEnterEventHandler(object sender, MouseEventArgs e)
        {

            ByteControl byteItem= sender as ByteControl;
            AsciiControl asciiItem = sender as AsciiControl;

            if (selectionFlag) // indicates that the user is dragging the mouse pointer to select more items in the editor
            {
             
                if (asciiItem != null)
                {
                    endSelection = getBaseVisibleIndex() + (int)asciiItem.Tag;
                    updateSelection();
                }
                else
                {
                    endSelection = getBaseVisibleIndex() + (int)byteItem.Tag;
                    updateSelection();
                }
            }
            else
            {
                //auto highlight items when mouse is hovering over them
                if (asciiItem != null)
                {
                    if (asciiItem.Background != Brushes.Blue)
                    {

                        asciiItem.highlightedState();
                    }
                }
                else
                {
                    if (byteItem.getControlState() != 2)
                    {
                        byteItem.highlightedState();
                    }
                }
            }

        }

        private void mouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            ByteControl byteItem = sender as ByteControl;
            AsciiControl asciiItem = sender as AsciiControl;

            if (asciiItem != null)
            {
                //if (asciiItem.Background != Brushes.Blue) { }
                //asciiItem.Background = Brushes.White;
                if (asciiItem.getControlState() == 1)
                    asciiItem.defaultState();
            }
            else
            {
                if (byteItem.getControlState() == 1)
                    byteItem.defaultState();
            }
        }

        private void updateSelection()
        {
            if (startSelection < endSelection)
                setByteSelection(startSelection, endSelection);
            else
                setByteSelection(endSelection, startSelection);
        }

        private void setByteSelection(int start, int end)
        {
            for (int i = 0; i < selectedHexItems.Length; i++)
            {
                if (i >= start && i <= end)
                    selectedHexItems[i] = true;
                else
                    selectedHexItems[i] = false;
            }
            updateByteSelectionUI();
        }



        private void updateByteSelectionUI()
        {
            for (int i = 0; i < DataStackPanel.Children.Count; i++)
            {
                StackPanel dataLineStack = (StackPanel)DataStackPanel.Children[i];
                for (int j = 0; j < dataLineStack.Children.Count; j++)
                {
                    ByteControl item = (ByteControl)dataLineStack.Children[j];
                    int index = ((getBaseVisibleIndex() + j) + (i * (int)BYTE_PER_LINE));
                    if (selectedHexItems[index] == true)
                    {
                        if (item.getControlState() != 2)
                        {
                            item.selectedState();
                            AsciiControl testItem = getAsciiControl((int)item.Tag);
                            testItem.selectedState();
                        }
                    }
                    else
                    {
                        item.defaultState();
                        getAsciiControl((int)item.Tag).defaultState();
                    }
                }
            }
        }
        #endregion Hex editor selection
    }
}
