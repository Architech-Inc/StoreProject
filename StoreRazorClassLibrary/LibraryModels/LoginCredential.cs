using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreRazorClassLibrary.LibraryModels
{
    public class LoginCredential
    {
        public LoginCredential()
        {

        }
        public LoginCredential(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }

        public string ValidateCredential()
        {
            try
            {
                Username = Username.Trim();
                Password = Password.Trim();
            }
            catch (Exception)
            {
                //throw;
            }
            if (Username == null && Password == null) return "No credential provided!";
            if (string.IsNullOrEmpty(Username)) return "Username not provided!";
            if (string.IsNullOrEmpty(Password)) return "Password not provided!";
            return JsonConvert.SerializeObject(new LoginCredential(this.Username, this.Password));
        }

        public LoginCredential ValidateCredential(string username, string password)
        {
            return new(username, password);
        }
    }
}
