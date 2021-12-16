using System;
using System.Windows.Controls;

namespace ClientProject
{
    public class OtherClient
    {
        public Guid guid;
        public String nickname;
        public bool local;
        public ChatChannel privateMessages;
        public Button button;
        public Label label;

        public OtherClient(Guid guid, string name, bool local)
        {
            this.guid = guid;
            this.nickname = name;
            this.local = local;
            if (!local)
                privateMessages = new ChatChannel();
        }

        public OtherClient(Guid guid, string name) : this(guid, name, false)
        {

        }

        public void ChangeName(string name, MainWindow form)
        {
            this.nickname = name;
            if (local)
                label.Dispatcher.Invoke(() =>
                {
                    label.Content = name;
                });
            else
                button.Dispatcher.Invoke(() =>
                {
                    button.Content = name;
                });
        }
    }
}
