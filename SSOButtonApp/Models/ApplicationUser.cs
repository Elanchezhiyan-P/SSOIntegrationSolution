using Microsoft.AspNetCore.Identity;

namespace SSOButtonApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public virtual string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public required DateTime DateOfBirth { get; set; }
        public virtual int Age
        {
            get
            {
                return DateTime.Now.Year - DateOfBirth.Year;
            }
        }
        public DateTime CreatedDt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string GreetMessage
        {
            get
            {
                var greeting = $"Welcome back, {FullName}!";

                if (DateOfBirth.Month == DateTime.Now.Month && DateOfBirth.Day == DateTime.Now.Day)
                {
                    greeting += $" Happy Birthday! 🎉";
                }
                return greeting;
            }
        }

        public bool IsBirthdayToday()
        {
            return DateOfBirth.Month == DateTime.Now.Month && DateOfBirth.Day == DateTime.Now.Day;
        }

        public bool IsFirstLoginOfTheDay()
        {
            return !LastLoginDate.HasValue || LastLoginDate.Value.Date != DateTime.Now.Date;
        }
    }
}
