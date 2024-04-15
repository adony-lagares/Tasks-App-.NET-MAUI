using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Tasks.Models;

namespace Tasks.Service
{
    internal class UsersService
    {
        private static UsersService _usersServices = new UsersService();

        private List<User> _users = new List<User>();

        private UsersService() 
        {
            _users.Add(new User { Id = 1, Name = "Ádony" });
            _users.Add(new User { Id = 2, Name = "Kaiky" });
            _users.Add(new User { Id = 3, Name = "Nicoly" });
            _users.Add(new User { Id = 4, Name = "Jade" });
        }

        public static UsersService Instance()
        {
            return _usersServices;
        }

        public List<User> All()
        {
            return _users;
        }





    }
}
