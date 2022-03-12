using BottomhalfCore.DatabaseLayer.Common.Code;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Code
{
    public class UserService: IUserService
    {
        private readonly IDb db;
        private readonly IFileService _fileService;


        public UserService(IDb db, IFileService fileService)
        {
            this.db = db;
            _fileService = fileService;
        }

        public string UploadResume(Files fileDetail, IFormFileCollection FileCollection)
        {
            return null;
        }
    }
}
