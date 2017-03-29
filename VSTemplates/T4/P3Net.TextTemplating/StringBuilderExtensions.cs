using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P3Net.TextTemplating
{
    /// <summary>Provides extension methods for <see cref="StringBuilder"/>.</summary>
    public static class StringBuilderExtensions
    {
        /// <summary>Appends a line.</summary>
        /// <param name="source">The source.</param>
        /// <param name="format">The line to add.</param>
        /// <param name="args">The format arguments.</param>
        /// <returns>The builder.</returns>
        public static StringBuilder AppendLine ( this StringBuilder source, string format, params object[] args )
        {
            source.AppendFormat(format, args);
            source.AppendLine();

            return source;
        }

        /// <summary>Determines if the string ends with the given value.</summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value to check for.</param>
        /// <returns><see langword="true"/> if it ends with the value or <see langword="false"/> otherwise.</returns>
        public static bool EndsWith ( this StringBuilder source, char value )
        {
            if (source.Length == 0)
                return false;

            return source[source.Length - 1] == value;
        }

        /// <summary>Determines if the string ends with the given value.</summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value to check for.</param>
        /// <returns><see langword="true"/> if it ends with the value or <see langword="false"/> otherwise.</returns>
        public static bool EndsWith ( this StringBuilder source, string value )
        {
            if (String.IsNullOrEmpty(value))
                return false;

            if (source.Length < value.Length)
                return false;

            int pos = source.Length - value.Length;
            foreach (var ch in value)
            {
                if (source[pos++] != ch)
                    return false;
            };

            return true;
        }

        /// <summary>Determines if the object is empty.</summary>
        /// <param name="source">The source.</param>
        /// <returns><see langword="true"/> if it is empty.</returns>
        public static bool IsEmpty ( this StringBuilder source )
        {
            return source.Length == 0;
        }

        /// <summary>Determines if the string starts with the given value.</summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value to check for.</param>
        /// <returns><see langword="true"/> if it starts with the value or <see langword="false"/> otherwise.</returns>
        public static bool StartsWith ( this StringBuilder source, char value )
        {
            if (source.Length == 0)
                return false;

            return source[0] == value;
        }

        /// <summary>Determines if the string starts with the given value.</summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value to check for.</param>
        /// <returns><see langword="true"/> if it starts with the value or <see langword="false"/> otherwise.</returns>
        public static bool StartsWith ( this StringBuilder source, string value )
        {
            if (String.IsNullOrEmpty(value))
                return false;

            if (source.Length < value.Length)
                return false;

            int pos = 0;
            foreach (var ch in value)
            {
                if (source[pos++] != ch)
                    return false;
            };

            return true;
        }
    }
}
