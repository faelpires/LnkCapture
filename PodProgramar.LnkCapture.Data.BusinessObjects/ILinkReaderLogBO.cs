using System;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public interface ILinkReaderLogBO
    {
        Task SaveLogAsync(Guid linkReaderId, bool IsAPIRequest, int rowCount);
    }
}