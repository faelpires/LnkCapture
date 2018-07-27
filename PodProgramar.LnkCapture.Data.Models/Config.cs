using System;
using System.Collections.Generic;

namespace PodProgramar.LnkCapture.Data.Models
{
    public partial class Config
    {
        public Guid ConfigId { get; set; }
        public long ChatId { get; set; }
        public Guid CultureId { get; set; }
        public bool EnableSavedMessage { get; set; }
        public bool EnableLinkAlreadyExistsMessage { get; set; }
        public bool EnableInvalidLinkMessage { get; set; }

        public CultureInfo Culture { get; set; }
    }
}
