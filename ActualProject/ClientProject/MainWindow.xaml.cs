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

namespace ClientProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Client client;

        public MainWindow(Client client)
        {
            this.client = client;

            InitializeComponent();

            //UserUUIDBox.Content += u.GetGuid().ToString();
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
            if (client.Connect(IPAddressInput.Text, int.Parse(PortInput.Text)))
            {
                client.Run();
            } else
            {
                MessageBox.Show("An error occured when connecting.", "Warning");
            }
        }

        private void InputMessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (InputMessageBox.Text.Length == 0)
                    return;
                string msg = UserNameInput.Text + ": " + InputMessageBox.Text;
                InputMessageBox.Clear();
                client.SendMessage(msg);
            }
        }

        /// <summary>
        /// Update the client's message box with a new message
        /// </summary>
        /// <param name="message">The message to append</param>
        public void UpdateChatBox(string message)
        {
            CurrentChannelText.Dispatcher.Invoke(() =>
            {
                CurrentChannelText.Text += (CurrentChannelText.Text.Length == 0 ? "" : "\r\n") + message;
                if (CurrentChannelScroll.VerticalOffset == CurrentChannelScroll.ScrollableHeight)
                    CurrentChannelScroll.ScrollToEnd();
            });
        }
    }
}
