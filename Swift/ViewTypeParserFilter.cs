// Borrowed from ASP .NET MVC
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.UI;

namespace Swift
{
    internal static class SwiftControlClassName
    {
        public static string Get(string modelTypeName)
        {
            return string.Format("Swift.SwiftControl<{0}>", modelTypeName);
        }

        //public static string GetCLR(string modelTypeName)
        //{
        //    return string.Format("Swift.SwiftControl`1[{0}, Swift]", modelTypeName);
        //}
    }

    internal interface ISwiftControlBuilder
    {
        string Inherits { set; }
    }

    internal sealed class SwiftViewPageControlBuilder : FileLevelPageControlBuilder, ISwiftControlBuilder
    {
        public string Inherits { get; set; }

        public override void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit, CodeTypeDeclaration baseType, CodeTypeDeclaration derivedType, CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod)
        {
            if (!string.IsNullOrWhiteSpace(Inherits))
            {
                derivedType.BaseTypes[0] = new CodeTypeReference(Inherits);
            }
        }
    }

    internal sealed class SwiftViewUserControlControlBuilder : FileLevelUserControlBuilder, ISwiftControlBuilder
    {
        public string Inherits { get; set; }

        public override void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit, CodeTypeDeclaration baseType, CodeTypeDeclaration derivedType, CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod)
        {
            if (!String.IsNullOrWhiteSpace(Inherits))
            {
                derivedType.BaseTypes[0] = new CodeTypeReference(Inherits);
            }
        }
    }

    // This class is referenced dynamically by the web.config built by project templates.
    // Do not delete this class based on it not being statically referenced by product code.

    internal class ViewTypeParserFilter : PageParserFilter
    {
        private static Dictionary<string, Type> _directiveBaseTypeMappings = new Dictionary<string, Type>
        {
            //{ "page", typeof(ViewPage) },
            { "control", typeof(BasicControl) },
            //{ "master", typeof(ViewMasterPage) },
        };

        private string _inherits;

        public ViewTypeParserFilter()
        {
        }

        public override bool AllowCode
        {
            get { return true; }
        }

        public override int NumberOfControlsAllowed
        {
            get { return -1; }
        }

        public override int NumberOfDirectDependenciesAllowed
        {
            get { return -1; }
        }

        public override int TotalNumberOfDependenciesAllowed
        {
            get { return -1; }
        }

        public override void PreprocessDirective(string directiveName, IDictionary attributes)
        {
            base.PreprocessDirective(directiveName, attributes);

            Type baseType;
            if (_directiveBaseTypeMappings.TryGetValue(directiveName, out baseType))
            {
                string inheritsAttribute = attributes["inherits"] as string;
                string codeBehindAttribute = attributes["codebehind"] as string ?? attributes["codefile"] as string;

                // Since the ASP.NET page parser doesn't understand native generic syntax, we
                // need to swap out whatever the user provided with the default base type for
                // the given directive (page vs. control vs. master). We stash the old value
                // and swap it back in inside the control builder. Our "is this generic?"
                // check here really only works for C# and VB.NET, since we're checking for
                // < or ( in the type name.
                //
                // We only change generic directives, because doing so breaks back-compat
                // for property setters on @Page, @Control, and @Master directives. The user
                // can work around this breaking behavior by using a non-generic inherits
                // directive, or by using the CLR syntax for generic type names.

                if (inheritsAttribute != null && codeBehindAttribute == null) // only do it for controls that have no code-behind (for backward compatibility)
                {
                    //attributes["inherits"] = baseType.FullName;
                    //_inherits = inheritsAttribute;
                    attributes["inherits"] = baseType.FullName; //SwiftControlClassName.GetCLR(inheritsAttribute);
                    _inherits = SwiftControlClassName.Get(inheritsAttribute);
                }
            }
        }

        public override void ParseComplete(ControlBuilder rootBuilder)
        {
            base.ParseComplete(rootBuilder);

            ISwiftControlBuilder builder = rootBuilder as ISwiftControlBuilder;
            if (builder != null)
            {
                builder.Inherits = _inherits;
            }
        }

        // Everything else in this class is unrelated to our 'inherits' handling.
        // Since PageParserFilter blocks everything by default, we need to unblock it

        public override bool AllowBaseType(Type baseType)
        {
            return true;
        }

        public override bool AllowControl(Type controlType, ControlBuilder builder)
        {
            return true;
        }

        public override bool AllowVirtualReference(string referenceVirtualPath, VirtualReferenceType referenceType)
        {
            return true;
        }

        public override bool AllowServerSideInclude(string includeVirtualPath)
        {
            return true;
        }
    }
}