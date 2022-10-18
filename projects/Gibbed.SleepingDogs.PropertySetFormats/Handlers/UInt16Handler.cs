﻿/* Copyright (c) 2015 Rick (rick 'at' gibbed 'dot' us)
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
    internal class UInt16Handler : BasicHandler<ushort>
    {
        internal UInt16Handler()
            : base(6, 2, 2, "uint16", "UInt16", false)
        {
        }

        protected override ushort Read(Stream input, Endian endian)
        {
            return input.ReadValueU16(endian);
        }

        protected override void Write(ushort value, Stream output, Endian endian, long ownerOffset)
        {
            output.WriteValueU16(value, endian);
        }
    }
}