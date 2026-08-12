namespace Store.Models.DTOs.Auth;

public class FidoCredentialDto
{
    public int Id { get; set; }
    public string CredentialType { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string AaGuid { get; set; } = string.Empty;
}
