using Microsoft.AspNetCore.Authorization;

namespace BMS_API.Middlewares
{
    public class BusinessRoleRequirement : IAuthorizationRequirement
    {
        public BusinessRoleRequirement() { }
    }
}
