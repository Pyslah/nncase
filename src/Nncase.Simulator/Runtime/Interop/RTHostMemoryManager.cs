﻿// Copyright (c) Canaan Inc. All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Nncase.Runtime.Interop;

namespace Nncase.Buffers;

internal unsafe class RTHostMemoryManager : MemoryManager<byte>
{
    private RTHostBuffer? _buffer;
    private IntPtr _pointer;
    private readonly uint _length;

    public RTHostMemoryManager(RTHostBuffer buffer, IntPtr pointer, uint length)
    {
        _buffer = buffer;
        _pointer = pointer;
        _length = length;

        if (length != 0)
        {
            GC.AddMemoryPressure(length);
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="RTHostMemoryManager"/> class.
    /// </summary>
#pragma warning disable CA2015 // Used only in DenseTensor
    ~RTHostMemoryManager()
#pragma warning restore CA2015
    {
        Dispose(false);
    }

    public IntPtr Pointer => _pointer;

    public override Span<byte> GetSpan()
    {
        if (_length == 0)
        {
            return Span<byte>.Empty;
        }

        return new Span<byte>((void*)_pointer, (int)_length);
    }

    public override MemoryHandle Pin(int elementIndex = 0)
    {
        if ((uint)elementIndex > _length)
        {
            throw new IndexOutOfRangeException();
        }

        return new MemoryHandle(Unsafe.Add<byte>((void*)_pointer, elementIndex), default, this);
    }

    public override void Unpin()
    {
    }

    protected override void Dispose(bool disposing)
    {
        var pointer = Interlocked.Exchange(ref _pointer, IntPtr.Zero);
        if (pointer != IntPtr.Zero && _buffer != null)
        {
            Native.HostBufferUnmap(_buffer.Handle);
            GC.RemoveMemoryPressure(_length);
            _buffer = null;
        }
    }
}