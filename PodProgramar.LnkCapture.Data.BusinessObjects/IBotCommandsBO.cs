using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface IBotCommandsBO
    {
        bool IsBotCommand(Update update);
        Task ExecuteCommandAsync(Update update);
    }
}