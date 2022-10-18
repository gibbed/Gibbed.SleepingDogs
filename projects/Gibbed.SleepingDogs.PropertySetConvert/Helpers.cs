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
using System.Globalization;
using Gibbed.SleepingDogs.FileFormats;

namespace Gibbed.SleepingDogs.PropertySetConvert
{
    internal class Helpers
    {
        private static bool TryParseHash(string s, out uint result, Func<string, uint> hasher)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (s.StartsWith("0x") == false)
            {
                result = hasher(s);
                return true;
            }

            s = s.Substring(2);

            uint dummy;
            if (uint.TryParse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out dummy) == false)
            {
                result = 0;
                return false;
            }

            result = dummy;
            return true;
        }

        public static bool TryParseSymbol(string s, out uint result)
        {
            uint dummy;
            if (TryParseHash(s, out dummy, StringHelpers.HashSymbol) == false)
            {
                result = 0;
                return false;
            }

            result = dummy;
            return true;
        }

        public static bool TryParseSymbol(string s, out DataFormats.Symbol result)
        {
            uint dummy;
            if (TryParseSymbol(s, out dummy) == false)
            {
                result = DataFormats.Symbol.Invalid;
                return false;
            }

            result = new DataFormats.Symbol(dummy);
            return true;
        }

        public static bool TryParseSymbolUpperCase(string s, out uint result)
        {
            uint dummy;
            if (TryParseHash(s, out dummy, StringHelpers.HashSymbolUpperCase) == false)
            {
                result = 0;
                return false;
            }

            result = dummy;
            return true;
        }

        public static bool TryParseSymbolUpperCase(string s, out DataFormats.SymbolUpperCase result)
        {
            uint dummy;
            if (TryParseSymbolUpperCase(s, out dummy) == false)
            {
                result = DataFormats.SymbolUpperCase.Invalid;
                return false;
            }

            result = new DataFormats.SymbolUpperCase(dummy);
            return true;
        }

        public static bool TryParseWwiseId(string s, out uint result)
        {
            uint dummy;
            if (TryParseHash(s, out dummy, StringHelpers.HashWwiseId) == false)
            {
                result = 0;
                return false;
            }

            result = dummy;
            return true;
        }

        public static bool TryParseWwiseId(string s, out DataFormats.WwiseId result)
        {
            uint dummy;
            if (TryParseWwiseId(s, out dummy) == false)
            {
                result = new DataFormats.WwiseId(0);
                return false;
            }

            result = new DataFormats.WwiseId(dummy);
            return true;
        }
    }
}
