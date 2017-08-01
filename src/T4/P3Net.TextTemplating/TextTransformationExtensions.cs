using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TextTemplating;

namespace P3Net.TextTemplating
{
    /// <summary>Provides extensions to <see cref="TextTransformation"/>.</summary>
    public static class TextTransformationExtensions
    {
        /// <summary>Indents subsequent generated code.</summary>
        /// <param name="source">The source.</param>
        /// <param name="count">The number of times to indent.</param>
        public static TextTransformation Indent ( this TextTransformation source, int count = 1 )
        {
            for (int index = 0; index < count; ++index)
                source.PushIndent("\t");

            return source;
        }

        /// <summary>Makes a unique identifier.</summary>
        /// <param name="source">The source.</param>
        /// <param name="prefix">The optional prefix.</param>
        /// <param name="suffix">The optional suffix.</param>
        /// <returns>The unique identifier.</returns>
        public static string MakeUniqueIdentifier ( this TextTransformation source, string prefix = null, string suffix = null )
        {
            if (source.Session == null)
                source.Session = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            object value;
            var current = source.Session.TryGetValue("UniqueIdentifierCounter", out value) ? (int)value : 1;

            var str = String.Format("_{0}temp{1}{2}", prefix ?? "", suffix ?? "", current++);

            source.Session["UniqueIdentifierCounter"] = current;

            return str;
        }

        /// <summary>Unindents subsequent generated code.</summary>
        /// <param name="source">The source.</param>
        /// <param name="count">The number of times to unindent.</param>
        public static TextTransformation Unindent ( this TextTransformation source, int count = 1 )
        {
            for (int index = 0; index < count; ++index)
                source.PopIndent();

            return source;
        }
    }
}
