using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Common;
using Api.Model;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AuthController(IConfiguration confg,
        IWebHostEnvironment webHostEnvironment)
        {
            _config = confg;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Esse método é utilzado para realizar o login na aplicação
        /// </summary>
        /// <response code="200">Login realizado</response>
        /// <response code="400">Caso ocorra algum erro de validação ou algum problema de conexão!</response>    
        [HttpPost("login")]
        public async Task<ActionResult> Post([FromBody] UserLoginRequest user)
        {
            try
            {
                string path = _webHostEnvironment.ContentRootPath;

                string filename = "firestore-auth.json";

                string fullfile = Path.Combine(path, filename);

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullfile);

                Console.WriteLine(fullfile);

                FirestoreDb db = FirestoreDb.Create("saleseverywhere-1f568");

                var collection = db.Collection("users");

                var docRef = collection.Document(user.UserName);

                var snapshot = await docRef.GetSnapshotAsync();

                var password = snapshot.GetValue<string>("password");
                var inputPasswordCrypt = Crypt.CreateMD5(user.Password);

                if (inputPasswordCrypt == password)
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Unauthorized();
            }
        }
    }
}