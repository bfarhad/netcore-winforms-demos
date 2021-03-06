# .Net Core 3 DevExpress WinForms Demos

> ⚠ **Important Note**
> 
> This repository contains a pre-release version of DevExpress demo applications for the .NET Core 3 platform.
> 
> Please visit [https://www.devexpress.com/dotnet-core-3/](https://www.devexpress.com/dotnet-core-3/) to download the most recent version of the DevExpress .NET Core 3 installer (contains shipping versions of all DevExpress .NET Core 3 demos).

 
This repository contains the DevExpress demo applications that target .Net Core 3: 
- Outlook-inspired App
- Stock Market Trader
 
## System requirements
- **Visual Studio 2019** with the **.NET desktop development** workload installed
 
- [.NET Core 3 SDK Preview 5 or more recent](https://dotnet.microsoft.com/download/dotnet-core/3.0)
 
 

## Run the demos

Open a solution in Visual Studio. 
Before you build the solution, ensure that the 'Use previews of the .NET Core SDK' option is enabled. 
You can find this setting from the Visual Studio main menu: 
- In Visual Studio 2019 version 16.1+: Tools -> Options -> Environment -> Preview Features
- In Visual Studio 2019 version 16.0: Tools -> Options -> Project and Solutions -> .NET Core

If you downloaded the solutions as a .zip file, you may encounter the following error when you build the solution:

`MSB3821	Couldn't process file *.resx due to its being in the Internet or Restricted zone or having the mark of the web on the file. Remove the mark of the web if you want to process these files.`

See the following link to learn how to resolve this issue:
https://developercommunity.visualstudio.com/content/problem/291761/couldnt-process-file-abcresx-due-to-its-being-in-t.html
 
## Integrate DevExpress WinForms Controls into a .NET Core 3 application
 
You need the DevExpress NuGet packages to create a .Net Core 3 project. Follow the steps below to add the packages to a solution:
 
1. [Register](https://docs.devexpress.com/GeneralInformation/116698/installation/install-devexpress-controls-using-nuget-packages/setup-visual-studio%27s-nuget-package-manager) the DevExpress Early Access feed in Visual Studio's NuGet Package Manager.
 
    `https://nuget.devexpress.com/early-access/api`
 
2. Install the **DevExpress.WindowsDesktop.Win** package (for .Net Core 3). 
 
## Feedback
 
You can provide feedback via [DevExpress Support Center](https://www.devexpress.com/Support/Center/Question/Create).
 
## See Also
 
[.NET Core 3.0 Windows Forms Samples](https://github.com/dotnet/samples/tree/master/windowsforms)
