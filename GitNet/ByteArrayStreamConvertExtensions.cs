using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitNet
{
    public static class ByteArrayStreamConvertExtensions
    {
        public static Stream ToStream(this byte[] byteArray, int blockSize = 8192)
        {
            byte[] newByteArray = new byte[byteArray.Length];

            int i = 0;
            while (i < byteArray.Length)
            {
                int toWrite = Math.Min(byteArray.Length - i, blockSize);
                Array.Copy(byteArray, i, newByteArray, i, toWrite);
                i += toWrite;
            }

            return new MemoryStream(newByteArray);
        }

        public static byte[] ToByteArray(this Stream stream, int blockSize = 8192)
        {
            List<Tuple<byte[], int>> buffers = new List<Tuple<byte[], int>>();

            int acutallyRead = -1;
            while (acutallyRead != 0)
            {
                byte[] buffer = new byte[blockSize];
                acutallyRead = stream.Read(buffer, 0, blockSize);
                buffers.Add(Tuple.Create(buffer, acutallyRead));
            }

            int i = 0;
            int j = 0;
            byte[] byteArray = new byte[buffers.Sum(n => n.Item2)];

            while (i < buffers.Count)
            {
                Array.Copy(buffers[i].Item1, 0, byteArray, j, buffers[i].Item2);
                j += buffers[i].Item2;
                i += 1;
            }

            return byteArray;
        }
    }
}