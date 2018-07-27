using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class BotCommandBO : BaseBotBO, IBotCommandsBO
    {
        private readonly IMessageBO _messageBO;
        private readonly IConfigBO _configBO;

        public BotCommandBO(IConfiguration configuration, ILogger<BotCommandBO> logger, IMessageBO messageBO, IConfigBO configBO) : base(configuration, logger)
        {
            Logger = logger;
            _messageBO = messageBO;
            _configBO = configBO;
        }

        public bool IsBotCommand(Update update)
        {
            if (update == null)
                return false;

            if (update.Type != UpdateType.Message)
                return false;

            if (update.Message.Type != MessageType.Text)
                return false;

            if (update.Message == null)
                return false;

            if (string.IsNullOrWhiteSpace(update.Message.Text))
                return false;

            if (update.Message.Entities == null)
                return false;

            foreach (var entity in update.Message.Entities)
            {
                if (entity.Type == MessageEntityType.BotCommand)
                {
                    var splitMsg = update.Message.Text.ToLowerInvariant().Split(' ');

                    return Enum.TryParse(splitMsg[0].Replace(@"/", "").Replace("@lnkcapturebot", ""), true, out BotCommand command);
                }
            }

            return false;
        }

        public async Task ExecuteCommandAsync(Update update)
        {
            if (!IsBotCommand(update))
                return;

            if (update.Message.Chat.Type == ChatType.Private)
                await ExecutePrivateChatCommandAsync(update);
            else
                await ExecuteNonPrivateChatCommandAsync(update);
        }

        private BotCommand ResolveBotCommand(Update update)
        {
            foreach (var entity in update.Message.Entities)
            {
                if (entity.Type == MessageEntityType.BotCommand)
                {
                    var splitMsg = update.Message.Text.ToLowerInvariant().Split(' ');

                    if (Enum.TryParse(splitMsg[0].Replace(@"/", "").Replace("@lnkcapturebot", ""), true, out BotCommand command))
                        return command;
                }
            }

            return BotCommand.UnknowCommand;
        }

        private async Task ExecutePrivateChatCommandAsync(Update update)
        {
            try
            {
                var command = ResolveBotCommand(update);

                switch (command)
                {
                    case BotCommand.Start:

                        await _messageBO.SendWelcomeMessageAsync(true, update.Message.Chat.Id);

                        break;

                    case BotCommand.Help:

                        await _messageBO.SendHelpMessageAsync(true, update.Message.Chat.Id, update.Message.MessageId, update.Message.From.Id);

                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                await _messageBO.SendBotOptionNotSetMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
            }
        }

        private async Task ExecuteNonPrivateChatCommandAsync(Update update)
        {
            try
            {
                var command = ResolveBotCommand(update);

                switch (command)
                {
                    case BotCommand.Start:

                        await _messageBO.SendWelcomeMessageAsync(false, update.Message.Chat.Id);

                        break;

                    case BotCommand.LinksUrl:

                        await _messageBO.SendLinksRecoverMessageAsync(update.Message.Chat.Id, update.Message.From.Id, update.Message.Chat.Title, update.Message.MessageId);

                        break;

                    case BotCommand.Help:

                        await _messageBO.SendHelpMessageAsync(false, update.Message.Chat.Id, update.Message.MessageId, update.Message.From.Id);

                        break;

                    case BotCommand.DisableSavedMsg:
                    case BotCommand.DisableLinkAlreadyExistsMsg:
                    case BotCommand.DisableInvalidLinkMsg:
                    case BotCommand.EnableSavedMsg:
                    case BotCommand.EnableLinkAlreadyExistsMsg:
                    case BotCommand.EnableInvalidLinkMsg:

                        var adminUsers = await BotClient.GetChatAdministratorsAsync(update.Message.Chat.Id);

                        if (!adminUsers.Any(p => p.User.Id == update.Message.From.Id))
                        {
                            await _messageBO.SendBotOptionNotAllowedMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                            return;
                        }

                        await _configBO.SaveAsync(update.Message.Chat.Id, command);
                        await _messageBO.SendBotOptionSetMessageAsync(update.Message.Chat.Id, update.Message.MessageId, command);

                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                await _messageBO.SendBotOptionNotSetMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
            }
        }
    }
}