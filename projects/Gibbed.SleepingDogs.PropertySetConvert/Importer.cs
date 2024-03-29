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

        public static void Run(ProjectData.Project project, List<string> args)
        {
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

            List<(string path, string name, DataFormats.PropertySetResourceFlags flags)> resources = new();
            using (var xml = File.OpenRead(xmlPath))
            {
                XPathDocument doc = new(xml);
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
                    resources.Add((path, name, flags));
                }
            }

            PropertySetInventory inventory = new();
            foreach (var resource in resources)
            {
                var itemPath = Path.Combine(inputPath, resource.Item1);

                PropertySetFormats.PropertySet propertySet;
                using (var xml = File.OpenRead(itemPath))
                {
                    XPathDocument doc = new(xml);
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

            List<PropertySetFormats.PropertySchema> properties = new();
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

            PropertySetFormats.PropertySet instance = new();
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
                if (Helpers.TryParseSymbol(rawParent.Value, out uint dummy) == false)
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
            PropertySetFormats.PropertyList instance = new();

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

                var weight = rawProperty.ParseAttributeUInt32("weight", uint.MaxValue);
                if (weight != uint.MaxValue)
                {
                    instance.Weights.Add(weight);
                }
            }

            instance.TotalWeight = instance.Weights.Aggregate<uint, uint>(0, (c, w) => c + w);

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
            return type switch
            {
                "Float32" => nav.ParseValueFloat32(),
                "Int8" => nav.ParseValueInt8(),
                "Int16" => nav.ParseValueInt16(),
                "Int32" => nav.ParseValueInt32(),
                "Int64" => nav.ParseValueInt64(),
                "UInt8" => nav.ParseValueUInt8(),
                "UInt16" => nav.ParseValueUInt16(),
                "UInt32" => nav.ParseValueUInt32(),
                "UInt64" => nav.ParseValueUInt64(),
                "Boolean" => nav.ParseValueBoolean(),
                "String" => nav.Value,
                "Symbol" => nav.ParseValueSymbol(),
                "SymbolUC" => nav.ParseValueSymbolUpperCase(),
                "WwiseID" => nav.ParseValueWwiseId(),
                "Vector2" => ParseVector2(nav),
                "Vector3" => ParseVector3(nav),
                "Vector4" => ParseVector4(nav),
                "TransQuat" => ParseTransQuat(nav),
                "Matrix44" => ParseMatrix44(nav),
                "Int32Ranged" => ParseInt32Ranged(nav),
                "WeightedListProperty" => throw new NotSupportedException(),
                "Float32Ranged" => ParseFloat32Ranged(nav),
                "List" => ReadPropertyList(nav, parent),
                "PropSet" => ReadPropertySet(nav, parent),
                _ => throw new NotImplementedException(),
            };
        }

        private static object ParseVector2(XPathNavigator nav)
        {
            var x = nav.ParseAttributeFloat32("x");
            var y = nav.ParseAttributeFloat32("y");
            return new DataFormats.Vector2(x, y);
        }

        private static object ParseVector3(XPathNavigator nav)
        {
            var x = nav.ParseAttributeFloat32("x");
            var y = nav.ParseAttributeFloat32("y");
            var z = nav.ParseAttributeFloat32("z");
            return new DataFormats.Vector3(x, y, z);
        }

        private static object ParseVector4(XPathNavigator nav)
        {
            var x = nav.ParseAttributeFloat32("x");
            var y = nav.ParseAttributeFloat32("y");
            var z = nav.ParseAttributeFloat32("z");
            var w = nav.ParseAttributeFloat32("w");
            return new DataFormats.Vector4(x, y, z, w);
        }

        private static object ParseTransQuat(XPathNavigator nav)
        {
            var tx = nav.ParseAttributeFloat32("tx");
            var ty = nav.ParseAttributeFloat32("ty");
            var tz = nav.ParseAttributeFloat32("tz");
            var rx = nav.ParseAttributeFloat32("rx");
            var ry = nav.ParseAttributeFloat32("ry");
            var rz = nav.ParseAttributeFloat32("rz");
            var rw = nav.ParseAttributeFloat32("rw");
            return new DataFormats.TransQuaternion(
                new(rx, ry, rz, rw),
                new(tx, ty, tz));
        }

        private static object ParseMatrix44(XPathNavigator nav)
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
            return new DataFormats.Matrix44(
                new(v0X, v0Y, v0Z, v0W),
                new(v1X, v1Y, v1Z, v1W),
                new(v2X, v2Y, v2Z, v2W),
                new(v3X, v3Y, v3Z, v3W));
        }

        private static object ParseInt32Ranged(XPathNavigator nav)
        {
            var range = nav.ParseAttributeInt32("range");
            var dummy = nav.ParseValueInt32();
            return new DataFormats.Ranged<int>(range, dummy);
        }

        private static object ParseFloat32Ranged(XPathNavigator nav)
        {
            var range = nav.ParseAttributeFloat32("range");
            var dummy = nav.ParseValueFloat32();
            return new DataFormats.Ranged<float>(range, dummy);
        }
    }
}
