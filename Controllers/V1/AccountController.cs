using BMS_API.Models;
using BMS_API.Models.DTOs;
using BMS_API.Services.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS_API.Controllers.V1
{
    [ApiController]
    [Route("api/account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("check-username-availability/{username}")]
        public async Task<IActionResult> CheckUsernameAvailability(string username)
        {
            var response = await _accountService.CheckUsernameAvailabilityAsync(username);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var response = await _accountService.RegisterAsync(model.UserName, model.UserName, model.Password);
            return Ok(response);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmailWithOtp([FromBody] ConfirmEmailWithOtpRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var response = await _accountService.ConfirmEmailAsync(model.Email, model.Otp);
            return Ok(response);
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Invalid login data."
                });
            }

            var response = await _accountService.LoginAsync(model.UsernameOrEmail, model.Password);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var response = await _accountService.RefreshTokenAsync(request.Token);
            return Ok(response);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var response = await _accountService.ChangePasswordAsync(User, model.CurrentPassword, model.NewPassword);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var response = await _accountService.ForgotPasswordAsync(model.Email);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetOtpRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var response = await _accountService.ResetPasswordAsync(model.Email, model.Otp, model.NewPassword);
            return Ok(response);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            var response = await _accountService.LogoutAsync(User);
            return Ok(response);
        }

        [HttpGet("request-delete-account-otp")]
        public async Task<IActionResult> RequestDeleteAccountOtp()
        {
            var response = await _accountService.RequestDeleteAccountOtpAsync(User);
            return Ok(response);
        }

        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromBody] VerifyOtpAndDeleteAccountRequestDTO model)
        {
            var response = await _accountService.DeleteAccountAsync(User, model.Otp);
            return Ok(response);
        }
    }
}
