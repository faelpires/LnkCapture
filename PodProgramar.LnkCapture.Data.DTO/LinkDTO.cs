using System;

namespace PodProgramar.LnkCapture.Data.DTO
{
    public partial class LinkDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUri { get; set; }
        public string Keywords { get; set; }
        public string Uri { get; set; }
        public DateTime CreateDate { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? UserId { get; set; }
    }
}