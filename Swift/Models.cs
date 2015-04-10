using System.Collections.Generic;

namespace Swift
{
    /// <summary>
    /// Defines a method that initializes the model object using a GtDataReader.
    /// </summary>
    public interface IPersistent
    {
        void Init(ISwiftDataReader reader);
    }
}
