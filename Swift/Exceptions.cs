using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Swift
{
    public class SwiftException : Exception
    {
        public IEnumerable<string> Suggestions { get; set; }
        public string CustomMessage { get; set; }
        public string Explanation { get; set; }

        public SwiftException()
        {
            
        }

        public SwiftException(Exception ex)
            : base (string.Empty, ex)
        {
            
        }

        public override string Message
        {
            get
            {
                return CustomMessage;
            }
        }
    }

    public class SwiftParseException : SwiftException
    {
        public SwiftParseException(Exception ex, string viewName)
            : base (ex)
        {
            CustomMessage = string.Format("Error in {0}: {1}", viewName, ex.Message);
            Explanation = string.Format("The underlying exception is HttpCompileException. This usually indicates there is a problem with the markup in the ascx file.");
            Suggestions = new List<string>
            {
                string.Format("Check for typos in {0}.", viewName),
                "Make sure all server tags (i.e., &lt;% ... %&gt;) are correctly formed.",
                "Make sure you are using the right type of the model. Are you trying to access a property that is null?",
                "If you are using any extension methods (e.g., Html.Label()), make sure you import the required namespaces by doing &lt;%@ Import Namespace=\"Something\"%&gt;",
            };
        }
    }

    public class SwiftViewNotFoundException : SwiftException
    {
        public SwiftViewNotFoundException(IEnumerable<string> paths, string viewName)
        {
            CustomMessage = string.Format("{0} could not be found.", viewName);
            Explanation = string.Format("Swift attempted to load view with name {0} because either this name was specified in the controller method (e.g., return View(\"{0}\")) or because of the convention (the convention is as follows: \"Test_Foo.ascx\" is loaded when the controller's name is TestController and the method name is Foo).", viewName);
            Explanation += "<br /><br />Swift looked in the following locations:<br />";
            foreach (var s in paths)
                Explanation += "<br />" + s;
            Suggestions = new List<string>
            {
                "Make sure you have called HtmlHelper.SetControlsVirtualPath() in your global.asax Application_Start function. This is the root folder that Swift will be looking at when it tries to load views.",
                "Make sure you copied your control to the controls root folder."
            };
        }
    }

    public class SwiftIncompatibleViewException : SwiftException
    {
        public SwiftIncompatibleViewException(string viewName)
        {
            CustomMessage = string.Format("{0} is not a Swift view (that is, does not inherit SwiftControl<T>).", viewName);
            Explanation = string.Format("{0} has to implement IViewWithModel in order to be used in Swift. Normally you derive from SwiftControl which already implements this interface.", viewName);
            Suggestions = new List<string>
            {
                "Go to the code behind of your control and derive it from SwiftControl."
            };
        }
    }
}