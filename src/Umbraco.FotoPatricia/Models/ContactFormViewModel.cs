using System.ComponentModel.DataAnnotations;

namespace Umbraco.FotoPatricia.Models;

public class ContactFormViewModel 
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    public virtual string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MinLength(10, ErrorMessage = "Mindestend 10 Zeichen sollte die Nachricht schon beinhalten!")]
    public string Message { get; set; } = string.Empty;
}