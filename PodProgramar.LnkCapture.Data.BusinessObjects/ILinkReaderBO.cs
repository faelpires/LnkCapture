using PodProgramar.LnkCapture.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface ILinkReaderBO
    {
        Task<LinkReader> GetAsync(int userId, long chatId);

        Task<LinkReader> GetAsync(Guid linkReaderId);

        Task<IDictionary<Guid, string>> GetRelatedLinkReadersAsync(Guid linkReaderId);

        Task<LinkReader> SaveAsync(int userId, long chatId);
    }
}