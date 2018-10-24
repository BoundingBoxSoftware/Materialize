
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

namespace UnityExtension
{
    public static class GameObjectExt
    {
        //------------------------------------------------------------------------------------------------------------
        public static T GetOrAddComponent<T>(this GameObject lGo) where T : Component
        {
            T lComponent = lGo.GetComponent<T>();
            if (lComponent == null)
            {
                lComponent = lGo.gameObject.AddComponent<T>();
            }
            return lComponent;
        }

        //------------------------------------------------------------------------------------------------------------
        public static T GetComponentOfChild<T>(this GameObject lGo, string lName) where T : Component
        {
            T lComponent = null;
            Transform lTransform = lGo.transform.Find(lName);
            if (lTransform != null)
            {
                lComponent = lTransform.GetComponent<T>();
            }
            return lComponent;
        }

        //------------------------------------------------------------------------------------------------------------
        public static T[] GetComponentsOfChild<T>(this GameObject lGo, string lName) where T : Component
        {
            T[] lComponent = null;
            Transform lTransform = lGo.transform.Find(lName);
            if (lTransform != null)
            {
                lComponent = lTransform.GetComponents<T>();
            }
            return lComponent;
        }

        //------------------------------------------------------------------------------------------------------------
        public static GameObject InstantiateAsChild(this GameObject lGo, GameObject lObject)
        {
            GameObject lNewGo = lObject != null ? (GameObject)GameObject.Instantiate(lObject) : new GameObject();
            lNewGo.transform.parent = lGo.transform;
            lNewGo.transform.localPosition = Vector3.zero;
            lNewGo.transform.localRotation = Quaternion.identity;
            lNewGo.transform.localScale = Vector3.one;
            return lNewGo;
        }

        //------------------------------------------------------------------------------------------------------------
        public static bool DestroyChildIfExists(this GameObject lGo, string lName)
        {
            Transform lTransform = lGo.transform.Find(lName);
            if (lTransform != null)
            {
                GameObject.Destroy(lTransform.gameObject);
                return true;
            }
            else
            {
                return false;
            }
        }

        //------------------------------------------------------------------------------------------------------------
        public static bool ContainsChildren(this GameObject lGo, params string[] lPaths)
        {
            for (int lCount = 0; lCount < lPaths.Length; ++lCount)
            {
                if (lGo.transform.Find(lPaths[lCount]) == null)
                {
                    return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------------------------------------------------------
        public static bool IsChildOf(this GameObject lGo, GameObject lParent)
        {
            Transform lTransform = lGo.transform;
            while (lTransform.parent != null)
            {
                if (lTransform == lParent.transform)
                {
                    return true;
                }
                lTransform = lTransform.parent;
            }
            return false;
        }

        //------------------------------------------------------------------------------------------------------------
        public static void SetLayerRecursively(this GameObject lGo, int lLayer)
        {
            lGo.layer = lLayer;
            foreach (Transform lTransform in lGo.transform)
            {
                lTransform.gameObject.SetLayerRecursively(lLayer);
            }
        }
    }
}
