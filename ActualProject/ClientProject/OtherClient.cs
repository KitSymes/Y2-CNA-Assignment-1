using System;

namespace ClientProject
{
    public class OtherClient
    {
        public Guid guid;
        public String name;
        public ChatChannel privateMessages;

        public OtherClient(Guid guid, string name)
        {
            this.guid = guid;
            this.name = name;
            privateMessages = new();
        }
    }
}
