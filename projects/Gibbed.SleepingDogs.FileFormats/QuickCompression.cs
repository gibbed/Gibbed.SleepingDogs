/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
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
using System.IO;
using Gibbed.IO;

namespace Gibbed.SleepingDogs.FileFormats
{
    public sealed class QuickCompression
    {
        public const uint Signature = 0x51434D50; // 'QCMP'

        public static void Decompress(Stream input, Stream output)
        {
            Decompress(input, out _, output);
        }

        public static void Decompress(Stream input, out ulong hash, Stream output)
        {
            var data = Decompress(input, out hash);
            output.WriteBytes(data);
        }

        public static byte[] Decompress(Stream input)
        {
            return Decompress(input, out _);
        }

        public static byte[] Decompress(Stream input, out ulong hash)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var type = input.ReadValueU16(endian); // 1 = LZ
            var version = input.ReadValueU16(endian);
            var dataOffset = input.ReadValueU32(endian);
            var extraSize = input.ReadValueU32(endian);
            var compressedSize = input.ReadValueS64(endian);
            var uncompressedSize = input.ReadValueS64(endian);
            var uncompressedHash = input.ReadValueU64(endian);
            input.Seek(24, SeekOrigin.Current); // 6 * 4

            if (type != 1 || version != 1)
            {
                throw new FormatException();
            }

            if (dataOffset != 64)
            {
                throw new FormatException();
            }

            var compressedBytes = input.ReadBytes((int)(compressedSize - dataOffset));
            var uncompressedBytes = new byte[uncompressedSize];

            var lengths = new ushort[32];
            var offsets = new ushort[32];

            int x = 0, y = 0, z = 0;
            for (; y < uncompressedSize;)
            {
                var op = compressedBytes[x++];

                if (op < 32)
                {
                    var length = op + 1;
                    Array.Copy(compressedBytes, x, uncompressedBytes, y, length);
                    x += length;
                    y += length;
                }
                else
                {
                    var mode = (op >> 5) & 0x07;
                    var index = (op >> 0) & 0x1F;

                    ushort length, offset;
                    if (mode == 1)
                    {
                        length = lengths[index];
                        offset = offsets[index];
                    }
                    else
                    {
                        offset = (ushort)(compressedBytes[x++] | (index << 8));
                        length = (ushort)((mode == 7 ? compressedBytes[x++] : mode) + 1);

                        offsets[z] = offset;
                        lengths[z] = length;
                        z = (z + 1) % 32;
                    }

                    for (int i = 0, j = y - offset; i < length; i++, j++)
                    {
                        uncompressedBytes[y] = uncompressedBytes[j];
                        y++;
                    }
                }
            }

            if (y != uncompressedSize)
            {
                throw new InvalidOperationException();
            }

            hash = uncompressedHash;
            return uncompressedBytes;
        }
    }
}
