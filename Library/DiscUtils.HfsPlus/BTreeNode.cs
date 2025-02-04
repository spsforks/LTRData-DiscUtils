﻿//
// Copyright (c) 2008-2011, Kenneth Bell
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

using System;
using System.Collections.Generic;
using DiscUtils.Streams;

namespace DiscUtils.HfsPlus;

internal abstract class BTreeNode : IByteArraySerializable
{
    public BTreeNode(BTree tree, BTreeNodeDescriptor descriptor)
    {
        Tree = tree;
        Descriptor = descriptor;
    }

    protected BTreeNodeDescriptor Descriptor { get; }

    public IList<BTreeNodeRecord> Records { get; private set; }

    protected BTree Tree { get; }

    public int Size => Tree.NodeSize;

    public int ReadFrom(ReadOnlySpan<byte> buffer)
    {
        Records = ReadRecords(buffer);

        return 0;
    }

    void IByteArraySerializable.WriteTo(Span<byte> buffer)
    {
        throw new NotImplementedException();
    }

    public static BTreeNode ReadNode(BTree tree, byte[] buffer, int offset)
    {
        var descriptor =
            EndianUtilities.ToStruct<BTreeNodeDescriptor>(buffer, offset);

        return descriptor.Kind switch
        {
            BTreeNodeKind.HeaderNode => new BTreeHeaderNode(tree, descriptor),
            BTreeNodeKind.IndexNode or BTreeNodeKind.LeafNode => throw new NotImplementedException("Attempt to read index/leaf node without key and data types"),
            _ => throw new NotImplementedException($"Unrecognized BTree node kind: {descriptor.Kind}"),
        };
    }

    public static BTreeNode ReadNode<TKey>(BTree tree, byte[] buffer, int offset)
        where TKey : BTreeKey, new()
    {
        var descriptor =
            EndianUtilities.ToStruct<BTreeNodeDescriptor>(buffer, offset);

        return descriptor.Kind switch
        {
            BTreeNodeKind.HeaderNode => new BTreeHeaderNode(tree, descriptor),
            BTreeNodeKind.LeafNode => new BTreeLeafNode<TKey>(tree, descriptor),
            BTreeNodeKind.IndexNode => new BTreeIndexNode<TKey>(tree, descriptor),
            _ => throw new NotImplementedException($"Unrecognized BTree node kind: {descriptor.Kind}"),
        };
    }

    protected virtual IList<BTreeNodeRecord> ReadRecords(ReadOnlySpan<byte> buffer)
    {
        throw new NotImplementedException();
    }
}