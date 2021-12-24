cmake_minimum_required(VERSION 3.8)

project(YAEZBlocker LANGUAGES CSharp)

include(CSharpUtilities)

SET(IMAGE_RESOURCES
    "resources/my_music.ico"
)

add_executable(YAEZBlocker
  App.config
  
  App.xaml
  App.xaml.cs
  
  MainWindow.xaml
  MainWindow.xaml.cs

  AudioUtils.cs
  MediaHook.cs

  Properties/AssemblyInfo.cs

  Properties/Settings.cs
  Properties/Settings.Designer.cs
  Properties/Settings.settings
  
  Properties/strings.Designer.cs
  Properties/strings.resx

  ${IMAGE_RESOURCES}  
)


set_property(SOURCE App.xaml PROPERTY VS_XAML_TYPE "ApplicationDefinition")
SET_SOURCE_FILES_PROPERTIES(${IMAGE_RESOURCES} PROPERTIES VS_TOOL_OVERRIDE "Resource")

set_target_properties(YAEZBlocker PROPERTIES
  WIN32_EXECUTABLE                   true
  VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.7.2"
  VS_GLOBAL_ROOTNAMESPACE            "YAEZBlocker"
)

set(CMAKE_CSharp_FLAGS "/langversion:latest")

csharp_set_designer_cs_properties(
  Properties/AssemblyInfo.cs

  Properties/Settings.cs
  Properties/Settings.Designer.cs
  Properties/Settings.settings
  
  Properties/strings.Designer.cs
  Properties/strings.resx
)

csharp_set_xaml_cs_properties(
  App.xaml
  App.xaml.cs
  
  MainWindow.xaml
  MainWindow.xaml.cs
)

set_property(TARGET YAEZBlocker
    PROPERTY VS_PACKAGE_REFERENCES
    "System.Runtime.InteropServices.WindowsRuntime_4.3.0"
    "System.Runtime.WindowsRuntime_5.0.0-preview.5.20278.1"
    "System.Runtime.WindowsRuntime.UI.Xaml_5.0.0-preview.5.20278.1"
    "Microsoft.Windows.SDK.Contracts_10.0.22518-preview"
)

set_property(TARGET YAEZBlocker PROPERTY VS_DOTNET_REFERENCES
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
    "System.Windows.Forms"
    "System.Xaml"
    "System.Xml"
)

set(CMAKE_SUPPRESS_REGENERATION ON)