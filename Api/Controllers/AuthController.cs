using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers
{
    [ApiController]
    [Route("auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration confg)
        {
            _config = confg;
        }

        /// <summary>
        /// Esse método é utilzado para realizar o login na aplicação
        /// </summary>
        /// <response code="200">Login realizado</response>
        /// <response code="400">Caso ocorra algum erro de validação ou algum problema de conexão!</response>    
        [HttpPost("login")]
        public async Task<ActionResult> Post([FromBody] UserLoginRequest user)
        {
            if (user.UserName == "teste" && user.Password == "teste123")
            {
                var issuer = _config.GetValue<string>("Jwt:Issuer");
                var audience = _config.GetValue<string>("Jwt:Audience");
                var key = Encoding.ASCII.GetBytes
                (_config.GetValue<string>("Jwt:Key"));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
             }),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                var stringToken = tokenHandler.WriteToken(token);
                return Ok(stringToken);
            }
            return Unauthorized();
        }
    }
}