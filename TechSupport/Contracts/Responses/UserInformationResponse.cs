namespace TechSupport.Contracts.Responses;

public class UserInformationResponse
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public bool IsSupport { get; set; }
}