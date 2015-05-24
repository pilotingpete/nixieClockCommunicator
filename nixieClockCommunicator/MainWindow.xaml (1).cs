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


using System.Drawing;


using System.IO.Ports;
using System.IO;

namespace serialLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        #region globals

        string stringToWrite = "";
        DateTime aMoment;

        string indata = "";
        string outData = "";

        SerialPort currentPort;

        

    #endregion


        public MainWindow()
        {
            InitializeComponent();

           // txtSerialOutHistor = true;

            string[] serialPorts = SerialPort.GetPortNames();

            foreach (string port in serialPorts)
            {
                cboComPort.Items.Add(port);
            }

            cboBaudRate.Items.Add("9600");
            cboBaudRate.Items.Add("28800");
            cboBaudRate.Items.Add("57600");
            cboBaudRate.Items.Add("115200");
            cboBaudRate.SelectedIndex = 2;

            chkEnableDatalog.IsChecked = true;

        }






        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            if (currentPort != null && currentPort.IsOpen)
            {

                currentPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                currentPort.Close();
            }
            Close();
        }



        private void btnStartSerial_Click(object sender, RoutedEventArgs e)
        {
            if (cboComPort.Text != "")
            {
                string port = cboComPort.Text;
                currentPort = new SerialPort(port, int.Parse(cboBaudRate.Text));
                currentPort.DtrEnable = true;    // Data-terminal-ready
                currentPort.RtsEnable = true;    // Request-to-send
                currentPort.Parity = Parity.None;
                currentPort.DataBits = 8;
                

                currentPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                currentPort.Open();
                currentPort.NewLine = "\r"; // Standard newline "\r\n"?
            }
            else
            {
                MessageBox.Show("Please select a COM port first.");
            }
        }

        private void btnSetTime_Click(object sender, RoutedEventArgs e)
        {
            

            while (DateTime.Now.Millisecond > 10) { }

            DateTime moment = DateTime.Now;

            // Send the new time only if we arent going to tick over to a new minute 
            // before the sending is omplete
            if (moment.Second < 55)
            {
                stringToWrite = "seconds=" + moment.Second + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
                System.Threading.Thread.Sleep(200);

                stringToWrite = "minutes=" + moment.Minute + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
                System.Threading.Thread.Sleep(200);

                stringToWrite = "hours=" + moment.Hour + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
            else 
            {
                MessageBox.Show("Please wait for seconds < 55.");
            }

            
        }




        private void btnSetDate_Click(object sender, RoutedEventArgs e)
        {
            DateTime aMoment = DateTime.Now;

            stringToWrite = "day=" + aMoment.Day + "\r";
            txtSerialOutHistory.AppendText(">" + stringToWrite);
            currentPort.WriteLine(stringToWrite);
 
            System.Threading.Thread.Sleep(200);

            stringToWrite = "month=" + aMoment.Month + "\r";
            txtSerialOutHistory.AppendText(">" + stringToWrite);
            currentPort.WriteLine(stringToWrite);

            System.Threading.Thread.Sleep(200);

            stringToWrite = "year=" + aMoment.Year + "\r";
            txtSerialOutHistory.AppendText(">" + stringToWrite);
            currentPort.WriteLine(stringToWrite);

        }



        //private delegate void UpdateUiTextDelegate(string text);
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

            indata = currentPort.ReadLine();
            // Because I am updating the UI from a thread other than the main thread.
            this.Dispatcher.Invoke((Action)(() =>
            {
                DisplayText();
            }));
        }

        //object sender, EventArgs e
        private void DisplayText()
        {
            aMoment = DateTime.Now;

            outData = aMoment.ToString("yyyy-MM-dd, HH':'mm':'ss.fff, ") + indata + Environment.NewLine;


            if (chkEnableDatalog.IsChecked == true)
                File.AppendAllText(@txtDatalogPath.Text, outData);

            txtSerialIn.Text = indata;
            txtDataLogLine.Text = outData;

            //if (!(indata == "\n"))
            //{
                txtSerialOutHistory.AppendText("<" + indata);
            //}
        }


        private void btnSendMsCal_Click(object sender, RoutedEventArgs e)
        {
            stringToWrite = "mscal=" + txtMsCal.Text + "\r";
            txtSerialOutHistory.AppendText(">" + stringToWrite);
            currentPort.WriteLine(stringToWrite);


            if( chkXtalIsFast.IsChecked == true )
            {
                stringToWrite = "xtalisfast=1" + "\r";
                txtSerialOutHistory.AppendText(">" + stringToWrite);
                currentPort.WriteLine(stringToWrite);
            }
            else
            {
                stringToWrite = "xtalisfast=0" + "\r";
                txtSerialOutHistory.AppendText(">" + stringToWrite);
                currentPort.WriteLine(stringToWrite);
            }
        }



        private void btnSendCustomText_Click(object sender, RoutedEventArgs e)
        {

            stringToWrite = txtCustomMessage.Text + "\r";
            currentPort.WriteLine(stringToWrite);

            txtSerialOutHistory.AppendText(">" + stringToWrite);
            //System.Threading.Thread.Sleep(1000);

        }

        private void btnSendThermScale_Click(object sender, RoutedEventArgs e)
        {
            if( celsius.IsChecked == true )
            {
                stringToWrite = "celsius=1" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
            else if( fahrenheit.IsChecked == true )
            {
                stringToWrite = "celsius=0" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if( radio12Hr.IsChecked == true )
            {
                stringToWrite = "miltime=0" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
            else if( radio24Hr.IsChecked == true )
            {
                stringToWrite = "miltime=1" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
        }

        private void btnSendDateTherm_Click(object sender, RoutedEventArgs e)
        {
            if ( chkShowDate.IsChecked == true)
            {
                stringToWrite = "showdate=1" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
            else 
            {
                stringToWrite = "showdate=0" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }

            System.Threading.Thread.Sleep(200);

            if (chkShowTherm.IsChecked == true)
            {
                stringToWrite = "showtherm=1" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
            else
            {
                stringToWrite = "showtherm=0" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }

        }

        private void btnSendSleepStartEnd_Click(object sender, RoutedEventArgs e)
        {
            stringToWrite = "nixiesleepstart=" + txtNixieSleepStart.Text + "\r";
            txtSerialOutHistory.AppendText(">" + stringToWrite);
            currentPort.WriteLine(stringToWrite);

            System.Threading.Thread.Sleep(200);

            stringToWrite = "nixiesleepend=" + txtNixieSleepEnd.Text + "\r";
            txtSerialOutHistory.AppendText(">" + stringToWrite);
            currentPort.WriteLine(stringToWrite);
        }

        private void btnCommitToEeprom_Click(object sender, RoutedEventArgs e)
        {
            stringToWrite = "ramtoeeprom" + "\r";
            txtSerialOutHistory.AppendText(">" + stringToWrite);
            currentPort.WriteLine(stringToWrite);
        }





        private void btnReadFromClock_Click(object sender, RoutedEventArgs e)
        {
            bool initialCheckboxState = false;


            // Send serial to prompt clock for parameters
            stringToWrite = "getall" + "\r";
            txtSerialOutHistory.AppendText(">" + stringToWrite);
            currentPort.WriteLine(stringToWrite);

            // Wait for the serial reply.
            //System.Threading.Thread.Sleep(400);

            
            // Turn off datalogging while we receive the clock parameters.
            initialCheckboxState = chkEnableDatalog.IsChecked.Value;
            chkEnableDatalog.IsChecked = false;

            //System.Threading.Thread.Sleep(4000);
            // Parse response and set indicators

            // Set the checkbox back to it's initial state.
            chkEnableDatalog.IsChecked = initialCheckboxState;

        }

        private void btnSendFreqLed_Click(object sender, RoutedEventArgs e)
        {
            if (chkShowFreq.IsChecked == true)
            {
                stringToWrite = "showfreq=1" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
            else
            {
                stringToWrite = "showfreq=0" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }

            System.Threading.Thread.Sleep(200);

            if (chkToggleLED.IsChecked == true)
            {
                stringToWrite = "toggleled=1" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
            else
            {
                stringToWrite = "toggleled=0" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
        }

        private void btnSendDoEchoAcClock_Click(object sender, RoutedEventArgs e)
        {
            if (chkDoEcho.IsChecked == true)
            {
                stringToWrite = "doecho=1" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
            else
            {
                stringToWrite = "doecho=0" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }

            System.Threading.Thread.Sleep(200);

            if (chkAcClock.IsChecked == true)
            {
                stringToWrite = "acclock=1" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
            else
            {
                stringToWrite = "acclock=0" + "\r";
                currentPort.WriteLine(stringToWrite);
                txtSerialOutHistory.AppendText(">" + stringToWrite);
            }
        }

 









    }
}
