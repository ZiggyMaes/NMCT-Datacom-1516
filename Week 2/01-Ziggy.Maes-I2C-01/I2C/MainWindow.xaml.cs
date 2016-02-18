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

namespace I2C
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MPUSB.OpenMPUSBDevice();
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            StartConditie();
            IC();
            getAck();

            for (int i=0;i<8;i++)
            {
                ZendNul();
            }
            getAck();
            StopConditie();
        }
        private void IC()
        {
            //0I00 0000
            ZendNul();
            ZendEen();
            ZendNul();
            ZendNul();

            ZendNul();
            ZendNul();
            ZendNul();
            ZendNul();
        }

        private void StartConditie()
        {
            MPUSB.WriteDigitalOutPortD(3);
            MPUSB.Wait(1);
            MPUSB.WriteDigitalOutPortD(2);
        }

        private void StopConditie()
        {
            MPUSB.WriteDigitalOutPortD(2);
            MPUSB.Wait(1);
            MPUSB.WriteDigitalOutPortD(3);
        }
        private void ZendNul()
        {
            MPUSB.WriteDigitalOutPortD(0);
            MPUSB.WriteDigitalOutPortD(2);
            MPUSB.Wait(1);
            MPUSB.WriteDigitalOutPortD(0);
        }
        private void ZendEen()
        {
            MPUSB.WriteDigitalOutPortD(0);
            MPUSB.WriteDigitalOutPortD(3);
            MPUSB.Wait(1);
            MPUSB.WriteDigitalOutPortD(0);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            StartConditie();
            IC();
            getAck();
            displayCipher(Convert.ToInt16(txtInput.Text));
            getAck();
            StopConditie();
        }
        private void displayCipher(int cipher)
        {
            byte dataCipher = 0;

            switch (cipher)
            {
                case 0:
                    dataCipher = 0x21;
                    break;
                case 1:
                    dataCipher = 0xbd;
                    break;
                case 2:
                    dataCipher = 0x13;
                    break;
                case 3:
                    dataCipher = 0x19;
                    break;
                case 4:
                    dataCipher = 0x8d;
                    break;
                case 5:
                    dataCipher = 0x49;
                    break;
                case 6:
                    dataCipher = 0x41;
                    break;
                case 7:
                    dataCipher = 0x40;
                    break;
                case 8:
                    dataCipher = 0x1;
                    break;
                case 9:
                    dataCipher = 0x9;
                    break;
            }

            SendByte(dataCipher);

            //data = Convert.ToString(dataCipher, 2).PadLeft(8, '0');

            //foreach (char c in data)
            //    {
            //        if (c == '0') ZendNul();
            //        else if (c == '1') ZendEen();
            //    }
        }
        private void getAck()
        {
            MPUSB.WriteDigitalOutPortD(1);
            MPUSB.Wait(1);
            MPUSB.WriteDigitalOutPortD(3);
            MPUSB.Wait(1);

            byte data = MPUSB.ReadDigitalInPortB();

            bool ack = (byte)(data & 32) != 0;

            if (ack) lblStatus.Content = "Ack!";
            else lblStatus.Content = "No Ack!!";

        }

        private void SendByte(byte data)
        {
            bool bit0 = (byte)(data & 1) == 0;
            bool bit1 = (byte)(data & 2) == 0;
            bool bit2 = (byte)(data & 4) == 0;
            bool bit3 = (byte)(data & 8) == 0;
            bool bit4 = (byte)(data & 16) == 0;
            bool bit5 = (byte)(data & 32) == 0;
            bool bit6 = (byte)(data & 64) == 0;
            bool bit7 = (byte)(data & 128) == 0;

            //Meest significante bit eerst
            StuurEenOfNul(bit7);
            StuurEenOfNul(bit6);
            StuurEenOfNul(bit5);
            StuurEenOfNul(bit4);
            StuurEenOfNul(bit3);
            StuurEenOfNul(bit2);
            StuurEenOfNul(bit1);
            StuurEenOfNul(bit0);
        }

        private void StuurEenOfNul(bool zendNul)
        {
            if (zendNul) ZendNul(); else ZendEen();
        }
    }
}
