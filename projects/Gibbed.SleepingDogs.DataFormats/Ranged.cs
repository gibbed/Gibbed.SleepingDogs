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
using System.Text;

namespace Gibbed.SleepingDogs.DataFormats
{
    public struct Ranged<T> : IEquatable<Ranged<T>>
    {
        private readonly T _Range;
        private readonly T _Value;

        public T Range
        {
            get { return this._Range; }
        }

        public T Value
        {
            get { return this._Value; }
        }

        public Ranged(T range, T value)
        {
            this._Range = range;
            this._Value = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(this._Range);
            sb.Append(", ");
            sb.Append(this._Value);
            sb.Append(")");
            return sb.ToString();
        }

        public bool Equals(Ranged<T> other)
        {
            return EqualityComparer<T>.Default.Equals(this._Range, other._Range) == true &&
                   EqualityComparer<T>.Default.Equals(this._Value, other._Value) == true;
        }

        public override bool Equals(object obj)
        {
            if (obj is null == true)
            {
                return false;
            }
            return obj is Ranged<T> ranged && Equals(ranged) == true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(this._Range) * 397) ^
                       (EqualityComparer<T>.Default.GetHashCode(this._Value));
            }
        }

        public static bool operator ==(Ranged<T> a, Ranged<T> b)
        {
            return a.Equals(b) == true;
        }

        public static bool operator !=(Ranged<T> a, Ranged<T> b)
        {
            return a.Equals(b) == false;
        }
    }
}
