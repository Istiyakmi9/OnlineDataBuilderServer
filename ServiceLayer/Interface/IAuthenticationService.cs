using ModalLayer.Modal;

namespace ServiceLayer.Interface
{
    public interface IAuthenticationService
    {
        public RefreshTokenModal Authenticate(long userId);
        RefreshTokenModal RenewAndGenerateNewToken(string Mobile, string Email);
        string ReadJwtToken();
    }
}
