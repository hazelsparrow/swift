using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Swift
{
    public class SwiftHandler<TController> : IHttpHandler
        where TController: ISwiftController
    {
        public void ProcessRequest(HttpContext context)
        {
            SwiftHelper.Execute<TController>();
        }

        public bool IsReusable { get { return false; } }
    }
}