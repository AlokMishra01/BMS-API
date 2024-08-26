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
    [Route("api/v{version:apiVersion}/profiles")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile-pictures");

        public UserProfileController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;

            // Ensure storage path exists
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        private async Task LogAuditAction(Guid userProfileId, string action, string actionDetails)
        {
            var auditLog = new AuditLog
            {
                UserProfileId = userProfileId,
                Action = action,
                ActionDetails = actionDetails,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        // GET: api/v1/profiles
        [HttpGet]
        public async Task<ActionResult<ApiResponse<UserProfile>>> GetUserProfile()
        {
            var userId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles
                .Include(up => up.PermanentAddress)
                .Include(up => up.TemporaryAddress)
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null)
            {
                return NotFound(new ApiResponse<UserProfile>
                {
                    Success = false,
                    Message = "User profile not found."
                });
            }

            return Ok(new ApiResponse<UserProfile>
            {
                Success = true,
                Message = "User profile retrieved successfully.",
                Data = userProfile
            });
        }

        // POST: api/v1/profiles/update
        [HttpPost("update")]
        public async Task<ActionResult<ApiResponse<UserProfile>>> CreateOrUpdateUserProfile(UserProfileDto dto)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

                if (userProfile == null)
                {
                    // Create a new profile
                    userProfile = new UserProfile
                    {
                        UserId = userId,
                        FirstName = dto.FirstName,
                        MiddleName = dto.MiddleName,
                        LastName = dto.LastName,
                        DateOfBirth = dto.DateOfBirth,
                        Bio = dto.Bio,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var savedUserProfile = _context.UserProfiles.Add(userProfile);
                    await LogAuditAction(savedUserProfile.Entity.Id, "Create Profile", $"Created profile for user {userId}");
                }
                else
                {
                    // Update existing profile
                    userProfile.FirstName = dto.FirstName;
                    userProfile.MiddleName = dto.MiddleName;
                    userProfile.LastName = dto.LastName;
                    userProfile.DateOfBirth = dto.DateOfBirth;
                    userProfile.Bio = dto.Bio;
                    userProfile.UpdatedAt = DateTime.UtcNow;

                    _context.UserProfiles.Update(userProfile);
                    await LogAuditAction(userProfile.Id, "Update Profile", $"Updated profile for user {userId}");
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<UserProfile>
                {
                    Success = true,
                    Message = "User profile updated successfully.",
                    Data = userProfile
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserProfile>
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // POST: api/v1/profiles/update-profile-picture
        [HttpPost("update-profile-picture")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateProfilePicture([FromForm] ProfilePictureUpdateDto dto)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

                if (userProfile == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User profile not found."
                    });
                }

                if (dto.ProfilePicture == null || dto.ProfilePicture.Length == 0)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No file uploaded."
                    });
                }

                var fileName = Path.GetFileName(dto.ProfilePicture.FileName);
                var filePath = Path.Combine(_storagePath, fileName);

                // Save the file to the server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfilePicture.CopyToAsync(fileStream);
                }

                // Update user profile with the file URL
                var fileUrl = $"/profile-pictures/{fileName}";
                userProfile.ProfilePictureUrl = fileUrl;

                _context.UserProfiles.Update(userProfile);
                await _context.SaveChangesAsync();

                await LogAuditAction(userProfile.Id, "Update Profile Picture", $"Updated profile picture URL to {fileUrl}");

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Profile picture updated successfully.",
                    Data = fileUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // PUT: api/v1/profiles/update-permanent-address
        [HttpPut("update-permanent-address")]
        public async Task<ActionResult<ApiResponse<object>>> UpdatePermanentAddress(AddressUpdateDto dto)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var userProfile = await _context.UserProfiles
                    .Include(up => up.PermanentAddress)
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                if (userProfile == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User profile not found."
                    });
                }

                var address = userProfile.PermanentAddress;
                if (address == null)
                {
                    // Create a new address
                    address = new Address
                    {
                        Street = dto.Street,
                        City = dto.City,
                        State = dto.State,
                        Country = dto.Country,
                        ZipCode = dto.ZipCode
                    };

                    _context.Addresses.Add(address);
                    userProfile.PermanentAddress = address;
                    _context.UserProfiles.Update(userProfile);
                    await LogAuditAction(userProfile.Id, "Create Permanent Address", $"Created permanent address for user {userId}");
                }
                else
                {
                    // Update existing address
                    address.Street = dto.Street;
                    address.City = dto.City;
                    address.State = dto.State;
                    address.Country = dto.Country;
                    address.ZipCode = dto.ZipCode;

                    _context.Addresses.Update(address);
                    await LogAuditAction(userProfile.Id, "Update Permanent Address", $"Updated permanent address for user {userId}");
                }

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // PUT: api/v1/profiles/update-temporary-address
        [HttpPut("update-temporary-address")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateTemporaryAddress(AddressUpdateDto dto)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var userProfile = await _context.UserProfiles
                    .Include(up => up.TemporaryAddress)
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                if (userProfile == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User profile not found."
                    });
                }

                var address = userProfile.TemporaryAddress;
                if (address == null)
                {
                    // Create a new address
                    address = new Address
                    {
                        Street = dto.Street,
                        City = dto.City,
                        State = dto.State,
                        Country = dto.Country,
                        ZipCode = dto.ZipCode
                    };

                    _context.Addresses.Add(address);
                    userProfile.TemporaryAddress = address;
                    _context.UserProfiles.Update(userProfile);
                    await LogAuditAction(userProfile.Id, "Create Temporary Address", $"Created temporary address for user {userId}");
                }
                else
                {
                    // Update existing address
                    address.Street = dto.Street;
                    address.City = dto.City;
                    address.State = dto.State;
                    address.Country = dto.Country;
                    address.ZipCode = dto.ZipCode;

                    _context.Addresses.Update(address);
                    await LogAuditAction(userProfile.Id, "Update Temporary Address", $"Updated temporary address for user {userId}");
                }

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
