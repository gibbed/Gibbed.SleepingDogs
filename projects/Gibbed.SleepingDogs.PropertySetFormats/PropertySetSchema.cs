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

using System.Collections.Generic;
using System.Linq;

namespace Gibbed.SleepingDogs.PropertySetFormats
{
    public class PropertySetSchema
    {
        private readonly PropertySchema[] _Properties;
        private readonly int _Count;
        private readonly ushort _DataSize;

        public PropertySetSchema(IEnumerable<PropertySchema> properties, ushort dataSize)
        {
            this._Properties = properties.ToArray();
            this._Count = this._Properties.Length;
            this._DataSize = dataSize;
        }

        public PropertySchema this[int i]
        {
            get { return this._Properties[i]; }
        }

        public int Count
        {
            get { return this._Count; }
        }

        public ushort DataSize
        {
            get { return this._DataSize; }
        }
    }
}
