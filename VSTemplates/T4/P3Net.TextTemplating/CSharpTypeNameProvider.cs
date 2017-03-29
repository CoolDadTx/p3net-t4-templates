using System;
using System.Collections.Generic;
using System.Linq;

namespace P3Net.TextTemplating
{
    /// <summary>Provides the C# version of <see cref="ITypeNameProvider"/>.</summary>
    public class CSharpTypeNameProvider : TypeNameProvider
    {
        #region Construction

        static CSharpTypeNameProvider ()
        {
            s_aliasMappings = new Dictionary<Type, string>() {
                                    { typeof(void), "void" },
                                    { typeof(char), "char" },
                                    { typeof(string), "string" },
                                    { typeof(bool), "bool" },

                                    { typeof(float), "float" },
                                    { typeof(double), "double" },
                                    { typeof(decimal), "decimal" },

                                    { typeof(sbyte), "sbyte" },
                                    { typeof(short), "short" },
                                    { typeof(int), "int" },
                                    { typeof(long), "long" },
                                    { typeof(byte), "byte" },
                                    { typeof(ushort), "ushort" },
                                    { typeof(uint), "uint" },
                                    { typeof(ulong), "ulong" },

                //Some standard System types
                { typeof(DateTime), "DateTime" },
                                    { typeof(Guid), "Guid" },
                                    { typeof(TimeSpan), "TimeSpan" },
                                };
        }
        #endregion        

        #region Protected Members

        protected override string FormatArrayType ( Type elementType, int dimensions )
        {
            //Format => Type[,,,]           
            return String.Format("{0}[{1}]", GetTypeName(elementType), new string(',', dimensions - 1));
        }

        protected override string FormatByRefType ( Type elementType )
        {
            //C# doesn't have a byref syntax for general types
            return GetTypeName(elementType);
        }

        protected override string FormatGenericType ( Type elementType, Type[] genericArguments )
        {
            //Get the generic arguments            
            var argStrings = String.Join(", ", from a in genericArguments select GetTypeName(a));

            //Format => Type<arg1, arg2, ...>
            return String.Format("{0}<{1}>", RemoveTrailingGenericSuffix(GetTypeName(elementType)), argStrings);
        }

        protected override string FormatNullableType ( Type elementType )
        {
            //Format => Type?   
            return GetTypeName(elementType) + "?";
        }

        protected override string FormatPointerType ( Type elementType )
        {
            //Format => Type*
            return GetTypeName(elementType) + "*";
        }

        protected override string FormatSimpleType ( Type type )
        {
            string result;
            if (s_aliasMappings.TryGetValue(type, out result))
                return result;

            return IncludeNamespace ? type.FullName : type.Name;
        }
        #endregion

        #region Private Members

        private static readonly Dictionary<Type, string> s_aliasMappings;
        #endregion
    }
}
