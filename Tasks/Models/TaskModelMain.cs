using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Enums;
using Tasks.Service;

namespace Tasks.Models
{
    public class TaskModelMain
    {
        public TaskModelMain() 
        {
            this.UpdatedDate = DateTime.Now;
            this.CreatedDate = DateTime.Now;
        }



        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }

        [Ignore]
        public User User
        {
            get
            {
                if (this.UserId == 0) return null;
                return UsersService.Instance().All().Find(u => u.Id == this.UserId);
            }
        }

        [Ignore]
        public string Username 
        {
            get 
            {
                if (this.User == null) return "User register empty.";
                return User?.Name;
            }
        }
        public Status? Status {get; set;}
    }
}
