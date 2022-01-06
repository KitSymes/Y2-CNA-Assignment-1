using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Packets;

namespace ClientProject
{
    public partial class MainWindow : Window
    {
        private Client client;
        public Bitmap bitmap;

        public MainWindow(Client client)
        {
            this.client = client;

            InitializeComponent();

            RenderOptions.SetBitmapScalingMode(Canvas, BitmapScalingMode.NearestNeighbor);
            bitmap = new Bitmap(166, 75);
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                    bitmap.SetPixel(x, y, System.Drawing.Color.White);
            UpdateBitmap();
            DisableCanvas();
            //EnableCanvas();

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
                    client.UDPSend(new EncryptedPrivateMessagePacket(client.EncryptString(msg), client.EncryptString(client.currentChannel.id.ToString())));
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

        public void EnableCanvas()
        {
            CurrentChannelScroll.Visibility = Visibility.Hidden;
            InputMessageBox.Visibility = Visibility.Hidden;

            Canvas.Visibility = Visibility.Visible;
            Colour1.Visibility = Visibility.Visible;
            Colour2.Visibility = Visibility.Visible;
            Colour3.Visibility = Visibility.Visible;
            Colour4.Visibility = Visibility.Visible;
            Colour5.Visibility = Visibility.Visible;
            Colour6.Visibility = Visibility.Visible;
            Colour7.Visibility = Visibility.Visible;
            Colour8.Visibility = Visibility.Visible;
            Colour9.Visibility = Visibility.Visible;
        }

        public void DisableCanvas()
        {
            CurrentChannelScroll.Visibility = Visibility.Visible;
            InputMessageBox.Visibility = Visibility.Visible;

            Canvas.Visibility = Visibility.Hidden;
            Colour1.Visibility = Visibility.Hidden;
            Colour2.Visibility = Visibility.Hidden;
            Colour3.Visibility = Visibility.Hidden;
            Colour4.Visibility = Visibility.Hidden;
            Colour5.Visibility = Visibility.Hidden;
            Colour6.Visibility = Visibility.Hidden;
            Colour7.Visibility = Visibility.Hidden;
            Colour8.Visibility = Visibility.Hidden;
            Colour9.Visibility = Visibility.Hidden;
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public void UpdateBitmap()
        {
            Canvas.Dispatcher.Invoke(() =>
            {
                Canvas.Source = BitmapToImageSource(bitmap);
            });
        }

        private void CanvasClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(((IInputElement)e.Source));
            int x = (int)(p.X / (Canvas.Width / bitmap.Width));
            int y = (int)(p.Y / (Canvas.Height / bitmap.Height));
            //bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(client.r, client.g, client.b));
            //UpdateBitmap();
            client.TCPSend(new CanvasPaintPacket(x, y, client.r, client.g, client.b));
        }

        private void ColorClick(object sender, RoutedEventArgs e)
        {
            client.r = ((SolidColorBrush)((Button)sender).Background).Color.R;
            client.g = ((SolidColorBrush)((Button)sender).Background).Color.G;
            client.b = ((SolidColorBrush)((Button)sender).Background).Color.B;
        }

        private void CanvasButton(object sender, RoutedEventArgs e)
        {
            EnableCanvas();
        }
    }
}
