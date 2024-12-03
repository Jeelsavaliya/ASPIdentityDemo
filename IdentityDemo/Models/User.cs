using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdentityDemo.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }



        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
