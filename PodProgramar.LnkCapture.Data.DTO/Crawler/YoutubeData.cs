using Newtonsoft.Json;

namespace PodProgramar.LnkCapture.Data.DTO.Crawler
{
    public class YoutubeData
    {
        [JsonProperty(PropertyName = "items")]
        public Item[] Items { get; set; }
    }

    public class Item
    {
        [JsonProperty(PropertyName = "snippet")]
        public Snippet Snippet { get; set; }
    }

    public class Snippet
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public string[] Tags { get; set; }

        [JsonProperty(PropertyName = "thumbnails")]
        public Thumbnails Thumbnails { get; set; }
    }

    public class Thumbnails
    {
        [JsonProperty(PropertyName = "default")]
        public Default Default { get; set; }
    }

    public class Default
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }
    }
}