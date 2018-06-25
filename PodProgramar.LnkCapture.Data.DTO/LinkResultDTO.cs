using System;
using System.Collections.Generic;

namespace PodProgramar.LnkCapture.Data.DTO
{
    public class LinkResultDTO
    {
        public string ChatIdEncrypted { get; set; }
        public string ChatTitle { get; set; }
        public long ChatId { get; set; }
        public DateTime CreateDate { get; set; }
        public List<LinkDTO> Items { get; set; }
        public int TotalSearchItems { get; set; }
        public int TotalItems { get; set; }
    }
}