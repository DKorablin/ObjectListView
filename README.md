# ObjectListView
[![Auto build](https://github.com/DKorablin/ObjectListView/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/ObjectListView/releases/latest)
[![Nuget](https://img.shields.io/nuget/v/AlphaOmega.ObjectListView)](https://www.nuget.org/packages/AlphaOmega.ObjectListView)

## Overview
ObjectListView is an enhanced Windows Forms ListView replacement for displaying and editing rich tabular object data with minimal boilerplate. It adds strong typing, data binding helpers, virtual mode data sources, filtering, grouping, sorting, column formatting, cell editing and custom rendering (images, progress bars, checkboxes) while remaining a drop‑in WinForms control.

## Features
- Bind collections of POCO objects (SetObjects / AddObject / RemoveObject)
- Virtual list data source for large datasets
- Per column: sorting, grouping, filtering, formatting, tooltips
- Fast text filtering (TextMatchFilter etc.)
- Custom cell renderers (images, progress bars, check states)
- Inline cell editing
- Column definitions via OLVColumn (AspectName, AspectGetter, AspectToStringFormat)
- Alternate row styling, selection decoration, hot item hints
- Multi-target: .NET 3.5, 6, 7, 8 + .NET Framework 4.8
- Strongly named & signed assembly

## Install
NuGet: `dotnet add package AlphaOmega.ObjectListView`

## Quick usage
```csharp
var olv = new ObjectListView { Dock = DockStyle.Fill }; // drop onto a Form
olv.Columns.Add(new OLVColumn("Name", "Name") { Width = 150 });
olv.Columns.Add(new OLVColumn("Status", "Status"));
olv.SetObjects(items); // items = IEnumerable<YourType>
```

Filtering example:
```csharp
olv.AdditionalFilter = TextMatchFilter.Contains(olv, "search text");
olv.UpdateColumnFiltering();
```

Virtual mode (large data): implement IVirtualListDataSource and assign to VirtualListDataSource.

## What's new
1. Fixed runtime error related to internal field changes in .NET 7+ Windows Forms.
2. Added assembly signature (PublicKeyToken=a8ac5fc45c3adb8d).
3. Added PE file signing (S/N: 00c18bc05b61a77408c694bd3542d035).
4. Added CI/CD pipelines.
5. Added .NET 6, 7, 8 support (.NET 3.5 still available).

## Compatibility
WinForms only. Incompatible with WPF because some logic relies on `Application.Idle` which does not fire in WPF by default.

## License
MIT (see repository).
