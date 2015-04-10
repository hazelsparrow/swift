# swift
A lightweight MVC framework for ASP.NET

Do you want to start using MVC pattern in your ASP.NET WebForms application but your boss thinks rewriting it is risky and not worth it? Swift is a lightweight MVC framework mimicking familiar ASP.NET MVC in many aspects but allowing you to use it when you need it, without having to convert old code to MVC. Suppose you were given a task to write a new ascx control, Foo_Something.ascx. It resides in a standard Web form and possibly in another ascx control that uses WebForms. You can then invoke Swift from this new control:

<%= SwiftHelper.Render<FooController>("Something") %>

The line above will use the FooController and call the Something method that returns Foo_Something.ascx as the view. 

public class FooController : SwiftController
{
     public IRenderResult Something()
     {
          return View();
     }
}

Main features of Swift:

1. Full MVC stack: controllers, views, and models.
2. Custom ViewTypeParserFilter to support ascx views with no code-behind.
3. ASP.NET MVC-like syntax in controllers, allowing for easier transition to ASP.NET MVC in future.
4. Minimal global configuration needed; fully compatible with WebForms.
5. Dependency injection in controllers.
6. DbHelper tools, making code working with database concise, readable, and elegant.
7. Native support of Json responses for ajax calls.
8. Action filters.
