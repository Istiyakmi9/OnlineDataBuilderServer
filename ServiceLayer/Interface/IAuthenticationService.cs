using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IAuthenticationService
    {
        public RefreshTokenModal Authenticate(string userId);
        RefreshTokenModal RenewAndGenerateNewToken();
    }
}
