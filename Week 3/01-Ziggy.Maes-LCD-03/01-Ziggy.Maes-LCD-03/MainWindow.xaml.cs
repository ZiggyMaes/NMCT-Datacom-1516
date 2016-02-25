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
using USB_DAQBoard;

namespace _01_Ziggy.Maes_LCD_03
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private static System.Timers.Timer displayShift;
        public MainWindow()
        {
            InitializeComponent();
            displayShift = new System.Timers.Timer(350);
            displayShift.AutoReset = true;
            displayShift.Elapsed += ShiftScreen;
        }

        private void ShiftScreen(Object source, System.Timers.ElapsedEventArgs e)
        {
            WriteLCD(0x19, true); //Shift screen left
        }

        private void EHoogInstructie()
        {
            // E = 1 / RW = 0 / RS = 0
            int x = MPUSB.ReadDigitalOutPortD();
            int y = x & 0x8FF;//E, RW en RS op 0 -> & 1000 1111 1111
            int z = y | 0x100;//E op 1 -> | 0001 0000 0000
            MPUSB.WriteDigitalOutPortD((short)z);
        }
        private void ELaagInstructie()
        {
            //E = 0, RS = 0, RW = 0
            int x = MPUSB.ReadDigitalOutPortD();
            int y = x & 0x0FF;//E, RW en RS op 0 -> & 0000 1111 1111
            MPUSB.WriteDigitalOutPortD((short)y);
        }
        private void EHoogData()
        {
            //E = 1, RS = 1, RW = 0
            int x = MPUSB.ReadDigitalOutPortD();
            int y = x & 0x0FF;//E, RW en RS op 0 -> & 0000 1111 1111
            int z = y | 0x500;//E op 1, RS op 1 -> | 0101 0000 0000
            MPUSB.WriteDigitalOutPortD((short)z);
        }
        private void ELaagData()
        {
            //E = 0, RS = 1, RW = 0
            int x = MPUSB.ReadDigitalOutPortD();
            int y = x & 0x8FF;//E, RW en RS op 0 -> & 1000 1111 1111
            int z = y | 0x400;//E op 0, RS op 1 -> | 0100 0000 0000
            MPUSB.WriteDigitalOutPortD((short)z);
        }
        private bool WriteLCD(byte data, bool instruction)
        {
            if (data > 0xFF) return false;

            if (instruction) EHoogInstructie();
            else EHoogData();

            int x = MPUSB.ReadDigitalOutPortD();
            int y = x & 0xF00;//datalijnen op 0 -> 1111 0000 0000
            int z = y | data;

            MPUSB.WriteDigitalOutPortD((short)z);

            if (instruction) ELaagInstructie();
            else ELaagData();

            MPUSB.Wait(1);

            return true;
        }

        private void WriteText(string data)
        {
            byte[] ascii = Encoding.ASCII.GetBytes(data);
            int i = 0;

            if (data.Length < 32)
            {    
                foreach (byte b in ascii)
                {
                    if (i == 16) WriteLCD(0x80 | 0x40, true);//Next line
                    WriteLCD(b, false);
                    i++;
                }
            }
            else
            {
                foreach (byte b in ascii)
                {
                    if (i == data.Length/2) WriteLCD(0x80 | 0x40, true);//Next line
                    WriteLCD(b, false);
                    i++;
                }
                displayShift.Enabled = true;
            }
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            MPUSB.OpenMPUSBDevice();
            if(rad8bit.IsChecked == true) WriteLCD(0x38, true);//Function set, 8b, 2L, 5x7 -> 0011 1000
            else WriteLCD(0x28, true);//Function set, 4b, 2L, 5x7 -> 0010 1000

            WriteLCD(0x02, true);//Cursor home
            WriteLCD(0x0F, true); //Display on, blink
        }

        private void btnWriteLine_Click(object sender, RoutedEventArgs e)
        {
            displayShift.Enabled = false;
            WriteLCD(0x01, true); //reset display + cursor home
            WriteLCD(0x02, true); //reset display + cursor home

            if (txtinput.Text.Length > 80) WriteText("Char overflow!");
            else WriteText(txtinput.Text);
        }
    }
}
