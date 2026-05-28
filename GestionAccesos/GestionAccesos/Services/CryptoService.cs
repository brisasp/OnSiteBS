using System.Text;

namespace GestionAccesos.Services;

public class CryptoService : ICryptoService
{
    private readonly AesEncryptionService _aes;

    public CryptoService(IConfiguration config)
    {
        var key = config["Encryption:Key"];
        var iv = config["Encryption:IV"];
        _aes = new AesEncryptionService(key!, iv!);
    }
    public string Encrypt(string plainText) => _aes.Encrypt(plainText);
    public string Decrypt(string cipherText) => _aes.Decrypt(cipherText);
    public byte[] EncryptBytes(byte[] data)
    {
        var base64 = Convert.ToBase64String(data);
        var encrypted = Encrypt(base64);
        return Encoding.UTF8.GetBytes(encrypted);
    }
    public byte[] DecryptBytes(byte[] data)
    {
        var encryptedText = Encoding.UTF8.GetString(data);
        var base64 = Decrypt(encryptedText);
        return Convert.FromBase64String(base64);
    }
}