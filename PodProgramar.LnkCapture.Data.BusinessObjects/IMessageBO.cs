using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface IMessageBO
    {
        Task SendLinksRecoverMessageAsync(Update update);

        Task<Message> SendLinkAlreadyExistsMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId);

        Task<Message> SendLinkSavedMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId);

        Task<Message> SendInvalidLinkMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId);
    }
}