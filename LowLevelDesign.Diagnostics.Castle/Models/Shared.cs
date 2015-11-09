using System;

namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public class JsonFormValidationResult
    {
        public bool IsSuccess { get; set; }

        public String Result { get; set; }

        public String[] Errors { get; set; }
    }
}