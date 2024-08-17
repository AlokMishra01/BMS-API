using BMS_API.Models;

namespace BMS_API.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Send an email
        /// </summary>
        /// <parma name="emailMessage">Message object to be sent</parma>
        Task Send(EmailMessageModel emailMessage);
    }
}
