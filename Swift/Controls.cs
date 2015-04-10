using System;
using System.Web.UI;

namespace Swift
{
    [FileLevelControlBuilder(typeof(SwiftViewUserControlControlBuilder))]
    public abstract class BasicControl: UserControl, IBasicView
    {
        protected BasicControl()
        {
            Html = new ViewUtility();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (ViewRender != null)
                ViewRender();
        }

        public event Action ViewRender;

        protected ViewUtility Html { get; private set; }
    }

    public class SwiftControl<T> : BasicControl, IView<T>, IViewWithModel
    {
        protected virtual void InitView()
        { }

        protected T Model { get; set; }

        public void SetModel(T model)
        {
            InitView();
            Model = model;
            OnModelChanged();
        }

        protected virtual void OnModelChanged()
        { }

        public void SetModel(object model)
        {
            T typedModel;
            try
            {
                typedModel = (T)model;
            }
            catch (InvalidCastException)
            {
                throw new Exception(string.Format("{0} view expects an object of type {1} as a model, but an object of type {2} was provided. Types {1} and {2} are incompatible.",
                    this.GetType(),
                    typeof(T),
                    model.GetType()));
            }
            SetModel(typedModel);
        }
    }

    public class SwiftControl : SwiftControl<dynamic> { }
}