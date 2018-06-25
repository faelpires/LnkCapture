using PodProgramar.LnkCapture.Data.DTO;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface ILinkBO
    {
        Task SaveLinkAsync(Update update);

        Task SendLinksRecoverMessageAsync(Update update, string chatId);

        Task<LinkResultDTO> GetChatLinksAsync(long id, string search, string user, DateTime? startDate, DateTime? endDate, int? pageIndex, int? pageSize);
    }
}