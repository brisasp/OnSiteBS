namespace GestionAccesos.Services;
public interface ICryptoService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    byte[] EncryptBytes(byte[] data);
    byte[] DecryptBytes(byte[] data);
}