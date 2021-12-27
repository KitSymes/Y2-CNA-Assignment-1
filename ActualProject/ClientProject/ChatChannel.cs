using System;
using System.Collections.Generic;

namespace ClientProject
{
    public class ChatChannel
    {
        private List<string> messages;
        public Guid id;

        public ChatChannel(Guid id)
        {
            messages = new List<string>();
            this.id = id;
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
