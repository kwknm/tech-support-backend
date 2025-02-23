namespace TechSupport.Contracts.Requests;

public record SignInRequest(string Email, string Password, bool StayLoggedIn = false);