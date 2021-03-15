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
    public class ProjectAttachment
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public DateTimeOffset Created { get; set; }

        [NotMapped]
        [Display(Name = "Upload File")]
        [MaxFileSize(2 * 1024 * 1024)]
        [AllowedExtention(new string[] { ".jpg",".jpeg", ".png", ".gif", ".doc", ".pdf", ".docx", ".xls", ".xlsx" })]
        public IFormFile FormFile { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        public string ContentType { get; set; }
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public string CustomUserId { get; set; }
        public virtual CustomUser CustomUser { get; set; }
    }
}
