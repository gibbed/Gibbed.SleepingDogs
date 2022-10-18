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
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Gibbed.SleepingDogs.DataFormats;

namespace Gibbed.SleepingDogs.PropertySetFormats
{
    public class PropertySet
    {
        #region Fields
        private Symbol _Name;
        private ushort _ReferenceCount;
        private Symbol _SchemaName;
        private PropertyCollectionFlags _Flags;
        private readonly List<ResourceHandle> _Parents;
        private readonly Dictionary<uint, object> _Properties;
        private readonly List<uint> _DefaultProperties;
        private PropertySetSchema _DefinedSchema;
        #endregion

        public PropertySet()
        {
            this._Parents = new();
            this._Properties = new();
            this._DefaultProperties = new();
        }

        #region Properties
        public Symbol Name
        {
            get { return this._Name; }
            set { this._Name = value; }
        }

        public ushort ReferenceCount
        {
            get { return this._ReferenceCount; }
            set { this._ReferenceCount = value; }
        }

        public Symbol SchemaName
        {
            get { return this._SchemaName; }
            set { this._SchemaName = value; }
        }

        public PropertyCollectionFlags Flags
        {
            get { return this._Flags; }
            set { this._Flags = value; }
        }

        public List<ResourceHandle> Parents => this._Parents;
        public Dictionary<uint, object> Properties => this._Properties;
        public List<uint> DefaultProperties => this._DefaultProperties;

        public PropertySetSchema DefinedSchema
        {
            get { return this._DefinedSchema; }
            set { this._DefinedSchema = value; }
        }
        #endregion

        public static PropertySet Read(
            Stream input,
            DataFormats.PropertySet resource,
            Endian endian,
            PropertySetSchemaProvider schemaProvider)
        {
            PropertySet instance = new();
            instance._Name = resource.Name;
            instance._ReferenceCount = resource.ReferenceCount;
            instance._SchemaName = resource.SchemaName;
            instance._Flags = resource.Flags;

            var parentHandles = new ResourceHandle[resource.ParentCount];
            if (resource.ParentCount > 0)
            {
                input.Position = resource.ParentsOffset;
                for (uint i = 0; i < resource.ParentCount; i++)
                {
                    parentHandles[i] = ResourceHandle.Read(input, endian);
                }
            }
            instance._Parents.Clear();
            instance._Parents.AddRange(parentHandles);

            PropertySetSchema schema;
            if (resource.SchemaName == Symbol.Invalid || resource.SchemaName == resource.Name)
            {
                if ((resource.Flags & PropertyCollectionFlags.InheritSchema) != 0)
                {
                    throw new InvalidOperationException();
                }

                var properties = new PropertySchema[resource.PropertyCount];
                if (resource.PropertyCount > 0)
                {
                    input.Position = resource.PropertiesOffset;
                    for (uint i = 0; i < resource.PropertyCount; i++)
                    {
                        properties[i] = PropertySchema.Read(input, endian);
                    }
                }

                schema = new PropertySetSchema(properties, resource.DataSize);

                if (resource.Name != Symbol.Invalid && resource.SchemaName == resource.Name)
                {
                    schemaProvider.Add(resource.Name.Id, schema);
                }

                instance._DefinedSchema = schema;
            }
            else
            {
                if (resource.PropertyCount != 0)
                {
                    throw new NotSupportedException();
                }

                schema = schemaProvider.Get(resource.SchemaName.Id);
            }

            instance._Properties.Clear();
            instance._DefaultProperties.Clear();

            if (schema.Count > 0)
            {
                if (resource.DataOffset == 0)
                {
                    throw new InvalidOperationException();
                }

                input.Position = resource.DefaultBitsOffset;

                var defaultBits = new uint[(schema.Count + 31) >> 5];
                for (int i = 0; i < defaultBits.Length; i++)
                {
                    defaultBits[i] = input.ReadValueU32(endian);
                }

                input.Position = resource.DataOffset;
                using (var data = input.ReadToMemoryStream(schema.DataSize))
                {
                    for (int i = 0; i < schema.Count; i++)
                    {
                        var property = schema[i];
                        var isDefault = (defaultBits[i >> 5] & (1u << (i % 32))) != 0;

                        var handler = HandlerFactory.Get(property.Type);

                        data.Position = property.Offset;

                        object value;
                        if (handler.UsesPointer == false)
                        {
                            value = handler.Read(data, endian, schemaProvider);
                        }
                        else
                        {
                            var pointerOffset = data.ReadOffset(endian);
                            if (pointerOffset == 0)
                            {
                                throw new InvalidOperationException();
                            }

                            input.Position = resource.DataOffset + pointerOffset;
                            value = handler.Read(input, endian, schemaProvider);
                        }

                        instance._Properties.Add(property.Id, value);

                        if (isDefault == true)
                        {
                            instance._DefaultProperties.Add(property.Id);
                        }
                    }
                }
            }

            return instance;
        }

        public static void Write(
            PropertySet instance,
            Stream output,
            Endian endian,
            DataFormats.PropertySet resource,
            long ownerOffset,
            PropertySetSchemaProvider schemaProvider)
        {
            resource.Flags = instance._Flags;
            resource.Name = instance._Name;
            resource.ReferenceCount = instance._ReferenceCount;
            resource.ParentCount = (ushort)instance._Parents.Count;
            resource.ParentMask = 0xFFFFFFFFu;
            resource.SchemaName = instance._SchemaName;
            resource.PropertyMask = 0xFFFFFFFFu;

            if (resource.ParentCount == 0)
            {
                resource.ParentsOffset = 0;
            }
            else
            {
                resource.ParentsOffset = output.Position;
                foreach (var parentHandle in instance._Parents)
                {
                    parentHandle.Write(output, endian);
                }
            }

            bool writeSchema;
            PropertySetSchema schema;
            if (resource.SchemaName == Symbol.Invalid || resource.SchemaName == resource.Name)
            {
                if ((resource.Flags & PropertyCollectionFlags.InheritSchema) != 0)
                {
                    throw new InvalidOperationException();
                }

                /*
                var properties = new List<PropertySchema>();
                uint runningOffset = 0;
                foreach (var kv in instance.Properties)
                {
                    var propertyValue = kv.Value;
                    if (propertyValue == null)
                    {
                        throw new InvalidOperationException();
                    }
                    var handler = HandlerFactory.Get(propertyValue.GetType());

                    runningOffset = runningOffset.Align((uint)handler.Alignment);

                    var propertyId = kv.Key;
                    var propertyType = handler.Id;
                    var propertyOffset = runningOffset;

                    properties.Add(new PropertySchema(propertyId, propertyType, propertyOffset));

                    if (handler.UsesPointer == false)
                    {
                        runningOffset += (uint)handler.ByteSize;
                    }
                    else
                    {
                        runningOffset += 8;
                    }
                }

                if (runningOffset > ushort.MaxValue)
                {
                    throw new InvalidOperationException();
                }
                */

                schema = instance._DefinedSchema;
                writeSchema = true;

                if (resource.Name != Symbol.Invalid && resource.SchemaName == resource.Name)
                {
                    schemaProvider.Add(resource.Name.Id, schema);
                }

                resource.PropertyCount = (ushort)instance._Properties.Count;
                resource.DataSize = schema.DataSize;
            }
            else
            {
                if (resource.PropertyCount != 0)
                {
                    throw new NotSupportedException();
                }

                schema = schemaProvider.Get(resource.SchemaName.Id);
                writeSchema = false;

                resource.PropertyCount = 0;
                resource.DataSize = 0;
            }

            if (schema.Count == 0)
            {
                resource.PropertiesOffset = 0;
                resource.DefaultBitsOffset = 0;
                resource.DataOffset = 0;
            }
            else
            {
                if (writeSchema == true)
                {
                    resource.PropertiesOffset = output.Position;
                    resource.DefaultBitsOffset = resource.PropertiesOffset + (8 * schema.Count);
                }
                else
                {
                    resource.PropertiesOffset = 0;
                    resource.DefaultBitsOffset = output.Position;
                }

                resource.DataOffset = resource.DefaultBitsOffset + (((schema.Count + 31) >> 5) * 4);

                var defaultBits = new uint[(schema.Count + 31) >> 5];

                output.Position = (resource.DataOffset + schema.DataSize).Align(16);
                using (MemoryStream data = new(schema.DataSize))
                {
                    for (int i = 0; i < schema.Count; i++)
                    {
                        var property = schema[i];

                        if (instance.Properties.ContainsKey(property.Id) == false)
                        {
                            throw new InvalidOperationException();
                        }

                        var value = instance.Properties[property.Id];
                        var handler = HandlerFactory.Get(property.Type);

                        if (instance.DefaultProperties.Contains(property.Id) == true)
                        {
                            defaultBits[i >> 5] |= 1u << (i % 32);
                        }

                        data.Position = property.Offset;

                        if (handler.UsesPointer == false)
                        {
                            handler.Write(value, data, ownerOffset, endian, schemaProvider);
                        }
                        else
                        {
                            output.Position = output.Position.Align(handler.Alignment);
                            var pointerOffset = output.Position - resource.DataOffset;
                            handler.Write(value, output, ownerOffset, endian, schemaProvider);

                            data.WriteOffset(pointerOffset, endian);
                        }
                    }

                    data.Flush();

                    if (data.Length != schema.DataSize)
                    {
                        throw new InvalidOperationException();
                    }

                    var endPosition = output.Position;

                    if (writeSchema == true)
                    {
                        output.Position = resource.PropertiesOffset;
                        for (int i = 0; i < schema.Count; i++)
                        {
                            var property = schema[i];
                            property.Write(output, endian);
                        }
                    }

                    output.Position = resource.DefaultBitsOffset;
                    foreach (var defaultBit in defaultBits)
                    {
                        output.WriteValueU32(defaultBit, endian);
                    }

                    output.Position = resource.DataOffset;
                    data.Position = 0;
                    output.WriteFromStream(data, schema.DataSize);

                    output.Position = endPosition;
                }
            }
        }

        public void Write(
            Stream data,
            Endian endian,
            DataFormats.PropertySet resource,
            long ownerOffset,
            PropertySetSchemaProvider schemaProvider)
        {
            Write(this, data, endian, resource, ownerOffset, schemaProvider);
        }
    }
}
