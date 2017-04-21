//
// Copyright (c) 2016, Bianco Veigel
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

namespace DiscUtils.Xfs
{
    using DiscUtils.Internal;
    using System;

    internal abstract class BtreeHeader : IByteArraySerializable
    {
        public uint Magic { get; private set; }

        public ushort Level { get; private set; }

        public ushort NumberOfRecords { get; private set; }

        public int LeftSibling { get; private set; }

        public int RightSibling { get; private set; }

        public ulong Bno { get; private set; }

        public ulong Lsn { get; private set; }

        public Guid UniqueId { get; private set; }

        public uint Owner { get; private set; }

        public uint Crc { get; private set; }

        public virtual int Size { get; private set; }

        public int InitSize(uint sbVersion)
        {
            if (sbVersion >= 5)
            {
                Size = 0x38;
                return 0x38;
            }
            Size = 0x1;
            return 0x1;
        }

        public virtual int ReadFrom(byte[] buffer, int offset)
        {
            Magic = Utilities.ToUInt32BigEndian(buffer, offset);
            Level = Utilities.ToUInt16BigEndian(buffer, offset + 0x4);
            NumberOfRecords = Utilities.ToUInt16BigEndian(buffer, offset + 0x6);
            LeftSibling = Utilities.ToInt32BigEndian(buffer, offset + 0x8);
            RightSibling = Utilities.ToInt32BigEndian(buffer, offset + 0xC);
            return Size;
        }

        public int ReadFromVersion5(byte[] buffer, int offset)
        {
            Bno = Utilities.ToUInt64BigEndian(buffer, offset + 0x10);
            Lsn = Utilities.ToUInt64BigEndian(buffer, offset + 0x18);
            UniqueId = Utilities.ToGuidBigEndian(buffer, offset + 0x20);
            Owner = Utilities.ToUInt32BigEndian(buffer, offset + 0x30);
            Crc = Utilities.ToUInt32BigEndian(buffer, offset + 0x34);
            return Size;
        }

        public virtual void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        public abstract void LoadBtree(AllocationGroup ag);
    }
}
