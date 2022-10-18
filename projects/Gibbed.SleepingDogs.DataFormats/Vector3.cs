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

using System.IO;
using Gibbed.IO;

namespace Gibbed.SleepingDogs.DataFormats
{
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static Vector3 Read(Stream input, Endian endian)
        {
            var x = input.ReadValueF32(endian);
            var y = input.ReadValueF32(endian);
            var z = input.ReadValueF32(endian);
            return new Vector3(x, y, z);
        }

        public static void Write(Vector3 instance, Stream output, Endian endian)
        {
            output.WriteValueF32(instance.X, endian);
            output.WriteValueF32(instance.Y, endian);
            output.WriteValueF32(instance.Z, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(this, output, endian);
        }
    }
}
