using Asp.Versioning;
using BMS_API.Data.Entities;
using BMS_API.Models;
using BMS_API.Models.DTOs;
using IdentityManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMS_API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/businesses")]
    [ApiController]
    [Authorize]
    public class BusinessController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public BusinessController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/v1/businesses/businesses
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserBusinessDto>>>> GetUserBusinesses()
        {
            var userId = _userManager.GetUserId(User);

            // Fetch businesses associated with the logged-in user
            var userBusinesses = await _context.UserBusinessRoles
                .Where(ubr => ubr.UserId == userId)
                .Select(ubr => new UserBusinessDto
                {
                    BusinessId = ubr.BusinessId,
                    BusinessName = ubr.Business.Name,
                    Role = ubr.Role
                })
                .ToListAsync();

            if (userBusinesses == null || !userBusinesses.Any())
            {
                return NotFound(new ApiResponse<List<UserBusinessDto>>
                {
                    Success = false,
                    Message = "No businesses found for the user."
                });
            }

            return Ok(new ApiResponse<List<UserBusinessDto>>
            {
                Success = true,
                Message = "Businesses retrieved successfully.",
                Data = userBusinesses
            });
        }

        // GET: api/v1/businesses/{businessId}
        [HttpGet("{businessId:guid}")]
        public async Task<ActionResult<ApiResponse<BusinessDetailsDto>>> GetBusinessById(Guid businessId)
        {
            var userId = _userManager.GetUserId(User);

            // Fetch the business
            var business = await _context.Businesses
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                return NotFound(new ApiResponse<BusinessDetailsDto>
                {
                    Success = false,
                    Message = "Business not found."
                });
            }

            // Check if the user is associated with the business
            var userRole = await _context.UserBusinessRoles
                .Where(ubr => ubr.BusinessId == businessId && ubr.UserId == userId)
                .Select(ubr => ubr.Role)
                .FirstOrDefaultAsync();

            if (userRole == null)
            {
                return BadRequest(new ApiResponse<BusinessDetailsDto>
                {
                    Success = false,
                    Message = "You do not have permission to view this business.",
                    Errors = new List<string> { "User is not associated with the business." }
                });
            }

            // Construct the response object
            var response = new ApiResponse<BusinessDetailsDto>
            {
                Success = true,
                Message = "Business details fetched successfully.",
                Data = new BusinessDetailsDto
                {
                    Business = business,
                    Role = userRole
                }
            };

            return Ok(response);
        }

        // GET: api/v1/businesses/{businessId}/users
        [HttpGet("{businessId:guid}/users")]
        public async Task<ActionResult<ApiResponse<List<UserRoleDto>>>> GetBusinessUsers(Guid businessId)
        {
            var userId = _userManager.GetUserId(User);
            var userRole = await _context.UserBusinessRoles
                .Where(ubr => ubr.BusinessId == businessId && ubr.UserId == userId)
                .Select(ubr => ubr.Role)
                .FirstOrDefaultAsync();

            if (userRole == null)
            {
                return BadRequest(new ApiResponse<List<UserRoleDto>>
                {
                    Success = false,
                    Message = "You do not have permission to view users of this business.",
                    Errors = new List<string> { "User is not associated with the business." }
                });
            }

            // Fetch all users associated with the business
            var businessUsers = await _context.UserBusinessRoles
                .Include(ubr => ubr.User)
                .Where(ubr => ubr.BusinessId == businessId)
                .Select(ubr => new UserRoleDto
                {
                    UserId = ubr.User.Id,
                    UserName = ubr.User.UserName,
                    Role = ubr.Role
                })
                .ToListAsync();

            // Filter based on user role
            if (userRole == BusinessRole.Employee)
            {
                return BadRequest(new ApiResponse<List<UserRoleDto>>
                {
                    Success = false,
                    Message = "Employees cannot access user list.",
                    Errors = new List<string> { "Insufficient permissions." }
                });
            }
            else if (userRole == BusinessRole.AdminEmployee)
            {
                businessUsers = businessUsers.Where(ur => ur.Role == BusinessRole.Employee).ToList();
            }
            else if (userRole == BusinessRole.Owner)
            {
                businessUsers = businessUsers.Where(ur => ur.Role != BusinessRole.SuperOwner && ur.Role != BusinessRole.Owner).ToList();
            }
            else if (userRole == BusinessRole.SuperOwner)
            {
                businessUsers = businessUsers.Where(ur => ur.Role != BusinessRole.SuperOwner).ToList();
            }

            var response = new ApiResponse<List<UserRoleDto>>
            {
                Success = true,
                Message = "Business users fetched successfully.",
                Data = businessUsers
            };

            return Ok(response);
        }

        // POST: api/v1/businesses
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Business>>> CreateBusiness([FromBody] CreateBusinessDto dto)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound(new ApiResponse<User>
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            // Create new business
            var business = new Business
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };



            // Assign the logged-in user as the SuperOwner of the new business
            var userBusinessRole = new UserBusinessRole
            {
                UserId = userId,
                BusinessId = business.Id,
                Role = BusinessRole.SuperOwner
            };

            // Set the active business for the user
            user.ActiveBusinessId = business.Id;
            _context.Users.Update(user);

            // Save business and user role to the database
            _context.Businesses.Add(business);
            _context.UserBusinessRoles.Add(userBusinessRole);
            await _context.SaveChangesAsync();

            // Return success response
            return Ok(new ApiResponse<Business>
            {
                Success = true,
                Message = "Business created successfully.",
                Data = business
            });
        }

        // PUT: api/v1/businesses/{businessId}
        [HttpPut("{businessId:guid}")]
        [Authorize(Policy = "SuperOwnerOrOwnerPolicy")]
        public async Task<ActionResult<ApiResponse<Business>>> UpdateBusiness(Guid businessId, [FromBody] CreateBusinessDto dto)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound(new ApiResponse<User>
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            // Check if the business exists
            var business = await _context.Businesses
                .Include(b => b.UserBusinessRoles)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                return NotFound(new ApiResponse<Business>
                {
                    Success = false,
                    Message = "Business not found."
                });
            }

            // Check if the user is a SuperOwner or Owner of this business
            var userRole = business.UserBusinessRoles
                .FirstOrDefault(ubr => ubr.UserId == userId &&
                    (ubr.Role == BusinessRole.SuperOwner || ubr.Role == BusinessRole.Owner));

            if (userRole == null)
            {
                return BadRequest(new ApiResponse<Business>
                {
                    Success = false,
                    Message = "User does not have permission to update this business.",
                });
            }

            // Update business details
            business.Name = dto.Name;
            business.Description = dto.Description;
            business.Address = dto.Address;
            business.Phone = dto.Phone;
            business.Email = dto.Email;
            business.UpdatedAt = DateTime.UtcNow;

            _context.Businesses.Update(business);

            user.ActiveBusinessId = businessId;
            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<Business>
            {
                Success = true,
                Message = "Business updated successfully.",
                Data = business
            });
        }

        // POST: api/v1/businesses/{businessId}/add-user
        [HttpPost("{businessId:guid}/add-user")]
        public async Task<ActionResult<ApiResponse<string>>> AddUserToBusiness(Guid businessId, [FromBody] AddUserToBusinessDto dto)
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (currentUser == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            var currentRole = await _context.UserBusinessRoles
                .Where(ub => ub.UserId == currentUserId && ub.BusinessId == businessId)
                .Select(ub => ub.Role)
                .FirstOrDefaultAsync();

            if (currentRole == null)
            {
                return Forbid();
            }

            // Check if the role can add the new user
            if ((currentRole == BusinessRole.SuperOwner && !Enum.IsDefined(typeof(BusinessRole), dto.Role)) ||
                (currentRole == BusinessRole.Owner && dto.Role == BusinessRole.SuperOwner) ||
                (currentRole == BusinessRole.AdminEmployee && dto.Role == BusinessRole.SuperOwner) ||
                (currentRole == BusinessRole.Employee && dto.Role != BusinessRole.Employee))
            {
                return Forbid();
            }

            var newUser = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (newUser == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User to add not found."
                });
            }

            var existingRole = await _context.UserBusinessRoles
                .AnyAsync(ub => ub.UserId == newUser.Id && ub.BusinessId == businessId);

            if (existingRole)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User is already assigned to this business."
                });
            }

            if (currentRole == BusinessRole.SuperOwner && dto.Role == BusinessRole.SuperOwner)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "SuperOwner role cannot be assigned to another user."
                });
            }

            _context.UserBusinessRoles.Add(new UserBusinessRole
            {
                UserId = newUser.Id,
                BusinessId = businessId,
                Role = dto.Role
            });
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "User added to the business successfully."
            });
        }

        // DELETE: api/v1/businesses/{businessId}/remove-user/{userId}
        [HttpDelete("{businessId:guid}/remove-user/{userId}")]
        public async Task<ActionResult<ApiResponse<string>>> RemoveUserFromBusiness(Guid businessId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (currentUser == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            var currentRole = await _context.UserBusinessRoles
                .Where(ub => ub.UserId == currentUserId && ub.BusinessId == businessId)
                .Select(ub => ub.Role)
                .FirstOrDefaultAsync();

            if (currentRole == null)
            {
                return Forbid();
            }

            var roleToRemove = await _context.UserBusinessRoles
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BusinessId == businessId);

            if (roleToRemove == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User is not part of this business."
                });
            }

            // Prevent SuperOwner from removing themselves or other SuperOwners
            if (currentRole == BusinessRole.SuperOwner && roleToRemove.Role == BusinessRole.SuperOwner)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Cannot remove SuperOwner role."
                });
            }

            // Check if the role can remove the user
            if ((currentRole == BusinessRole.SuperOwner && (roleToRemove.Role == BusinessRole.SuperOwner || roleToRemove.Role == BusinessRole.Owner)) ||
                (currentRole == BusinessRole.Owner && (roleToRemove.Role == BusinessRole.SuperOwner || roleToRemove.Role == BusinessRole.Owner)) ||
                (currentRole == BusinessRole.AdminEmployee && roleToRemove.Role == BusinessRole.SuperOwner) ||
                (currentRole == BusinessRole.Employee))
            {
                return Forbid();
            }

            _context.UserBusinessRoles.Remove(roleToRemove);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "User removed from the business successfully."
            });
        }

        // DELETE: api/v1/businesses/{businessId}
        [HttpDelete("{businessId:guid}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteBusiness(Guid businessId)
        {
            var userId = _userManager.GetUserId(User);

            // Check if the logged-in user is a SuperOwner of the business
            var userBusinessRole = await _context.UserBusinessRoles
                .FirstOrDefaultAsync(ubr => ubr.BusinessId == businessId && ubr.UserId == userId && ubr.Role == BusinessRole.SuperOwner);

            if (userBusinessRole == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "You do not have permission to delete this business.",
                    Errors = new List<string> { "User is not a SuperOwner." }
                });
            }

            // Find the business
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Business not found."
                });
            }

            // Delete the business
            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Business deleted successfully.",
                Data = $"Business with Name {business.Name} has been deleted."
            });
        }
    }
}
