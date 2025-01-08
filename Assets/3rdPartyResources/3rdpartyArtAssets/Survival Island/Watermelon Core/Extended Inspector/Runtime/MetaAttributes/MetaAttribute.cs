using System;

namespace Watermelon
{
    public abstract class MetaAttribute : Attribute
    {
        private int order = 0;

        public int Order
        {
            get
            {
                return this.order;
            }
            set
            {
                this.order = value;
            }
        }
    }
}