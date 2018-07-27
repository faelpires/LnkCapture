using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodProgramar.LnkCapture.Data.BusinessObjects.Resources;
using PodProgramar.Utils.Text.Extensions;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class MessageBO : BaseBotBO, IMessageBO
    {
        #region Fields

        private readonly ILinkReaderBO _linkReaderBO;
        private readonly IChatBO _chatBO;

        #endregion Fields

        #region Constructors

        public MessageBO(IConfiguration configuration, ILogger<MessageBO> logger, ILinkReaderBO linkReaderBO, IChatBO chatBO) : base(configuration, logger)
        {
            Logger = logger;
            _linkReaderBO = linkReaderBO;
            _chatBO = chatBO;
        }

        #endregion Constructors

        #region Methods

        public async Task SendLinkAlreadyExistsMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId)
        {
            try
            {
                var rootImagesUri = Configuration.GetSection("AppConfiguration")["ImagesUri"];
                var message = new StringBuilder();

                if (uriCount > 1)
                    message.Append(uri).AppendLine();

                message.Append(MessageResources.LinkAlreadyExists.GetRandomText());

                try
                {
                    await BotClient.SendPhotoAsync(chatId, new InputOnlineFile($"{rootImagesUri}/link_already_exists_{new Random().Next(1, 3)}.jpg"), message.ToString(), ParseMode.Default, true, replyToMessageId);
                }
                catch (ApiRequestException)
                {
                    try
                    {
                        await BotClient.SendPhotoAsync(chatId, new InputOnlineFile($"{rootImagesUri}/link_already_exists_{new Random().Next(1, 3)}.jpg"), message.ToString(), ParseMode.Default, true);
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError(exception.Message);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);
            }
        }

        public async Task SendLinkSavedMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId)
        {
            try
            {
                var message = new StringBuilder();

                if (uriCount > 1)
                    message.Append(uri).AppendLine();

                message.Append(MessageResources.LinkSaved.GetRandomText());

                try
                {
                    await BotClient.SendTextMessageAsync(chatId, message.ToString(), ParseMode.Default, true, true, replyToMessageId);
                }
                catch (ApiRequestException)
                {
                    try
                    {
                        await BotClient.SendTextMessageAsync(chatId, message.ToString(), ParseMode.Default, true, true);
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError(exception.Message);

                        await Task.CompletedTask;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                await Task.CompletedTask;
            }
        }

        public async Task SendInvalidLinkMessageAsync(long chatId, string uri, int uriCount, int replyToMessageId)
        {
            try
            {
                var message = new StringBuilder();

                if (uriCount > 1)
                    message.Append(uri).AppendLine();

                message.Append(MessageResources.LinkInvalid.GetRandomText());

                try
                {
                    await BotClient.SendTextMessageAsync(chatId, message.ToString(), ParseMode.Default, true, true, replyToMessageId);
                }
                catch (ApiRequestException)
                {
                    try
                    {
                        await BotClient.SendTextMessageAsync(chatId, message.ToString(), ParseMode.Default, true, true);
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError(exception.Message);

                        await Task.CompletedTask;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                await Task.CompletedTask;
            }
        }

        public async Task SendLinksRecoverMessageAsync(long chatId, int userId, string chatTitle, int replyToMessageId)
        {
            try
            {
                var rootUri = Configuration.GetSection("AppConfiguration")["RootUri"];
                var linkReader = await _linkReaderBO.GetAsync(userId, chatId);

                if (linkReader == null)
                    linkReader = await _linkReaderBO.SaveAsync(userId, chatId);

                var message = string.Format(MessageResources.LinksRecover,
                                            string.IsNullOrEmpty(chatTitle) ? "" : $" {chatTitle}",
                                            $"{rootUri}/{linkReader.LinkReaderId.ToString()}"
                                            );

                await BotClient.SendTextMessageAsync(userId, message, ParseMode.Default, true, true);
            }
            catch (ChatNotInitiatedException exception)
            {
                Logger.LogError(exception.Message);

                var replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton { Text = MessageResources.StartChat, Url = Configuration.GetSection("AppConfiguration")["StartChatUri"] });

                await BotClient.SendTextMessageAsync(chatId, ExceptionResources.ChatNotInitiatedException, ParseMode.Markdown, true, false, replyToMessageId, replyMarkup);
            }
            catch (ApiRequestException exception)
            {
                Logger.LogError(exception.Message);

                await BotClient.SendTextMessageAsync(chatId, ExceptionResources.ApiRequestException, ParseMode.Default, true, true, replyToMessageId);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                await BotClient.SendTextMessageAsync(chatId, ExceptionResources.UnknownException, ParseMode.Default, true, true, replyToMessageId);
            }
        }

        public async Task SendBotOptionsMessageAsync(Update update)
        {
            if (update.Type != UpdateType.Message)
                return;

            if (update.Message.Type == MessageType.Text)
            {
                var languageCode = "pt-br";

                if (update.Message.From.LanguageCode != null)
                    languageCode = update.Message.From.LanguageCode;

                var cultureInfo = new CultureInfo(languageCode);

                BotCommandResources.Culture = cultureInfo;

                var replyMarkup = new ReplyKeyboardMarkup {
                    Keyboard = new KeyboardButton[][]
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton(BotCommandResources.RevokeLink),
                            new KeyboardButton(BotCommandResources.RenewLink)
                        }
                    }
                };

                await BotClient.SendTextMessageAsync(update.Message.Chat.Id, "Text", ParseMode.Markdown, true, false, 0, replyMarkup);
            }
        }

        public async Task SendHelpMessageAsync(bool isPrivateChat, long chatId, int replyToMessageId, int userId)
        {
            try
            {
                if (isPrivateChat)
                {
                    var message = MessageResources.HelpAdmin;

                    var replyMarkup = new InlineKeyboardMarkup(new[]
                    {
                        new InlineKeyboardButton { Text = MessageResources.StartGroup, Url = Configuration.GetSection("AppConfiguration")["StartGroupUri"] },
                        new InlineKeyboardButton { Text = MessageResources.RateBot, Url = Configuration.GetSection("AppConfiguration")["RateBotUri"] }
                    });

                    await BotClient.SendTextMessageAsync(chatId, message, ParseMode.Markdown, true, false, 0, replyMarkup);
                }
                else
                {
                    try
                    {
                        await BotClient.SendTextMessageAsync(userId, MessageResources.Help, ParseMode.Default, true);
                        await BotClient.SendTextMessageAsync(chatId, MessageResources.HelpToChat, ParseMode.Markdown, true, false, replyToMessageId);
                    }
                    catch (ChatNotInitiatedException)
                    {
                        var replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton { Text = MessageResources.StartChat, Url = Configuration.GetSection("AppConfiguration")["StartChatUri"] });

                        await BotClient.SendTextMessageAsync(chatId, ExceptionResources.ChatNotInitiatedException, ParseMode.Markdown, true, false, replyToMessageId, replyMarkup);
                    }
                    catch (ApiRequestException)
                    {
                        await BotClient.SendTextMessageAsync(chatId, $"{ExceptionResources.ApiRequestException}", ParseMode.Default, true, true, replyToMessageId);
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError(exception.Message);

                        await BotClient.SendTextMessageAsync(chatId, $"{ExceptionResources.UnknownException}", ParseMode.Default, true, true, replyToMessageId);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        public async Task SendWelcomeMessageAsync(bool isPrivateChat, long chatId)
        {
            try
            {
                if (isPrivateChat)
                {
                    var message = MessageResources.StartPrivate;

                    var replyMarkup = new InlineKeyboardMarkup(new[]
                    {
                        new InlineKeyboardButton { Text = MessageResources.StartGroup, Url = Configuration.GetSection("AppConfiguration")["StartGroupUri"] },
                        new InlineKeyboardButton { Text = MessageResources.RateBot, Url = Configuration.GetSection("AppConfiguration")["RateBotUri"] }
                    });

                    await BotClient.SendTextMessageAsync(chatId, message, ParseMode.Markdown, true, false, 0, replyMarkup);
                }
                else
                {
                    var chat = await _chatBO.GetChatAsync(chatId);
                    var message = string.Format(MessageResources.Start, chat.Title);

                    var replyMarkup = new InlineKeyboardMarkup(new[]
                    {
                        new InlineKeyboardButton { Text = MessageResources.StartGroup, Url = Configuration.GetSection("AppConfiguration")["StartGroupUri"] },
                        new InlineKeyboardButton { Text = MessageResources.RateBot, Url = Configuration.GetSection("AppConfiguration")["RateBotUri"] }
                    });

                    await BotClient.SendTextMessageAsync(chatId, message, ParseMode.Markdown, true, false, 0, replyMarkup);
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);
            }
        }

        public async Task SendBotOptionSetMessageAsync(long chatId, int replyToMessageId, BotCommand botCommand)
        {
            try
            {
                string message = null;

                switch (botCommand)
                {
                    case BotCommand.DisableSavedMsg:
                        message = MessageResources.SavedMessagesDisabled;
                        break;

                    case BotCommand.DisableLinkAlreadyExistsMsg:
                        message = MessageResources.LinkAlreadyExistsMessageDisabled;
                        break;

                    case BotCommand.DisableInvalidLinkMsg:
                        message = MessageResources.InvalidLinkMessageDisabled;
                        break;

                    case BotCommand.EnableSavedMsg:
                        message = MessageResources.SavedMessagesEnabled;
                        break;

                    case BotCommand.EnableLinkAlreadyExistsMsg:
                        message = MessageResources.LinkAlreadyExistsMessageEnabled;
                        break;

                    case BotCommand.EnableInvalidLinkMsg:
                        message = MessageResources.InvalidLinkMessageEnabled;
                        break;
                }

                await BotClient.SendTextMessageAsync(chatId, message, ParseMode.Default, true, true, replyToMessageId);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        public async Task SendBotOptionNotSetMessageAsync(long chatId, int replyToMessageId)
        {
            try
            {
                await BotClient.SendTextMessageAsync(chatId, MessageResources.BotOptionNotSet, ParseMode.Default, true, true, replyToMessageId);
            }
            catch (ApiRequestException exception)
            {
                Logger.LogError(exception.Message);

                try
                {
                    await BotClient.SendTextMessageAsync(chatId, MessageResources.BotOptionNotSet, ParseMode.Default, true, true);
                }
                catch (Exception innerException)
                {
                    Logger.LogError(innerException.Message);
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);
            }
        }

        public async Task SendBotOptionNotAllowedMessageAsync(long chatId, int replyToMessageId)
        {
            try
            {
                await BotClient.SendTextMessageAsync(chatId, MessageResources.BotOptionNotAllowed, ParseMode.Default, true, true, replyToMessageId);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        #endregion Methods
    }
}