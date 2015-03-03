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
using System.Linq;
using System.Xml.XPath;
using Gibbed.IO;
using Gibbed.SleepingDogs.FileFormats;

namespace Gibbed.SleepingDogs.PropertySetConvert
{
    internal class Importer
    {
        private static DataFormats.PropertyCollectionFlags _NonSetListMask;

        static Importer()
        {
            _NonSetListMask =
                ~(DataFormats.PropertyCollectionFlags.IsSet |
                  DataFormats.PropertyCollectionFlags.IsList |
                  DataFormats.PropertyCollectionFlags.OwnerIsSet |
                  DataFormats.PropertyCollectionFlags.OwnerIsList);
        }

        public static void Run(string currentProject, List<string> args)
        {
            var manager = ProjectData.Manager.Load(currentProject);
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Warning: no active project loaded.");
            }

            string inputPath = args[0];
            string outputPath = args.Count > 1 ? args[1] : Path.ChangeExtension(inputPath, null) + ".bin";
            string xmlPath;

            if (File.Exists(inputPath) == true &&
                Path.GetFileName(inputPath) == "@resource.xml")
            {
                xmlPath = inputPath;
                inputPath = Path.GetDirectoryName(inputPath);
            }
            else
            {
                xmlPath = Path.Combine(inputPath, "@resource.xml");
            }

            if (inputPath == null)
            {
                throw new InvalidOperationException();
            }

            var resources = new List<Tuple<string, string, DataFormats.PropertySetResourceFlags>>();
            using (var xml = File.OpenRead(xmlPath))
            {
                var doc = new XPathDocument(xml);
                var nav = doc.CreateNavigator();
                var root = nav.SelectSingleNode("Resources");
                if (root == null)
                {
                    throw new InvalidOperationException();
                }

                var rawResources = root.Select("Resource");
                while (rawResources.MoveNext() == true)
                {
                    var rawResource = rawResources.Current;
                    var name = rawResource.ParseAttributeString("name");
                    var flags = rawResource.ParseAttributeEnum<DataFormats.PropertySetResourceFlags>("flags");
                    var path = rawResource.Value;
                    resources.Add(new Tuple<string, string, DataFormats.PropertySetResourceFlags>(path, name, flags));
                }
            }

            var inventory = new PropertySetInventory();
            foreach (var resource in resources)
            {
                var itemPath = Path.Combine(inputPath, resource.Item1);

                PropertySetFormats.PropertySet propertySet;
                using (var xml = File.OpenRead(itemPath))
                {
                    var doc = new XPathDocument(xml);
                    var nav = doc.CreateNavigator();

                    var root = nav.SelectSingleNode("Resource");
                    if (root == null)
                    {
                        throw new InvalidOperationException();
                    }

                    propertySet = ReadPropertySet(root, null);
                }

                inventory.Items.Add(new PropertySetInventory.Item()
                {
                    Id = propertySet.Name.Id,
                    DebugName = PropertySetInventory.Item.GetDebugName(resource.Item2),
                    Flags = resource.Item3,
                    SourceTextHash = 0x20424947u,
                    Name = resource.Item2,
                    Root = propertySet,
                });
            }

            using (var output = File.Create(outputPath))
            {
                inventory.Serialize(output, Endian.Little);
            }
        }

        private static DataFormats.PropertyCollectionFlags GetOwnerFlag(object parent)
        {
            if (parent is PropertySetFormats.PropertySet)
            {
                return DataFormats.PropertyCollectionFlags.OwnerIsSet;
            }

            if (parent is PropertySetFormats.PropertyList)
            {
                return DataFormats.PropertyCollectionFlags.OwnerIsList;
            }

            return DataFormats.PropertyCollectionFlags.None;
        }

        private static PropertySetFormats.PropertySetSchema ReadPropertySetSchema(XPathNavigator nav)
        {
            var dataSize = nav.ParseAttributeUInt16("datasize");

            var properties = new List<PropertySetFormats.PropertySchema>();
            var rawProperties = nav.Select("PropertySchema");
            while (rawProperties.MoveNext() == true)
            {
                var rawProperty = rawProperties.Current;
                var name = rawProperty.ParseAttributeSymbol("name");
                var type = rawProperty.ParseAttributeString("type");
                var offset = rawProperty.ParseAttributeUInt32("offset");
                var handler = PropertySetFormats.HandlerFactory.Get(type);
                properties.Add(new PropertySetFormats.PropertySchema(name.Id, handler.Id, offset));
            }

            return new PropertySetFormats.PropertySetSchema(properties, dataSize);
        }

        private static PropertySetFormats.PropertySet ReadPropertySet(XPathNavigator nav, object parent)
        {
            PropertySetFormats.PropertySetSchema definedSchema = null;
            var rawSchema = nav.SelectSingleNode("PropertySetSchema");
            if (rawSchema != null)
            {
                definedSchema = ReadPropertySetSchema(rawSchema);
            }

            var root = nav.SelectSingleNode("PropertySet");
            if (root == null)
            {
                throw new InvalidOperationException();
            }

            var instance = new PropertySetFormats.PropertySet();
            instance.DefinedSchema = definedSchema;

            instance.Name = root.ParseAttributeSymbol("name");

            instance.Flags = root.ParseAttributeEnum("flags", DataFormats.PropertyCollectionFlags.None);

            instance.Flags &= _NonSetListMask;
            instance.Flags |= DataFormats.PropertyCollectionFlags.IsSet;
            instance.Flags |= GetOwnerFlag(parent);

            instance.SchemaName = root.ParseAttributeSymbol("schema", DataFormats.Symbol.Invalid);

            if (root.ParseAttributeBoolean("skipparentcheck", false) == true)
            {
                instance.Flags |= DataFormats.PropertyCollectionFlags.SkipParentCheck;
            }

            instance.ReferenceCount = root.ParseAttributeUInt16("refs", 0);

            instance.Parents.Clear();
            var rawParents = root.Select("Parent");
            while (rawParents.MoveNext() == true)
            {
                var rawParent = rawParents.Current;
                uint dummy;
                if (Helpers.TryParseSymbol(rawParent.Value, out dummy) == false)
                {
                    throw new FormatException("failed to parse parent");
                }
                instance.Parents.Add(new DataFormats.ResourceHandle(dummy));
            }

            instance.Properties.Clear();
            instance.DefaultProperties.Clear();

            var rawProperties = root.Select("Property");
            while (rawProperties.MoveNext() == true)
            {
                var rawProperty = rawProperties.Current;

                var name = rawProperty.ParseAttributeSymbol("name");
                var isDefault = rawProperty.ParseAttributeBoolean("default", false);
                var value = ParseProperty(rawProperty, instance);
                instance.Properties.Add(name.Id, value);

                if (isDefault == true)
                {
                    instance.DefaultProperties.Add(name.Id);
                }
            }

            return instance;
        }

        private static PropertySetFormats.PropertyList ReadPropertyList(XPathNavigator nav, object parent)
        {
            var instance = new PropertySetFormats.PropertyList();

            instance.Flags = nav.ParseAttributeEnum("flags", DataFormats.PropertyCollectionFlags.None);
            instance.Flags &= _NonSetListMask;
            instance.Flags |= DataFormats.PropertyCollectionFlags.IsList;
            instance.Flags |= GetOwnerFlag(parent);

            var rawProperties = nav.Select("ListProperty");
            while (rawProperties.MoveNext() == true)
            {
                var rawProperty = rawProperties.Current;
                var value = ParseProperty(rawProperty, instance);
                instance.Items.Add(value);
            }

            if (instance.Items.Count == 0)
            {
                instance.TypeId = 29;
            }
            else
            {
                var type = instance.Items.First().GetType();
                if (instance.Items.Any(i => i.GetType() != type))
                {
                    throw new InvalidOperationException();
                }

                instance.TypeId = PropertySetFormats.HandlerFactory.Get(type).Id;
            }

            return instance;
        }

        private static object ParseProperty(XPathNavigator nav, object parent)
        {
            var type = nav.ParseAttributeString("type");
            object value;
            switch (type)
            {
                case "Float32":
                {
                    value = nav.ParseValueFloat32();
                    break;
                }

                case "Int8":
                {
                    value = nav.ParseValueInt8();
                    break;
                }

                case "Int16":
                {
                    value = nav.ParseValueInt16();
                    break;
                }

                case "Int32":
                {
                    value = nav.ParseValueInt32();
                    break;
                }

                case "Int64":
                {
                    value = nav.ParseValueInt64();
                    break;
                }

                case "UInt8":
                {
                    value = nav.ParseValueUInt8();
                    break;
                }

                case "UInt16":
                {
                    value = nav.ParseValueUInt16();
                    break;
                }

                case "UInt32":
                {
                    value = nav.ParseValueUInt32();
                    break;
                }

                case "UInt64":
                {
                    value = nav.ParseValueUInt64();
                    break;
                }

                case "Boolean":
                {
                    value = nav.ParseValueBoolean();
                    break;
                }

                case "String":
                {
                    value = nav.Value;
                    break;
                }

                case "Symbol":
                {
                    value = nav.ParseValueSymbol();
                    break;
                }

                case "SymbolUC":
                {
                    value = nav.ParseValueSymbolUpperCase();
                    break;
                }

                case "WwiseID":
                {
                    value = nav.ParseValueWwiseId();
                    break;
                }

                case "Vector2":
                {
                    var x = nav.ParseAttributeFloat32("x");
                    var y = nav.ParseAttributeFloat32("y");
                    value = new DataFormats.Vector2(x, y);
                    break;
                }

                case "Vector3":
                {
                    var x = nav.ParseAttributeFloat32("x");
                    var y = nav.ParseAttributeFloat32("y");
                    var z = nav.ParseAttributeFloat32("z");
                    value = new DataFormats.Vector3(x, y, z);
                    break;
                }

                case "Vector4":
                {
                    var x = nav.ParseAttributeFloat32("x");
                    var y = nav.ParseAttributeFloat32("y");
                    var z = nav.ParseAttributeFloat32("z");
                    var w = nav.ParseAttributeFloat32("w");
                    value = new DataFormats.Vector4(x, y, z, w);
                    break;
                }

                case "TransQuat":
                {
                    var tx = nav.ParseAttributeFloat32("tx");
                    var ty = nav.ParseAttributeFloat32("ty");
                    var tz = nav.ParseAttributeFloat32("tz");
                    var rx = nav.ParseAttributeFloat32("rx");
                    var ry = nav.ParseAttributeFloat32("ry");
                    var rz = nav.ParseAttributeFloat32("rz");
                    var rw = nav.ParseAttributeFloat32("rw");
                    var transform = new DataFormats.Vector3(tx, ty, tz);
                    var rotation = new DataFormats.Quaternion(rx, ry, rz, rw);
                    value = new DataFormats.TransQuaternion(rotation, transform);
                    break;
                }

                case "Matrix44":
                {
                    var v0 = nav.SelectSingleNode("V0");
                    var v0X = v0.ParseAttributeFloat32("x");
                    var v0Y = v0.ParseAttributeFloat32("y");
                    var v0Z = v0.ParseAttributeFloat32("z");
                    var v0W = v0.ParseAttributeFloat32("w");
                    var v1 = nav.SelectSingleNode("V1");
                    var v1X = v1.ParseAttributeFloat32("x");
                    var v1Y = v1.ParseAttributeFloat32("y");
                    var v1Z = v1.ParseAttributeFloat32("z");
                    var v1W = v1.ParseAttributeFloat32("w");
                    var v2 = nav.SelectSingleNode("V2");
                    var v2X = v2.ParseAttributeFloat32("x");
                    var v2Y = v2.ParseAttributeFloat32("y");
                    var v2Z = v2.ParseAttributeFloat32("z");
                    var v2W = v2.ParseAttributeFloat32("w");
                    var v3 = nav.SelectSingleNode("V3");
                    var v3X = v3.ParseAttributeFloat32("x");
                    var v3Y = v3.ParseAttributeFloat32("y");
                    var v3Z = v3.ParseAttributeFloat32("z");
                    var v3W = v3.ParseAttributeFloat32("w");
                    value = new DataFormats.Matrix44(
                        new DataFormats.Vector4(v0X, v0Y, v0Z, v0W),
                        new DataFormats.Vector4(v1X, v1Y, v1Z, v1W),
                        new DataFormats.Vector4(v2X, v2Y, v2Z, v2W),
                        new DataFormats.Vector4(v3X, v3Y, v3Z, v3W));
                    break;
                }

                case "Int32Ranged":
                {
                    var range = nav.ParseAttributeInt32("range");
                    var dummy = nav.ParseValueInt32();
                    value = new DataFormats.Ranged<int>(range, dummy);
                    break;
                }

                case "WeightedListProperty":
                {
                    throw new NotSupportedException();
                }

                case "Float32Ranged":
                {
                    var range = nav.ParseAttributeFloat32("range");
                    var dummy = nav.ParseValueFloat32();
                    value = new DataFormats.Ranged<float>(range, dummy);
                    break;
                }

                case "List":
                {
                    value = ReadPropertyList(nav, parent);
                    break;
                }

                case "PropSet":
                {
                    value = ReadPropertySet(nav, parent);
                    break;
                }

                default:
                {
                    throw new NotImplementedException();
                }
            }
            return value;
        }
    }
}
