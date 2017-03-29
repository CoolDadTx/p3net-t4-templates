using System;
using System.Collections.Generic;
using System.Linq;

namespace P3Net.TextTemplating
{
    /// <summary>Provides type name information for a type.</summary>
    /// <remarks>
    /// The provider is generally language-specific and handles conversion of various types to the language-specific format.
    /// </remarks>
    public interface ITypeNameProvider
    {
        /// <summary>Gets or sets whether to include the namespace in the type name.</summary>
        bool IncludeNamespace { get; set; }

        /// <summary>Given a type it returns the type name.</summary>
        /// <param name="type">The type to get the name of.</param>
        /// <returns>The type name or an empty string if no conversion is possible.</returns>
        string GetTypeName ( Type type );
    }
}
