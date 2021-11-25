using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Packets;

namespace ClientProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Client client;
        private ChatChannel currentlyWatched;

        public MainWindow(Client client)
        {
            this.client = client;

            InitializeComponent();

            UserUUIDBox.Content += client.guid.ToString();
            client.name = UserNameInput.Text;
            currentlyWatched = client.mainChannel;
            currentlyWatched.watched = true;
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
                client.Send(new ClientJoinPacket(client.guid, client.name));
            }
            else
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
                string msg = InputMessageBox.Text;
                InputMessageBox.Clear();
                client.Send(new ChatMessagePacket(msg));
            }
        }

        /// <summary>
        /// Update the client's message box with a new message
        /// </summary>
        /// <param name="message">The message to append</param>
        public void UpdateChatBox(string channel)
        {
            CurrentChannelText.Dispatcher.Invoke(() =>
            {
                bool wasAtMax = CurrentChannelScroll.VerticalOffset == CurrentChannelScroll.ScrollableHeight;
                CurrentChannelText.Text = channel;
                if (wasAtMax)
                    CurrentChannelScroll.ScrollToEnd();
            });
        }
    }
}
