using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        [Display(Name = "Project Name")]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }

        [Display(Name = "User")]
        public string CustomUserId { get; set; }
        public virtual CustomUser CustomUser { get; set; }

        [Display(Name = "Status")]
        public int StatusId { get; set; }
        public virtual Status Status { get; set; }


        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        [Display(Name = "Upload Image")]
        public Byte[] ImageData { get; set; }
        public string ContentType { get; set; }


        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();


    }
}
