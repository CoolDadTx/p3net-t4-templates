using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace P3Net.TextTemplating
{
    /// <summary>Provides extension methods for <see cref="Type"/>.</summary>
    public static class TypeExtensions
    {
        #region GetFriendlyName

        /// <summary>Gets the C# friendly name of a type, if any.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        /// <remarks>
        /// This method uses the <see cref="CSharpTypeNameProvider"/> for getting the name.
        /// </remarks>
        public static string GetFriendlyName ( this Type source )
        {
            return GetFriendlyName(source, new CSharpTypeNameProvider());
        }

        /// <summary>Gets the C# friendly name of a type, if any.</summary>
        /// <param name="type">The type.</param>
        /// <param name="includeNamespace"><see langword="true"/> to include the namespace in the type.</param>
        /// <returns>The type name.</returns>
        /// <remarks>
        /// This method uses the <see cref="CSharpTypeNameProvider"/> for getting the name.
        /// </remarks>
        public static string GetFriendlyName ( this Type source, bool includeNamespace )
        {
            return GetFriendlyName(source, new CSharpTypeNameProvider() { IncludeNamespace = includeNamespace });
        }

        /// <summary>Gets the C# friendly name of a type, if any.</summary>
        /// <param name="type">The type.</param>
        /// <param name="provider">The provider.</param>
        /// <returns>The type name.</returns>        
        /// <exception cref="ArgumentNullException"><paramref name="provider"/> is <see langword="null"/>.</exception>
        public static string GetFriendlyName ( this Type source, ITypeNameProvider provider )
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            return provider.GetTypeName(source);
        }
        #endregion

        /// <summary>Gets all the public instance methods of a type.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The methods.</returns>
        /// <remarks>
        /// Only public, instance methods of the type are included.  Property accessors, special methods and
        /// static methods are ignored.
        /// </remarks>
        public static IEnumerable<MethodInfo> GetPublicInstanceMethods ( this Type source )
        {
            return (from m in source.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    where !m.IsSpecialName && !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_")
                    select m);
        }

        /// <summary>Determines if the type is <see langword="void"/>.</summary>
        /// <param name="source">The source.</param>
        /// <returns><see langword="true"/> if <see langword="void"/>.</returns>
        public static bool IsVoid ( this Type source )
        {
            return source == typeof(void);
        }
    }
}
