using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class FilterModel
    {
        public bool? IsActive { get; set; }
        public string SearchString { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SortBy { get; set; }
    }
}
