namespace TechSupport.Services;

public interface IEncryptionService
{
    string AesEncrypt(string text);
    string AesDecrypt(string cipherText);
}