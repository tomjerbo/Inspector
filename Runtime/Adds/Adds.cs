using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Jerbo.Inspector
{
    public static class Adds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public static T GetIndex<T>(this List<T> list, int index)
        {
            int collectionAmount = list.Count;
            if (collectionAmount == 0) {
                return default;
            }
            
            if (index >= collectionAmount) {
                return list[index % collectionAmount];
            }
            
            // Negative index
            if (index < 0) {
                index = -index;
                index = (collectionAmount-1) - (index % collectionAmount);
            }
            
            return list[index];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public static T GetIndex<T>(this T[] array, int index)
        {
            int collectionAmount = array.Length;
            if (collectionAmount == 0) {
                return default;
            }
            
            if (index >= collectionAmount) {
                return array[index % collectionAmount];
            }
            
            // Negative index
            if (index < 0) {
                index = -index;
                index = (collectionAmount-1) - (index % collectionAmount);
            }
            
            return array[index];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public static T Random<T>(this List<T> list) {
            int maxCount = list.Count;
            if (maxCount == 0) return default;
            return list[UnityEngine.Random.Range(0, maxCount)];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public static T Random<T>(this T[] array) {
            int maxCount = array.Length;
            if (maxCount == 0) return default;
            return array[UnityEngine.Random.Range(0, maxCount)];
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public static int Map(this int value, int min, int max)
        {
            int offset = 0;
            if (min == 0)
            {
                min++;
                max++;
                offset = -1;
            }
            
            int range = max - min;
            return ((min + offset) + value % range);
        }
        
        
        #region Vector Extentions
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Abs(this float f) => f >= 0 ? f : -f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 NoX(this Vector3 v) => new (0, v.y, v.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 NoY(this Vector3 v) => new (v.x, 0, v.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 NoZ(this Vector3 v) => new (v.x, v.y, 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 InvertX(this Vector3 v) => new (-v.x, v.y, v.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 InvertY(this Vector3 v) => new (v.x, -v.y, v.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 InvertZ(this Vector3 v) => new (v.x, v.y, -v.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 AddX(this Vector3 v, float x) => new (v.x + x, v.y, v.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 AddY(this Vector3 v, float y) => new (v.x, v.y + y, v.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 AddZ(this Vector3 v, float z) => new (v.x, v.y, v.z + z);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 AddX(this Vector2 v, float x) => new (v.x + x, v.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 AddY(this Vector2 v, float y) => new (v.x, v.y + y);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 X(this Vector3 v) => new (v.x, v.x, v.x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 Y(this Vector3 v) => new (v.y, v.y, v.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 Z(this Vector3 v) => new (v.z, v.z, v.z);

        
        #endregion

    }
}
