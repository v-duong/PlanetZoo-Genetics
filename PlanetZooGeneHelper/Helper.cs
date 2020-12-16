using System;
using System.IO;

namespace PlanetZooGeneHelper
{
    internal class Helper
    {
        public static bool CheckFirstFourBits(byte b, byte compare)
        {
            return (b & 0xF0) == compare;
        }
        public static uint ConvertLastThreeBytes(byte a, byte b)
        {
            return (uint)(((a & 0x0F) << 8) + b);
        }

        public static long Seek(Stream stream, byte[] search, long endIndex = -1)
        {
            int bufferSize = 1024;
            if (bufferSize < search.Length * 2) bufferSize = search.Length * 2;

            var buffer = new byte[bufferSize];
            var size = bufferSize;
            var offset = 0;
            var position = stream.Position;

            while (true)
            {
                if (endIndex > -1 && position > endIndex)
                    return -1;
                var r = stream.Read(buffer, offset, size);

                // when no bytes are read -- the string could not be found
                if (r <= 0) return -1;

                // when less then size bytes are read, we need to slice
                // the buffer to prevent reading of "previous" bytes
                ReadOnlySpan<byte> ro = buffer;
                if (r < size)
                {
                    ro = ro.Slice(0, offset + size);
                }

                // check if we can find our search bytes in the buffer
                var i = ro.IndexOf(search);
                if (i > -1) return position + i;

                // when less then size was read, we are done and found nothing
                if (r < size) return -1;

                // we still have bytes to read, so copy the last search
                // length to the beginning of the buffer. It might contain
                // a part of the bytes we need to search for

                offset = search.Length;
                size = bufferSize - offset;
                Array.Copy(buffer, buffer.Length - offset, buffer, 0, offset);
                position += bufferSize - offset;
            }
        }
    }
}