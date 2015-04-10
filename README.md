# swift
A lightweight MVC framework for ASP.NET

Would you like to start using an MVC pattern in your existing ASP.NET WebForms application but the risk and cost of rewriting everything seems too high? Swift is a lightweight MVC framework mimicking familiar ASP.NET MVC in many aspects but allowing implementation without having to convert any old projects to MVC. 

For example: 
You need to write a new ascx control, ‘Foo_Something.ascx’. It resides in a standard Web form and possibly in another ascx control that uses WebForms. You can invoke Swift from this new control by doing this:

<%= SwiftHelper.Render("Something") %>

The line above will use the ‘FooController’ (see below) and call the ‘Something’ method that returns ‘Foo_Something.ascx’ as the view.

public class FooController : SwiftController { public IRenderResult Something() { return View(); } }

Main features of Swift:
1. Full MVC stack: controllers, views, and models.
2. Custom ViewTypeParserFilter to support ascx views with no code-behind.
3. ASP.NET MVC-like syntax in controllers. (Allowing for easier transition to ASP.NET MVC in future.)
4. Minimal global configuration required.
5. Fully compatible with WebForms.
6. Dependency injection in controllers.
7. DbHelper tools, making code working with database concise, readable, and elegant.
8. Native support for Json responses and ajax calls.
9. Action filters.
