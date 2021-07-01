﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class DocumentFile
    {
        public int FileUid { set; get; }
        public int DocumentId { set; get; }
    }

    public class Files : DocumentFile
    {
        public string FilePath { set; get; }
        public string FileName { set; get; }
        public string FileExtension { set; get; }
    }
}