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
using System.Windows.Threading;
using USB_DAQBoard;

namespace _2NMCT7_Ziggy.Maes__digitaalInOut_01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
        }
        private void btnOpen(object sender, RoutedEventArgs e)
        {
            if (MPUSB.OpenMPUSBDevice() == 0) txtVersie.Text = "Connected";
            else if (MPUSB.OpenMPUSBDevice() == -1)
            {
                txtVersie.Text = "Connection failed";
                return;
            }
            DispatcherTimer UITimer = new DispatcherTimer();
            UITimer.Interval = TimeSpan.FromMilliseconds(100);
            UITimer.Tick += UITimer_Tick;
            UITimer.IsEnabled = true;

            DispatcherTimer ButtonTimer = new DispatcherTimer();
            ButtonTimer.Interval = TimeSpan.FromMilliseconds(100);
            ButtonTimer.Tick += ButtonTimer_Tick;
            ButtonTimer.IsEnabled = true;

            DispatcherTimer InputBTimer = new DispatcherTimer();
            InputBTimer.Interval = TimeSpan.FromMilliseconds(1);
            InputBTimer.Tick += InputBTimer_Tick;
            InputBTimer.IsEnabled = true;
        }
        private void btnClose(object sender, RoutedEventArgs e)
        {
            //MPUSB.CloseMPUSBDevice();
        }

        private void UpdateUI()
        {
            int value = MPUSB.ReadDigitalOutPortD();
            string binary = Convert.ToString(value, 2).PadLeft(8, '0');
            int l = binary.Length;

            for (int i = 0; i < l; i++)
            {
                CheckBox chk = this.stpChk.Children[i] as CheckBox;
                chk.Tag = i;
                var b = binary.Substring(7 - i, 1);

                chk.Checked -= chkChanged;
                chk.Unchecked -= chkChanged;

                if (b == "1")
                    chk.IsChecked = true;
                else
                    chk.IsChecked = false;

                chk.Checked += chkChanged;
                chk.Unchecked += chkChanged;
            }
        }
        void ButtonTimer_Tick(object sender, EventArgs e)
        {
            var b = MPUSB.ReadDigitalInPortB();

            
            string binary = Convert.ToString(b, 2);
            txtVersie.Text = binary.ToString();
            string button1 = binary.Substring(binary.Length - 1, 1);
            string button2 = binary.Substring(binary.Length - 2, 1);
            int val1 = Convert.ToInt32(button1);
            int val2 = Convert.ToInt32(button2);

        }

        void UITimer_Tick(object sender, EventArgs e)
        {
            UpdateUI();
        }

        string oldTurnValue = null;
        void InputBTimer_Tick(object sender, EventArgs e)
        {
            byte value = MPUSB.ReadDigitalInPortB();

            string pValue = Convert.ToString(value, 2).Substring(4, 1);
            bool isButtonPressed = false;
            if (pValue == "0") isButtonPressed = true;

            //txtInputB.Text = Convert.ToString(value, 2).PadLeft(8,'0');

            string turnValue = Convert.ToString(value, 2).PadLeft(8, '0').Substring(0, 2);
            if (turnValue != oldTurnValue)
            {
                if (oldTurnValue == "11")
                    if (turnValue == "10")
                    {
                        txtInputB.Text = "links";
                        if (isButtonPressed)
                            MoveLedsDown();
                    }
                    else if (turnValue == "01")
                    {
                        txtInputB.Text = "rechts";
                        if (isButtonPressed)
                            MoveLedsUp();
                    }
                    else if (oldTurnValue == "00")
                        if (turnValue == "10")
                        {
                            txtInputB.Text = "rechts";
                            if (isButtonPressed)
                                MoveLedsDown();
                        }
                        else if (turnValue == "01")
                        {
                            txtInputB.Text = "links";
                            if (isButtonPressed)
                                MoveLedsUp();
                        }

                oldTurnValue = turnValue;
            }
        }

        private void btnVersie(object sender, RoutedEventArgs e)
        {
            txtVersie.Text = MPUSB.GetVersion();
        }

        private void btnLedsAan(object sender, RoutedEventArgs e)
        {
            MPUSB.WriteDigitalOutPortD(255);
            UpdateUI();
        }

        private void btnLedsUit(object sender, RoutedEventArgs e)
        {
            MPUSB.WriteDigitalOutPortD(0);
            UpdateUI();
        }

        private void MoveLedsUp()
        {
            var value = (short)MPUSB.ReadDigitalOutPortD();
            short b128 = 128;
            var ish = value & b128;
            if (ish != 0)
                value -= 128;
            var newv = value << 1;
            if (ish != 0)
                newv += 1;
            MPUSB.WriteDigitalOutPortD((short)newv);
            Console.WriteLine(MPUSB.ReadDigitalOutPortD());
        }

        private void MoveLedsDown()
        {
            var value = (short)MPUSB.ReadDigitalOutPortD();
            var newv = value >> 1;
            if (value % 2 == 1)
            {
                newv += 128;
            }

            MPUSB.WriteDigitalOutPortD((short)newv);
        }

        private void btnOmhoog(object sender, RoutedEventArgs e)
        {
            MoveLedsUp();
        }

        private void btnOmlaag(object sender, RoutedEventArgs e)
        {
            MoveLedsDown();
        }

        private void btnTeller(object sender, RoutedEventArgs e)
        {
            for (short i = 0; i <= 255; i++)
            {
                MPUSB.WriteDigitalOutPortD(i);
            }
        }


        private void chkChanged(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            int current = MPUSB.ReadDigitalOutPortD();
            string currentString = Convert.ToString(current, 2).PadLeft(8, '0');

            int change = (int)Math.Pow(2, (int)chk.Tag);
            string changeString = Convert.ToString(change, 2).PadLeft(8, '0');

            int newValue = current ^ change;
            MPUSB.WriteDigitalOutPortD((short)newValue);
        }
    }
}
