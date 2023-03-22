using System;

namespace ModalLayer.MarkerInterface
{
    public class Table : Attribute
    {
        public string _name;
        public Table(string name)
        {
            _name = name;
        }
    }
}
