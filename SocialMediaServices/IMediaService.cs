using SocialMediaServices.Modal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaServices
{
    public interface IMediaService
    {
        Task<GoogleResponseModal> FetchUserProfileByAccessToken(string AccessToken);
    }
}
