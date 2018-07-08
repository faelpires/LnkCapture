using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodProgramar.LnkCapture.Data.BusinessObjects.Resources;
using PodProgramar.Utils.Text.Extensions;
using System;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class MessageBO : BaseBotBO, IMessageBO
    {
        #region Fields

        private readonly ILinkReaderBO _linkReaderBO;

        #endregion Fields

        #region Constructors

        public MessageBO(IConfiguration configuration, ILogger<MessageBO> logger, ILinkReaderBO linkReaderBO) : base(configuration, logger)
        {
            Logger = logger;
            _linkReaderBO = linkReaderBO;
        }

        #endregion Constructors

        #region Methods

        public async Task<Message> SendLinkAlreadyExistsMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId)
        {
            try
            {
                var rootImagesUri = Configuration.GetSection("AppConfiguration")["ImagesUri"];
                var message = new StringBuilder();

                if (uriCount > 1)
                    message.Append(uri).AppendLine();

                message.Append(MessageResources.LinkAlreadyExists.GetRandomText());

                return await BotClient.SendPhotoAsync(chatId, new InputOnlineFile($"{rootImagesUri}/link_already_exists_{new Random().Next(1, 3)}.jpg"), message.ToString(), ParseMode.Default, true, replyToMessageId);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                return await Task.FromResult<Message>(null);
            }
        }

        public async Task<Message> SendLinkSavedMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId)
        {
            try
            {
                var message = new StringBuilder();

                if (uriCount > 1)
                    message.Append(uri).AppendLine();

                message.Append(MessageResources.LinkSaved.GetRandomText());

                return await BotClient.SendTextMessageAsync(chatId, message.ToString(), ParseMode.Default, true, true, replyToMessageId);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                return await Task.FromResult<Message>(null);
            }
        }

        public async Task<Message> SendInvalidLinkMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId)
        {
            try
            {
                var message = new StringBuilder();

                if (uriCount > 1)
                    message.Append(uri).AppendLine();

                message.Append(MessageResources.LinkInvalid.GetRandomText());

                return await BotClient.SendTextMessageAsync(chatId, message.ToString(), ParseMode.Default, true, true, replyToMessageId);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        public async Task SendLinksRecoverMessageAsync(Update update)
        {
            if (update.Type != UpdateType.Message)
                return;

            if (update.Message.Type == MessageType.Text)
            {
                try
                {
                    var rootUri = Configuration.GetSection("AppConfiguration")["RootUri"];
                    var chatIdInternal = await _linkReaderBO.GetAsync(update.Message.From.Id, update.Message.Chat.Id);

                    if (chatIdInternal == null)
                        chatIdInternal = await _linkReaderBO.SaveAsync(update.Message.From.Id, update.Message.Chat.Id);

                    var message = $"{string.Format(MessageResources.LinksRecover, string.IsNullOrEmpty(update.Message.Chat.Title) ? "" : $" {update.Message.Chat.Title}")} {rootUri}/{chatIdInternal.LinkReaderId.ToString()}";

                    await BotClient.SendTextMessageAsync(update.Message.From.Id, message, ParseMode.Default, true, true);
                }
                catch (ChatNotInitiatedException)
                {
                    await BotClient.SendTextMessageAsync(update.Message.Chat.Id, $"{MessageResources.ChatNotInitiatedException}", ParseMode.Default, true, true, update.Message.MessageId);
                }
                catch (ForbiddenException)
                {
                    await BotClient.SendTextMessageAsync(update.Message.Chat.Id, $"{MessageResources.ForbiddenException}", ParseMode.Default, true, true, update.Message.MessageId);
                }
                catch (Exception exception)
                {
                    Logger.LogError(exception.Message);

                    await BotClient.SendTextMessageAsync(update.Message.Chat.Id, $"{MessageResources.UnknownException}", ParseMode.Default, true, true, update.Message.MessageId);
                }
            }
        }

        #endregion Methods
    }
}