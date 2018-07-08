using System;
using System.Collections.Generic;

namespace PodProgramar.LnkCapture.Data.Models
{
    public partial class LinkReader
    {
        public LinkReader()
        {
            LinkReaderLog = new HashSet<LinkReaderLog>();
        }

        public Guid LinkReaderId { get; set; }
        public int UserId { get; set; }
        public long ChatId { get; set; }
        public DateTime CreateDate { get; set; }

        public ICollection<LinkReaderLog> LinkReaderLog { get; set; }
    }
}
