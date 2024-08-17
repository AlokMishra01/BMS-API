using Asp.Versioning;
using BMS_API.Data.Entities;
using BMS_API.Models.DTOs;
using BMS_API.Services;
using IdentityManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMS_API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<SystemUser> _userManager;
        private readonly SignInManager<SystemUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly OtpService _otpService;
        private readonly TokenService _tokenService;

        public AccountController(
            UserManager<SystemUser> userManager,
            SignInManager<SystemUser> signInManager,
            ApplicationDbContext context,
            IEmailSender emailSender,
            OtpService otpService,
            TokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
            _otpService = otpService;
            _tokenService = tokenService;
        }

        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameAvailability(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is required.");

            var user = await _userManager.FindByNameAsync(username);
            var isAvailable = user == null;

            return Ok(new
            {
                isAvailable,
                message = isAvailable ? "Username is available." : "Username is already taken."
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new SystemUser { UserName = model.UserName, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return Ok(new { result = "User created successfully!" });

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = model.UsernameOrEmail.Contains("@")
                ? await _userManager.FindByEmailAsync(model.UsernameOrEmail)
                : await _userManager.FindByNameAsync(model.UsernameOrEmail);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or email." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
            {
                var accessToken = _tokenService.GenerateJwtToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                return Ok(new { token = accessToken, refreshToken = refreshToken.Token });
            }

            return Unauthorized(new { message = "Invalid password." });
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO request)
        {
            if (string.IsNullOrEmpty(request?.Token))
                return BadRequest("Invalid refresh token.");

            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == request.Token);

            if (storedToken == null || storedToken.IsRevoked || storedToken.IsUsed || storedToken.ExpiryDate < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token.");

            storedToken.IsUsed = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            var newJwtToken = _tokenService.GenerateJwtToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return Ok(new { token = newJwtToken, refreshToken = newRefreshToken.Token });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized("User not found.");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password changed successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Ok(new { message = "If the email is registered, a password reset OTP will be sent." });

            var otp = _otpService.GenerateOtp(model.Email);
            await _emailSender.SendEmailAsync(model.Email, "Password Reset OTP", $"Your OTP is: {otp}");

            return Ok(new { message = "If the email is registered, a password reset OTP will be sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetOtpRequestDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_otpService.ValidateOtp(model.Email, model.Otp))
                return BadRequest("Invalid or expired OTP.");

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("User not found.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            _otpService.RemoveOtp(model.Email);

            return Ok(new { message = "Password has been reset successfully." });
        }

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                return BadRequest("Authorization header missing or invalid.");

            var accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();
            _tokenService.BlacklistToken(accessToken);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == user.Id && !rt.IsUsed && !rt.IsRevoked);

            if (refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Logout successful." });
        }

        [HttpPost("request-delete-account-otp")]
        [Authorize]
        public async Task<IActionResult> RequestDeleteAccountOtp()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var otp = _otpService.GenerateOtp(user.Email);
            await _emailSender.SendEmailAsync(user.Email, "Delete Account OTP", $"Your OTP is: {otp}");

            return Ok(new { message = "OTP has been sent to your email." });
        }

        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] VerifyOtpAndDeleteAccountRequestDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized("User not found.");

            if (!_otpService.ValidateOtp(user.Email, model.Otp))
                return BadRequest("Invalid or expired OTP.");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            _otpService.RemoveOtp(user.Email);

            return Ok(new { message = "Account has been deleted successfully." });
        }
    }
}
