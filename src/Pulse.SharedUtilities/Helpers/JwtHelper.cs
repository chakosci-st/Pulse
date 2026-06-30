using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Pulse.SharedUtilities.Helpers
{
    public class JwtHelper
    {
        public static string GenerateToken(string username, string usergroups, string modulecodes,
            IDictionary<string, string> customClaims, // dynamic claims
            string secretkey, string issuer, string audience, int? expireMinutes = 30)
        {
            var claims = new List<Claim>    {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, usergroups),
                new Claim("ModuleCodes", modulecodes)
            };

            // Add dynamic custom claims
            if (customClaims != null)
            {
                foreach (var kvp in customClaims)
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes.Value), 
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
