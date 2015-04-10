using System;
using System.Web;
using System.Web.UI;

namespace Swift
{
    public class ViewUtility
    {
        /// <summary>
        /// Loads a control, casts it to IPartialView and calls IPartialView.SetModel(model).
        /// Then renders the control and returns it as a string.
        /// Example of usage: Html.Partial("_Example.ascx")
        /// Note: ascx extension is required.
        /// Note: _Example.ascx is expected to be in the whatever folder was specified when HtmlHelper.SetVirtualPath()
        /// method was called.
        /// Note: Model can be null. It is the responsibility of the partial view to check it for null
        /// and to cast it to the expected type. If cast is not valid, the partial view is expected to throw an exception.
        /// Check with the documentation what type of model the partial view you are about to render expects.
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public string Partial(string controlName, object model = null)
        {
            return HtmlHelper.PartialView(controlName, model);
        }

        public string Render<TController>(string actionName = null)
            where TController : ISwiftController
        {
            return SwiftHelper.Render<TController>(actionName);
        }

        /// <summary>
        /// This function is intended to be used for legacy controls that are not based on Swift.
        /// </summary>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public string RenderLegacyControl(string controlName)
        {
            return HtmlHelper.RenderControl(controlName); 
        }

        public HttpContext HttpContext { get { return HttpContext.Current; } }
    }
}