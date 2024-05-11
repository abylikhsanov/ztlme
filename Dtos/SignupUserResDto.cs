namespace ztlme.Dtos;

public class SignupUserResDto
{
    public bool CanBeSignedUp { get; set; } = false;
    public bool DocumentSigned { get; set; } = false;
    public bool? CreditScoreApiCalled { get; set; } = true;
}