/* Copyright (c) 2015 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace Gibbed.SleepingDogs.PropertySetConvert
{
    internal static class XPathNavigatorHelpers
    {
        #region Magic
        private static class Magic<T>
        {
            public delegate bool TryParseBasicDelegate(
                string value,
                out T result);

            public static T ParseValue(XPathNavigator navigator, TryParseBasicDelegate tryParse)
            {
                T dummy;
                if (tryParse(navigator.Value, out dummy) == false)
                {
                    var lineInfo = (IXmlLineInfo)navigator;
                    throw new FormatException(
                        string.Format("could not parse '{0}' as {1} at line {2} position {3}",
                                      navigator.Value,
                                      typeof(T).Name,
                                      lineInfo.LineNumber,
                                      lineInfo.LinePosition));
                }
                return dummy;
            }

            public static T ParseAttribute(
                XPathNavigator navigator,
                string name,
                TryParseBasicDelegate tryParse)
            {
                if (navigator.MoveToAttribute(name, "") == false)
                {
                    var lineInfo = (IXmlLineInfo)navigator;
                    throw new KeyNotFoundException(
                        string.Format("could not find attribute '{0}' at line {1} position {2}",
                                      name,
                                      lineInfo.LineNumber,
                                      lineInfo.LinePosition));
                }

                var dummy = ParseValue(navigator, tryParse);
                navigator.MoveToParent();
                return dummy;
            }

            public static T ParseAttribute(
                XPathNavigator navigator,
                string name,
                TryParseBasicDelegate tryParse,
                T defaultValue)
            {
                if (navigator.MoveToAttribute(name, "") == false)
                {
                    return defaultValue;
                }

                var dummy = ParseValue(navigator, tryParse);
                navigator.MoveToParent();
                return dummy;
            }

            public delegate bool TryParseNumberDelegate(
                string s,
                NumberStyles style,
                IFormatProvider provider,
                out T result);

            public static T ParseValue(XPathNavigator navigator, TryParseNumberDelegate tryParse, NumberStyles style)
            {
                T dummy;
                if (tryParse(navigator.Value, style, CultureInfo.InvariantCulture, out dummy) == false)
                {
                    var lineInfo = (IXmlLineInfo)navigator;
                    throw new FormatException(
                        string.Format("could not parse '{0}' as {1} at line {2} position {3}",
                                      navigator.Value,
                                      typeof(T).Name,
                                      lineInfo.LineNumber,
                                      lineInfo.LinePosition));
                }
                return dummy;
            }

            public static T ParseAttribute(
                XPathNavigator navigator,
                string name,
                TryParseNumberDelegate tryParse,
                NumberStyles style)
            {
                if (navigator.MoveToAttribute(name, "") == false)
                {
                    var lineInfo = (IXmlLineInfo)navigator;
                    throw new KeyNotFoundException(
                        string.Format("could not find attribute '{0}' at line {1} position {2}",
                                      name,
                                      lineInfo.LineNumber,
                                      lineInfo.LinePosition));
                }

                var dummy = ParseValue(navigator, tryParse, style);
                navigator.MoveToParent();
                return dummy;
            }

            public static T ParseAttribute(
                XPathNavigator navigator,
                string name,
                TryParseNumberDelegate tryParse,
                NumberStyles style,
                T defaultValue)
            {
                if (navigator.MoveToAttribute(name, "") == false)
                {
                    return defaultValue;
                }

                var dummy = ParseValue(navigator, tryParse, style);
                navigator.MoveToParent();
                return dummy;
            }

            public delegate bool TryParseEnumDelegate(
                string value,
                bool ignoreCase,
                out T result);

            public static T ParseValue(XPathNavigator navigator, TryParseEnumDelegate tryParse, bool ignoreCase)
            {
                T dummy;
                if (tryParse(navigator.Value, ignoreCase, out dummy) == false)
                {
                    var lineInfo = (IXmlLineInfo)navigator;
                    throw new FormatException(
                        string.Format("could not parse '{0}' as {1} at line {2} position {3}",
                                      navigator.Value,
                                      typeof(T).Name,
                                      lineInfo.LineNumber,
                                      lineInfo.LinePosition));
                }
                return dummy;
            }

            public static T ParseAttribute(
                XPathNavigator navigator,
                string name,
                TryParseEnumDelegate tryParse,
                bool ignoreCase)
            {
                if (navigator.MoveToAttribute(name, "") == false)
                {
                    var lineInfo = (IXmlLineInfo)navigator;
                    throw new KeyNotFoundException(
                        string.Format("could not find attribute '{0}' at line {1} position {2}",
                                      name,
                                      lineInfo.LineNumber,
                                      lineInfo.LinePosition));
                }

                var dummy = ParseValue(navigator, tryParse, ignoreCase);
                navigator.MoveToParent();
                return dummy;
            }

            public static T ParseAttribute(
                XPathNavigator navigator,
                string name,
                TryParseEnumDelegate tryParse,
                bool ignoreCase,
                T defaultValue)
            {
                if (navigator.MoveToAttribute(name, "") == false)
                {
                    return defaultValue;
                }

                var dummy = ParseValue(navigator, tryParse, ignoreCase);
                navigator.MoveToParent();
                return dummy;
            }
        }
        #endregion

        public static string ParseAttributeString(this XPathNavigator navigator, string name)
        {
            if (navigator.MoveToAttribute(name, "") == false)
            {
                var lineInfo = (IXmlLineInfo)navigator;
                throw new KeyNotFoundException(
                    string.Format("could not find attribute '{0}' at line {1} position {2}",
                                  name,
                                  lineInfo.LineNumber,
                                  lineInfo.LinePosition));
            }

            var value = navigator.Value;
            navigator.MoveToParent();
            return value;
        }

        public static string ParseAttributeString(this XPathNavigator navigator, string name, string defaultValue)
        {
            if (navigator.MoveToAttribute(name, "") == false)
            {
                return defaultValue;
            }

            var value = navigator.Value;
            navigator.MoveToParent();
            return value;
        }

        public static T ParseValueEnum<T>(this XPathNavigator navigator)
            where T : struct
        {
            return Magic<T>.ParseValue(navigator, Enum.TryParse, true);
        }

        public static bool ParseValueBoolean(this XPathNavigator navigator)
        {
            return Magic<bool>.ParseValue(navigator, bool.TryParse);
        }

        public static byte ParseValueUInt8(this XPathNavigator navigator)
        {
            return Magic<byte>.ParseValue(navigator, byte.TryParse, NumberStyles.Integer);
        }

        public static ushort ParseValueUInt16(this XPathNavigator navigator)
        {
            return Magic<ushort>.ParseValue(navigator, ushort.TryParse, NumberStyles.Integer);
        }

        public static uint ParseValueUInt32(this XPathNavigator navigator)
        {
            return Magic<uint>.ParseValue(navigator, uint.TryParse, NumberStyles.Integer);
        }

        public static ulong ParseValueUInt64(this XPathNavigator navigator)
        {
            return Magic<ulong>.ParseValue(navigator, ulong.TryParse, NumberStyles.Integer);
        }

        public static sbyte ParseValueInt8(this XPathNavigator navigator)
        {
            return Magic<sbyte>.ParseValue(navigator, sbyte.TryParse, NumberStyles.Integer);
        }

        public static short ParseValueInt16(this XPathNavigator navigator)
        {
            return Magic<short>.ParseValue(navigator, short.TryParse, NumberStyles.Integer);
        }

        public static int ParseValueInt32(this XPathNavigator navigator)
        {
            return Magic<int>.ParseValue(navigator, int.TryParse, NumberStyles.Integer);
        }

        public static long ParseValueInt64(this XPathNavigator navigator)
        {
            return Magic<long>.ParseValue(navigator, long.TryParse, NumberStyles.Integer);
        }

        public static float ParseValueFloat32(this XPathNavigator navigator)
        {
            return Magic<float>.ParseValue(navigator, float.TryParse, NumberStyles.Float);
        }

        public static DataFormats.Symbol ParseValueSymbol(this XPathNavigator navigator)
        {
            return Magic<DataFormats.Symbol>.ParseValue(navigator, Helpers.TryParseSymbol);
        }

        public static DataFormats.SymbolUpperCase ParseValueSymbolUpperCase(this XPathNavigator navigator)
        {
            return Magic<DataFormats.SymbolUpperCase>.ParseValue(navigator, Helpers.TryParseSymbolUpperCase);
        }

        public static DataFormats.WwiseId ParseValueWwiseId(this XPathNavigator navigator)
        {
            return Magic<DataFormats.WwiseId>.ParseValue(navigator, Helpers.TryParseWwiseId);
        }

        public static T ParseAttributeEnum<T>(this XPathNavigator navigator, string name)
            where T : struct
        {
            return Magic<T>.ParseAttribute(navigator, name, Enum.TryParse, true);
        }

        public static T ParseAttributeEnum<T>(this XPathNavigator navigator, string name, T defaultValue)
            where T : struct
        {
            return Magic<T>.ParseAttribute(navigator, name, Enum.TryParse, true, defaultValue);
        }

        public static bool ParseAttributeBoolean(this XPathNavigator navigator, string name)
        {
            return Magic<bool>.ParseAttribute(navigator, name, bool.TryParse);
        }

        public static bool ParseAttributeBoolean(this XPathNavigator navigator, string name, bool defaultValue)
        {
            return Magic<bool>.ParseAttribute(navigator, name, bool.TryParse, defaultValue);
        }

        public static byte ParseAttributeUInt8(this XPathNavigator navigator, string name)
        {
            return Magic<byte>.ParseAttribute(navigator, name, byte.TryParse, NumberStyles.Integer);
        }

        public static byte ParseAttributeUInt8(this XPathNavigator navigator, string name, byte defaultValue)
        {
            return Magic<byte>.ParseAttribute(navigator, name, byte.TryParse, NumberStyles.Integer, defaultValue);
        }

        public static ushort ParseAttributeUInt16(this XPathNavigator navigator, string name)
        {
            return Magic<ushort>.ParseAttribute(navigator, name, ushort.TryParse, NumberStyles.Integer);
        }

        public static ushort ParseAttributeUInt16(this XPathNavigator navigator, string name, ushort defaultValue)
        {
            return Magic<ushort>.ParseAttribute(navigator, name, ushort.TryParse, NumberStyles.Integer, defaultValue);
        }

        public static uint ParseAttributeUInt32(this XPathNavigator navigator, string name)
        {
            return Magic<uint>.ParseAttribute(navigator, name, uint.TryParse, NumberStyles.Integer);
        }

        public static uint ParseAttributeUInt32(this XPathNavigator navigator, string name, uint defaultValue)
        {
            return Magic<uint>.ParseAttribute(navigator, name, uint.TryParse, NumberStyles.Integer, defaultValue);
        }

        public static ulong ParseAttributeUInt64(this XPathNavigator navigator, string name)
        {
            return Magic<ulong>.ParseAttribute(navigator, name, ulong.TryParse, NumberStyles.Integer);
        }

        public static ulong ParseAttributeUInt64(this XPathNavigator navigator, string name, ulong defaultValue)
        {
            return Magic<ulong>.ParseAttribute(navigator, name, ulong.TryParse, NumberStyles.Integer, defaultValue);
        }

        public static sbyte ParseAttributeInt8(this XPathNavigator navigator, string name)
        {
            return Magic<sbyte>.ParseAttribute(navigator, name, sbyte.TryParse, NumberStyles.Integer);
        }

        public static sbyte ParseAttributeInt8(this XPathNavigator navigator, string name, sbyte defaultValue)
        {
            return Magic<sbyte>.ParseAttribute(navigator, name, sbyte.TryParse, NumberStyles.Integer, defaultValue);
        }

        public static short ParseAttributeInt16(this XPathNavigator navigator, string name)
        {
            return Magic<short>.ParseAttribute(navigator, name, short.TryParse, NumberStyles.Integer);
        }

        public static short ParseAttributeInt16(this XPathNavigator navigator, string name, short defaultValue)
        {
            return Magic<short>.ParseAttribute(navigator, name, short.TryParse, NumberStyles.Integer, defaultValue);
        }

        public static int ParseAttributeInt32(this XPathNavigator navigator, string name)
        {
            return Magic<int>.ParseAttribute(navigator, name, int.TryParse, NumberStyles.Integer);
        }

        public static int ParseAttributeInt32(this XPathNavigator navigator, string name, int defaultValue)
        {
            return Magic<int>.ParseAttribute(navigator, name, int.TryParse, NumberStyles.Integer, defaultValue);
        }

        public static long ParseAttributeInt64(this XPathNavigator navigator, string name)
        {
            return Magic<long>.ParseAttribute(navigator, name, long.TryParse, NumberStyles.Integer);
        }

        public static long ParseAttributeInt64(this XPathNavigator navigator, string name, long defaultValue)
        {
            return Magic<long>.ParseAttribute(navigator, name, long.TryParse, NumberStyles.Integer, defaultValue);
        }

        public static float ParseAttributeFloat32(this XPathNavigator navigator, string name)
        {
            return Magic<float>.ParseAttribute(navigator, name, float.TryParse, NumberStyles.Float);
        }

        public static float ParseAttributeFloat32(this XPathNavigator navigator, string name, float defaultValue)
        {
            return Magic<float>.ParseAttribute(navigator, name, float.TryParse, NumberStyles.Float, defaultValue);
        }

        public static DataFormats.Symbol ParseAttributeSymbol(this XPathNavigator navigator, string name)
        {
            return Magic<DataFormats.Symbol>.ParseAttribute(navigator, name, Helpers.TryParseSymbol);
        }

        public static DataFormats.Symbol ParseAttributeSymbol(
            this XPathNavigator navigator,
            string name,
            DataFormats.Symbol defaultValue)
        {
            return Magic<DataFormats.Symbol>.ParseAttribute(navigator, name, Helpers.TryParseSymbol, defaultValue);
        }

        public static DataFormats.SymbolUpperCase ParseAttributeSymbolUpperCase(
            this XPathNavigator navigator,
            string name)
        {
            return Magic<DataFormats.SymbolUpperCase>.ParseAttribute(navigator, name, Helpers.TryParseSymbolUpperCase);
        }

        public static DataFormats.SymbolUpperCase ParseAttributeSymbolUpperCase(
            this XPathNavigator navigator,
            string name,
            DataFormats.SymbolUpperCase defaultValue)
        {
            return Magic<DataFormats.SymbolUpperCase>.ParseAttribute(
                navigator,
                name,
                Helpers.TryParseSymbolUpperCase,
                defaultValue);
        }

        public static DataFormats.WwiseId ParseAttributeWwiseId(this XPathNavigator navigator, string name)
        {
            return Magic<DataFormats.WwiseId>.ParseAttribute(navigator, name, Helpers.TryParseWwiseId);
        }

        public static DataFormats.WwiseId ParseAttributeWwiseId(
            this XPathNavigator navigator,
            string name,
            DataFormats.WwiseId defaultValue)
        {
            return Magic<DataFormats.WwiseId>.ParseAttribute(navigator, name, Helpers.TryParseWwiseId, defaultValue);
        }
    }
}
