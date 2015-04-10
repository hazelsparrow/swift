using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace Swift
{
    public class HtmlHelper
    {
        private static string controlsVirtualPath = null;

        public static void SetControlsVirtualPath(string virtualPath)
        {
            lock (typeof(HtmlHelper))
            {
                controlsVirtualPath = virtualPath;
            }
        }

        public static string RenderControl(Control control)
        {
            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            var htmlWriter = new HtmlTextWriter(stringWriter);

            control.RenderControl(htmlWriter);

            var renderedControl = stringBuilder.ToString();
            renderedControl = RenderInLayout(renderedControl);
            return renderedControl;
        }

        private static string RenderInLayout(string renderedControl)
        {
            //var layoutName = GetLayoutName(renderedControl);
            //if (layoutName != null)
            //{
            //    renderedControl = WrapInLayout(renderedControl, layoutName);
            //    return RemoveLayoutNameTag(renderedControl);
            //}
            return renderedControl; // this control has no layout
        }

        private static readonly Regex layoutNameRegex = new Regex(@"\[layout (.+)\]", RegexOptions.Compiled);

        private static string GetLayoutName(string renderedControl)
        {
            var m = layoutNameRegex.Match(renderedControl);
            if (m.Success)
                return m.Groups[1].Value;
            return null;
        }

        private static string WrapInLayout(string renderedControl, string layoutName)
        {
            var layoutControl = LoadControl<IBasicView>(layoutName);
            return renderedControl;
        }

        private static string RemoveLayoutNameTag(string renderedControl)
        {
            return renderedControl;
        }

        public static string PartialView(string controlName, object model = null)
        {
            var typedControl = LoadControl<IViewWithModel>(controlName);
            typedControl.Abstraction.SetModel(model);
            return RenderControl(typedControl.Implementation);
        }

        public static TypedControl<T> LoadControl<T>(string controlName)
        {
            return _LoadControl<T>(string.Format("{0}/{1}", controlsVirtualPath, controlName));
        }

        private static TypedControl<T> _LoadControl<T>(string virtualPath)
        {
            Control c = null;
            try
            {
                c = new Page().LoadControl(virtualPath);
            }
            catch (HttpCompileException ex)
            {
                throw new SwiftParseException(ex, Path.GetFileName(virtualPath)); 
            }
            catch (HttpException ex)
            {
                if (ex.Message.StartsWith("The file") && ex.Message.EndsWith("not exist."))
                    throw new SwiftViewNotFoundException(new[] { virtualPath }, Path.GetFileName(virtualPath));
                else
                    throw new SwiftParseException(ex, Path.GetFileName(virtualPath));
            }
            object obj = c;
            try
            {
                return new TypedControl<T>((T)obj, c);
            }
            catch (InvalidCastException)
            {
                throw new SwiftIncompatibleViewException(Path.GetFileName(virtualPath));
            }
        }

        public static TypedControl<object> LoadControl(string controlName)
        {
            return LoadControl<object>(controlName);
        }

        /// <summary>
        /// This function is intended to be used for legacy controls that are not using Swift.
        /// </summary>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public static string RenderControl(string controlName)
        {
            var typedControl = LoadControl(controlName);
            return RenderControl(typedControl.Implementation);
        }
    }
}
