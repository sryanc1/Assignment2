using System.ComponentModel.DataAnnotations;

namespace NwbaExample.Models
{
    public class Login
    {
        [StringLength(8)]
        public string LoginID { get; set; }

        public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; }

        [Required, StringLength(64)]
        public string PasswordHash { get; set; }
    }
}
