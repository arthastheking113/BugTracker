using BugTracker.Extentions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public DateTimeOffset Created { get; set; }

        [NotMapped]
        [Display(Name = "Upload File")]
        [MaxFileSize(2 * 1024 * 1024)]
        [AllowedExtention(new string[] { ".jpg", ".png", ".gif", ".doc", ".pdf", ".docx", ".xls", ".xlsx"})]
        public IFormFile FormFile { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }



        public int TicketId { get; set; }
        public virtual Ticket Ticket { get; set; }

        public int CustomUserId { get; set; }
        public virtual CustomUser CustomUser { get; set; }
    }
}
