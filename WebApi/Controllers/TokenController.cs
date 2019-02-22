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
            var claims = userData["claims"].Split(',');

            if (IsValid(username, password))
            {
                var token = GenerateToken(username, claims);
                return new ObjectResult(token);
            }

            return BadRequest();
        }

        private object GenerateToken(string username, string[] userClaims)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "client"),

                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
            };

            claims.AddRange(userClaims.Select(userClaim => new Claim(userClaim.Trim(), "true")));

            var token = new JwtSecurityToken(
                    new JwtHeader(new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)),
                    new JwtPayload(claims));

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }

        private bool IsValid(string username, string password)
        {
            return true;
        }
    }
}