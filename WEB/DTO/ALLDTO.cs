namespace WEB.DTO
{
    public class ALLDTO
    {
        public record AuthRequestDTO(string Email, string Password);
        public record EditProductDTO(string Name, string Description, string CategoryName);
    }
}
