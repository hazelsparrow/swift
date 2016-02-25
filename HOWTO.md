# Getting started

#### Adding reference to Swift
You can use Swift in any ASP .NET project. Just add a reference to Swift to your project.

#### Installing Swift extension for Visual Studio
This extension adds a template for a Swift view. A Swift view is just an `.ascx` file, so you can create Swift views by adding a new user control to your project. However, the Swift View template comes with a few useful features. It removes the `.ascx.cs` and `.ascx.cs.designer` files, leaving only the `.ascx` file. It also does `<%@ Import Namespace="Swift" %>` in new view, so that you can use Swift methods and classes right away.

It is recommended to add new Swift views through this extension. After installing this extension, a new item will be added to "Add new item" menu in Visual Studio.

#### Configuring Swift View Parser
Swift comes with a custom view parser tGhat is required to handle ascx files with no codebehind and support Intellisense when working with models in `.ascx` files. To set it up, change the `<pages>` node in your `web.config` as follows:

```
<pages validateRequest="false" enableEventValidation="false" pageParserFilterType="Swift.ViewTypeParserFilter, Swift" userControlBaseType="Swift.SwiftControl, Swift">
      <controls>
        <add assembly="Swift" namespace="Swift" tagPrefix="sw" />
      </controls>
</pages>
````
#### Setting up Swift in global.asax
There's three things that you have to do in your global.asax file, typically in Application_Start:

1. Tell Swift where to look for all views (ascx files). Swift expects all views to be in the single folder. Nested or multiple folders are currently not supported. 
2. Set default connection name to use with DbHelper. (This is optional if you are not going to use DbHelper and instead will be using an external ORM or any other DAL framework.)
3. Call LoadDependencyModules so that Swift can pick up all dependency modules. This is used later for resolving dependencies when Swift instantiates controllers.

The end result will look something like this:

```
Swift.HtmlHelper.SetControlsVirtualPath("/usercontrols");
Swift.DbHelper.SetDefaultConnectionName(ConfigurationManager.ConnectionStrings["default"]); // optional
Swift.DependencyResolver.LoadDependencyModules(); 
```
#### Summary
To start using Swift in your project:

1. Add a reference to Swift.
2. Install the Swift extension for Visual Studio.
3. Modify your web.config to enable Swift's custom view parser.
4. Initialize Swift in Application_Start in your global.asax.


# Using Swift

You can start using Swift right away, without having to convert your existing projects. Suppose we have an existing user control MyControl.ascx. It's using postbacks and all the standard WebForms controls: <asp:Button>, <asp:DropDownList>, etc. Imagine that you need to add a new piece of functionality to this control, for example a new search form. 

Assuming you have created a SearchFormController with a single method Index(), you can invoke it in MyControl.ascx as follows:

```
<%= SwiftHelper.Render<SearchFormController>("Index") %>
```

This call will instantiate SearchFormController and call its method Index. If Index accepts any arguments, they will be populated from the request (more on that later). It will then use the return value from Index to insert it back into MyControl.ascx.

Another way of using Swift is rendering a partial view. Partial views are basically the same Swift views but what makes them "partial" is the fact that they're missing a controller that would create a model and pass it to them. Instead, a model is being passed to such views directly from where they are being used, e.g.:

```
<%= SwiftHelper.PartialView("_SearchForm.ascx", new SearchFormViewModel { ... }) %>
```

Note that the same view can be used as a partial view or a full-fledged view (rendered from within a controller).

#### Swift controllers
Any class implementing ISwiftController interface can be a Swift controller. Usually you would want to derive your controllers from SwiftController, which is a base class providing a bunch of useful methods.

An example of a Swift controller is given below.

```
public class MyController: SwiftController
{
      ...
      
      public IRenderResult Index()
      {
            return View("MyView.ascx", null); // render the view with model = null
      }
      
      [HttpPost] // this method only allows POST requests
      public object Products(int categoryId) // categoryId will be populated from request
      {
            var products = repository.GetProductsByCategory(categoryId);
            return products; // equivalent to return Json(products); -- this will automatically serialize the result to JSON
      }
      
      ...
}
```

This controller defines two methods. Index is available through GET and POST requests and doesn't take any arguments. Products is only available through POST requests and reads categoryId from request (it could be in the URL, e.g., "?categoryId=223", or in the POST payload). The Products method is useful for AJAX calls returning JSON data.

#### Swift views
MyController above uses view MyView.ascx in the Index method. This view could look something like below:

```
<%@ Control Inherits="object"  AutoEventWireup="false" EnableTheming="false" EnableViewState="false" Language="C#" %>
<%@ Import Namespace="Swift" %>

<p>
      Hello, this is a Swift view!
</p>
```

**Important:** `Inherits` attribute in Swift views contains the type of the model used by the view (similarly to the @model directive in Razor), rather than the base class for the ASP .NET control. This helps with Intellisense when you use models in your view.

In most cases, you would want to create a new class for a model for each complex view. For example:

```
public class HelloWorldViewModel
{
      public HelloWorldViewModel() { }
      
      private string user = null;
      
      public HelloWorldViewModel(string user)
      {
            this.user = user;
      }
      public string Greeting 
      {
            get
            {
                  return string.Format("Hello, {0}!", user ?? "World");
            }
      }
}

public class MyController: SwiftController
{
      public IRenderResult HelloWorld(string user)
      {
            return View("HelloWorld.ascx", new HelloWorldViewModel(user));
      }
}
```

In HelloWorld.ascx:

```
<%@ Control Inherits="HelloWorldViewModel"  AutoEventWireup="false" EnableTheming="false" EnableViewState="false" Language="C#" %>
<%@ Import Namespace="Swift" %>

<p>
      <%= Model.Greeting %>
</p>
```

**Note:** you can also create normal user controls (ascx) with code behind and use them as Swift views. To do so, you need to inherit them from SwiftControl<T>, where T is the type of your model. If you decide to follow this approach, you don't need to add Swift's custom view parser to your web config. You also won't need the Swift extension for visual studio.

#### Dependency injection

You can declare dependencies for your controllers like this:

```
public class MyController : SwiftController
{
      private IMyRepository repository = null;
      
      public MyController(IMyRepository repository)
      {
            this.repository = repository;
      }
}

public class MyDependencyModule : DependencyModule
{
      public override void Load()
      {
            Bind<IMyRepository>().To<MyRepository>(); 
      }
}

public class MyRepository : IMyRepository
{
      ... // implementation of IMyRepository
}
```

When MyController gets instantiated, Swift will notice that it requires an instance of IMyRepository and will look it up in all the dependency modules in your bin folder. (Swift gathers all dependency modules only once when the application starts.) It will automatically create an instance of MyRepository and pass it to the constructor of MyController. If MyRepository's constructor also includes any dependencies, Swift will instantiate them recursively as well.

#### Typical folder structure 

The project structure below is just one possible way to organize your Swift projects -- feel free to use it as a reference. Naturally, Swift doesn't care how your project is structured as long as it has access to all types (controllers, models, etc.) that it needs. Note that all views have to be under the same folder though. If you have multiple Swift projects, you would want to have a post-build script that copies all ascx files to a single location.

```
MyProject.csproj
      Controllers
            MyController.cs
            OtherController.cs
      Models
            Product.cs
            Agent.cs
            Customer.cs
      Repositories
            ProductRepository.cs
            AgentRepository.cs
            CustomerRepository.cs
      Views
            MyHome.ascx
            ProductList.ascx
            CustomerList.ascx
            CustomerDetails.ascx
            _Pagination.ascx
      Tools
            MyProjectDependencyModule.cs
```

# DbHelper
While completely optional, Swift offers a thin wrapper around ADO .NET for working with database.

A typical example would be getting a list of customers from the database. Assuming you have a table Customer and a class Customer, you can get that list as follows:

```
public class Customer : IPersistent 
{
      public string Name { get; set; }
      public int Id { get; set; }

      public void Init(ISwiftDataReader reader)
      {
            Id = reader.GetInt32("CustomerId"); // assumes there is an integer column CustomerId in the Customer table
            Name = reader.GetString("CustomerName");
      }
}

public class CustomerRepository
{
      public IEnumerable<Customer> GetCustomers(string filter, DateTime minRegisteredDate, int rating)
      {
            // call the GetCustomers stored proc and pass in three arguments
            return DbHelper.GetList<Customer>("GetCustomers",
                  new DbParameter("filter", filter),
                  new DbParameter("minRegisteredDate", minRegisteredDate),
                  new DbParameter("rating", rating));
      }
}
```

There's a number of different methods in DbHelper -- they are pretty self-explanatory. 
Note that those methods use the DefaultConnectionString (that you had set up in the beginning). It's also possible to call methods of DbHelper with a different connection string (look at overloaded versions). 

**Note**: The underlying DAL, SwiftDataAccess, also supports ad-hoc SQL statements (not necessarily stored procs), but for simplicity of method signatures DbHelper does not include support for them. If in your product you need arbitrary SQL statements, feel free to modify/add functions to DbHelper.

# Miscellanious

#### Action filters
You can use action filters to define actions that will be executed before or after invoking a controller method. 

```
public class MyController : SwiftController
{
      ...
      
      [MyFilter]
      public IRenderResult Example()
      {
            return View(...);
      }
      
      ...
}

public class MyFilterAttribute : ActionFilterAttribute
{
      public override void ExecuteBeforeAction(ActionContext context)
      {
            // this method will be invoked before invoking controller's method
            // if you assign anything to context.RenderResult, controller's method won't get invoked
            ...
            context.RenderResult = SwiftHelper.Render<SomeOtherController>("SomeView.ascx", model);
            // or, for example, you can use a  redirect
            context.RenderResult = new RedirectResult("/loginRequired/");
      }
      
      public override void ExecuteAfterAction(ActionContext context)
      {
            // this method will be invoked before invoking controller's method
      }
}
```

Swift supports CORS by default; just mark your controller method with `[EnableCors]` attribute.

#### View utility

Each Swift view has access to the Html object (of type ViewUtility) that provides a number of useful shortcuts.

```
<%= Html.Partial("_MyPartial.ascx") %> 
```
(This is lieu of the more generic `SwiftHelper.RenderPartial("_MyPartial.ascx")` that is accessible anywhere, not just inside Swift views.)

You can also add extension methods to ViewUtility to add application-specific shortcuts.

#### Swift dependencies

Swift depends on `Ninject` which it includes through a NuGet package. 
