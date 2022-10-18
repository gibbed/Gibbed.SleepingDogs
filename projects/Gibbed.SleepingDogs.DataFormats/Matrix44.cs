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

using System.IO;
using Gibbed.IO;

namespace Gibbed.SleepingDogs.DataFormats
{
    public struct Matrix44
    {
        public Vector4 V0;
        public Vector4 V1;
        public Vector4 V2;
        public Vector4 V3;

        public Matrix44(Vector4 v0, Vector4 v1, Vector4 v2, Vector4 v3)
        {
            this.V0 = v0;
            this.V1 = v1;
            this.V2 = v2;
            this.V3 = v3;
        }

        public static Matrix44 Read(Stream input, Endian endian)
        {
            var v0 = Vector4.Read(input, endian);
            var v1 = Vector4.Read(input, endian);
            var v2 = Vector4.Read(input, endian);
            var v3 = Vector4.Read(input, endian);
            return new Matrix44(v0, v1, v2, v3);
        }

        public static void Write(Stream output, Matrix44 instance, Endian endian)
        {
            instance.V0.Write(output, endian);
            instance.V1.Write(output, endian);
            instance.V2.Write(output, endian);
            instance.V3.Write(output, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }
    }
}
