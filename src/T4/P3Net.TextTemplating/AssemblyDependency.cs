using System;
using System.Collections.Generic;
using System.Linq;

namespace P3Net.TextTemplating
{
    /// <summary>Defines an assembly dependency.</summary>
    public struct AssemblyDependency
    {
        #region Construction

        /// <summary>Initializes an instance of the <see cref="AssemblyDependency"/> structure.</summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="assemblyName">The assembly name.</param>        
        public AssemblyDependency ( string displayName, string assemblyName ) : this(displayName, assemblyName, null)
        {
        }

        /// <summary>Initializes an instance of the <see cref="AssemblyDependency"/> structure.</summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="version">The minimal version.</param>
        public AssemblyDependency ( string displayName, string assemblyName, Version version ) : this()
        {
            m_assemblyName = assemblyName ?? "";
            m_displayName = displayName ?? "";
            m_version = version;
        }
        #endregion

        #region Public Members

        /// <summary>Gets the assembly name.</summary>
        public string AssemblyName
        {
            get { return m_assemblyName ?? ""; }
        }

        /// <summary>Gets the assembly minimal version.</summary>
        public Version AssemblyVersion
        {
            get { return m_version; }
        }

        /// <summary>Gets the display name.</summary>
        public string DisplayName
        {
            get { return m_displayName ?? ""; }
        }
        #endregion

        #region Private Members

        private readonly string m_displayName;
        private readonly string m_assemblyName;
        private readonly Version m_version;
        #endregion
    }
}
