namespace DocMaker.HtmlToDocx
{
    public interface IHTMLConverter
    {
        string ToDocx(string templatePath, string destinationFolder, string headerLogoPath);
    }
}
