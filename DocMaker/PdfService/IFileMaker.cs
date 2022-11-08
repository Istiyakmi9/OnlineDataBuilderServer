using ModalLayer.Modal;
using System.IO;

namespace DocMaker.PdfService
{
    public interface IFileMaker
    {
        FileDetail _fileDetail { set; get; }
    }
}
