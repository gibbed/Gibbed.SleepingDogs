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

namespace Gibbed.SleepingDogs.FileFormats.Hashing
{
    public sealed class FNV32
    {
        public const uint Initial = 0x811C9DC5u;

        public static uint Compute(byte[] data)
        {
            return Compute(data, 0, data.Length, Initial);
        }

        public static uint Compute(byte[] data, int offset, int length)
        {
            return Compute(data, offset, length, Initial);
        }

        public static uint Compute(byte[] data, int offset, int length, uint seed)
        {
            uint hash = seed;
            for (int i = offset; i < offset + length; i++)
            {
                hash *= 0x1000193u;
                hash ^= data[i];
            }
            return hash;
        }

        public static uint Compute(string s)
        {
            return Compute(s, Initial);
        }

        public static uint Compute(string s, uint seed)
        {
            if (string.IsNullOrEmpty(s) == true)
            {
                return seed;
            }

            var hash = seed;
            foreach (char t in s)
            {
                hash *= 0x1000193u;
                hash ^= t;
            }
            return hash;
        }
    }
}
