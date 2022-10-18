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
using System.Linq;

namespace Gibbed.SleepingDogs.PropertySetFormats
{
    public static class HandlerFactory
    {
        private static readonly Dictionary<byte, IHandler> _HandlersById;
        private static readonly Dictionary<string, IHandler> _HandlersByXmlTag;
        private static readonly Dictionary<Type, IHandler> _HandlersByType;

        static HandlerFactory()
        {
            IHandler[] handlers =
            {
                new Handlers.Int8Handler(),
                new Handlers.Int16Handler(),
                new Handlers.Int32Handler(),
                new Handlers.Int64Handler(),
                new Handlers.Int128Handler(),
                new Handlers.UInt8Handler(),
                new Handlers.UInt16Handler(),
                new Handlers.UInt32Handler(),
                new Handlers.UInt64Handler(),
                new Handlers.BooleanHandler(),
                new Handlers.Float32Handler(),
                new Handlers.Float64Handler(),
                new Handlers.StringHandler(),
                new Handlers.Float32RangedHandler(),
                new Handlers.UInt32RangedHandler(),
                new Handlers.Int32RangedHandler(),
                new Handlers.Vector2Handler(),
                new Handlers.Vector3Handler(),
                new Handlers.Vector4Handler(),
                new Handlers.Matrix44Handler(),
                new Handlers.ResourceHandler(),
                new Handlers.SymbolHandler(),
                new Handlers.SymbolUpperCaseHandler(),
                new Handlers.WwiseIdHandler(),
                new Handlers.ListHandler(),
                new Handlers.PropertySetHandler(),
                new Handlers.TransRotationHandler(),
                new Handlers.TransQuaternionHandler(),
            };

            _HandlersById = handlers.ToDictionary(h => h.Id, h => h);
            _HandlersByXmlTag = handlers.ToDictionary(h => h.XmlTag, h => h);
            _HandlersByType = handlers.ToDictionary(h => h.NativeType, h => h);
        }

        public static IHandler Get(uint id)
        {
            if (id > byte.MaxValue)
            {
                throw new InvalidOperationException();
            }

            var bid = (byte)id;
            if (_HandlersById.ContainsKey(bid) == false)
            {
                throw new KeyNotFoundException();
            }

            var handler = _HandlersById[bid];
            if (handler == null)
            {
                throw new NotImplementedException();
            }

            return handler;
        }

        public static IHandler Get(string xmlTag)
        {
            if (xmlTag == null)
            {
                throw new ArgumentNullException(nameof(xmlTag));
            }

            if (_HandlersByXmlTag.ContainsKey(xmlTag) == false)
            {
                throw new KeyNotFoundException();
            }

            var handler = _HandlersByXmlTag[xmlTag];
            if (handler == null)
            {
                throw new NotImplementedException();
            }

            return handler;
        }

        public static IHandler Get(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_HandlersByType.ContainsKey(type) == false)
            {
                throw new KeyNotFoundException();
            }

            var handler = _HandlersByType[type];
            if (handler == null)
            {
                throw new NotImplementedException();
            }

            return handler;
        }
    }
}
