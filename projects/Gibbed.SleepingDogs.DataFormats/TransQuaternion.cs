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
    public struct TransQuaternion
    {
        public Quaternion Rotation;
        public Vector3 Transform;

        public TransQuaternion(Quaternion rotation, Vector3 transform)
        {
            this.Rotation = rotation;
            this.Transform = transform;
        }

        public static TransQuaternion Read(Stream input, Endian endian)
        {
            var rotation = Quaternion.Read(input, endian);
            var transform = Vector3.Read(input, endian);
            return new TransQuaternion(rotation, transform);
        }

        public static void Write(TransQuaternion instance, Stream output, Endian endian)
        {
            instance.Rotation.Write(output, endian);
            instance.Transform.Write(output, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(this, output, endian);
        }
    }
}
