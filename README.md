# ObjectListView
[![Auto build](https://github.com/DKorablin/ObjectListView/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/ObjectListView/releases/latest)
[![Nuget](https://img.shields.io/nuget/v/AlphaOmega.ObjectListView)](https://www.nuget.org/packages/AlphaOmega.ObjectListView)

## What's new
1. Fixed runtime error related to internal fields changes in the new version of .NET 7+ Windows Forms framework.
2. Added assembly signature (PublicKeyToken=a8ac5fc45c3adb8d)
3. Added PE file signing. (S/N: 00c18bc05b61a77408c694bd3542d035)
4. Added CI/CD pipelines
5. Added .NET 6, 7, 8 support (.NET 3.5 is still available)

Incompatible with WPF controls because some of the code depends on the Application.Idle event which will not occur for WPF applications out of the box...