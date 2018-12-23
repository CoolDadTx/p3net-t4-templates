using System;
using System.Collections.Generic;
using System.Linq;

namespace P3Net.TextTemplating
{
    /// <summary>Provides control for finding referenced assemblies.</summary>
    [Flags]
    public enum ReferencedAssemblyFlags
    {
        None = 0,

        /// <summary>User-defined binary references.</summary>
        UserDefinedAssembly = 1,

        /// <summary>System binary references.</summary>
        SystemAssembly = 2,

        /// <summary>COM object assemblies.</summary>
        ComReferences = 4,

        /// <summary>Project references.</summary>
        ProjectReferences = 8,

        AssemblyReferences = UserDefinedAssembly | SystemAssembly,

        /// <summary>All references.</summary>
        All = AssemblyReferences | ComReferences | ProjectReferences,
    }
}
