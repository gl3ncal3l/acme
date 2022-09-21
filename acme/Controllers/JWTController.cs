using acme.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace acme.Controllers
{
    [ApiController]
    [Route("acme/[controller]")]
    public class JWTController : ControllerBase
    {
        private IConfiguration _configuration;

        public JWTController(IConfiguration config)
        {
            _configuration = config;

        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] Usuario usuario)
        {
            var user = BuscarUsuario(usuario);
            if (user != null)
            {
                var token = GenerarToken(user);
                return Ok(token);
            }
            return NotFound("Usuario o contraseña incorrecta!");

        }

        private Usuario BuscarUsuario(Usuario userLogin)
        {
            var currentUser = UsuariosConstantes.listaUsuarios.FirstOrDefault(usuario => usuario.usuario.ToLower() == userLogin.usuario.ToLower() && usuario.contrasenia == userLogin.contrasenia);
            if (currentUser != null)
            {
                return currentUser;
            }
            return null;
        }

        private string GenerarToken(Usuario user)
        {
            // Obtenemos la clave secreta
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.usuario),

            };
            // Creamos el token
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);
            // Retornamos el token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
