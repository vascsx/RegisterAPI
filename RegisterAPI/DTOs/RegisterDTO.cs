using System.ComponentModel.DataAnnotations;

namespace RegisterAPI.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "O campo 'Nome Completo' é obrigatório.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "O campo 'E-mail' é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail fornecido não é válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo 'Senha' é obrigatório.")]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
        public string Password { get; set; }
    }
}
