using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json; // Asigură-te că ai instalat pachetul Newtonsoft.Json

namespace DealershipApp
{
    public class TinyDB
    {
        private string dbFilePath = "users.json";

        // Constructor care creează fișierul JSON dacă nu există
        public TinyDB()
        {
            if (!File.Exists(dbFilePath))
            {
                File.WriteAllText(dbFilePath, "[]");
            }
        }

        // Metodă pentru a adăuga un utilizator nou
        public void AddUser(string username, string password)
        {
            var users = GetAllUsers();
            users.Add(new User { Username = username, Password = password });
            File.WriteAllText(dbFilePath, JsonConvert.SerializeObject(users));
        }

        // Metodă pentru a verifica un utilizator
        public bool VerifyUser(string username, string password)
        {
            var users = GetAllUsers();
            foreach (var user in users)
            {
                if (user.Username == username && user.Password == password)
                {
                    return true;
                }
            }
            return false;
        }

        // Metodă pentru a obține toți utilizatorii
        private List<User> GetAllUsers()
        {
            var json = File.ReadAllText(dbFilePath);
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
