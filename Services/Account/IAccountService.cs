using BMS_API.Models;
using BMS_API.Models.DTOs;
using System.Security.Claims;

namespace BMS_API.Services.Account
{
    public interface IAccountService
    {
        Task<ApiResponse<bool>> CheckUsernameAvailabilityAsync(string username);
        Task<ApiResponse<bool>> RegisterAsync(string userName, string email, string password);
        Task<ApiResponse<string>> ConfirmEmailAsync(string email, string otp);
        Task<ApiResponse<LoginResponseDTO>> LoginAsync(string usernameOrEmail, string password);
        Task<ApiResponse<LoginResponseDTO>> RefreshTokenAsync(string token);
        Task<ApiResponse<string>> ChangePasswordAsync(ClaimsPrincipal claimsPrincipal, string currentPassword, string newPassword);
        Task<ApiResponse<string>> ForgotPasswordAsync(string email);
        Task<ApiResponse<string>> ResetPasswordAsync(string email, string otp, string newPassword);
        Task<ApiResponse<string>> LogoutAsync(ClaimsPrincipal claimsPrincipal);
        Task<ApiResponse<string>> RequestDeleteAccountOtpAsync(ClaimsPrincipal claimsPrincipal);
        Task<ApiResponse<string>> DeleteAccountAsync(ClaimsPrincipal claimsPrincipal, string otp);
    }
}
