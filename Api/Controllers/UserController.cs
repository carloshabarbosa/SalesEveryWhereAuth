using Api.Common;
using Api.Model;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("user")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Esse método é utilzado para criar um usário
        /// </summary>
        /// <response code="200">Usuário criado</response>
        /// <response code="400">Caso ocorra algum erro de validação ou algum problema de conexão!</response>    
        [HttpPost()]
        public async Task<ActionResult> Post([FromBody] User user)
        {
            FirestoreDb db = FirestoreDb.Create("saleseverywhere-1f568");

            var collection = db.Collection("users");

            var userFirebase = new UserFirebase
            {
                UserName = user.UserName,
                Password = Crypt.CreateMD5(user.Password),
                BusinessUnit = user.BusinessUnit
            };

            await collection.Document(user.UserName).SetAsync(userFirebase);

            return Ok();
        }
    }
}