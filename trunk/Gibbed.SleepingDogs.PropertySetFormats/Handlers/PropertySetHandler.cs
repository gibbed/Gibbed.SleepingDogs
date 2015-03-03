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
using System.IO;
using Gibbed.IO;

namespace Gibbed.SleepingDogs.PropertySetFormats.Handlers
{
    public class PropertySetHandler : IHandler
    {
        public Type NativeType
        {
            get { return typeof(PropertySet); }
        }

        public byte Id
        {
            get { return 26; }
        }

        public int ByteSize
        {
            get { return 8; }
        }

        public int Alignment
        {
            get { return 8; }
        }

        public string Name
        {
            get { return "PropertySet"; }
        }

        public string XmlTag
        {
            get { return "PropSet"; }
        }

        public bool UsesPointer
        {
            get { return true; }
        }

        object IHandler.Read(Stream input, Endian endian, PropertySetSchemaProvider schemaProvider)
        {
            var resource = new DataFormats.PropertySet();
            resource.Deserialize(input, endian);
            return PropertySet.Read(input, resource, endian, schemaProvider);
        }

        public void Write(Stream output, object value, Endian endian, long ownerOffset, PropertySetSchemaProvider schemaProvider)
        {
            var startPosition = output.Position;
            var resource = new DataFormats.PropertySet();
            output.Position += resource.Size;

            ((PropertySet)value).Write(output, endian, resource, startPosition, schemaProvider);

            var endPosition = output.Position;

            output.Position = startPosition;
            resource.OwnerOffset = ownerOffset;
            resource.Serialize(output, endian);

            output.Position = endPosition;
        }
    }
}
