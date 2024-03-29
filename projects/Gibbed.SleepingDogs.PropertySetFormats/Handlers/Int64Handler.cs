﻿/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.SleepingDogs.PropertySetFormats.Handlers
{
    internal class Int64Handler : BasicHandler<long>
    {
        internal Int64Handler()
            : base(3, 8, 8, "int64", "Int64", false)
        {
        }

        protected override long Read(Stream input, Endian endian)
        {
            return input.ReadValueS64(endian);
        }

        protected override void Write(long value, Stream output, Endian endian, long ownerOffset)
        {
            output.WriteValueS64(value, endian);
        }
    }
}
