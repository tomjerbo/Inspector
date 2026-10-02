using System.Runtime.CompilerServices;
using UnityEngine;

public static class Rect_Extensions {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect reduce(this Rect rect, float amount) {
        rect.x += amount;
        rect.y += amount;
        rect.width -= amount * 2;
        rect.height -= amount * 2;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect reduce_x(this Rect rect, float amount) {
        rect.x += amount;
        rect.width -= amount * 2;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect reduce_y(this Rect rect, float amount) {
        rect.y += amount;
        rect.height -= amount * 2;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect extend(this Rect rect, float amount) {
        rect.width += amount * 2;
        rect.height += amount * 2;
        rect.x -= amount;
        rect.y -= amount;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect extend_left(this Rect rect, float amount) {
        rect.x -= amount;
        rect.width += amount;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect extend_right(this Rect rect, float amount) {
        rect.width += amount;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect extend_up(this Rect rect, float amount) {
        rect.y -= amount;
        rect.height += amount;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect extend_down(this Rect rect, float amount) {
        rect.height += amount;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect trim_left(this Rect rect, float amount) {
        rect.x += amount;
        rect.width -= amount;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect trim_right(this Rect rect, float amount) {
        rect.width -= amount;
        return rect;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect trim_top(this Rect rect, float amount) {
        rect.y += amount;
        rect.height -= amount;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect trim_bottom(this Rect rect, float amount) {
        rect.height -= amount;
        return rect;
    }

    /// <summary>
    /// ratio is normalized, aka 0.0 - 1.0
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect scale_x(this Rect rect, float ratio) {
        rect.width *= ratio;
        return rect;
    }

    /// <summary>
    /// ratio is normalized, aka 0.0 - 1.0
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect scale_y(this Rect rect, float ratio) {
        rect.height *= ratio;
        return rect;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect split_even_horizontal(this Rect rect, int splits, float spacing) {
        rect.width = (rect.width / splits) - spacing;
        return rect;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect[] split_horizontal_custom(this Rect rect, float spacing, params float[] split_ratios) {
        Rect[] result_rects = new Rect[split_ratios.Length];
        float x_offset = rect.x;
        for (int idx = 0; idx < split_ratios.Length; idx++) {
            float width = rect.width * split_ratios[idx];
            
            Rect split = new () {
                y = rect.y,
                height = rect.height,

                x = x_offset,
                width = width,
            };

            x_offset += width;
            
            if (idx == 0) {
                split = split.trim_right(spacing * 0.5f);
            }
            else if (idx == split_ratios.Length - 1) {
                split = split.trim_left(spacing * 0.5f);
            }
            else {
                split = split.reduce_x(spacing);
            }
            
            result_rects[idx] = split;
        }
        
        return result_rects;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect[] split_vertical_custom(this Rect rect, float spacing, params float[] split_ratios) {
        Rect[] result_rects = new Rect[split_ratios.Length];
        float y_offset = rect.y;
        for (int idx = 0; idx < split_ratios.Length; idx++) {
            float height = rect.height * split_ratios[idx];
            
            Rect split = new () {
                x = rect.x,
                width = rect.width,

                y = y_offset,
                height = height,
            };

            y_offset += height;
            
            if (idx == 0) {
                split = split.trim_bottom(spacing * 0.5f);
            }
            else if (idx == split_ratios.Length - 1) {
                split = split.trim_top(spacing * 0.5f);
            }
            else {
                split = split.trim_top(spacing * 0.5f).trim_bottom(spacing * 0.5f);
            }
            
            result_rects[idx] = split;
        }
        
        return result_rects;
    }
}