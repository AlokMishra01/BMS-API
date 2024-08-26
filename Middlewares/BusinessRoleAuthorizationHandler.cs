using BMS_API.Data.Entities;
using IdentityManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BMS_API.Middlewares
{
    public class BusinessRoleAuthorizationHandler : AuthorizationHandler<BusinessRoleRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public BusinessRoleAuthorizationHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BusinessRoleRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var businessIdClaim = context.User.FindFirst("BusinessId");

            if (userId == null || businessIdClaim == null)
            {
                return;
            }

            var businessId = Guid.Parse(businessIdClaim.Value);

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userBusinessRole = await dbContext.UserBusinessRoles
                    .FirstOrDefaultAsync(ubr => ubr.UserId == userId && ubr.BusinessId == businessId);

                if (userBusinessRole != null &&
                    (userBusinessRole.Role == BusinessRole.SuperOwner || userBusinessRole.Role == BusinessRole.Owner))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
