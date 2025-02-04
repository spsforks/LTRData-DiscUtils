//
// Copyright (c) 2016, Bianco Veigel
// Copyright (c) 2017, Timo Walter
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

using System.Collections.Generic;
using System.IO;

using DiscUtils.Streams;
using System;

namespace DiscUtils.Xfs;
internal class BTreeInodeNode : BtreeHeader
{
    public uint[] Keys { get; private set; }

    public uint[] Pointer { get; private set; }

    public Dictionary<uint, BtreeHeader> Children { get; private set; }

    public override int Size => base.Size + (NumberOfRecords * 0x8);

    public BTreeInodeNode(uint superBlockVersion) : base(superBlockVersion) { }

    public override int ReadFrom(ReadOnlySpan<byte> buffer)
    {
        base.ReadFrom(buffer);
        var offset = base.Size;
        if (Level == 0)
        {
            throw new IOException("invalid B+tree level - expected 0");
        }

        Keys = new uint[NumberOfRecords];
        Pointer = new uint[NumberOfRecords];
        for (var i = 0; i < NumberOfRecords; i++)
        {
            Keys[i] = EndianUtilities.ToUInt32BigEndian(buffer.Slice(offset));
        }

        for (var i = 0; i < NumberOfRecords; i++)
        {
            Pointer[i] = EndianUtilities.ToUInt32BigEndian(buffer.Slice(offset));
        }

        return Size;
    }

    public override void LoadBtree(AllocationGroup ag)
    {
        Children = new Dictionary<uint,BtreeHeader>(NumberOfRecords);
        for (var i = 0; i < NumberOfRecords; i++)
        {
            BtreeHeader child;
            if (Level == 1)
            {
                child = new BTreeInodeLeaf(SbVersion);
            }
            else
            {
                child = new BTreeInodeNode(SbVersion);
            }

            var data = ag.Context.RawStream;

            data.Position = ((long)Pointer[i] * ag.Context.SuperBlock.Blocksize) + ag.Offset;
            child.ReadFrom(data, (int)ag.Context.SuperBlock.Blocksize);
            child.LoadBtree(ag);
            Children.Add(Keys[i], child);
        }
    }
}
