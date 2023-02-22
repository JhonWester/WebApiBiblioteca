namespace WebApiAutores.DTOs
{
    public class RespuestaAutenticacion
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
