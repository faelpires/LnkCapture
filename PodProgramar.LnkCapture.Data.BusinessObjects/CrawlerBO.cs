using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PodProgramar.LnkCapture.Data.BusinessObjects.Resources;
using PodProgramar.LnkCapture.Data.DAL;
using PodProgramar.LnkCapture.Data.DTO.Crawler;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class CrawlerBO : BaseDataBO, ICrawlerBO
    {
        public CrawlerBO(LnkCaptureContext lnkCaptureContext, IConfiguration configuration, ILogger<CrawlerBO> logger) : base(lnkCaptureContext, configuration, logger)
        {
            Logger = logger;
        }

        public async Task<UriData> GetUriDataAsync(Uri uri)
        {
            var uriData = new UriData {
                Uri = uri,
                IsValid = false
            };

            try
            {
                if (IsYoutubeUri(uri, out var videoId))
                    return await GetYoutubeData(uri, videoId);
                else
                    return await GetDataAsync(uri);
            }
            catch (Exception)
            {
                uriData.Title = null;

                return uriData;
            }
        }

        private async Task<UriData> GetDataAsync(Uri uri)
        {
            try
            {
                var uriData = await DownloadStringAwareOfEncodingAsync(uri);

                var document = new HtmlDocument();
                document.LoadHtml(uriData);

                var openGraphData = GetOpenGraphData(uri, document);

                openGraphData.IsValid = true;

                if (openGraphData.HasOpenGraphData)
                    return openGraphData;
                else
                {
                    try
                    {
                        var title = document.DocumentNode.Descendants("title").FirstOrDefault();

                        if (title != null)
                            openGraphData.Title = WebUtility.HtmlDecode(title.InnerText).Trim();
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError(exception.Message);
                    }

                    return openGraphData;
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                return new UriData {
                    Uri = uri,
                    IsValid = false
                };
            }
        }

        private UriData GetOpenGraphData(Uri uri, HtmlDocument htmlDocument)
        {
            var uriData = new UriData {
                Uri = uri,
                IsValid = false
            };

            try
            {
                var metaTags = htmlDocument.DocumentNode.SelectNodes("//meta");

                if (metaTags != null)
                {
                    var matchCount = 0;

                    foreach (var tag in metaTags)
                    {
                        var tagName = tag.Attributes["name"];
                        var tagContent = tag.Attributes["content"];
                        var tagProperty = tag.Attributes["property"];

                        if (tagName != null && tagContent != null)
                        {
                            switch (tagName.Value.ToLower())
                            {
                                case "title":
                                    uriData.Title = tagContent.Value.Trim();
                                    matchCount++;
                                    break;

                                case "description":
                                    uriData.Description = string.IsNullOrWhiteSpace(tagContent.Value) ? null : tagContent.Value.Trim();
                                    matchCount++;
                                    break;

                                case "twitter:title":
                                    uriData.Title = string.IsNullOrWhiteSpace(uriData.Title) ? tagContent.Value.Trim() : uriData.Title;
                                    matchCount++;
                                    break;

                                case "twitter:description":
                                    uriData.Description = string.IsNullOrWhiteSpace(uriData.Description) ? tagContent.Value.Trim() : uriData.Description;
                                    matchCount++;
                                    break;

                                case "keywords":
                                    uriData.Keywords = tagContent.Value.Trim();
                                    matchCount++;
                                    break;

                                case "twitter:image":
                                    uriData.ThumbnailUri = uriData.ThumbnailUri ?? new Uri(tagContent.Value.Trim());
                                    matchCount++;
                                    break;
                            }
                        }
                        else if (tagProperty != null && tagContent != null)
                        {
                            switch (tagProperty.Value.ToLower())
                            {
                                case "og:title":
                                    uriData.Title = string.IsNullOrWhiteSpace(uriData.Title) ? tagContent.Value.Trim() : uriData.Title;
                                    matchCount++;
                                    break;

                                case "og:description":
                                    uriData.Description = string.IsNullOrWhiteSpace(uriData.Description) ? tagContent.Value.Trim() : uriData.Description;
                                    matchCount++;
                                    break;

                                case "og:image":
                                    uriData.ThumbnailUri = uriData.ThumbnailUri ?? new Uri(tagContent.Value.Trim());
                                    matchCount++;
                                    break;
                            }
                        }
                    }

                    uriData.HasOpenGraphData = matchCount > 0;
                }

                return uriData;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                return uriData;
            }
        }

        private async Task<UriData> GetYoutubeData(Uri uri, string videoId)
        {
            var uriData = new UriData {
                Uri = uri,
                IsValid = false
            };

            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add(CrawlerResources.WebClientUserAgent);

                    var rawData = await webClient.DownloadStringTaskAsync(string.Format(Configuration.GetSection("AppConfiguration")["YoutubeApiUri"], videoId, Configuration.GetSection("AppConfiguration")["YoutubeApiKey"]));
                    var youtubeData = JsonConvert.DeserializeObject<YoutubeData>(rawData);

                    if (youtubeData == null || youtubeData.Items == null || youtubeData.Items.Length == 0 || youtubeData.Items[0].Snippet == null)
                        return uriData;

                    uriData.IsValid = true;

                    if (!string.IsNullOrWhiteSpace(youtubeData.Items[0].Snippet.Title))
                        uriData.Title = youtubeData.Items[0].Snippet.Title;

                    if (!string.IsNullOrWhiteSpace(youtubeData.Items[0].Snippet.Description))
                        uriData.Description = youtubeData.Items[0].Snippet.Description.Trim();

                    if (youtubeData.Items[0].Snippet.Thumbnails != null && youtubeData.Items[0].Snippet.Thumbnails.Default != null & youtubeData.Items[0].Snippet.Thumbnails.Default.Url != null)
                        uriData.ThumbnailUri = new Uri(youtubeData.Items[0].Snippet.Thumbnails.Default.Url);

                    if (youtubeData.Items[0].Snippet.Tags != null && youtubeData.Items[0].Snippet.Tags.Length > 0)
                        uriData.Keywords = string.Join(",", youtubeData.Items[0].Snippet.Tags);

                    return uriData;
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                return uriData;
            }
        }

        private bool IsYoutubeUri(Uri uri, out string videoId)
        {
            var YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
            var regexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
            string[] validAuthorities = { "youtube.com", "www.youtube.com", "youtu.be", "www.youtu.be" };

            videoId = null;

            try
            {
                var authority = new UriBuilder(uri).Uri.Authority.ToLower();

                if (validAuthorities.Contains(authority))
                {
                    var regeEx = regexExtractId.Match(uri.ToString());
                    if (regeEx.Success)
                    {
                        videoId = regeEx.Groups[1].Value;
                        return regeEx.Success;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private async Task<string> DownloadStringAwareOfEncodingAsync(Uri uri)
        {
            var uriData = new UriData();

            try
            {
                using (var webClient = new WebClient())
                {
                    try
                    {
                        webClient.Headers.Add(CrawlerResources.WebClientUserAgent);

                        var rawData = await webClient.DownloadDataTaskAsync(uri);
                        var encoding = GetEncodingFrom(webClient.ResponseHeaders, Encoding.UTF8);

                        uriData.Content = encoding.GetString(rawData);

                        return uriData.Content;
                    }
                    catch (WebException exception)
                    {
                        var response = ((HttpWebResponse)exception.Response);

                        if (response.StatusCode == HttpStatusCode.MovedPermanently)
                        {
                            if (response.Headers.AllKeys.Contains("Location"))
                            {
                                var location = response.Headers["Location"];

                                if (location != null)
                                    return await DownloadStringAwareOfEncodingAsync(new Uri(location));
                            }
                        }

                        throw;
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError(exception.Message);

                        throw;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        private Encoding GetEncodingFrom(NameValueCollection responseHeaders, Encoding defaultEncoding = null)
        {
            try
            {
                if (responseHeaders == null)
                    throw new ArgumentNullException("responseHeaders");

                var contentType = responseHeaders["Content-Type"];

                if (contentType == null)
                    return defaultEncoding;

                var contentTypeParts = contentType.Split(';');

                if (contentTypeParts.Length <= 1)
                    return defaultEncoding;

                var charsetPart = contentTypeParts.Skip(1).FirstOrDefault(p => p.TrimStart().StartsWith("charset", StringComparison.InvariantCultureIgnoreCase));

                if (charsetPart == null)
                    return defaultEncoding;

                var charsetPartParts = charsetPart.Split('=');

                if (charsetPartParts.Length != 2)
                    return defaultEncoding;

                var charsetName = charsetPartParts[1].Trim();

                if (charsetName == "")
                    return defaultEncoding;

                return Encoding.GetEncoding(charsetName);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                return defaultEncoding ?? Encoding.UTF8;
            }
        }
    }
}