using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Web;
using System.Web.UI;

namespace Swift
{
    public interface ISwiftController
    {
        IRenderResult RenderView(string actionName = null);
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpGetAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPostAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ActionNameAttribute : Attribute
    {
        public string Name { get; set; }

        public ActionNameAttribute(string name)
        {
            Name = name;
        }
    }

    public abstract class SwiftController : ISwiftController
    {
        /// <summary>
        /// Attempts to find a method in a derived class that renders the view.
        /// If the method accepts any arguments, they will be initialized from context using RequestHelper.GetFromContext.
        /// The method is chosen as follows:
        /// 1) If "mode" parameter is provided in the context, try to find a method with this name. E.g., mode=list will look
        /// for method List().
        /// 2) If not found, look at the [ActionName] attribute of the each method.
        /// 3) If not found, return error.
        /// 4) If found more than one, look at the [HttpPost], [HttpGet] attribute. Find a method that matches the current http method
        /// (post, get, etc.) Default is always [HttpGet].
        /// 5) If still more than one match, return error.
        /// 6) If "mode" is missing, there has to be only one method available to choose. If there are more than one, throw error.
        /// Action methods must be non-static public methods.
        /// </summary>
        /// <returns></returns>
        private IRenderResult RenderViewInternal(string actionName)
        {
            var mode = RequestHelper.GetFromContext<string>("mode");
            if (actionName != null)
                mode = actionName;

            var methods = GetType().GetMethods(BindingFlags.DeclaredOnly |
                BindingFlags.Instance |
                BindingFlags.Public).ToList();

            if (string.IsNullOrEmpty(mode))
            {
                foreach (var m in methods.Where(i => ActionNameMatch(i, "Index")))
                {
                    if (HttpMethodMatch(m))
                        return InvokeActionMethod(m);
                }

                foreach (var m in methods)
                {
                    if (HttpMethodMatch(m))
                        return InvokeActionMethod(m);
                }

                throw new InvalidOperationException(string.Format(
                    "Can't resolve the action name. Mode parameter is missing, and there is more than one public function in the controller (or none). Controller: {0}",
                    GetType().Name));
            }
            else
            {
                methods = methods.Where(i => ActionNameMatch(i, mode) && HttpMethodMatch(i)).ToList();

                if (!methods.Any())
                {
                    var method = "GET";
                    if (IsPost)
                        method = "POST";
                    throw new InvalidOperationException(string.Format("Can't find method [{0}] {1} in {2}. Swift attempted to find this method because mode parameter was set to this value. Make sure that method {1} exists and accepts {0} requests.", method, mode, GetType().Name));
                }
                else if (methods.Count() > 1)
                {
                    throw new InvalidOperationException(string.Format("More than one method matches current request. Matching methods: {0}",
                        string.Join(", ", methods.Select(i => i.Name))));
                }
                else
                    return InvokeActionMethod(methods.First());
            }
        }

        private bool ActionNameMatch(MethodInfo methodInfo, string name)
        {
            if (methodInfo.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) return true;

            var attr = methodInfo.GetCustomAttribute<ActionNameAttribute>();

            return attr != null && attr.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool HttpMethodMatch(MethodInfo methodInfo)
        {
            var attrGet = methodInfo.GetCustomAttribute<HttpGetAttribute>();
            var attrPost = methodInfo.GetCustomAttribute<HttpPostAttribute>();

            if (attrPost != null && IsPost) return true;
            if ((attrGet != null || attrPost == null) && !IsPost) return true;

            return false;
        }

        private bool IsArrayOrEnumerable(Type type)
        {
            return type.IsArray ||
                typeof(IEnumerable<>).IsAssignableFrom(type);
        }

        private IRenderResult InvokeActionMethod(MethodInfo method)
        {
            // before invoking the method, check for filter attributes
            var filterResult = ExecuteFilters(method);
            if (filterResult != null)
                return filterResult;

            // proceed with invoking the method
            var parameters = new List<object>();
            foreach (var p in method.GetParameters())
            {
                if (p.ParameterType.IsClass && p.ParameterType != typeof(string) && !IsArrayOrEnumerable(p.ParameterType))
                    parameters.Add(RequestHelper.GetObjectFromContext(p.ParameterType, null));
                else
                    parameters.Add(RequestHelper.GetFromContext(p.Name, p.ParameterType, null));
            }

            currentAction = method.Name;
            try
            {
                var result = method.Invoke(this, parameters.ToArray());

                if (method.ReturnType == typeof(void))
                    return new EmptyResult();

                var renderResult = result as IRenderResult;
                if (renderResult != null) return renderResult;

                if (result.GetType().IsClass && result.GetType() != typeof(string))
                {
                    return Json(result);
                }

                return new HtmlResult(result.ToString());
            }
            catch (TargetInvocationException ex)
            {
                var exInfo = ExceptionDispatchInfo.Capture(ex.InnerException);
                exInfo.Throw();
                throw;
            }
        }

        private IRenderResult ExecuteFilters(MethodInfo method)
        {
            var context = new ActionContext();
            foreach (var item in method.GetCustomAttributes<ActionFilterAttribute>())
            {
                item.ExecuteBeforeAction(context);
                if (!string.IsNullOrEmpty(context.RedirectUrl))
                    return Redirect(context.RedirectUrl);
            }
            return null;
        }

        protected IRenderResult NotFound(string missingObjectName = null)
        {
            return new NotFoundResult(missingObjectName);
        }

        protected IRenderResult Empty()
        {
            return new EmptyResult();
        }

        protected IRenderResult Redirect(string url)
        {
            return new RedirectResult(url);
        }

        private string currentAction = null;

        protected IRenderResult Json(object jsonObject = null)
        {
            return new JsonResult(jsonObject);
        }

        protected IRenderResult Jsonp(object jsonObject = null, string callback = null)
        {
            return new JsonpResult(jsonObject, callback);
        }

        protected IRenderResult View(object model = null)
        {
            string html = RenderViewToString(model);
            return new HtmlResult(html);
        }

        protected IRenderResult View(string viewName, object model)
        {
            string html = RenderViewToString(viewName, model);
            return new HtmlResult(html);
        }

        private TypedControl<IViewWithModel> LoadView(string controlName = null)
        {
            if (controlName == null)
            {
                controlName = string.Format("{0}_{1}.ascx", GetType().Name.Replace("Controller", string.Empty),
                    currentAction);
            }

            return HtmlHelper.LoadControl<IViewWithModel>(controlName);
        }

        protected string RenderViewToString(object model = null)
        {
            return RenderViewToString(null, model);
        }

        protected string RenderViewToString(string viewName, object model)
        {
            var view = LoadView(viewName);
            view.Abstraction.SetModel(model);
            return HtmlHelper.RenderControl(view.Implementation);
        }

        public IRenderResult RenderView(string actionName = null)
        {
            try
            {
                return RenderViewInternal(actionName);
            }
            catch (Exception ex)
            {
                if (IsDebug())
                    return new HtmlResult
                    {
                        Html = ErrorMessage(ex)
                    };
                else
                    throw;
            }
        }

        private const string WILL_ONLY_SHOW_IN_DEBUG = "This error will only be shown if SwiftController.IsDebug() function returns true which only happens for debug builds.";

        private string ErrorMessage(Exception ex)
        {
            if (ex is SwiftException) return ErrorMessage((SwiftException)ex);

            return string.Format("<h2>[{0}] {1}</h2><h4>Stack trace:</h4><p>{2}</p><p style='margin-top:20px;'>{3}</p>",
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace.Replace("\r\n", "<br />"),
                WILL_ONLY_SHOW_IN_DEBUG
                );
        }

        private string ErrorMessage(SwiftException ex)
        {
            Func<IEnumerable<string>, string> suggestions = array =>
                {
                    var result = "<ol>";
                    foreach (var item in array)
                        result += "<li>" + item + "</li>";
                    return result + "</ol>";
                };
            return string.Format("<h2>[{0}] {1}</h2><h4>Explanation:</h4>{2}<h4>Suggestions:</h4>{3}<h4>Stack trace:</h4><p>{4}</p><p style='margin-top:20px;'>{5}</p>",
                ex.GetType().Name,
                ex.Message,
                ex.Explanation,
                suggestions(ex.Suggestions),
                ex.StackTrace.Replace("\r\n", "<br />"),
                WILL_ONLY_SHOW_IN_DEBUG
                );
        }

        protected bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        protected HttpContext HttpContext
        {
            get
            {
                return HttpContext.Current;
            }
        }

        protected HttpRequest Request
        {
            get
            {
                return HttpContext.Current.Request;
            }
        }

        protected HttpResponse Response
        {
            get
            {
                return HttpContext.Current.Response;
            }
        }

        protected bool IsPost
        {
            get
            {
                return Request.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}