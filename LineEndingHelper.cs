using System;
using System.Text;

namespace Noteblack {
    public enum LineEndingType {
        CRLF, // Windows \r\n
        LF,   // Unix / Linux / macOS \n
        CR    // Classic Macintosh \r
    }

    public static class LineEndingHelper {
        public static string GetDisplayName(LineEndingType type) {
            switch (type) {
                case LineEndingType.LF: return "Unix (LF)";
                case LineEndingType.CR: return "Macintosh (CR)";
                case LineEndingType.CRLF:
                default: return "Windows (CRLF)";
            }
        }

        public static string GetString(LineEndingType type) {
            switch (type) {
                case LineEndingType.LF: return "\n";
                case LineEndingType.CR: return "\r";
                case LineEndingType.CRLF:
                default: return "\r\n";
            }
        }

        public static LineEndingType DetectLineEnding(string text) {
            if (string.IsNullOrEmpty(text)) {
                return LineEndingType.CRLF;
            }

            int crlfCount = 0;
            int lfCount = 0;
            int crCount = 0;

            for (int i = 0; i < text.Length; i++) {
                char c = text[i];
                if (c == '\r') {
                    if (i + 1 < text.Length && text[i + 1] == '\n') {
                        crlfCount++;
                        i++; // Skip the '\n' of '\r\n'
                    } else {
                        crCount++;
                    }
                } else if (c == '\n') {
                    lfCount++;
                }
            }

            if (lfCount > crlfCount && lfCount >= crCount) {
                return LineEndingType.LF;
            }
            if (crCount > crlfCount && crCount > lfCount) {
                return LineEndingType.CR;
            }
            return LineEndingType.CRLF;
        }

        public static string NormalizeLineEndings(string text, LineEndingType targetType) {
            if (string.IsNullOrEmpty(text)) {
                return string.Empty;
            }

            string targetStr = GetString(targetType);
            var sb = new StringBuilder(text.Length + 64);

            for (int i = 0; i < text.Length; i++) {
                char c = text[i];
                if (c == '\r') {
                    if (i + 1 < text.Length && text[i + 1] == '\n') {
                        i++; // Skip '\n'
                    }
                    sb.Append(targetStr);
                } else if (c == '\n' || c == '\u0085' || c == '\u2028' || c == '\u2029') {
                    // Also support Unicode Next Line (NEL), Line Separator (LS), Paragraph Separator (PS)
                    sb.Append(targetStr);
                } else {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
