using BMS_API.Controllers.V1;
using BMS_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BMS_API.Middlewares
{
    public class CustomJwtBearerEvents : JwtBearerEvents
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TokenService _tokenService;

        public CustomJwtBearerEvents(IServiceProvider serviceProvider, TokenService tokenService)
        {
            _serviceProvider = serviceProvider;
            _tokenService = tokenService;
        }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var accountController = _serviceProvider.GetRequiredService<AccountController>();
            var token = context.SecurityToken.ToString();

            if (_tokenService.IsTokenBlacklisted(token))
            {
                context.Fail("Token has been revoked.");
            }

            await base.TokenValidated(context);
        }
    }

}
