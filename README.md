# ObjectListView
[![Auto build](https://github.com/DKorablin/ObjectListView/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/ObjectListView/releases/latest)
[![Nuget](https://img.shields.io/nuget/v/AlphaOmega.ObjectListView)](https://www.nuget.org/packages/AlphaOmega.ObjectListView)

Fixed runtime error related to internal fields changes in the new version of .NET 7+ Windows Forms framework.
Incompatible with WPF controls because some of the code depends on the Application.Idle event which will not occur for WPF applications out of the box..