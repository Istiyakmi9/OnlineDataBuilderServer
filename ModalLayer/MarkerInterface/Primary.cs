using System;

namespace ModalLayer.MarkerInterface
{
    public class Primary : Attribute
    {
        private readonly string _key;
        public Primary(string key)
        {
            _key = key;
        }

        public string GetName()
        {
            return _key;
        }

        public Primary() { }
    }
}
