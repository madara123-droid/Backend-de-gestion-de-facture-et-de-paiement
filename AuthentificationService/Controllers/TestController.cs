using AuthentificationSerice.Core.Interfaces.Repositories;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;


namespace AuthentificationService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class TestController : ControllerBase
    {
        private readonly IUserRepository _userRepository;


        public TestController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("check-email/{email}")]// cela veut dire que la methode ci-dessous est expose comme une route HTTP et prend comme parametre dans l'url email
        public async Task<IActionResult> checkEmail(string email)
        {
            var exists = await _userRepository.EmailExistAsync(email);
            return Ok(new {email, exists });
        }
    }
}
