﻿using System;
using System.Numerics;

namespace MonoNanoGUI
{
    public static class VectorExtensions
    {
        public static float Component(this Vector2 v, int i) => i == 0 ? v.X : v.Y;
        public static float Component(this Vector2 v, int i, float value) => i == 0 ? v.X = value : v.Y = value;

        public static float Sum (this Vector4 v)
        {
            return v.X + v.Y + v.Z + v.W;
        }

        // public static float Dot (this Vector4 v, Vector4 other)
        // {
        //     Vector4 ret = v;
        //     ret.Scale (other);
        //     return ret.Sum ();
        // }

        public static Vector4 Div (this Vector4i v, float divisor)
        {
	        Vector4 ret;
	        ret.X = v.X / divisor;
	        ret.Y = v.Y / divisor;
	        ret.Z = v.Z / divisor;
            ret.W = v.W / divisor;
	        return ret;
        }

        public static Vector3 Div (this Vector3i v, float divisor)
        {
            Vector3 ret;
            ret.X = v.X / divisor;
            ret.Y = v.Y / divisor;
            ret.Z = v.Z / divisor;
            return ret;
        }
    }
}
