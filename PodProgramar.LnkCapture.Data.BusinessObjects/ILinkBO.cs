using PodProgramar.LnkCapture.Data.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface ILinkBO
    {
        Task SaveLinkAsync(Update update);

        Task UpdateTitlesAsync();


        Task SendLinksRecoverMessageAsync(Update update, string chatId);

        Task<LinkResultDTO> GetChatLinksAsync(long id);
    }
}