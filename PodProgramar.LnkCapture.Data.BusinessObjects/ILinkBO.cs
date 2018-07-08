using PodProgramar.LnkCapture.Data.DTO;
using PodProgramar.LnkCapture.Data.Models;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface ILinkBO
    {
        Task SaveAsync(Update update);

        Task<LinkResultDTO> GetAsync(LinkReader linkReader, bool isAPIRequest, string search, string user, DateTime? startDate, DateTime? endDate, int? pageIndex, int? pageSize);
    }
}