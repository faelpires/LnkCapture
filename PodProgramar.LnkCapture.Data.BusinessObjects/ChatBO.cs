using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodProgramar.LnkCapture.Data.BusinessObjects.Resources;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class ChatBO : BaseBotBO, IChatBO
    {
        public ChatBO(IConfiguration configuration, ILogger<ChatBO> logger) : base(configuration, logger)
        {
            Logger = logger;
        }

        public async Task<Chat> GetChatAsync(long chatId)
        {
            try
            {
                var chat = await BotClient.GetChatAsync(chatId);

                if (string.IsNullOrEmpty(chat.Title))
                    chat.Title = ChatResources.DefaultChatName;

                return chat;
            }
            catch (ChatNotFoundException)
            {
                return new Chat() { Id = chatId, Title = ChatResources.ChatNotFoundTitle };
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                return new Chat() { Id = chatId, Title = ChatResources.ChatUnknown };
            }
        }

        public async Task<ChatMember> GetChatMemberAsync(long chatId, int userId)
        {
            try
            {
                var chatMember = await BotClient.GetChatMemberAsync(chatId, userId);

                return chatMember;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}