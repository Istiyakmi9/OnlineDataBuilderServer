using BottomhalfCore.DatabaseLayer.Common.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using ModalLayer.Modal.Profile;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.IO;

namespace ServiceLayer.Code
{
    public class UserService : IUserService
    {
        private readonly IDb db;
        private readonly IFileService _fileService;
        private readonly FileLocationDetail _fileLocationDetail;

        public UserService(IDb db, IFileService fileService, IOptions<FileLocationDetail> options)
        {
            this.db = db;
            _fileService = fileService;
            _fileLocationDetail = options.Value;
        }

        public string UploadUserInfo(string userId, UserInfo userInfo, IFormFileCollection FileCollection)
        {
            if (string.IsNullOrEmpty(userInfo.Email))
            {
                throw new HiringBellException("Email id is required field.");
            }

            List<Files> files = new List<Files>
            {
                new Files
                {

                }
            };

            string folderPath = Path.Combine(_fileLocationDetail.Location)
            _fileService.SaveFile(null, files, FileCollection, userId);
            return null;
        }
    }
}
