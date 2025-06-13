using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PPC.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateCounselorToken(string accountId, string counselorId, string fullname, int? role, string avartar)
        {
            if (avartar == null)
                avartar = "not found";

            // Check cấu hình
            var secretKey = _configuration["JWTSettings:SecretKey"];
            var issuer = _configuration["JWTSettings:Issuer"];
            var audience = _configuration["JWTSettings:Audience"];
            var expire = _configuration["JWTSettings:ExpireMinutes"];

            if (string.IsNullOrEmpty(secretKey)) throw new Exception("JWT SecretKey is missing");
            if (string.IsNullOrEmpty(issuer)) throw new Exception("JWT Issuer is missing");
            if (string.IsNullOrEmpty(audience)) throw new Exception("JWT Audience is missing");
            if (string.IsNullOrEmpty(expire)) throw new Exception("JWT ExpireMinutes is missing");

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, fullname),
        new Claim(ClaimTypes.Role, role.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("accountId", accountId),
        new Claim("counselorId", counselorId),
        new Claim("avartar", avartar)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(expire)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
