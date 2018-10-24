
using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

namespace UnityExtension
{
    public static class Texture2DExt
    {
        //------------------------------------------------------------------------------------------------------------
        public static byte[] EncodeToTGA(this Texture2D lTexture)
        {
            MemoryStream lDataStream = new MemoryStream(18 + (lTexture.width * lTexture.height * 3));
            BinaryWriter lDataWriter = new BinaryWriter(lDataStream);

            if (lDataWriter != null)
            {
                lDataWriter.Write((short)0);
                lDataWriter.Write((byte)2);
                lDataWriter.Write((int)0);
                lDataWriter.Write((int)0);
                lDataWriter.Write((byte)0);
                lDataWriter.Write((short)lTexture.width);
                lDataWriter.Write((short)lTexture.height);
                lDataWriter.Write((byte)24);
                lDataWriter.Write((byte)0);

                Color32[] lPixelData = lTexture.GetPixels32();
                for (int lCount = 0; lCount < lPixelData.Length; ++lCount)
                {
                    lDataWriter.Write(lPixelData[lCount].b);
                    lDataWriter.Write(lPixelData[lCount].g);
                    lDataWriter.Write(lPixelData[lCount].r);
                }
            }

            return lDataStream.GetBuffer();
        }

        //------------------------------------------------------------------------------------------------------------
        public static void ConvertLightmapToMobile(this Texture2D lTexture)
        {
            Color[] lColorData = lTexture.GetPixels();
            for (int lCount = 0; lCount < lColorData.Length; ++lCount)
            {
                lColorData[lCount] = (lColorData[lCount] * (8f * lColorData[lCount].a)) * 0.5f;
            }
            lTexture.SetPixels(lColorData);
            lTexture.Apply();
            lColorData = null;
        }
    }
}