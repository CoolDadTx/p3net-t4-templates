using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace P3Net.TextTemplating
{
    /// <summary>Provides extension methods for <see cref="ICustomAttributeProvider"/>.</summary>
    public static class CustomAttributeProvider
    {
        /// <summary>Determines if an attribute is applied to a provider.</summary>
        /// <typeparam name="T">The attribute to check.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="inherit"><see langword="true"/> to include inherited types.</param>
        /// <returns><see langword="true"/> if the source has the attribute.</returns>
        public static bool HasAttribute<T>( this ICustomAttributeProvider source, bool inherit )
        {
            return source.GetCustomAttributes(typeof(T), inherit).Any();
        }
    }
}
