using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface IChatBO
    {
        Task<Chat> GetChatAsync(long chatId);
    }
}