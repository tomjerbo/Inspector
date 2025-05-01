using System;
using UnityEngine;

namespace Jerbo.Inspector {
    public class Tab : PropertyAttribute {
        public UnityEngine.Color color;
        public string title;

        public enum Color {
            White,
            Yellow,
            Green,
            Blue,
            Grey,
            Red,
            Purple,
            Pink,
        }
        
        public Tab(string title, Color color) {
            this.title = title;
            this.color = color switch {
                Color.White => new Color32(255, 255, 255, 255),
                Color.Yellow => new Color32(253, 203, 110, 255),
                Color.Green => new Color32(0, 184, 148, 255),
                Color.Blue => new Color32(9, 132, 227, 255),
                Color.Grey => new Color32(178, 189, 195, 255),
                Color.Red => new Color32(225, 112, 85, 255),
                Color.Purple => new Color32(162, 155, 254, 255),
                Color.Pink => new Color32(253, 121, 168, 255),
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
            };
        }
        
        public Tab(string title) {
            this.title = title;
            color = UnityEngine.Color.white;
        }

        public Tab(string title, float[] col) {
            this.title = title;
            color = UnityEngine.Color.white;
            for (int i = 0; i < Mathf.Min(col.Length, 4); i++) {
                color[i] = col[i];
            }
        }

        public Tab(string title, byte[] col) {
            this.title = title;
            Color32 c = new (255, 255, 255, 255);
            for (int i = 0; i < Mathf.Min(col.Length, 4); i++) {
                c[i] = col[i];
            }

            color = c;
        }

        public Tab(string title, float r, float g, float b, float a = 1) {
            this.title = title;
            color = new UnityEngine.Color(r, g, b, a);
        }

        public Tab(string title, byte r, byte g, byte b, byte a = 255) {
            this.title = title;
            color = new Color32(r, g, b, a);
        }
    }
}