using System;
using System.Collections.Generic;

namespace PodProgramar.LnkCapture.Data.Models
{
    public partial class LinkReaderLog
    {
        public Guid LinkReaderLogId { get; set; }
        public Guid LinkReaderId { get; set; }
        public DateTime AccessDate { get; set; }
        public bool IsAPIRequest { get; set; }
        public int RowCount { get; set; }

        public LinkReader LinkReader { get; set; }
    }
}
