using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace MarketingPlatform.Common
{
    public class FormUrlEncodedContent : ByteArrayContent
    {
        internal const char c_DummyChar = (char)0xFFFF;     //An Invalid Unicode character used as a dummy char passed into the parameter
        internal const int c_MaxUriBufferSize = int.MaxValue;
        const short c_MaxUnicodeCharsReallocate = 40;
        const short c_MaxAsciiCharsReallocate = 40;
        const short c_MaxUTF_8BytesPerUnicodeChar = 4;
        const short c_EncodedCharsPerByte = 3;
        private const string RFC2396ReservedMarks = @";/?:@&=+$,";
        private const string RFC3986ReservedMarks = @":/?#[]@!$&'()*+,;=";
        private const string RFC2396UnreservedMarks = @"-_.!~*'()";
        private const string RFC3986UnreservedMarks = @"-._~";

        private static readonly char[] HexUpperChars = {
                                   '0', '1', '2', '3', '4', '5', '6', '7',
                                   '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public FormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection) : base(FormUrlEncodedContent.GetContentByteArray(nameValueCollection))
        {
            base.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
        }

        private static byte[] GetContentByteArray(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
        {
            if (nameValueCollection == null)
            {
                throw new ArgumentNullException("nameValueCollection");
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> current in nameValueCollection)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append('&');
                }
                stringBuilder.Append(FormUrlEncodedContent.Encode(current.Key));
                stringBuilder.Append('=');
                stringBuilder.Append(FormUrlEncodedContent.Encode(current.Value));
            }
            return HttpRuleParser.DefaultHttpEncoding.GetBytes(stringBuilder.ToString());
        }

        private static string Encode(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }
            return EscapeDataString(data).Replace("%20", "+");
        }

        static string EscapeDataString(string stringToEscape)
        {
            if ((object)stringToEscape == null)
                throw new ArgumentNullException("stringToEscape 为 null。");

            if (stringToEscape.Length == 0)
                return string.Empty;

            int position = 0;
            char[] dest = EscapeString(stringToEscape, 0, stringToEscape.Length, null, ref position, false,
                c_DummyChar, c_DummyChar, c_DummyChar);
            if (dest == null)
                return stringToEscape;
            return new string(dest, 0, position);
        }

        internal unsafe static char[] EscapeString(string input, int start, int end, char[] dest, ref int destPos,
            bool isUriString, char force1, char force2, char rsvd)
        {
            if (end - start >= c_MaxUriBufferSize)
                throw new UriFormatException(string.Format("stringToEscape 的长度超过 {0} 个字符。", c_MaxUriBufferSize));

            int i = start;
            int prevInputPos = start;
            byte* bytes = stackalloc byte[c_MaxUnicodeCharsReallocate * c_MaxUTF_8BytesPerUnicodeChar];   // 40*4=160



            fixed (char* pStr = input)
            {
                for (; i < end; ++i)
                {
                    char ch = pStr[i];

                    // a Unicode ?
                    if (ch > '\x7F')
                    {
                        //Debug.WriteLine("Enter ch > 7f");

                        short maxSize = (short)Math.Min(end - i, (int)c_MaxUnicodeCharsReallocate - 1);

                        short count = 1;
                        for (; count < maxSize && pStr[i + count] > '\x7f'; ++count)
                            ;

                        // Is the last a high surrogate?
                        if (pStr[i + count - 1] >= 0xD800 && pStr[i + count - 1] <= 0xDBFF)
                        {
                            // Should be a rare case where the app tries to feed an invalid Unicode surrogates pair
                            if (count == 1 || count == end - i)
                                throw new UriFormatException(string.Format(" 无效的 URI: 字符串中有无效的序列。"));
                            // need to grab one more char as a Surrogate except when it's a bogus input
                            ++count;
                        }

                        dest = EnsureDestinationSize(pStr, dest, i,
                            (short)(count * c_MaxUTF_8BytesPerUnicodeChar * c_EncodedCharsPerByte),
                            c_MaxUnicodeCharsReallocate * c_MaxUTF_8BytesPerUnicodeChar * c_EncodedCharsPerByte,
                            ref destPos, prevInputPos);

                        short numberOfBytes = (short)Encoding.UTF8.GetBytes(pStr + i, count, bytes,
                            c_MaxUnicodeCharsReallocate * c_MaxUTF_8BytesPerUnicodeChar);

                        // This is the only exception that built in UriParser can throw after a Uri ctor.
                        // Should not happen unless the app tries to feed an invalid Unicode String
                        if (numberOfBytes == 0)
                            throw new UriFormatException(string.Format("无效的 URI: 字符串中有无效的序列。"));

                        i += (count - 1);

                        for (count = 0; count < numberOfBytes; ++count)
                            EscapeAsciiChar((char)bytes[count], dest, ref destPos);

                        prevInputPos = i + 1;

                        //Debug.WriteLine("Leave ch > 7f");

                    }
                    else if (ch == '%' && rsvd == '%')
                    {
                        //Debug.WriteLine("Enter ch == '%'");

                        // Means we don't reEncode '%' but check for the possible escaped sequence
                        dest = EnsureDestinationSize(pStr, dest, i, c_EncodedCharsPerByte,
                            c_MaxAsciiCharsReallocate * c_EncodedCharsPerByte, ref destPos, prevInputPos);
                        if (i + 2 < end && EscapedAscii(pStr[i + 1], pStr[i + 2]) != c_DummyChar)
                        {
                            // leave it escaped
                            dest[destPos++] = '%';
                            dest[destPos++] = pStr[i + 1];
                            dest[destPos++] = pStr[i + 2];
                            i += 2;
                        }
                        else
                        {
                            EscapeAsciiChar('%', dest, ref destPos);
                        }
                        prevInputPos = i + 1;

                        //Debug.WriteLine("Leave ch == '%'");

                    }
                    else if (ch == force1 || ch == force2)
                    {
                        //Debug.WriteLine("Enter ch == force1");

                        dest = EnsureDestinationSize(pStr, dest, i, c_EncodedCharsPerByte,
                            c_MaxAsciiCharsReallocate * c_EncodedCharsPerByte, ref destPos, prevInputPos);
                        EscapeAsciiChar(ch, dest, ref destPos);
                        prevInputPos = i + 1;


                        //Debug.WriteLine("Leave ch == force1");
                    }
                    else if (ch != rsvd && (isUriString ? !IsReservedUnreservedOrHash(ch) : !IsUnreserved(ch)))
                    {
                        Debug.WriteLine("Enter ch != rsvd");


                        dest = EnsureDestinationSize(pStr, dest, i, c_EncodedCharsPerByte,
                            c_MaxAsciiCharsReallocate * c_EncodedCharsPerByte, ref destPos, prevInputPos);
                        EscapeAsciiChar(ch, dest, ref destPos);
                        prevInputPos = i + 1;

                        Debug.WriteLine("Leave ch != rsvd");
                    }
                    Debug.WriteLine("i: " + i);
                }


                if (prevInputPos != i)
                {
                    // need to fill up the dest array ?
                    if (prevInputPos != start || dest != null)
                        dest = EnsureDestinationSize(pStr, dest, i, 0, 0, ref destPos, prevInputPos);
                }
            }

            return dest;
        }

        //
        // ensure destination array has enough space and contains all the needed input stuff
        //
        private unsafe static char[] EnsureDestinationSize(char* pStr, char[] dest, int currentInputPos,
            short charsToAdd, short minReallocateChars, ref int destPos, int prevInputPos)
        {
            if ((object)dest == null || dest.Length < destPos + (currentInputPos - prevInputPos) + charsToAdd)
            {
                // allocating or reallocating array by ensuring enough space based on maxCharsToAdd.
                char[] newresult = new char[destPos + (currentInputPos - prevInputPos) + minReallocateChars];

                if ((object)dest != null && destPos != 0)
                    Buffer.BlockCopy(dest, 0, newresult, 0, destPos << 1);
                dest = newresult;
            }

            // ensuring we copied everything form the input string left before last escaping
            while (prevInputPos != currentInputPos)
                dest[destPos++] = pStr[prevInputPos++];
            return dest;
        }
        internal static void EscapeAsciiChar(char ch, char[] to, ref int pos)
        {
            to[pos++] = '%';
            to[pos++] = HexUpperChars[(ch & 0xf0) >> 4];
            to[pos++] = HexUpperChars[ch & 0xf];
        }

        internal static char EscapedAscii(char digit, char next)
        {
            if (!(((digit >= '0') && (digit <= '9'))
                || ((digit >= 'A') && (digit <= 'F'))
                || ((digit >= 'a') && (digit <= 'f'))))
            {
                return c_DummyChar;
            }

            int res = (digit <= '9')
                ? ((int)digit - (int)'0')
                : (((digit <= 'F')
                ? ((int)digit - (int)'A')
                : ((int)digit - (int)'a'))
                   + 10);

            if (!(((next >= '0') && (next <= '9'))
                || ((next >= 'A') && (next <= 'F'))
                || ((next >= 'a') && (next <= 'f'))))
            {
                return c_DummyChar;
            }

            return (char)((res << 4) + ((next <= '9')
                    ? ((int)next - (int)'0')
                    : (((next <= 'F')
                        ? ((int)next - (int)'A')
                        : ((int)next - (int)'a'))
                       + 10)));
        }

        private static unsafe bool IsReservedUnreservedOrHash(char c)
        {
            if (IsUnreserved(c))
            {
                return true;
            }
            if (ShouldUseLegacyV2Quirks)
            {
                return ((RFC2396ReservedMarks.IndexOf(c) >= 0) || c == '#');
            }
            return (RFC3986ReservedMarks.IndexOf(c) >= 0);
        }

        internal static bool IsAsciiLetterOrDigit(char character)
        {
            return IsAsciiLetter(character) || (character >= '0' && character <= '9');
        }

        //Only consider ASCII characters
        private static bool IsAsciiLetter(char character)
        {

            return (character >= 'a' && character <= 'z') ||
                   (character >= 'A' && character <= 'Z');
        }

        internal static unsafe bool IsUnreserved(char c)
        {
            if (IsAsciiLetterOrDigit(c))
            {
                return true;
            }
            if (ShouldUseLegacyV2Quirks)
            {
                return (RFC2396UnreservedMarks.IndexOf(c) >= 0);
            }
            return (RFC3986UnreservedMarks.IndexOf(c) >= 0);
        }


        private enum UriQuirksVersion
        {
            // V1 = 1, // RFC 1738 - Not supported
            V2 = 2, // RFC 2396
            V3 = 3, // RFC 3986, 3987
        }

        // Store in a static field to allow for test manipulation and emergency workarounds via reflection.
        // Note this is not placed in the Uri class in order to avoid circular static dependencies.
        private static readonly UriQuirksVersion s_QuirksVersion = UriQuirksVersion.V2;

        internal static bool ShouldUseLegacyV2Quirks
        {
            get
            {
                return s_QuirksVersion <= UriQuirksVersion.V2;
            }
        }

    }

}
