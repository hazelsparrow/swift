using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Swift
{
    public interface IRenderResult
    {
        void ExecuteResult(ControllerContext context);
    }

    /// <summary>
    /// Represents an empty result.
    /// </summary>
    public class EmptyResult : IRenderResult
    {
        public void ExecuteResult(ControllerContext context)
        {
            // nothing to see here.
        }
    }

    /// <summary>
    /// Represents html, plain string, or any other string content that can be rendered directly on the page.
    /// </summary>
    public class HtmlResult : IRenderResult
    {
        public string Html { get; set; }

        public void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "text/html";
            context.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.HttpContext.Response.Write(Html);
        }

        public HtmlResult()
        {
            
        }

        public HtmlResult(string html)
        {
            Html = html;
        }
    }

    public class JsonResult : IRenderResult
    {
        public object JsonObject { get; set; }

        public void ExecuteResult(ControllerContext context)
        {
            var jsonSerializer = new JavaScriptSerializer();
            var json = jsonSerializer.Serialize(JsonObject);
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.HttpContext.Response.Write(json);
        }

        public JsonResult()
        {
            
        }

        public JsonResult(object jsonObject)
        {
            JsonObject = jsonObject;
        }
    }

    public class JsonpResult : IRenderResult
    {
        public string Callback { get; set; }
        public JsonResult JsonResult { get; set; }

        public JsonpResult()
            : this (null)
        {
            
        }

        /// <summary>
        /// Callback can be null, in which case is assumed to be "callback".
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <param name="callback"></param>
        public JsonpResult(object jsonObject, string callback = null)
        {
            JsonResult = new JsonResult(jsonObject);
            Callback = callback ?? "callback";
        }

        public void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Write(string.Format("{0}(", Callback));
            JsonResult.ExecuteResult(context);
            context.HttpContext.Response.Write(")");

            context.HttpContext.Response.ContentType = "application/javascript";
        }
    }

    public class RedirectResult : IRenderResult
    {
        public string Url { get; set; }

        public RedirectResult(string url)
        {
            Url = url;
        }

        public void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Redirect(Url);
        }
    }

    public class NotFoundResult : IRenderResult
    {
        public string MissingObjectName { get; set; }

        public NotFoundResult(string missingObjectName = null)
        {
            MissingObjectName = missingObjectName;
        }

        public void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.StatusCode = 404;
            context.HttpContext.Response.ContentType = "text/html";
            if (!string.IsNullOrEmpty(MissingObjectName))
                context.HttpContext.Response.Write(GetMessage());
        }

        private string GetMessage()
        {
            return string.Format("[{0}] not found.", MissingObjectName);
        }
    }
}