using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class User
    {
        private Guid guid;
        public String currentName;

        public User()
        {
            guid = Guid.NewGuid();
        }

        public Guid GetGuid()
        {
            return guid;
        }
    }
}
