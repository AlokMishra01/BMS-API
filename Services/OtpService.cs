using Microsoft.Extensions.Caching.Memory;

namespace BMS_API.Services
{
    public class OtpService
    {
        private readonly IMemoryCache _memoryCache;

        public OtpService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public string GenerateOtp(string key)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            _memoryCache.Set(key, otp, TimeSpan.FromMinutes(10)); // Store OTP in cache for 10 minutes
            return otp;
        }

        public bool ValidateOtp(string key, string otp)
        {
            return _memoryCache.TryGetValue(key, out string cachedOtp) && cachedOtp == otp;
        }

        public void RemoveOtp(string key)
        {
            _memoryCache.Remove(key);
        }
    }

}
