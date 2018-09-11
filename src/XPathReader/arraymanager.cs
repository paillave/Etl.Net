//------------------------------------------------------------------------------
// <copyright file="ArrayManager.cs" company="Microsoft">
//     
//      Copyright (c) 2002 Microsoft Corporation.  All rights reserved.
//     
//      The use and distribution terms for this software are contained in the file
//      named license.txt, which can be found in the root of this distribution.
//      By using this software in any fashion, you are agreeing to be bound by the
//      terms of this license.
//     
//      You must not remove this notice, or any other, from this software.
//     
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections;

namespace GotDotNet.XPath
{
	internal class ArrayManager {
        internal class Buffer {
            public char[] _charBuffer;
            public int    _offset;
            public int    _size;
            
            public Buffer(char[] buffer, int offset, int size) {
                _charBuffer = buffer;
                _offset     = offset;
                _size       = size;
            }
        }

        private Queue  _BufferQueue;
        private int    _offset;
        private Buffer _CurrentBuffer;
        
        private Queue BufferQueue
        {
            get
            {
                if (_BufferQueue == null)
                    _BufferQueue = new Queue();
                return _BufferQueue;
            }
            set
            {
                _BufferQueue = value;
            }
        }

        private int Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                _offset = value;
            }
        }

        internal char[] CurrentBuffer
        {
            get{
                if (_CurrentBuffer != null)
                    return _CurrentBuffer._charBuffer;
                return null;
            }
        }

        internal int CurrentBufferOffset
        {
            get
            {
                if (_CurrentBuffer != null)
                    return _CurrentBuffer._offset;
                return 0;
            }
        }

        internal int CurrentBufferLength
        {
            get
            {
                if (_CurrentBuffer != null)
                    return _CurrentBuffer._size;
                return 0;
            }
        }

        internal int Length
        {
            get
            {
                int len = 0;
                if (_CurrentBuffer != null)
                    len += (_CurrentBuffer._size - _CurrentBuffer._offset);
                IEnumerator enumerator = BufferQueue.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Buffer element = (Buffer) enumerator.Current;
                    len += (element._size - element._offset);
                }
                return len;
            }
        }

        internal char this[int index]
        {
            get
            {
                char ch = '\0';
                if (_CurrentBuffer == null) {
                    if (BufferQueue.Count > 0)
                        _CurrentBuffer = (Buffer) BufferQueue.Dequeue();
                    else
                        return ch;
                }

                if (!((_CurrentBuffer._offset + index - Offset) < _CurrentBuffer._size)) {
                    Offset = index;
                    _CurrentBuffer = (BufferQueue.Count > 0) ? (Buffer) BufferQueue.Dequeue():null;
                }

                if (_CurrentBuffer != null)
                    ch = _CurrentBuffer._charBuffer[_CurrentBuffer._offset + (index - Offset)];
                return ch;
            }
        }
            
        internal void Append(char[] buffer, int offset, int size) {
            BufferQueue.Enqueue(new Buffer(buffer, offset, size));
        }

        internal void CleanUp(int internalBufferOffset) {
            if (_CurrentBuffer != null) {
                _CurrentBuffer._offset += internalBufferOffset - Offset;
                Offset = 0;
            }
        }

        internal void Refresh() {
            BufferQueue = new Queue();
            _CurrentBuffer = null;
            _offset = 0;
        }

        internal ArrayManager() {
            BufferQueue     = null;
            _offset         = 0;
            _CurrentBuffer = null;
        }
    }
}
