using System;
using System.IO;
using System.Text;

namespace Noteblack {
    public enum DocumentEncoding {
        Utf8NoBom,
        Utf8Bom,
        Utf16LE,
        Utf16BE,
        ANSI
    }

    public static class EncodingHelper {
        public static string GetDisplayName(DocumentEncoding enc) {
            switch (enc) {
                case DocumentEncoding.Utf8Bom: return "UTF-8 with BOM";
                case DocumentEncoding.Utf16LE: return "UTF-16 LE";
                case DocumentEncoding.Utf16BE: return "UTF-16 BE";
                case DocumentEncoding.ANSI: return "ANSI";
                case DocumentEncoding.Utf8NoBom:
                default: return "UTF-8";
            }
        }

        public static Encoding GetSystemEncoding(DocumentEncoding enc) {
            switch (enc) {
                case DocumentEncoding.Utf8Bom:
                    return new UTF8Encoding(true);
                case DocumentEncoding.Utf16LE:
                    return new UnicodeEncoding(false, true);
                case DocumentEncoding.Utf16BE:
                    return new UnicodeEncoding(true, true);
                case DocumentEncoding.ANSI:
                    return Encoding.Default;
                case DocumentEncoding.Utf8NoBom:
                default:
                    return new UTF8Encoding(false);
            }
        }

        public static bool DetectFile(string path, out string content, out DocumentEncoding encoding, out LineEndingType lineEnding) {
            byte[] bytes = File.ReadAllBytes(path);

            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) {
                encoding = DocumentEncoding.Utf8Bom;
                content = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
            } else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) {
                encoding = DocumentEncoding.Utf16LE;
                content = Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
            } else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) {
                encoding = DocumentEncoding.Utf16BE;
                content = Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
            } else {
                // Check if valid UTF-8
                if (IsValidUtf8(bytes)) {
                    encoding = DocumentEncoding.Utf8NoBom;
                    content = new UTF8Encoding(false).GetString(bytes);
                } else {
                    encoding = DocumentEncoding.ANSI;
                    content = Encoding.Default.GetString(bytes);
                }
            }

            lineEnding = LineEndingHelper.DetectLineEnding(content);
            return true;
        }

        public static void SaveFile(string path, string content, DocumentEncoding encoding, LineEndingType lineEnding) {
            string normalized = LineEndingHelper.NormalizeLineEndings(content, lineEnding);
            Encoding enc = GetSystemEncoding(encoding);
            byte[] preamble = enc.GetPreamble();
            byte[] textBytes = enc.GetBytes(normalized);

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                if (preamble != null && preamble.Length > 0) {
                    fs.Write(preamble, 0, preamble.Length);
                }
                fs.Write(textBytes, 0, textBytes.Length);
            }
        }

        private static bool IsValidUtf8(byte[] bytes) {
            int i = 0;
            int length = bytes.Length;
            while (i < length) {
                byte b = bytes[i];
                if ((b & 0x80) == 0) {
                    i++;
                } else if ((b & 0xE0) == 0xC0) {
                    if (i + 1 >= length || (bytes[i + 1] & 0xC0) != 0x80) return false;
                    i += 2;
                } else if ((b & 0xF0) == 0xE0) {
                    if (i + 2 >= length || (bytes[i + 1] & 0xC0) != 0x80 || (bytes[i + 2] & 0xC0) != 0x80) return false;
                    i += 3;
                } else if ((b & 0xF8) == 0xF0) {
                    if (i + 3 >= length || (bytes[i + 1] & 0xC0) != 0x80 || (bytes[i + 2] & 0xC0) != 0x80 || (bytes[i + 3] & 0xC0) != 0x80) return false;
                    i += 4;
                } else {
                    return false;
                }
            }
            return true;
        }
    }
}
