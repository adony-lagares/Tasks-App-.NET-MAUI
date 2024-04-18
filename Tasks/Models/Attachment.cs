using SQLite;
using Tasks.Enums;
using Tasks.Service;

namespace Tasks.Models
{
    public class Attachment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string File { get; set; }

        public int TaskId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
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
                return User.Name;
            }
        }
    }
}