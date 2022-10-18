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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Gibbed.IO;
using Gibbed.SleepingDogs.FileFormats;

namespace Gibbed.SleepingDogs.PropertySetConvert
{
    internal class Exporter
    {
        private static ProjectData.HashList<uint> _PropertyNames;
        private static ProjectData.HashList<uint> _SymbolNames;

        public static void Run(ProjectData.Project project, List<string> extras)
        {
            _PropertyNames = project.LoadListsPropertySetPropertyNames();
            _SymbolNames = project.LoadListsPropertySetSymbolNames();

            string inputPath = extras[0];
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, null) + "_unpack";

            Directory.CreateDirectory(outputPath);

            var inventory = new PropertySetInventory();
            using (var input = File.OpenRead(inputPath))
            {
                inventory.Deserialize(input, Endian.Little);
            }

            foreach (var item in inventory.Items)
            {
                if (item.Name.HashSymbol() == item.Id && _SymbolNames.Contains(item.Id) == false)
                {
                    _SymbolNames.Add(item.Id, item.Name);
                }
            }

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                CheckCharacters = false,
            };

            var indexPath = Path.Combine(outputPath, "@resource.xml");
            using (var indexWriter = XmlWriter.Create(indexPath, settings))
            {
                indexWriter.WriteStartDocument();
                indexWriter.WriteStartElement("Resources");

                foreach (var item in inventory.Items)
                {
                    string xmlName;
                    if (_SymbolNames.Contains(item.Id) == false)
                    {
                        if ((uint)item.Name.HashSymbol() != item.Id)
                        {
                            // todo: make this look up correct names from lists
                            Console.WriteLine(
                                "Hash of {0:X8} doesn't match hash of '{1}' -- name probably got truncated!",
                                item.Id,
                                item.Name);
                            xmlName = string.Format(@"__TRUNCATED\{0}_{1:X8}.xml", item.Name, item.Id);
                        }
                        else
                        {
                            xmlName = item.Name + ".xml";
                        }
                    }
                    else
                    {
                        xmlName = _SymbolNames[item.Id] + ".xml";
                    }

                    if (item.Id != item.Root.Name.Id)
                    {
                        throw new InvalidOperationException();
                    }

                    indexWriter.WriteStartElement("Resource");
                    indexWriter.WriteAttributeString("name", item.Name);
                    indexWriter.WriteAttributeString("flags", item.Flags.ToString());
                    indexWriter.WriteValue(xmlName);
                    indexWriter.WriteEndElement();

                    var itemPath = Path.Combine(outputPath, xmlName);
                    using (var itemWriter = XmlWriter.Create(itemPath, settings))
                    {
                        itemWriter.WriteStartDocument();
                        itemWriter.WriteStartElement("Resource");
                        WritePropertySet(itemWriter, item.Root);
                        itemWriter.WriteEndElement();
                        itemWriter.WriteEndDocument();
                        itemWriter.Flush();
                    }
                }

                indexWriter.WriteEndElement();
                indexWriter.WriteEndDocument();
                indexWriter.Flush();
            }
        }

        private static string GetPropertyName(uint id)
        {
            if (_PropertyNames.Contains(id) == false)
            {
                return "0x" + id.ToString("X8");
            }

            return _PropertyNames[id];
        }

        private static string GetPropertyName(DataFormats.Symbol symbol)
        {
            return GetPropertyName(symbol.Id);
        }

        private static string GetSymbolName(uint id)
        {
            if (_SymbolNames.Contains(id) == false)
            {
                return "0x" + id.ToString("X8");
            }

            return _SymbolNames[id];
        }

        private static string GetSymbolName(DataFormats.Symbol symbol)
        {
            return GetSymbolName(symbol.Id);
        }

        private static void WritePropertySet(XmlWriter writer, PropertySetFormats.PropertySet data)
        {
            if (data.DefinedSchema != null)
            {
                WritePropertySetSchema(writer, data.DefinedSchema);
            }

            writer.WriteStartElement("PropertySet");
            writer.WriteAttributeString("name", GetSymbolName(data.Name));

            if (data.SchemaName != DataFormats.Symbol.Invalid)
            {
                writer.WriteAttributeString("schema", GetSymbolName(data.SchemaName));
            }

            if ((data.Flags & DataFormats.PropertyCollectionFlags.SkipParentCheck) != 0)
            {
                writer.WriteAttributeString("skipparentcheck", "True");
            }

            if (data.ReferenceCount != 0)
            {
                writer.WriteAttributeString("refs", data.ReferenceCount.ToString(CultureInfo.InvariantCulture));
            }

            var ignoredFlags = DataFormats.PropertyCollectionFlags.IsSet |
                               DataFormats.PropertyCollectionFlags.OwnerIsList |
                               DataFormats.PropertyCollectionFlags.OwnerIsSet |
                               DataFormats.PropertyCollectionFlags.SkipParentCheck;
            var cleanedFlags = data.Flags & ~ignoredFlags;
            if (cleanedFlags != DataFormats.PropertyCollectionFlags.None)
            {
                writer.WriteAttributeString("flags", cleanedFlags.ToString());
            }

            foreach (var parent in data.Parents)
            {
                writer.WriteElementString("Parent", GetSymbolName(parent.NameId));
            }

            foreach (var kv in data.Properties)
            {
                writer.WriteStartElement("Property");
                WriteProperty(writer, kv.Key, kv.Value, data.DefaultProperties.Contains(kv.Key) == true);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static void WritePropertySetSchema(XmlWriter writer, PropertySetFormats.PropertySetSchema schema)
        {
            writer.WriteStartElement("PropertySetSchema");
            writer.WriteAttributeString("datasize", schema.DataSize.ToString(CultureInfo.InvariantCulture));
            for (int i = 0; i < schema.Count; i++)
            {
                var property = schema[i];
                var handler = PropertySetFormats.HandlerFactory.Get(property.Type);
                writer.WriteStartElement("PropertySchema");
                writer.WriteAttributeString("name", GetSymbolName(property.Id));
                writer.WriteAttributeString("type", handler.XmlTag);
                writer.WriteAttributeString("offset", property.Offset.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private static void WritePropertyList(XmlWriter writer, PropertySetFormats.PropertyList data)
        {
            bool hasWeights = data.Weights.Count > 0;
            if (data.TotalWeight != 0)
            {
                if (data.TotalWeight != data.Weights.Sum(w => w))
                {
                    throw new InvalidOperationException();
                }
            }

            var ignoredFlags = DataFormats.PropertyCollectionFlags.OwnerIsList |
                               DataFormats.PropertyCollectionFlags.IsList |
                               DataFormats.PropertyCollectionFlags.OwnerIsSet;
            var cleanedFlags = data.Flags & ~ignoredFlags;
            if (cleanedFlags != DataFormats.PropertyCollectionFlags.None)
            {
                writer.WriteAttributeString("flags", cleanedFlags.ToString());
            }

            for (int i = 0; i < data.Items.Count; i++)
            {
                writer.WriteStartElement("ListProperty");
                WriteProperty(writer, null, data.Items[i], false, hasWeights == false ? (uint?)null : data.Weights[i]);
                writer.WriteEndElement();
            }
        }

        private static void WriteProperty(XmlWriter writer, uint? name, object value, bool isDefault, uint? weight = null)
        {
            if (value == null)
            {
                throw new InvalidOperationException();
            }

            var culture = CultureInfo.InvariantCulture;
            var type = value.GetType();
            var handler = PropertySetFormats.HandlerFactory.Get(type);

            if (name.HasValue == true)
            {
                writer.WriteAttributeString("name", GetPropertyName(name.Value));
            }

            writer.WriteAttributeString("type", handler.XmlTag);

            if (isDefault == true)
            {
                writer.WriteAttributeString("default", "True");
            }

            if (weight.HasValue == true)
            {
                writer.WriteAttributeString("weight", weight.Value.ToString(culture));
            }

            switch (handler.XmlTag)
            {
                case "Float32":
                {
                    writer.WriteValue(((float)value).ToString(culture));
                    break;
                }

                case "Int8":
                {
                    writer.WriteValue(((sbyte)value).ToString(culture));
                    break;
                }

                case "Int16":
                {
                    writer.WriteValue(((short)value).ToString(culture));
                    break;
                }

                case "Int32":
                {
                    writer.WriteValue(((int)value).ToString(culture));
                    break;
                }

                case "Int64":
                {
                    writer.WriteValue(((long)value).ToString(culture));
                    break;
                }

                case "UInt8":
                {
                    writer.WriteValue(((byte)value).ToString(culture));
                    break;
                }

                case "UInt16":
                {
                    writer.WriteValue(((ushort)value).ToString(culture));
                    break;
                }

                case "UInt32":
                {
                    writer.WriteValue(((uint)value).ToString(culture));
                    break;
                }

                case "UInt64":
                {
                    writer.WriteValue(((ulong)value).ToString(culture));
                    break;
                }

                case "Boolean":
                {
                    writer.WriteValue(((bool)value).ToString(culture));
                    break;
                }

                case "String":
                {
                    writer.WriteValue((string)value);
                    break;
                }

                case "Symbol":
                {
                    writer.WriteValue(GetSymbolName(((DataFormats.Symbol)value).Id));
                    break;
                }

                case "SymbolUC":
                {
                    writer.WriteValue(GetSymbolName(((DataFormats.SymbolUpperCase)value).Id));
                    break;
                }

                case "WwiseID":
                {
                    writer.WriteValue(GetSymbolName(((DataFormats.WwiseId)value).Id));
                    break;
                }

                case "Vector2":
                {
                    var vector = (DataFormats.Vector2)value;
                    writer.WriteAttributeString("x", vector.X.ToString(culture));
                    writer.WriteAttributeString("y", vector.Y.ToString(culture));
                    break;
                }

                case "Vector3":
                {
                    var vector = (DataFormats.Vector3)value;
                    writer.WriteAttributeString("x", vector.X.ToString(culture));
                    writer.WriteAttributeString("y", vector.Y.ToString(culture));
                    writer.WriteAttributeString("z", vector.Z.ToString(culture));
                    break;
                }

                case "Vector4":
                {
                    var vector = (DataFormats.Vector4)value;
                    writer.WriteAttributeString("x", vector.X.ToString(culture));
                    writer.WriteAttributeString("y", vector.Y.ToString(culture));
                    writer.WriteAttributeString("z", vector.Z.ToString(culture));
                    writer.WriteAttributeString("w", vector.W.ToString(culture));
                    break;
                }

                case "TransQuat":
                {
                    var vector = (DataFormats.TransQuaternion)value;
                    writer.WriteAttributeString("tx", vector.Transform.X.ToString(culture));
                    writer.WriteAttributeString("ty", vector.Transform.Y.ToString(culture));
                    writer.WriteAttributeString("tz", vector.Transform.Z.ToString(culture));
                    writer.WriteAttributeString("rx", vector.Rotation.X.ToString(culture));
                    writer.WriteAttributeString("ry", vector.Rotation.Y.ToString(culture));
                    writer.WriteAttributeString("rz", vector.Rotation.Z.ToString(culture));
                    writer.WriteAttributeString("rw", vector.Rotation.W.ToString(culture));
                    break;
                }

                case "Matrix44":
                {
                    var vector = (DataFormats.Matrix44)value;
                    writer.WriteStartElement("V0");
                    writer.WriteAttributeString("x", vector.V0.X.ToString(culture));
                    writer.WriteAttributeString("y", vector.V0.Y.ToString(culture));
                    writer.WriteAttributeString("z", vector.V0.Z.ToString(culture));
                    writer.WriteAttributeString("w", vector.V0.W.ToString(culture));
                    writer.WriteEndElement();
                    writer.WriteStartElement("V1");
                    writer.WriteAttributeString("x", vector.V1.X.ToString(culture));
                    writer.WriteAttributeString("y", vector.V1.Y.ToString(culture));
                    writer.WriteAttributeString("z", vector.V1.Z.ToString(culture));
                    writer.WriteAttributeString("w", vector.V1.W.ToString(culture));
                    writer.WriteEndElement();
                    writer.WriteStartElement("V2");
                    writer.WriteAttributeString("x", vector.V2.X.ToString(culture));
                    writer.WriteAttributeString("y", vector.V2.Y.ToString(culture));
                    writer.WriteAttributeString("z", vector.V2.Z.ToString(culture));
                    writer.WriteAttributeString("w", vector.V2.W.ToString(culture));
                    writer.WriteEndElement();
                    writer.WriteStartElement("V3");
                    writer.WriteAttributeString("x", vector.V3.X.ToString(culture));
                    writer.WriteAttributeString("y", vector.V3.Y.ToString(culture));
                    writer.WriteAttributeString("z", vector.V3.Z.ToString(culture));
                    writer.WriteAttributeString("w", vector.V3.W.ToString(culture));
                    writer.WriteEndElement();
                    break;
                }

                case "Int32Ranged":
                {
                    var ranged = (DataFormats.Ranged<int>)value;
                    writer.WriteAttributeString("range", ranged.Range.ToString(culture));
                    writer.WriteValue(ranged.Value.ToString(culture));
                    break;
                }

                case "WeightedListProperty":
                {
                    throw new NotSupportedException();
                }

                case "Float32Ranged":
                {
                    var ranged = (DataFormats.Ranged<float>)value;
                    writer.WriteAttributeString("range", ranged.Range.ToString(culture));
                    writer.WriteValue(ranged.Value.ToString(culture));
                    break;
                }

                case "List":
                {
                    WritePropertyList(writer, (PropertySetFormats.PropertyList)value);
                    break;
                }

                case "PropSet":
                {
                    WritePropertySet(writer, (PropertySetFormats.PropertySet)value);
                    break;
                }

                default:
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
