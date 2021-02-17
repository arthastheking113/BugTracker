using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Extentions
{
    public class AllowedExtentionAttribute : ValidationAttribute
    {
        private readonly string[] _extension;

        public AllowedExtentionAttribute(string[] extensions)
        {
            _extension = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extension.Contains(extension.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage(extension));
                }
            }
            return ValidationResult.Success;
        }
        public string GetErrorMessage(string ext)
        {
            return $"The file extention {ext} is not alowed.";
        }
    }
}
