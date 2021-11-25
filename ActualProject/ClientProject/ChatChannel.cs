using System.Collections.Generic;

namespace ClientProject
{
    public class ChatChannel
    {
        private List<string> messages;
        public bool watched;

        public ChatChannel()
        {
            messages = new();
            watched = false;
        }

        public void Add(string message)
        {
            messages.Add(message);
        }

        public string Format()
        {
            string textBox = "";

            foreach (string s in messages)
                textBox += (textBox.Length == 0 ? "" : "\r\n") + s;

            return textBox;
        }
    }
}
