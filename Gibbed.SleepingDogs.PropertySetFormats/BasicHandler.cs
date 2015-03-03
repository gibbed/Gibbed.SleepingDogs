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

namespace Gibbed.SleepingDogs.PropertySetFormats
{
    internal abstract class BasicHandler<T> : IHandler
    {
        #region Fields
        private readonly byte _Id;
        private readonly int _ByteSize;
        private readonly int _Alignment;
        private readonly string _Name;
        private readonly string _XmlTag;
        private readonly bool _UsesPointer;
        #endregion

        public BasicHandler(byte id, int byteSize, int alignment, string name, string xmlTag, bool usesPointer)
        {
            this._Id = id;
            this._ByteSize = byteSize;
            this._Alignment = alignment;
            this._Name = name;
            this._XmlTag = xmlTag;
            this._UsesPointer = usesPointer;
        }

        #region Properties
        public Type NativeType
        {
            get { return typeof(T); }
        }

        public byte Id
        {
            get { return this._Id; }
        }

        public int ByteSize
        {
            get { return this._ByteSize; }
        }

        public int Alignment
        {
            get { return this._Alignment; }
        }

        public string Name
        {
            get { return this._Name; }
        }

        public string XmlTag
        {
            get { return this._XmlTag; }
        }

        public bool UsesPointer
        {
            get { return this._UsesPointer; }
        }
        #endregion

        object IHandler.Read(Stream input, Endian endian, PropertySetSchemaProvider schemaProvider)
        {
            return this.Read(input, endian);
        }

        protected abstract T Read(Stream input, Endian endian);

        void IHandler.Write(
            Stream output,
            object value,
            Endian endian,
            long ownerOffset,
            PropertySetSchemaProvider schemaProvider)
        {
            this.Write(output, (T)value, endian, ownerOffset);
        }

        protected abstract void Write(Stream output, T value, Endian endian, long ownerOffset);
    }
}
