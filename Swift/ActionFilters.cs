using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Swift
{
    public class ActionContext
    {
        public string RedirectUrl { get; set; }
    }

    public abstract class ActionFilterAttribute : Attribute
    {
        public abstract void ExecuteBeforeAction(ActionContext context);
    }
}