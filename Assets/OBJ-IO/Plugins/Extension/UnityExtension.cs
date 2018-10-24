
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

namespace UnityExtension
{
    //------------------------------------------------------------------------------------------------------------
    public static class Utils
    {
        //------------------------------------------------------------------------------------------------------------
        public static bool HasKeys(Dictionary<string, object> lData, params string[] lKeys)
        {
            if (lKeys != null)
            {
                for (int lCount = 0; lCount < lKeys.Length; ++lCount)
                {
                    if (!lData.ContainsKey(lKeys[lCount]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //------------------------------------------------------------------------------------------------------------
        public static void ClearChildren(GameObject lGo, string lTarget)
        {
            if (lGo != null)
            {
                Transform lTransform = null;
                for (int lCount = lGo.transform.childCount - 1; lCount > -1; --lCount)
                {
                    lTransform = lGo.transform.GetChild(lCount);
                    if (lTransform.name.Contains(lTarget))
                    {
                        lTransform.parent = null;
                        GameObject.Destroy(lTransform.gameObject);
                    }
                }
            }
        }

        //------------------------------------------------------------------------------------------------------------
        public static void ClearChildrenRegex(GameObject lGo, string lPattern)
        {
            if (lGo != null)
            {
                Transform lTransform = null;
                Regex lRegex = new Regex(lPattern);
                for (int lCount = lGo.transform.childCount - 1; lCount > -1; --lCount)
                {
                    lTransform = lGo.transform.GetChild(lCount);
                    if (lRegex.IsMatch(lTransform.name))
                    {
                        lTransform.parent = null;
                        GameObject.Destroy(lTransform.gameObject);
                    }
                }
            }
        }

        //------------------------------------------------------------------------------------------------------------
        public static void VerifyObjects(string lMsg, params object[] lObjects)
        {
            for (int lCount = 0; lCount < lObjects.Length; ++lCount)
            {
                if (lObjects[lCount] == null)
                {
                    Debug.LogError(lMsg);
                    break;
                }
            }
        }

        //------------------------------------------------------------------------------------------------------------
        public static bool JSONCheck(string lText)
        {
            return !string.IsNullOrEmpty(lText) && lText[0] == '{';
        }

        //------------------------------------------------------------------------------------------------------------
        public static Vector3 ParseVector3Json(string lJsonData)
        {
            string[] lVector3Array = lJsonData.Replace("(", "").Replace(")", "").Replace(" ", "").Split(',');
            Vector3 lVector3 = Vector3.zero;

            if (float.TryParse(lVector3Array[0], out lVector3.x) == false)
            {
                return Vector3.zero;
            }
            if (float.TryParse(lVector3Array[1], out lVector3.y) == false)
            {
                return Vector3.zero;
            }
            if (float.TryParse(lVector3Array[2], out lVector3.z) == false)
            {
                return Vector3.zero;
            }

            return lVector3;
        }

        //------------------------------------------------------------------------------------------------------------
        public static Vector4 ParseVector4Json(string lJsonData)
        {
            string[] lVector4Array = lJsonData.Replace("(", "").Replace(")", "").Replace(" ", "").Split(',');
            Vector4 lVector4 = Vector4.zero;

            if (float.TryParse(lVector4Array[0], out lVector4.x) == false)
            {
                return Vector4.zero;
            }
            if (float.TryParse(lVector4Array[1], out lVector4.y) == false)
            {
                return Vector4.zero;
            }
            if (float.TryParse(lVector4Array[2], out lVector4.z) == false)
            {
                return Vector4.zero;
            }
            if (float.TryParse(lVector4Array[3], out lVector4.w) == false)
            {
                return Vector4.zero;
            }

            return lVector4;
        }

        //------------------------------------------------------------------------------------------------------------
        public static Vector2 ParseVector2String(string lData, char lSeperator = ' ')
        {
            string[] lParts = lData.Split(new char[] { lSeperator }, StringSplitOptions.RemoveEmptyEntries);

            float lX = lParts[0].ParseInvariantFloat();
            float lY = lParts[1].ParseInvariantFloat();

            return new Vector2(lX, lY);
        }

        //------------------------------------------------------------------------------------------------------------
        public static Vector3 ParseVector3String(string lData, char lSeperator = ' ')
        {
            string[] lParts = lData.Split(new char[] { lSeperator }, StringSplitOptions.RemoveEmptyEntries);

            float lX = lParts[0].ParseInvariantFloat();
            float lY = lParts[1].ParseInvariantFloat();
            float lZ = lParts[2].ParseInvariantFloat();

            return new Vector3(lX, lY, lZ);
        }

        //------------------------------------------------------------------------------------------------------------
        public static Vector4 ParseVector4String(string lData, char lSeperator = ' ')
        {
            string[] lParts = lData.Split(new char[] { lSeperator }, StringSplitOptions.RemoveEmptyEntries);

            float lX = lParts[0].ParseInvariantFloat();
            float lY = lParts[1].ParseInvariantFloat();
            float lZ = lParts[2].ParseInvariantFloat();
            float lW = lParts[3].ParseInvariantFloat();

            return new Vector4(lX, lY, lZ, lW);
        }

        //------------------------------------------------------------------------------------------------------------
        public static Quaternion ParseQuaternion(string lJsonData)
        {
            string[] lQuaternionArray = lJsonData.Replace("(", "").Replace(")", "").Replace(" ", "").Split(',');
            Quaternion lQuaternion = Quaternion.identity;

            if (float.TryParse(lQuaternionArray[0], out lQuaternion.x) == false)
            {
                return Quaternion.identity;
            }
            if (float.TryParse(lQuaternionArray[1], out lQuaternion.y) == false)
            {
                return Quaternion.identity;
            }
            if (float.TryParse(lQuaternionArray[2], out lQuaternion.z) == false)
            {
                return Quaternion.identity;
            }
            if (float.TryParse(lQuaternionArray[3], out lQuaternion.w) == false)
            {
                return Quaternion.identity;
            }

            return lQuaternion;
        }

        //------------------------------------------------------------------------------------------------------------
        public static string Vector3String(Vector3 lVector3)
        {
            return "(" +
                lVector3.x.ToString("f3") + "," +
                lVector3.y.ToString("f3") + "," +
                lVector3.z.ToString("f3") +
                ")";
        }

        //------------------------------------------------------------------------------------------------------------
        public static string Vector4String(Vector4 lVector4)
        {
            return "(" +
                lVector4.x.ToString("f3") + "," +
                lVector4.y.ToString("f3") + "," +
                lVector4.z.ToString("f3") + "," +
                lVector4.w.ToString("f3") +
                ")";
        }

        //------------------------------------------------------------------------------------------------------------
        public static string QuaternionString(Quaternion lQuaternion)
        {
            return "(" +
                lQuaternion.x.ToString("f3") + "," +
                lQuaternion.y.ToString("f3") + "," +
                lQuaternion.z.ToString("f3") + "," +
                lQuaternion.w.ToString("f3") +
                ")";
        }

        //------------------------------------------------------------------------------------------------------------
        public static int FirstInt(string lJsonData)
        {
            string lDigits = "";
            for (int lCount = 0; lCount < lJsonData.Length && Char.IsDigit(lJsonData[lCount]); ++lCount)
            {
                lDigits += lJsonData[lCount];
            }
            return int.Parse(lDigits);
        }
    }
}
