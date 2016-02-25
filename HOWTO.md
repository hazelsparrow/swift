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

