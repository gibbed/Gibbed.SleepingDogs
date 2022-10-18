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
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Gibbed.SleepingDogs.DataFormats;

namespace Gibbed.SleepingDogs.PropertySetFormats
{
    public class PropertyList
    {
        #region Fields
        private PropertyCollectionFlags _Flags;
        private byte _TypeId;
        private readonly List<object> _Items;
        private uint _TotalWeight;
        private readonly List<uint> _Weights;
        #endregion

        public PropertyList()
        {
            this._Items = new List<object>();
            this._Weights = new List<uint>();
        }

        #region Properties
        public PropertyCollectionFlags Flags
        {
            get { return this._Flags; }
            set { this._Flags = value; }
        }

        public byte TypeId
        {
            get { return this._TypeId; }
            set { this._TypeId = value; }
        }

        public List<object> Items
        {
            get { return this._Items; }
        }

        public uint TotalWeight
        {
            get { return this._TotalWeight; }
            set { this._TotalWeight = value; }
        }

        public List<uint> Weights
        {
            get { return this._Weights; }
        }
        #endregion

        public static PropertyList Read(
            Stream input,
            DataFormats.PropertyList resource,
            Endian endian,
            PropertySetSchemaProvider schemaProvider)
        {
            var instance = new PropertyList();
            instance._Flags = resource.Flags;
            instance._TypeId = (byte)resource.TypeId;
            instance._TotalWeight = resource.TotalWeight;

            instance.Items.Clear();
            instance.Weights.Clear();

            if (resource.ItemCount > 0)
            {
                var handler = HandlerFactory.Get(resource.TypeId);
                if (handler.ByteSize != resource.ItemSize)
                {
                    throw new InvalidOperationException();
                }

                if (resource.DataOffset == 0)
                {
                    throw new InvalidOperationException();
                }

                input.Position = resource.DataOffset;
                using (var data = input.ReadToMemoryStream(resource.ItemCount * resource.ItemSize))
                {
                    for (int i = 0, o = 0; i < resource.ItemCount; i++, o += resource.ItemSize)
                    {
                        data.Position = o;

                        object item;
                        if (handler.UsesPointer == false)
                        {
                            item = handler.Read(data, endian, schemaProvider);
                        }
                        else
                        {
                            var pointerOffset = data.ReadOffset(endian);
                            if (pointerOffset == 0)
                            {
                                throw new InvalidOperationException();
                            }

                            input.Position = resource.DataOffset + pointerOffset;
                            item = handler.Read(input, endian, schemaProvider);
                        }
                        instance.Items.Add(item);
                    }
                }

                if (resource.WeightsOffset != 0)
                {
                    input.Position = resource.WeightsOffset;
                    for (uint i = 0; i < resource.ItemCount; i++)
                    {
                        var weight = input.ReadValueU32(endian);
                        instance.Weights.Add(weight);
                    }
                }
            }

            return instance;
        }

        public static void Write(
            Stream output,
            PropertyList instance,
            Endian endian,
            DataFormats.PropertyList resource,
            long ownerOffset,
            PropertySetSchemaProvider schemaProvider)
        {
            if (instance.TypeId == 29)
            {
                resource.Flags = instance._Flags;
                resource.DataOffset = 0;
                resource.TypeId = instance._TypeId;
                resource.ItemSize = 0;
                resource.WeightsOffset = 0;
                resource.ItemCount = 0;
                resource.TotalWeight = 0;
                return;
            }

            if (instance.Weights.Count > 0 &&
                instance.Weights.Count != instance.Items.Count)
            {
                throw new InvalidOperationException();
            }

            var handler = HandlerFactory.Get(instance._TypeId);

            resource.Flags = instance._Flags;
            resource.TypeId = instance._TypeId;
            resource.ItemSize = handler.ByteSize;
            resource.ItemCount = instance.Items.Count;
            resource.TotalWeight = instance._TotalWeight;

            var dataSize = resource.ItemSize * resource.ItemCount;

            resource.DataOffset = output.Position;
            resource.WeightsOffset = resource.DataOffset + dataSize;

            long dataOffset;
            if (instance.Weights.Count > 0)
            {
                resource.WeightsOffset = resource.DataOffset + dataSize;
                dataOffset = resource.WeightsOffset + (4 * resource.ItemCount);
            }
            else
            {
                dataOffset = resource.WeightsOffset;
                resource.WeightsOffset = 0;
            }

            if (resource.ItemCount > 0)
            {
                output.Position = dataOffset;

                using (var data = new MemoryStream())
                {
                    foreach (var item in instance.Items)
                    {
                        if (handler.UsesPointer == false)
                        {
                            handler.Write(data, item, endian, ownerOffset, schemaProvider);
                            data.Position = data.Position.Align(handler.Alignment);
                            data.SetLength(data.Position);
                        }
                        else
                        {
                            var pointerOffset = output.Position - resource.DataOffset;
                            handler.Write(output, item, endian, ownerOffset, schemaProvider);
                            output.Position = output.Position.Align(handler.Alignment);

                            data.WriteOffset(pointerOffset, endian);
                            data.Position = data.Position.Align(8);
                            data.SetLength(data.Position);
                        }
                    }

                    data.Flush();

                    if (data.Length != dataSize)
                    {
                        throw new InvalidOperationException();
                    }

                    var endPosition = output.Position;

                    output.Position = resource.DataOffset;
                    data.Position = 0;
                    output.WriteFromStream(data, data.Length);

                    if (instance.Weights.Count > 0)
                    {
                        output.Position = resource.WeightsOffset;
                        foreach (var weight in instance.Weights)
                        {
                            output.WriteValueU32(weight, endian);
                        }
                    }

                    output.Position = endPosition;
                }
            }
        }

        public void Write(
            Stream data,
            Endian endian,
            DataFormats.PropertyList resource,
            long ownerOffset,
            PropertySetSchemaProvider schemaProvider)
        {
            Write(data, this, endian, resource, ownerOffset, schemaProvider);
        }
    }
}
