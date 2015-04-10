using System.Web.UI;

namespace Swift
{
    public class TypedControl<T>
    {
        public T Abstraction { get; set; }
        public Control Implementation { get; set; }

        public TypedControl(T abstraction, Control implementation)
        {
            this.Abstraction = abstraction;
            this.Implementation = implementation;
        }
    }
}
