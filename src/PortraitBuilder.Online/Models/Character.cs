using PortraitBuilder.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortraitBuilder.Online.Models
{
    public class Character
    {
        public string DNA { get; set; }
        public string Properties { get; set; }
        public string Culture { get; set; }
        public bool? Female { get; set; }
        public int? Age { get; set; }
        public int? Year { get; set; }
        public string? Rank { get; set; }
        public string? Government { get; set; }
    }
}
