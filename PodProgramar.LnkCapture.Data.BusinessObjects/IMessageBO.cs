using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface IMessageBO
    {
        Task SendWelcomeMessageAsync(bool isPrivateChat, long chatId);

        Task SendHelpMessageAsync(bool isPrivateChat, long chatId, int replyToMessageId, int userId);

        Task SendLinksRecoverMessageAsync(long chatId, int userId, string chatTitle, int replyToMessageId);

        Task SendBotOptionsMessageAsync(Update update);

        Task SendLinkAlreadyExistsMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId);

        Task SendLinkSavedMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId);

        Task SendInvalidLinkMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId);

        Task SendBotOptionSetMessageAsync(long chatId, int replyToMessageId, BotCommand botCommand);

        Task SendBotOptionNotSetMessageAsync(long chatId, int replyToMessageId);

        Task SendBotOptionNotAllowedMessageAsync(long chatId, int replyToMessageId);
    }
}