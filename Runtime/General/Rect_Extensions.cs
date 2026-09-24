using System.Runtime.CompilerServices;
using UnityEngine;

public static class Rect_Extensions {
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect reduce(this Rect rect, float amount) {
        rect.x      += amount;
        rect.y      += amount;
        rect.width  -= amount * 2;
        rect.height -= amount * 2;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect reduce_x(this Rect rect, float amount) {
        rect.x      += amount;
        rect.width  -= amount * 2;
        return rect;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect reduce_y(this Rect rect, float amount) {
        rect.y       += amount;
        rect.height  -= amount * 2;
        return rect;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect extend(this Rect rect, float amount) {
        rect.width  += amount * 2;
        rect.height += amount * 2;
        return rect;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect extend_left(this Rect rect, float amount) {
        rect.x      -= amount;
        rect.width  += amount;
        return rect;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect extend_right(this Rect rect, float amount) {
        rect.width  += amount;
        return rect;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect extend_up(this Rect rect, float amount) {
        rect.y       -= amount;
        rect.height  += amount;
        return rect;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect extend_down(this Rect rect, float amount) {
        rect.height  += amount;
        return rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect split_even_horizontal(this Rect rect, int splits, float spacing) {
        rect.width = (rect.width / splits) - spacing;
        return rect;
    }
}