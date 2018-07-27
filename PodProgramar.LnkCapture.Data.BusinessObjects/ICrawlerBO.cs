using PodProgramar.LnkCapture.Data.DTO.Crawler;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface ICrawlerBO
    {
        Task<UriData> GetUriDataAsync(Uri uri);
    }
}
