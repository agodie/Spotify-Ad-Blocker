cmake_minimum_required(VERSION 3.8)

project(EZBlocker LANGUAGES CSharp)

include(CSharpUtilities)

add_executable(EZBlocker
  App.config


  EZBlockerMain.Designer.cs
  EZBlockerMain.cs
  EZBlockerMain.resx

  AudioUtils.cs
  MediaHook.cs
  Program.cs

  Properties/AssemblyInfo.cs

  Properties/Settings.cs
  Properties/Settings.Designer.cs
  Properties/Settings.settings
  
  Properties/strings.Designer.cs
  Properties/strings.resx)


set_target_properties(EZBlocker PROPERTIES
  WIN32_EXECUTABLE                   true
  VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.7.2"
  VS_GLOBAL_ROOTNAMESPACE            "EZBlocker"
)

set(CMAKE_CSharp_FLAGS "/langversion:latest")

csharp_set_windows_forms_properties(
  EZBlockerMain.Designer.cs
  EZBlockerMain.cs
  EZBlockerMain.resx
)
csharp_set_designer_cs_properties(
  Properties/AssemblyInfo.cs

  Properties/Settings.cs
  Properties/Settings.Designer.cs
  Properties/Settings.settings
  
  Properties/strings.Designer.cs
  Properties/strings.resx
)


set_property(TARGET EZBlocker
    PROPERTY VS_PACKAGE_REFERENCES
    "System.Runtime.InteropServices.WindowsRuntime_4.3.0"
    "System.Runtime.WindowsRuntime_5.0.0-preview.5.20278.1"
    "System.Runtime.WindowsRuntime.UI.Xaml_5.0.0-preview.5.20278.1"
    "Microsoft.Windows.SDK.Contracts_10.0.22518-preview"
)

set_property(TARGET EZBlocker PROPERTY VS_DOTNET_REFERENCES
    "Microsoft.CSharp"
    "PresentationCore"
    "PresentationFramework"
    "WindowsBase"
    "System"
    "System.Core"
    "System.Configuration"
    "System.Data"
    "System.Drawing"
    "System.IO.Compression.FileSystem"
    "System.Net"
    "System.Net.Http"
    "System.Web.Extensions"
    "System.Windows.Forms"
    "System.Xml"
)

set(CMAKE_SUPPRESS_REGENERATION ON)