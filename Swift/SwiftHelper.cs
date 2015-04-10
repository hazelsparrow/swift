using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Swift
{
    public static class SwiftHelper
    {
        /// <summary>
        /// Instantiates a controller and renders the view.
        /// This is an alternative to HtmlHelper.PartialView() for more complex controls that 
        /// require special presentation logic.
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <returns></returns>
        public static void Execute<TController>()
            where TController : ISwiftController
        {
            var controller = DependencyResolver.GetService<TController>();
            RenderController(controller, HttpContext.Current);
        }

        public static void Execute<TController>(string actionName = null)
            where TController : ISwiftController
        {
            var controller = DependencyResolver.GetService<TController>();
            RenderController(controller, HttpContext.Current, actionName);
        }

        public static string Render<TController>(string actionName = null)
            where TController : ISwiftController
        {
            using (var writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                var httpContext = new HttpContext(HttpContext.Current.Request, new HttpResponse(writer));
                var controller = DependencyResolver.GetService<TController>();
                RenderController(controller, httpContext, actionName);
                return writer.ToString();
            }
        }

        private static void RenderController(ISwiftController swiftController, HttpContext httpContext, string actionName = null)
        {
            var result = swiftController.RenderView(actionName);
            var context = new ControllerContext
            {
                HttpContext = httpContext
            };

            result.ExecuteResult(context);
        }
    }
}