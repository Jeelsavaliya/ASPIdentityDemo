using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdentityDemo.Models
{
    public class Student
    {

        [Key]
        public int StudentID { get; set; }
        [Column("Name", TypeName = "nvarchar(50)")]
        public string Name { get; set; }
        [Column("Std", TypeName = "int")]
        public int Std { get; set; }
        [Column("Address", TypeName = "nvarchar(50)")]
        public string Address { get; set; }
        [Column("City", TypeName = "nvarchar(50)")]
        public string City { get; set; }

    }
}
