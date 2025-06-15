using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PPC.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace PPC.Service.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GenerateAccountToken(string accountId, string id, string fullname, int? role, string avartar, string idClaimName)
        {
            if (avartar == null)
                avartar = "not found";

            var secretKey = _configuration["JWTSettings:SecretKey"];

            var issuer = _configuration["JWTSettings:Issuer"];

            var expire = _configuration["JWTSettings:ExpireMinutes"];

            var audiences = _configuration.GetSection("JWTSettings:Audiences").Get<List<string>>()
                    ?? new List<string>();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, fullname),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("accountId", accountId),
                new Claim(idClaimName, id),
                new Claim("avartar", avartar)
            };
            foreach (var aud in audiences)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, aud)); // thêm nhiều aud
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(expire)),
                signingCredentials: creds,
                claims: claims
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateCounselorToken(string accountId, string counselorId, string fullname, int? role, string avartar)
        {
            return GenerateAccountToken(accountId, counselorId, fullname, role, avartar, "counselorId");

        }

        public string GenerateMemberToken(string accountId, string memberId, string fullname, int? role, string avartar)
        {
            return GenerateAccountToken(accountId, memberId, fullname, role, avartar, "memberId");

        }

        public string GenerateAdminToken(string accountId, int? role)
        {
            var adminId = accountId;
            var fullname = "Administrator";
            var avartar = "not found";
            return GenerateAccountToken(accountId, adminId, fullname, role, avartar, "adminId");

        }
    }
}
