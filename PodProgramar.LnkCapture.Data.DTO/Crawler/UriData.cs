using System;

namespace PodProgramar.LnkCapture.Data.DTO.Crawler
{
    public class UriData
    {
        public Uri Uri { get; set; }
        public Uri RedirectUri { get; set; }
        public string Content { get; set; }
        public bool HasOpenGraphData { get; set; }
        public bool IsValid { get; set; }
        public bool HasTitle { get { return !string.IsNullOrWhiteSpace(Title); } }
        public bool IsYoutubeUri { get; set; }
        public string Title { get; set; }
        public bool HasDescription { get { return !string.IsNullOrWhiteSpace(Description); } }
        public string Description { get; set; }
        public bool HasKeywords { get { return !string.IsNullOrWhiteSpace(Keywords); } }
        public string Keywords { get; set; }
        public bool HasTumbnailUri { get { return ThumbnailUri != null && !string.IsNullOrWhiteSpace(ThumbnailUri.AbsoluteUri); } }
        public Uri ThumbnailUri { get; set; }
    }
}