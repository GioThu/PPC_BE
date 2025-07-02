using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Utils
{
    public static class Utils
    {

        public static string GenerateIdModel(string model)
        {
            string randomString = Guid.NewGuid().ToString("N").Substring(0, 10);

            return $"{model}_{randomString}";
        }
        public static DateTime GetTimeNow()
        {
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            return vietnamTime;
        }

        public static string GetNameUnderscore(string input)
        {
            // Replace spaces with underscores
            string output = input.Replace(" ", "_");
            return output;
        }

        public static string GenerateAccessCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 9)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
