using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;
using WebApiAutores.Services;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/cuentas")]
    public class CuentasController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly HashService hashService;
        private readonly IDataProtector dataProtector;

        public CuentasController(UserManager<IdentityUser> userManager, IConfiguration configuration, SignInManager<IdentityUser> signInManager,
            IDataProtectionProvider dataProtectionProvider, HashService hash)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.hashService = hash;
            dataProtector = dataProtectionProvider.CreateProtector("valor_unico_llave");
        }

        [HttpGet("hash/{textoPlano}")]
        public ActionResult RealizarHash(string textoPlano)
        {
            var resultado1 = hashService.Hash(textoPlano);
            var resultado2 = hashService.Hash(textoPlano);

            return Ok(new
            {
                textoPlano = textoPlano,
                Hash1 = resultado1,
                Hash22 = resultado2
            });
        }

        [HttpGet("Encriptar")]
        public ActionResult Encriptar()
        {
            var textoPlano = "Jhon James";
            var textoEncriptado = dataProtector.Protect(textoPlano);
            var textoDesencriptado = dataProtector.Unprotect(textoEncriptado);

            return Ok(new { textoPlano = textoPlano, textoEncriptado = textoEncriptado, textoDesencriptado = textoDesencriptado });
        }

        [HttpGet("EncriptarTiempo")]
        public ActionResult EncriptarTiempo()
        {
            var protectorTiempo = dataProtector.ToTimeLimitedDataProtector();
            var textoPlano = "Jhon James";
            var textoEncriptado = protectorTiempo.Protect(textoPlano, lifetime: TimeSpan.FromSeconds(5));
            Thread.Sleep(6000);
            var textoDesencriptado = protectorTiempo.Unprotect(textoEncriptado);

            return Ok(new { textoPlano = textoPlano, textoEncriptado = textoEncriptado, textoDesencriptado = textoDesencriptado });
        }

        [HttpPost("registrar", Name = "registrarUsuario")] // api/cuentas/registrar
        public async Task<ActionResult<RespuestaAutenticacion>> Post([FromBody]CredencialesUsuario CredencialesUsuario)
        {
            var user = new IdentityUser 
            { UserName = CredencialesUsuario.Email, Email = CredencialesUsuario.Email };

            var resultado = await userManager.CreateAsync(user, CredencialesUsuario.Password);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(CredencialesUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost("Login", Name = "loginUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login([FromBody]CredencialesUsuario credencialesUsuario)
        {
            var result = await signInManager.PasswordSignInAsync(credencialesUsuario.Email, 
                credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest("Login Incorrecto");
            }
        }

        [HttpGet("RenovarToken", Name = "renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var credencialesUsuario = new CredencialesUsuario()
            {
                Email = email
            };

            return await ConstruirToken(credencialesUsuario);
        }

        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuario.Email),
                new Claim("Prueba", "Prueba de claim")

            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            var claimsDB = await userManager.GetClaimsAsync(usuario);

            claims.AddRange(claimsDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["keyJWT"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddDays(1);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiration, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiration = expiration
            };
        }

        [HttpPost("HacerAdmin", Name = "hacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.AddClaimAsync(usuario, new Claim("isAdmin", "1"));
            return NoContent();
        }

        [HttpPost("RemoverAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByIdAsync(editarAdminDTO.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("isAdmin", "1"));
            return NoContent();
        }
    }
}
