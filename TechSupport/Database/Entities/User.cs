using Microsoft.AspNetCore.Identity;

namespace TechSupport.Database.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}