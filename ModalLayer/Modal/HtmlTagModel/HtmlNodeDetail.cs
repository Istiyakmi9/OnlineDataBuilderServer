using System.Collections.Generic;

namespace ModalLayer.Modal.HtmlTagModel
{
    public class HtmlNodeDetail
    {
        public string TagName { set; get; }
        public string TagType { set; get; }
        public string Value { set; get; }
        public List<Pair> Properties { set; get; }
        public List<HtmlNodeDetail> ChildNodes { set; get; }
        public List<ComponentStyle> StyleSheet { set; get; }
    }

    public class ComponentStyle
    {
        public string ClassName { set; get; }
        public List<Pair> Value { set; get; }
    }

    public class Pair
    {
        public string Key { set; get; }
        public string Value { set; get; }
        public List<Pair> InlineStyle { set; get; }
    }
}
