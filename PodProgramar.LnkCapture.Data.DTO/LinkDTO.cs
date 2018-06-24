using System;

namespace PodProgramar.LnkCapture.Data.DTO
{
    public partial class LinkDTO
    {
        public string Title { get; set; }
        public string Uri { get; set; }
        public DateTime CreateDate { get; set; }
        public string Username { get; set; }
        public int? UserId { get; set; }
    }
}