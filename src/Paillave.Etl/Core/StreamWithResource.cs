// using System;
// using System.IO;

// namespace Paillave.Etl.Core;

// public class StreamWithResource : System.IO.Stream, IDisposable
// {
//     private readonly System.IO.Stream _underlyingStream;
//     private readonly IDisposable[] _relatedDisposables;
//     private bool disposedValue;

//     public StreamWithResource(Stream underlyingStream, params IDisposable[] relatedDisposables)
//         => (_underlyingStream, _relatedDisposables) = (underlyingStream, relatedDisposables);

//     public override bool CanRead => _underlyingStream.CanRead;

//     public override bool CanSeek => _underlyingStream.CanSeek;

//     public override bool CanWrite => _underlyingStream.CanWrite;

//     public override long Length => _underlyingStream.Length;

//     public override long Position { get => _underlyingStream.Position; set => _underlyingStream.Position = value; }

//     public override void Flush()
//     {
//         _underlyingStream.Flush();
//     }

//     public override int Read(byte[] buffer, int offset, int count)
//     {
//         return _underlyingStream.Read(buffer, offset, count);
//     }

//     public override long Seek(long offset, SeekOrigin origin)
//     {
//         return _underlyingStream.Seek(offset, origin);
//     }

//     public override void SetLength(long value)
//     {
//         _underlyingStream.SetLength(value);
//     }

//     public override void Write(byte[] buffer, int offset, int count)
//     {
//         _underlyingStream.Write(buffer, offset, count);
//     }

//     protected override void Dispose(bool disposing)
//     {
//         if (!disposedValue)
//         {
//             if (disposing)
//             {
//                 foreach (var disposable in _relatedDisposables)
//                 {
//                     disposable.Dispose();
//                 }
//                 // TODO: dispose managed state (managed objects)
//             }

//             disposedValue = true;
//         }
//     }
// }