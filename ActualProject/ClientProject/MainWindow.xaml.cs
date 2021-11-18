using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        User u;
        public MainWindow()
        {
            InitializeComponent();
            u = new();
            UserUUIDBox.Content += u.GetGuid().ToString();
        }

        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            // IPAddress Validation
            if (IPAddressInput.Text.Length == 0)
            {
                MessageBox.Show("IPAddress is empty!", "Warning");
                return;
            }
            Regex ipReg = new("[^0-9\\.]");
            if (ipReg.IsMatch(IPAddressInput.Text))
            {
                MessageBox.Show("IPAddress is not valid!", "Warning");
                return;
            }
            // Port Validation
            if (PortInput.Text.Length == 0)
            {
                MessageBox.Show("Port is empty!", "Warning");
                return;
            }
            Regex portReg = new("[^0-9]");
            if (portReg.IsMatch(PortInput.Text))
            {
                MessageBox.Show("Port is not valid!", "Warning");
                return;
            }

            // TODO
        }

        private void InputMessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (InputMessageBox.Text.Length == 0)
                    return;
                string msg = UserNameInput.Text + ": " + InputMessageBox.Text;
                InputMessageBox.Clear();
                new Thread(() => { Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(SendMessage), msg); }).Start();
            }
        }

        private void SendMessage(string message)
        {
            CurrentChannelText.Text += (CurrentChannelText.Text.Length == 0 ? "" : "\r\n") + message;
            if (CurrentChannelScroll.VerticalOffset == CurrentChannelScroll.ScrollableHeight)
                CurrentChannelScroll.ScrollToEnd();
        }
    }
}
