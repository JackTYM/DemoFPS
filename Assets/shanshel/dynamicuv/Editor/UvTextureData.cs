using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace shanshel.duv
{
    [SerializeField]
    public class UvTextureData
    {
        public List<Color> colors = new List<Color>();
        public static Texture2D texture;
        public bool IsInited = false;
    
        public static void CreateColor(int colorIndex, Color colorToCreate)
        {

            int yEvery = Mathf.FloorToInt(256 / UvDynamic.colorSize);
            int _yRealIndex = Mathf.FloorToInt(colorIndex / yEvery);

            for (var i = colorIndex * UvDynamic.colorSize; i < (colorIndex + 1) * UvDynamic.colorSize; i++)
            {
                for (var y = _yRealIndex * UvDynamic.colorSize; y < (_yRealIndex + 1) * UvDynamic.colorSize; y++)
                {
                    texture.SetPixel(i, y, colorToCreate);
                }
            }
        }

        public static void SaveTexture(string texturePath)
        {
         
            if (texture == null || texturePath == null)
            {

                return;
            }
            texture.Apply();
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(texturePath, bytes);
            AssetDatabase.Refresh();
        
        }


        public static void InitTexutre(string relResourcePath, string absFilePath)
        {

            var _findAssets = AssetDatabase.FindAssets("shanshel_shared_texture t:texture2D", new[] { relResourcePath });
            if (_findAssets.Length == 0)
            {

                texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;

                texture.Apply();
                byte[] bytes = texture.EncodeToPNG();


                File.WriteAllBytes(absFilePath, bytes);

                AssetDatabase.Refresh();
            }


           


        }


        public static void LoadTexture(string relFilePath, string fileName)
        {
            texture = Resources.Load<Texture2D>("shanshel/dynamicuv/textures/" + fileName);
 


            if (texture != null && !texture.isReadable)
            {
                var tImporter = AssetImporter.GetAtPath(relFilePath) as TextureImporter;

                if (tImporter != null)
                {
                    tImporter.textureType = TextureImporterType.Default;
                    tImporter.textureCompression = TextureImporterCompression.Uncompressed;
                    tImporter.isReadable = true;
                    tImporter.filterMode = FilterMode.Point;
                    tImporter.SaveAndReimport();
                }
            }
        }



    }

}
