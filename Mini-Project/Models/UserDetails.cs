using System;
using System.ComponentModel.DataAnnotations;

namespace Mini_Project.Models
{
    public class UserDetails
    {
        // User's Email
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        // User's Password 
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        public string Username { get; set; }

        // Role (Default is Visitor, read-only)
        public string Role { get; set; } = "Visitor";

        // User's Age
        [Required(ErrorMessage = "Age is required.")]
        [Range(18, 100, ErrorMessage = "Age must be between 18 and 100.")]
        public int Age { get; set; }

        // User's Gender
        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        // User's Province
        [Required(ErrorMessage = "Province is required.")]
        public string Province { get; set; }

        // Festival Attendance Amount
        [Required(ErrorMessage = "Festival attendance amount is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid amount.")]
        public int FestivalAttendanceAmt { get; set; }

        // Date when the user details were created
        public DateTime DateCreated { get; set; }

        // Firebase UID 
        public string UID { get; set; }
    }
}
