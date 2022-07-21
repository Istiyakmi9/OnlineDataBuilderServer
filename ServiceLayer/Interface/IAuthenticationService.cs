using ModalLayer.Modal;

namespace ServiceLayer.Interface
{
    public interface IAuthenticationService
    {
        public RefreshTokenModal Authenticate(long userId, long managerId, int roleId);
        RefreshTokenModal RenewAndGenerateNewToken(string Mobile, string Email, string UserRole);
        string ReadJwtToken();
        string Encrypt(string textOrPassword, string secretKey);
        string Decrypt(string encryptedText, string secretKey);
    }
}
