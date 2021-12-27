using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Packets;

namespace ClientProject
{
    public partial class MainWindow : Window
    {
        private Client client;

        public MainWindow(Client client)
        {
            this.client = client;

            InitializeComponent();

            UserUUIDBox.Content += client.guid.ToString();
            client.nickname = UserNameInput.Text;
        }

        public void AddClient(OtherClient cl)
        {
            Dispatcher.Invoke(() =>
            {
                if (cl.local)
                {
                    Label control = new Label();
                    control.Content = cl.nickname;
                    control.Style = (Style)userList.FindResource("SidebarLabelStyle");
                    userList.Children.Add(control);
                    cl.label = control;
                }
                else
                {
                    Button newButton = new Button();
                    newButton.Content = cl.nickname;
                    newButton.Style = (Style)userList.FindResource("SidebarButtonStyle");
                    userList.Children.Add(newButton);
                    cl.button = newButton;
                    newButton.Click += (x, y) => client.ChangeChannel(cl.privateMessages);
                }
            });
        }

        public void RemoveClient(OtherClient cl)
        {
            Dispatcher.Invoke(() =>
            {
                if (cl.local)
                    userList.Children.Remove(cl.label);
                else
                    userList.Children.Remove(cl.button);
            });
        }

        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            // IPAddress Validation
            if (IPAddressInput.Text.Length == 0)
            {
                MessageBox.Show("IPAddress is empty!", "Warning");
                return;
            }
            Regex ipReg = new Regex("[^0-9\\.]");
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
            Regex portReg = new Regex("[^0-9]");
            if (portReg.IsMatch(PortInput.Text))
            {
                MessageBox.Show("Port is not valid!", "Warning");
                return;
            }

            if (client.Connect(IPAddressInput.Text, int.Parse(PortInput.Text)))
            {
                client.Run();
            }
            else
            {
                MessageBox.Show("An error occured when connecting.", "Warning");
            }
        }

        private void SendMessage(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (InputMessageBox.Text.Length == 0)
                    return;
                string msg = InputMessageBox.Text;
                InputMessageBox.Clear();

                if (client.currentChannel.id == client.mainChannel.id)
                {
                    if (useEncryptionBox.IsChecked.Value)
                        client.UDPSend(new EncryptedChatMessagePacket(client.EncryptString(msg)));
                    else
                        client.UDPSend(new ChatMessagePacket(msg));
                }
                else
                {
                    client.MessageChannel(client.currentChannel, client.nickname + ": " + msg);
                    client.UDPSend(new EncryptedPrivateMessagePacket(client.EncryptString(msg), client.EncryptString(client.currentChannel.id.ToString())));

                }
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

        public void ClearChatBox()
        {
            CurrentChannelText.Dispatcher.Invoke(() =>
            {
                CurrentChannelText.Text = "";
                CurrentChannelScroll.ScrollToEnd();
            });
        }

        private void UserNameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (UserNameInput.Text.Length == 0)
                    return;
                string name = UserNameInput.Text;
                client.TCPSend(new ClientNameChangePacket(name));
                client.nickname = name;
            }
        }

        private void MainChannelButton(object sender, RoutedEventArgs e)
        {
            client.ChangeChannel(client.mainChannel);
        }
    }
}
