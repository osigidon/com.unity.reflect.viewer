using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CivilFX.Generic2
{
    public static class FloatExtensions
    {
        public static bool Compare(this float lvalue, float rvalue, float variance = 0.01f)
        {
            float cmp = variance;

            if (lvalue > rvalue) {
                cmp = lvalue - rvalue;
            } else if (rvalue > lvalue) {
                cmp = rvalue - lvalue;
            }
            return cmp <= variance;
        }
    }
}