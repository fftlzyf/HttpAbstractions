﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.Http.Internal
{
    public struct InplaceStringFormatter
    {
        private int _length;
        private int _offset;
        private bool _writing;
        private string _value;

        public InplaceStringFormatter(int length) : this()
        {
            _length = length;
        }

        public void AppendLength(string s)
        {
            AppendLength(s.Length);
        }
        public void AppendLength(char c)
        {
            AppendLength(1);
        }

        public void AppendLength(int length)
        {
            if (_writing)
            {
                throw new InvalidOperationException("Cannot append lenght after write started.");
            }
            _length += length;
        }

        public unsafe void Append(string s)
        {
            EnsureValue(s.Length);
            fixed (char* value = _value)
            fixed (char* pDomainToken = s)
            {
                Unsafe.CopyBlock(value + _offset, pDomainToken, (uint)s.Length * 2);
                _offset += s.Length;
            }
        }
        public unsafe void Append(char c)
        {
            EnsureValue(1);
            fixed (char* value = _value)
            {
                value[_offset++] = c;
            }
        }

        private void EnsureValue(int length)
        {
            if (_value == null)
            {
                _writing = true;
                _value = new string('\0', _length);
            }
            if (_offset + length > _length)
            {
                throw new InvalidOperationException($"Not enough space to write '{length}' characters, only '{_length - _offset}' left.");
            }
        }

        public override string ToString()
        {
            if (_offset != _length)
            {
                throw new InvalidOperationException($"Entire reserved lenght was not used. Length: '{_length}', written '{_offset}'.");
            }
            return _value;
        }
    }
}