# TSK VSCode Editor

[![openupm](https://img.shields.io/npm/v/com.tsk.ide.vscode?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.tsk.ide.vscode/) [![Discord](https://img.shields.io/discord/1106106269837819914?color=D1495B&logo=discord&logoColor=FFFFFF&style=flat)](https://discord.gg/VU8EhUY7bX)

Code editor integration for VSCode. **(2021.3+)**

See [Changelog](https://github.com/Chizaruu/com.tsk.ide.vscode/wiki/CHANGELOG).

If you find my package useful, please consider giving it a Star 🌟 to show your support. Thank you!

## Features

### Project SDK Support

This package offers comprehensive project SDK support based on .Net 7 and .Net 6 standards. By leveraging this support, you can utilize the latest C# features and language enhancements within your Unity projects, subject to Unity's compatibility.

### Organized .csproj Files

To enhance project structure and maintain cleanliness, the `com.tsk.ide.vscode` package facilitates the automatic separation of `.csproj` files into individual folders. These folders are consolidated within a main directory named "CSProjFolders." This organization ensures a more streamlined and organized project structure, contributing to improved clarity and ease of navigation.

### Successful Dotnet Build

The `com.tsk.ide.vscode` package ensures a seamless build process by guaranteeing successful execution of the `dotnet build` command. This means that your project can be compiled and built without any issues, ensuring a smooth development experience.

### Microsoft.Unity.Analyzers Integration [![NuGet](https://img.shields.io/nuget/v/Microsoft.Unity.Analyzers.svg)](https://nuget.org/packages/Microsoft.Unity.Analyzers)

In addition to its core features, this package includes seamless integration with the Microsoft.Unity.Analyzers library. This integration provides you with access to a wide range of code analysis and validation tools specifically designed for Unity projects. With the support of these analyzers, you can enhance code quality, identify potential issues, and adhere to best practices, ultimately improving the overall robustness and maintainability of your Unity projects.

### Automated Configuration Setup

The `com.tsk.ide.vscode` package provides a professional solution for effortlessly setting up Visual Studio Code to seamlessly integrate with Unity. It automates the generation of essential configuration files, namely omnisharp.json and .editorconfig, sparing you valuable time and effort that would otherwise be spent on manual setup.

### Enable the Full Potential of Modern .NET

By eliminating the need to disable the use of ModernNet, you can effortlessly access the complete range of modern .NET features within your Unity projects. This seamless integration unlocks a multitude of benefits, including enhanced performance and improved stability, empowering you to optimize your projects like never before.

### Customization Options with Externals Tools

Furthermore, the package introduces a dedicated configuration section within External Tools. This empowers users to exercise complete control over the settings files generated by the package, allowing for manual customization to accommodate individual preferences and project-specific requirements. With this added flexibility, you can fine-tune your development environment to achieve optimal results.

## Prerequisites

1. Install both the .Net 7 and .Net 6 SDKs - <https://dotnet.microsoft.com/en-us/download>
2. **[Windows only]** Logout or restart Windows to allow changes to `%PATH%` to take effect.
3. **[macOS only]** To avoid seeing "Some projects have trouble loading. Please review the output for more details", make sure to install the latest stable [Mono](https://www.mono-project.com/download/) release.
    - Note: This version of Mono, which is installed into your system, will not interfere with the version of MonoDevelop that is installed by Unity.
4. Install the C# extension from the VS Code Marketplace.
5. Install Build Tools for Visual Studio (Windows only)

The C# extension no longer ships with Microsoft Build Tools so they must be installed manually.

-   Download the [Build Tools for Visual Studio 2022](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022).
-   Install the .NET desktop build tools workload. No other components are required.

## Install via Package Manager

### Unity

-   Open Window/Package Manager
-   Click +
-   Select Add package from git URL
-   Paste `https://github.com/Chizaruu/com.tsk.ide.vscode.git#upm` into URL
-   Click Add

### OpenUPM

Please follow the instrustions:

-   Open Edit/Project Settings/Package Manager
-   Add a new Scoped Registry (or edit the existing OpenUPM entry)

```text
  Name: package.openupm.com
  URL: https://package.openupm.com
  Scope(s): com.tsk.ide.vscode
```

-   Click Save (or Apply)
-   Open Window/Package Manager
-   Click +
-   Select Add package by name... or Add package from git URL...
-   Paste com.tsk.ide.vscode into name
-   Paste 1.4.1 into version
-   Click Add

Alternatively, merge the snippet to Packages/manifest.json

```json
{
    "scopedRegistries": [
        {
            "name": "package.openupm.com",
            "url": "https://package.openupm.com",
            "scopes": ["com.tsk.ide.vscode"]
        }
    ],
    "dependencies": {
        "com.tsk.ide.vscode": "1.4.1"
    }
}
```

## Contributing

Thank you for considering contributing to the `com.tsk.ide.vscode` package! To contribute, please follow these guidelines:

-   Create a new branch for your changes.
-   Discuss your changes by creating a new issue in the repository before starting work.
-   Follow the existing coding conventions and style.
-   Provide a clear description of your changes in your pull request.
-   Submit your pull request to the default branch.

We appreciate all contributions to com.tsk.ide.vscode!
