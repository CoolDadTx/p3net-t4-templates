using System;
using System.Collections.Generic;
using System.Linq;

namespace P3Net.TextTemplating
{
    /// <summary>Provides a base implementation of <see cref="ITypeNameProvider"/>.</summary>
    public abstract class TypeNameProvider : ITypeNameProvider
    {
        /// <summary>Gets or sets whether the namespace should be included as part of the type.</summary>
        public bool IncludeNamespace { get; set; }

        /// <summary>Gets the type name given a type.</summary>
        /// <param name="type">The type to get the name of.</param>
        /// <returns>The type name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public string GetTypeName ( Type type )
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return GetTypeNameCore(type);
        }

        #region Protected Members

        /// <summary>Formats an array type.</summary>
        /// <param name="elementType">The element type.</param>
        /// <param name="dimensions">The number of dimensions.</param>
        /// <returns>The type name.</returns>
        protected abstract string FormatArrayType ( Type elementType, int dimensions );

        /// <summary>Formats a byref type.</summary>
        /// <param name="elementType">The element type.</param>
        /// <returns>The type name.</returns>
        protected abstract string FormatByRefType ( Type elementType );

        /// <summary>Formats a generic type.</summary>
        /// <param name="genericType">The generic type.</param>
        /// <param name="genericArguments">The generic type arguments.</param>
        /// <returns>The type name.</returns>
        protected abstract string FormatGenericType ( Type genericType, Type[] genericArguments );

        /// <summary>Formats a nullable type.</summary>
        /// <param name="elementType">The element type.</param>
        /// <returns>The type name.</returns>
        protected abstract string FormatNullableType ( Type elementType );

        /// <summary>Formats a pointer type.</summary>
        /// <param name="elementType">The element type.</param>
        /// <returns>The type name.</returns>
        protected abstract string FormatPointerType ( Type elementType );

        /// <summary>Formats a simple type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        /// <remarks>
        /// If <see cref="IncludeNamespace"/> is <see langword="true"/> then the name includes the namespace, if appropriate.
        /// </remarks>
        protected abstract string FormatSimpleType ( Type type );

        /// <summary>Generates a type name given a type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        /// <remarks>
        /// The default implementation calls the various Process methods to process a type based upon its attributes.
        /// </remarks>
        protected virtual string GetTypeNameCore ( Type type )
        {
            //Pointers and references should use the element type
            if (type.IsPointer)
                return ProcessPointerType(type);
            if (type.IsByRef)
                return ProcessByRefType(type);
            if (type.IsArray)
                return ProcessArrayType(type);

            //Check for a nullable type next before we get into the generics
            if (type.IsGenericType && !type.IsGenericTypeDefinition && type.IsValueType && (type.Name == "Nullable`1"))
                return ProcessNullableType(type);

            //Check for a generic type next
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                return ProcessGenericType(type);

            //Simple types
            return FormatSimpleType(type);
        }

        /// <summary>Processes an array type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        /// <remarks>
        /// The default implementation calls <see cref="FormatArrayType"/> with the appropriate parmeters.
        /// </remarks>
        protected virtual string ProcessArrayType ( Type type )
        {
            //Multidimensional arrays are rectangular so they follow the form [,]
            //Jagged arrays come across as arrays of arrays they are single dimensional to us
            var elementType = type.GetElementType();
            var dimensions = type.GetArrayRank();

            return FormatArrayType(elementType, dimensions);
        }

        /// <summary>Processes a byref type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        /// <remarks>
        /// The default implementation calls <see cref="FormatByRefType"/> with the appropriate parmeters.
        /// </remarks>
        protected virtual string ProcessByRefType ( Type type )
        {
            var refType = type.GetElementType();

            return FormatByRefType(refType);
        }

        /// <summary>Processes a generic type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        /// <remarks>
        /// The default implementation calls <see cref="FormatGenericType"/> with the appropriate parmeters.
        /// </remarks>
        protected virtual string ProcessGenericType ( Type type )
        {
            //Get the generic type
            var genericType = type.GetGenericTypeDefinition();

            //Get the generic arguments            
            var args = type.GetGenericArguments();

            return FormatGenericType(genericType, args);
        }

        /// <summary>Processes a nullable type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        /// <remarks>
        /// The default implementation calls <see cref="FormatNullableType"/> with the appropriate parmeters.
        /// </remarks>
        protected virtual string ProcessNullableType ( Type type )
        {
            var nullableType = type.GetGenericArguments()[0];

            return FormatNullableType(nullableType);
        }

        /// <summary>Processes a pointer type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        /// <remarks>
        /// The default implementation calls <see cref="FormatPointerType"/> with the appropriate parmeters.
        /// </remarks>
        protected virtual string ProcessPointerType ( Type type )
        {
            var pointerType = type.GetElementType();

            return FormatPointerType(pointerType);
        }

        /// <summary>Removes any trailing generic suffix on a name.</summary>
        /// <param name="value">The name.</param>
        /// <returns>The trimmed string.</returns>
        protected static string RemoveTrailingGenericSuffix ( string value )
        {
            var tokens = value.Split(new char[] { '`' }, 2);

            return tokens[0];
        }
        #endregion
    }
}
