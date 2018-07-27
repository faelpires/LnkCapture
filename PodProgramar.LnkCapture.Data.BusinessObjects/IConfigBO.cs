using PodProgramar.LnkCapture.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface IConfigBO
    {
        Task<Config> GetAsync(long chatId);
        Task<Config> SaveAsync(long chatId, BotCommand botCommand);
    }
}
