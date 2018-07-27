using System;
using System.Collections.Generic;

namespace PodProgramar.LnkCapture.Data.Models
{
    public partial class CultureInfo
    {
        public CultureInfo()
        {
            Config = new HashSet<Config>();
        }

        public Guid CultureId { get; set; }
        public string Culture { get; set; }
        public string DisplayName { get; set; }

        public ICollection<Config> Config { get; set; }
    }
}
