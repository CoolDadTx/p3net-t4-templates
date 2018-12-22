using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace P3Net.TextTemplating
{
    /// <summary>Provides extension methods for <see cref="ParameterInfo"/>.</summary>
    public static class ParameterInfoExtensions
    {
        /// <summary>Determines if a parameter is an out or ref parameter.</summary>
        /// <param name="source">The source.</param>
        /// <returns><see langword="true"/> if the value is either of the parameter types.</returns>
        /// <seealso cref="ParameterInfo.IsOut">IsOut</seealso>
        /// <seealso cref="IsRef" />
        public static bool IsOutOrRef ( this ParameterInfo source )
        {
            return source.IsOut || source.IsRef();
        }

        /// <summary>Determines if a parameter is a ref parameter.</summary>
        /// <param name="source">The source.</param>
        /// <returns><see langword="true"/> if the value is a ref parameter.</returns>
        /// <seealso cref="ParameterInfo.IsOut">IsOut</seealso>
        /// <seealso cref="IsOutOrRef" />
        public static bool IsRef ( this ParameterInfo source )
        {
            return source.ParameterType.IsByRef;
        }
    }
}
