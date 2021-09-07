using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebClientDemo.Models
{
    public class User
    {
        public static User userData = new User();
        public string password { get; private set; }

        private string m_username;
        public string username { set {
                m_username = value;
            }
            get
            {
                return @"UA\"+m_username;
            }
        }
       


        public User()
        {

        }

        public User(string username, string password)
        {
            this.username = username;
             this.password = password;
            userData.username = username;
            userData.password = password;
        }






  
    }
}
