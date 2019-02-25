using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : Controller
    {
        private IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenController(IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenSecret"]));
        }

        [Route("/token")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost]
        public IActionResult Create([FromBody] JObject user)
        {
            var userData = user.ToObject<Dictionary<string, string>>();
            var username = userData["username"];
            var password = userData["password"];

            if (IsValid(username, password))
            {
                var token = GenerateToken(username);
                return new ObjectResult(token);
            }

            return BadRequest();
        }

        private object GenerateToken(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "client"),

                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
            };

            var userClaims = GetHashedPasswordAndClaimsFor(username).claims;

            claims.AddRange(userClaims.Select(userClaim => new Claim(userClaim.Trim(), "true")));

            var token = new JwtSecurityToken(
                    new JwtHeader(new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)),
                    new JwtPayload(claims));

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }

        private bool IsValid(string username, string password)
        {
            var hashedPassword = GetHashedPasswordAndClaimsFor(username).password;

            var hasher = new PasswordHasher<string>();
            // var hashed = hasher.HashPassword(username, password);

            var result = hasher.VerifyHashedPassword(username, hashedPassword, password);

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                Log.Warning($"{username}'s password was hased using a depricated algorithm - update it.");
                result = PasswordVerificationResult.Success;
            }

            return result == PasswordVerificationResult.Success;
        }

        private (string password, string[] claims) GetHashedPasswordAndClaimsFor(string username)
        {
            var user = _config.GetSection($"Users:{username}");
            var hashedPassword = user.GetValue<string>("Password");
            var userClaims = !string.IsNullOrWhiteSpace(user.GetValue<string>("Claims"))
                ? user.GetValue<string>("Claims").Split(',')
                : new string[0];
            return (hashedPassword, userClaims);
        }
    }
}