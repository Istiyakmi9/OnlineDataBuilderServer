using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class JwtSetting
    {
        public string Key { set; get; }
        public string Issuer { get; set; }
        public long AccessTokenExpiryTimeInHours { set; get; }
        public long RefreshTokenExpiryTimeInHours { set; get; }
    }
}
