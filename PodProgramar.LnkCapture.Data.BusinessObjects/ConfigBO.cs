using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodProgramar.LnkCapture.Data.DAL;
using PodProgramar.LnkCapture.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class ConfigBO : BaseDataBO, IConfigBO
    {
        public ConfigBO(LnkCaptureContext lnkCaptureContext, IConfiguration configuration, ILogger<ConfigBO> logger) : base(lnkCaptureContext, configuration, logger)
        {
            Logger = logger;
        }

        public async Task<Config> GetAsync(long chatId)
        {
            try
            {
                var config = await Context.Config.SingleOrDefaultAsync(p => p.ChatId == chatId);

                if (config == null)
                {
                    return await SaveAsync(chatId, BotCommand.UnknowCommand);
                }
                else
                    return config;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        public async Task<Config> SaveAsync(long chatId, BotCommand botCommand)
        {
            try
            {
                var config = await Context.Config.SingleOrDefaultAsync(p => p.ChatId == chatId);

                if (config == null)
                {
                    config = new Config {
                        ConfigId = Guid.NewGuid(),
                        ChatId = chatId,
                        CultureId = Context.CultureInfo.Single(p => p.Culture == "en-US").CultureId,
                        EnableInvalidLinkMessage = true,
                        EnableLinkAlreadyExistsMessage = true,
                        EnableSavedMessage = true
                    };

                    Context.Config.Add(config);
                }

                switch (botCommand)
                {
                    case BotCommand.DisableSavedMsg:
                        config.EnableSavedMessage = false;
                        break;

                    case BotCommand.DisableLinkAlreadyExistsMsg:
                        config.EnableLinkAlreadyExistsMessage = false;
                        break;

                    case BotCommand.DisableInvalidLinkMsg:
                        config.EnableInvalidLinkMessage = false;
                        break;

                    case BotCommand.EnableSavedMsg:
                        config.EnableSavedMessage = true;
                        break;

                    case BotCommand.EnableLinkAlreadyExistsMsg:
                        config.EnableLinkAlreadyExistsMessage = true;
                        break;

                    case BotCommand.EnableInvalidLinkMsg:
                        config.EnableInvalidLinkMessage = true;
                        break;
                }

                await Context.SaveChangesAsync();

                return config;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }
    }
}