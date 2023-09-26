// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Style",
    "IDE0056:Use index operator",
    Justification = "Not supported in netstandard2.0",
    Scope = "member",
    Target = "~M:PSSharp.PowerShell.Async.ArgumentQuotation.Extract(System.String,System.String@)~PSSharp.PowerShell.Async.ArgumentQuotation"
)]
[assembly: SuppressMessage(
    "Style",
    "IDE0057:Use range operator",
    Justification = "Not supported in netstandard2.0",
    Scope = "member",
    Target = "~M:PSSharp.PowerShell.Async.ArgumentQuotation.Extract(System.String,System.String@)~PSSharp.PowerShell.Async.ArgumentQuotation"
)]
[assembly: SuppressMessage(
    "Style",
    "IDE0090:Use 'new(...)'",
    Justification = "Not supported in netstandard2.0"
)]
[assembly: SuppressMessage(
    "Style",
    "IDE0070:Use 'System.HashCode'",
    Justification = "Not supported in netstandard2.0",
    Scope = "member",
    Target = "~M:PSSharp.PowerShell.Async.ArgumentCompleterContext.GetHashCode~System.Int32"
)]
