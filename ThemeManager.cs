using System;
using System.Windows.Media;

namespace Noteblack {
    public enum BlackThemeStyle {
        PureBlack,   // 100% OLED Black
        DeepDark,    // Sleek Dark Gray / Black (VS Code / Win11 dark style)
        Charcoal     // Charcoal Dark
    }

    public class ThemeColors {
        public Color WindowBackground { get; set; }
        public Color TitleBarBackground { get; set; }
        public Color EditorBackground { get; set; }
        public Color MenuBackground { get; set; }
        public Color MenuHover { get; set; }
        public Color MenuBorder { get; set; }
        public Color StatusBarBackground { get; set; }
        public Color TextForeground { get; set; }
        public Color BorderColor { get; set; }
        public Color PanelBackground { get; set; }
    }

    public static class ThemeManager {
        public static ThemeColors GetColors(BlackThemeStyle style) {
            switch (style) {
                case BlackThemeStyle.PureBlack:
                    return new ThemeColors {
                        WindowBackground = Color.FromRgb(0, 0, 0),
                        TitleBarBackground = Color.FromRgb(5, 5, 5),
                        EditorBackground = Color.FromRgb(0, 0, 0),
                        MenuBackground = Color.FromRgb(8, 8, 8),
                        MenuHover = Color.FromRgb(28, 28, 28),
                        MenuBorder = Color.FromRgb(35, 35, 35),
                        StatusBarBackground = Color.FromRgb(8, 8, 8),
                        TextForeground = Color.FromRgb(245, 245, 245),
                        BorderColor = Color.FromRgb(30, 30, 30),
                        PanelBackground = Color.FromRgb(12, 12, 12)
                    };
                case BlackThemeStyle.Charcoal:
                    return new ThemeColors {
                        WindowBackground = Color.FromRgb(22, 24, 28),
                        TitleBarBackground = Color.FromRgb(20, 22, 26),
                        EditorBackground = Color.FromRgb(24, 26, 31),
                        MenuBackground = Color.FromRgb(20, 22, 26),
                        MenuHover = Color.FromRgb(40, 44, 52),
                        MenuBorder = Color.FromRgb(45, 50, 60),
                        StatusBarBackground = Color.FromRgb(20, 22, 26),
                        TextForeground = Color.FromRgb(235, 237, 240),
                        BorderColor = Color.FromRgb(40, 44, 52),
                        PanelBackground = Color.FromRgb(28, 31, 38)
                    };
                case BlackThemeStyle.DeepDark:
                default:
                    return new ThemeColors {
                        WindowBackground = Color.FromRgb(16, 16, 16),
                        TitleBarBackground = Color.FromRgb(18, 18, 18),
                        EditorBackground = Color.FromRgb(12, 12, 12),
                        MenuBackground = Color.FromRgb(18, 18, 18),
                        MenuHover = Color.FromRgb(36, 36, 36),
                        MenuBorder = Color.FromRgb(44, 44, 44),
                        StatusBarBackground = Color.FromRgb(18, 18, 18),
                        TextForeground = Color.FromRgb(240, 240, 240),
                        BorderColor = Color.FromRgb(36, 36, 36),
                        PanelBackground = Color.FromRgb(24, 24, 24)
                    };
            }
        }
    }
}
