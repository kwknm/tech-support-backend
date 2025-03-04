using System.Security.Cryptography;
using System.Text;

namespace TechSupport.Services;

public class EncryptionService : IEncryptionService
{
    private readonly IConfiguration _configuration;

    public EncryptionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Encrypt(string text)
    {
        
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_configuration.GetSection("Aes:Key").Get<string>()!);
        aes.IV = Encoding.UTF8.GetBytes(_configuration.GetSection("Aes:IV").Get<string>()!);
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(text);
        }
        var encryptedBytes = msEncrypt.ToArray();
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string cipherText)
    {
        var cipherBytes = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_configuration.GetSection("Aes:Key").Get<string>()!);
        aes.IV = Encoding.UTF8.GetBytes(_configuration.GetSection("Aes:IV").Get<string>()!);
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }
}