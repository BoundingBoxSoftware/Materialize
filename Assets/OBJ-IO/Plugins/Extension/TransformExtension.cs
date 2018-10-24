
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

namespace UnityExtension
{
    public static class TransformExt
    {
        //------------------------------------------------------------------------------------------------------------
        public static void LocalReset(this Transform lTransform)
        {
            lTransform.localPosition = Vector3.zero;
            lTransform.localRotation = Quaternion.identity;
            lTransform.localScale = Vector3.one;
        }

        //------------------------------------------------------------------------------------------------------------
        public static void Align(this Transform lTransform, Transform lTarget)
        {
            lTransform.position = lTarget.position;
            lTransform.rotation = lTarget.rotation;
        }
    }
}
