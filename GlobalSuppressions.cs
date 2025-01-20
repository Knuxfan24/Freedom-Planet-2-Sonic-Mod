// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Here because VS thinks my custom Unity components are unused.
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>", Scope = "module")]

// Here because the Type arrays for Harmony CAN'T be simplified in this way (it fails to compile).
[assembly: SuppressMessage("Style", "IDE0300:Simplify collection initialization", Justification = "<Pending>", Scope = "module")]
