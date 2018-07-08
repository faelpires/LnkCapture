using System;
using System.Collections.Generic;

namespace PodProgramar.LnkCapture.Data.Models
{
    public partial class Link
    {
        public Guid LinkId { get; set; }
        public string Message { get; set; }
        public string Uri { get; set; }
        public string Title { get; set; }
        public DateTime CreateDate { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; }
        public int UserId { get; set; }
    }
}
