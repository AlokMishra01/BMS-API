using BMS_API.Data.Entities;
using BMS_API.Helpers;
using BMS_API.Models;
using BMS_API.Models.DTOs;
using IdentityManager.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace BMS_API.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<SystemUser> _userManager;
        private readonly SignInManager<SystemUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly OtpService _otpService;
        private readonly TokenService _tokenService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            UserManager<SystemUser> userManager,
            SignInManager<SystemUser> signInManager,
            ApplicationDbContext context,
            IEmailSender emailSender,
            OtpService otpService,
            TokenService tokenService,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
            _otpService = otpService;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> CheckUsernameAvailabilityAsync(string username)
        {
            var match = Regex.Match(username, "^[^\\s]{3,50}$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Username must be between 3 and 50 characters with no spaces.",
                    Data = false
                };
            }

            var user = await _userManager.FindByNameAsync(username);
            var isAvailable = user == null;

            return new ApiResponse<bool>
            {
                Success = true,
                Message = isAvailable ? "Username is available." : "Username is already taken.",
                Data = isAvailable
            };
        }

        public async Task<ApiResponse<bool>> RegisterAsync(string userName, string email, string password)
        {
            var user = new SystemUser { UserName = userName, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var otp = _otpService.GenerateOtp(user.Email);
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Confirm your email",
                    $"Your OTP for email confirmation is: {otp}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "User created successfully!",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "User was not created!",
                Data = false
            };
        }

        public async Task<ApiResponse<string>> ConfirmEmailAsync(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found.",
                };
            }

            if (!_otpService.ValidateOtp(email, otp))
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid or expired OTP.",
                };
            }

            // Confirm email
            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Email confirmation failed.",
                };
            }

            // Remove OTP
            _otpService.RemoveOtp(email);

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Email confirmed successfully.",
            };
        }

        public async Task<ApiResponse<LoginResponseDTO>> LoginAsync(string usernameOrEmail, string password)
        {
            var user = EmailValidator.IsValidEmail(usernameOrEmail)
                ? await _userManager.FindByEmailAsync(usernameOrEmail)
                : await _userManager.FindByNameAsync(usernameOrEmail);

            if (user == null || !user.EmailConfirmed)
            {
                return new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Invalid username or email."
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                return new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Invalid password."
                };
            }

            var accessToken = _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new ApiResponse<LoginResponseDTO>
            {
                Success = true,
                Message = "Login successful.",
                Data = new LoginResponseDTO
                {
                    Token = accessToken,
                    RefreshToken = refreshToken.Token
                }
            };
        }

        public async Task<ApiResponse<LoginResponseDTO>> RefreshTokenAsync(string token)
        {
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);

            if (storedToken == null || storedToken.IsRevoked || storedToken.IsUsed || storedToken.ExpiryDate < DateTime.UtcNow)
            {
                return new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Invalid or expired refresh token."
                };
            }

            storedToken.IsUsed = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            var newJwtToken = _tokenService.GenerateJwtToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return new ApiResponse<LoginResponseDTO>
            {
                Success = true,
                Message = "Token regenerated successfully!",
                Data = new LoginResponseDTO
                {
                    Token = newJwtToken,
                    RefreshToken = newRefreshToken.Token
                }
            };
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(ClaimsPrincipal claimsPrincipal, string currentPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(claimsPrincipal);
            if (user == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Password could not be changed.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Password changed successfully."
            };
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var otp = _otpService.GenerateOtp(email);
            await _emailSender.SendEmailAsync(email, "Password Reset OTP", $"Your OTP is: {otp}");

            return new ApiResponse<string>
            {
                Success = true,
                Message = "If the email is registered, a password reset OTP will be sent."
            };
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            if (!_otpService.ValidateOtp(email, otp))
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid or expired OTP."
                };
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (!result.Succeeded)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Password could not be reset.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            _otpService.RemoveOtp(email);

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Password has been reset successfully."
            };
        }

        public async Task<ApiResponse<string>> LogoutAsync(ClaimsPrincipal claimsPrincipal)
        {
            var user = await _userManager.GetUserAsync(claimsPrincipal);
            if (user == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == user.Id && !rt.IsUsed && !rt.IsRevoked);
            if (refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync();
            }

            await _signInManager.SignOutAsync();

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Successfully logged out."
            };
        }

        public async Task<ApiResponse<string>> RequestDeleteAccountOtpAsync(ClaimsPrincipal claimsPrincipal)
        {
            var user = await _userManager.GetUserAsync(claimsPrincipal);
            if (user == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var otp = _otpService.GenerateOtp(user.Email);
            await _emailSender.SendEmailAsync(user.Email, "Account Deletion OTP", $"Your OTP is: {otp}");

            return new ApiResponse<string>
            {
                Success = true,
                Message = "OTP for account deletion has been sent to your email."
            };
        }

        public async Task<ApiResponse<string>> DeleteAccountAsync(ClaimsPrincipal claimsPrincipal, string otp)
        {
            var user = await _userManager.GetUserAsync(claimsPrincipal);
            if (user == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            if (!_otpService.ValidateOtp(user.Email, otp))
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid or expired OTP."
                };
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to delete the account.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            _otpService.RemoveOtp(user.Email);

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Account deleted successfully."
            };
        }
    }
}