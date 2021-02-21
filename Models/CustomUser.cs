using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class CustomUser : IdentityUser
    {
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} character long.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} character long.", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Change Avatar")]
        public Byte[] ImageData { get; set; }
        public string ContentType { get; set; }

        [NotMapped]
        public string FullName { get { return $"{FirstName} {LastName}"; } }

        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
