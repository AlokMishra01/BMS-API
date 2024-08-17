using BMS_API.Extensions;
using FluentEmail.Core.Interfaces;
using FluentEmail.Smtp;
using System.Net.Mail;

namespace BMS_API.Extensions
{
    public static class FluentEmailExtensions
    {
        public static void AddFluentEmail(this IServiceCollection services, ConfigurationManager configuration)
        {
            var emailSetting = configuration.GetSection("EmailSettings");
            var defaultFromEmail = emailSetting["DefaultFromEmail"];
            var host = emailSetting["Host"];
            var port = emailSetting.GetValue<int>("Port");
            services.AddFluentEmail(defaultFromEmail);
            services.AddSingleton<ISender>(x => new SmtpSender(new SmtpClient(host, port)));
        }
    }
}
