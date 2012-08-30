using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seringa.Engine.Utils
{
    public static class EncodingHelper
    {
        public static string GetEncodingFromChunk(string chunk)
        {
            string charset = null;
            int charsetStart = chunk.IndexOf("charset=");
            int charsetEnd = -1;
            if (charsetStart != -1)
            {
                charsetEnd = chunk.IndexOfAny(new[] { ' ', '\"', ';','\r','\n' }, charsetStart);
                if (charsetEnd != -1)
                {
                    int start = charsetStart + 8;
                    charset = chunk.Substring(start, charsetEnd - start + 1);
                    charset = charset.TrimEnd(new Char[] { '>', '"','\r','\n' });
                }
            }
            return charset;
        }
    }
}
