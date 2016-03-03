using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace _01_Ziggy.Maes_Analoog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker bw = new BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
            MPUSB.OpenMPUSBDevice();

            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;

            bw.DoWork += Bw_DoWork;
            bw.ProgressChanged += Bw_ProgressChanged;

            bw.RunWorkerAsync();

            pgbIngang1.Value = Convert.ToInt16(MPUSB.ReadAnalogIn(1));
            pgbIngang2.Value = Convert.ToInt16(MPUSB.ReadAnalogIn(0));
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            AnalogOutput AnalogData = (AnalogOutput)e.UserState;

            pgbIngang1.Value = AnalogData.Potent1;
            pgbIngang2.Value = AnalogData.Potent2;
            pgbLight.Value = AnalogData.LightSensor;


            double test = (double)AnalogData.LightSensor / 1024 * 256;

            MPUSB.WriteDigitalOutPortD((short)test);
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            AnalogOutput data = new AnalogOutput();

            do
            {
                System.Threading.Thread.Sleep(500);

                data.Potent1 = MPUSB.ReadAnalogIn(1);
                data.Potent2 = MPUSB.ReadAnalogIn(0);
                data.LightSensor = MPUSB.ReadAnalogIn(3);

                bw.ReportProgress(100, data);
                
            }
            while (!bw.CancellationPending);

            e.Cancel = true;
        }
    }
}
