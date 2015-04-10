using System;

namespace Swift
{
    /// <summary>
    /// Standard view interface 
    /// </summary>
    public interface IBasicView
    {
        event Action ViewRender;
    }

    public interface IViewWithModel
    {
        void SetModel(object model);
    }

    public interface IView<T> : IBasicView
    {
        void SetModel(T model);
    }
}
