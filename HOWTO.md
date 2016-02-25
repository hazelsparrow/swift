# Getting started

#### Adding reference to Swift
You can use Swift in any ASP .NET project. Just add a reference to Swift to your project.

#### Installing Swift extension for Visual Studio
This extension adds a template for a Swift view. A Swift view is just an `.ascx` file, so you can create Swift views by adding a new user control to your project. However, the Swift View template comes with a few useful features. It removes the `.ascx.cs` and `.ascx.cs.designer` files, leaving only the `.ascx` file. It also does `<%@ Import Namespace="Swift" %>` in new view, so that you can use Swift methods and classes right away.

It is recommended to add new Swift views through this extension. After installing this extension, a new item will be added to "Add new item" menu in Visual Studio.

#### Configuring Swift View Parser
Swift comes with a custom view parser that is required to handle ascx files with no codebehind and support Intellisense when working with models in `.ascx` files. To set it up, change the `<pages>` node in your `web.config` as follows:

```
<pages validateRequest="false" enableEventValidation="false" pageParserFilterType="Swift.ViewTypeParserFilter, Swift" userControlBaseType="Swift.SwiftControl, Swift">
      <controls>
        <add assembly="Swift" namespace="Swift" tagPrefix="sw" />
      </controls>
</pages>
````
#### Setting up Swift
There's three things that you have to do in your global.asax file, typically in Application_Start:

1. Tell Swift where to look for all views (ascx files). Swift expects all views to be in the single folder. Nested or multiple folders are currently not supported. 
2. Set default connection name to use with DbHelper. (This is optional if you are not going to use DbHelper and instead will be using an external ORM or any other DAL framework.)
3. Call LoadDependencyModules so that Swift can pick up all dependency modules. This is used later for resolving dependencies when Swift instantiates controllers.

The end result will look something like this:

```
Swift.HtmlHelper.SetControlsVirtualPath("/usercontrols");
Swift.DbHelper.SetDefaultConnectionName(DbConnections.RemaxReadOnly().ConnectionString); // optional
Swift.DependencyResolver.LoadDependencyModules(); 
```
