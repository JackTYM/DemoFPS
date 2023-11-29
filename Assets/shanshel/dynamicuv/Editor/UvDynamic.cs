using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace shanshel.duv
{
    [CustomEditor(typeof(MeshRenderer))] 
    public class UvDynamic : Editor
    {

        MeshRenderer targetRenderer;
        MeshFilter targetMeshFilter;

    
        bool isMeshAlreadyGeneratedByTool = false;
        bool canShowApplyButton = false;
        bool canShowAlert = false;
        bool isUVPointsSupported = false;
        //Paths
        string pathToResources = "Assets/Resources/";
        string pathToResourcesDev, pathToResourcesPackage, 
            pathToCurrentFolder, pathToEditorResource, pathToResourceTextureFolder, 
            pathToTextureFile;
        string devName = "shanshel";
        string packageName = "dynamicuv";
      


        //Abs path
        string absPath_app, absPath_current,
            absPath_resource, absPath_resource_package,
            absPath_textureFolder, absPath_textureFile,
            absPath_editorResources, absPath_textureJsonFile,
            absPath_meshJsonDataFolder, 
            absPath_meshJsonDataFile;

        string textureFileName = "shanshel_shared_texture.png";
        string colorJsonFileName = "shared_texture.json";
        string sharedMaterialName = "shared_shanshel.mat";


        bool needToRefresh = false;
        UvMesh _tempMeshData = new UvMesh();

        UvTextureData uvTextureData;
        bool isInited = false;

        //Some data 
        public static int colorSize = 2;
        public static int textureSize = 256;
        public static int devidedSize = 128;

        private void Awake()
        {
           

            DefineTargets();
            if (!canBeUsedWithSelectedTarget()) return;

            canShowApplyButton = true;
            isMeshAlreadyGeneratedByTool = IsMeshAlreadyGeneratedByTool();

            PreparePaths();
            PrepareFolders();

            if (needToRefresh) AssetDatabase.Refresh();


            UvTextureData.LoadTexture(pathToTextureFile, textureFileName.Replace(".png", ""));
            LoadColorJsonFile();
            LoadMeshJson();


        }


        //Prepare
        void DefineTargets()
        {
            targetRenderer = (MeshRenderer)target;
            targetMeshFilter = targetRenderer.gameObject.GetComponent<MeshFilter>();

        }

        void PreparePaths()
        {
            var dynamicUVGUID = AssetDatabase.FindAssets("UvDynamic t:Script");
            pathToCurrentFolder = AssetDatabase.GUIDToAssetPath(dynamicUVGUID[0]).Replace("UvDynamic.cs", "");
            pathToCurrentFolder = pathToCurrentFolder.Substring(0, pathToCurrentFolder.Length - 7);
            pathToResourcesDev = pathToResources + devName + "/";
            pathToResourcesPackage = pathToResourcesDev + packageName + "/";
            pathToResourceTextureFolder = pathToResourcesPackage + "textures/";
            pathToTextureFile = pathToResourceTextureFolder + textureFileName;
            pathToEditorResource = pathToCurrentFolder + "Editor/Resources/";

            absPath_current = (Application.dataPath + "/" + pathToCurrentFolder.Substring(7)).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            absPath_app = (Application.dataPath + "/").Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            absPath_resource = (absPath_app + "Resources/").Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            absPath_resource_package = (absPath_resource + devName + "/" + packageName + "/").Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            absPath_textureFolder = (absPath_resource_package + "textures/").Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            absPath_textureFile = (absPath_textureFolder + textureFileName).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            absPath_editorResources = (absPath_current + "Editor/Resources/").Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            absPath_textureJsonFile = absPath_editorResources + colorJsonFileName;
            absPath_meshJsonDataFolder = (absPath_editorResources + "meshes/").Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar); ;
            absPath_meshJsonDataFile = absPath_meshJsonDataFolder + targetMeshFilter.sharedMesh.name + ".json";



            //absPath_texture = (indielanResourcePath + "uv/" + textureFileName).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        }
        void PrepareFolders()
        {

            if (!Directory.Exists(pathToResources))
            {
                Directory.CreateDirectory(pathToResources);
                needToRefresh = true;
            }
            if (!Directory.Exists(pathToResourcesDev))
            {
                Directory.CreateDirectory(pathToResourcesDev);
                needToRefresh = true;
            }
            if (!Directory.Exists(pathToResourcesPackage))
            {
                Directory.CreateDirectory(pathToResourcesPackage);
                needToRefresh = true;
            }
            if (!Directory.Exists(pathToResourcesPackage + "textures" + Path.DirectorySeparatorChar))
            {
                Directory.CreateDirectory(pathToResourcesPackage + "textures" + Path.DirectorySeparatorChar);
                needToRefresh = true;
            }

            if (!Directory.Exists(pathToResourcesPackage + "models" + Path.DirectorySeparatorChar))
            {
                Directory.CreateDirectory(pathToResourcesPackage + "models" + Path.DirectorySeparatorChar);
                needToRefresh = true;
            }

            if (!Directory.Exists(pathToEditorResource))
            {
                Directory.CreateDirectory(pathToEditorResource);
                needToRefresh = true;
            }

            if (!Directory.Exists(pathToEditorResource + "meshes" + Path.DirectorySeparatorChar))
            {
                Directory.CreateDirectory(pathToEditorResource + "meshes" + Path.DirectorySeparatorChar);
                needToRefresh = true;
            }

        }

     


        //Draw
        public override void OnInspectorGUI()
        {
            if (canShowApplyButton)
            {
                if (!isMeshAlreadyGeneratedByTool)
                {
                    if (GUILayout.Button("Apply Mesh & Material")) ApplyDynamicUV();
                }
            }

            if (canShowAlert)
            {
              
                if (!isUVPointsSupported) GUILayout.Label("This mesh not supported as it contains " + _tempMeshData.groups.Count + " UV Points");
            }

            if (isMeshAlreadyGeneratedByTool)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                for (var i = 0; i < _tempMeshData.colors.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _tempMeshData.colors[i].color = EditorGUILayout.ColorField(_tempMeshData.colors[i].color);
                    EditorGUILayout.EndHorizontal();
                    if (_tempMeshData.colors[i].oldColor != _tempMeshData.colors[i].color)
                    {
                        _tempMeshData.colors[i].oldColor = _tempMeshData.colors[i].color;
                        OnColorChange(i);
                    }
                }

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }



            DrawDefaultInspector();
        }



        void ApplyDynamicUV(bool isColorChange = false)
        {
            UvTextureData.InitTexutre(pathToResourcesPackage, absPath_textureFile);
            UvTextureData.LoadTexture(pathToTextureFile, textureFileName.Replace(".png", ""));
           
            CreateColorJsonFile();
            LoadColorJsonFile();
            if (!isColorChange)
                AddToColorJsonFile();


            CreateInternalMesh();
            CreateMaterial();

            CreateMeshJson();
            LoadMeshJson();




        }

        void OnColorChange(int colorIndex)
        {
            if (!isInited)
            {
                isInited = true;
                ApplyDynamicUV(true);
            }

            UvTextureData.CreateColor(_tempMeshData.colors[colorIndex].indexOnTexture, _tempMeshData.colors[colorIndex].color);

          
            ChangeUV(colorIndex);
            UvTextureData.SaveTexture(absPath_textureFile);
            uvTextureData.colors[_tempMeshData.colors[colorIndex].indexOnTexture] = _tempMeshData.colors[colorIndex].color;

            //UvTextureData.InitTexutre(pathToResourcesPackage, absPath_textureFile, pathToTextureFile, textureFileName.Replace(".png", ""));
        }

        void CreateInternalMesh()
        {
            Mesh meshToSave;


            if (absPath_meshJsonDataFile.Contains("_shanshel_"))
            {
                meshToSave = AssetDatabase.LoadAssetAtPath<Mesh>(pathToResourcesPackage + "models/" + targetMeshFilter.sharedMesh.name + ".asset");
            }
            else
            {
                meshToSave = AssetDatabase.LoadAssetAtPath<Mesh>(pathToResourcesPackage + "models/" + targetMeshFilter.sharedMesh.name + "_shanshel_.asset");
            }

            if (meshToSave == null)
            {
                meshToSave = Instantiate(targetMeshFilter.sharedMesh) as Mesh;
                AssetDatabase.CreateAsset(meshToSave, pathToResourcesPackage + "models/" + targetMeshFilter.sharedMesh.name + "_shanshel_.asset");
            }

            
            EditorUtility.SetDirty(targetMeshFilter.gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(targetMeshFilter);
            targetMeshFilter.sharedMesh = meshToSave;

        }

        void CreateMaterial()
        {
            var _foundAssets = AssetDatabase.FindAssets(sharedMaterialName.Replace(".mat", "") + " t:Material", new[] { pathToResourcesPackage });

            if (_foundAssets.Length == 0)
            {
      
                Shader _shader = Shader.Find("Standard");
                if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
                    _shader = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.defaultShader;


                Material _mat = new Material(_shader);
                _mat.mainTexture = UvTextureData.texture;
                _mat.SetFloat("_Glossiness", 0f);
                _mat.SetFloat("_Smoothness", 0f);
                AssetDatabase.CreateAsset(_mat, pathToResourcesPackage + sharedMaterialName);
                AssetDatabase.Refresh();
            }


            Material _materialToUse = Resources.Load<Material>("shanshel/dynamicuv/" + sharedMaterialName.Replace(".mat", ""));
            targetRenderer.sharedMaterial = _materialToUse;
        }


        bool isCreatingNewJson = false;
        void CreateColorJsonFile()
        {
            if (!File.Exists(absPath_textureJsonFile))
            {
                var _uvTextureData = new UvTextureData();

                for (var i = 0; i < _tempMeshData.colors.Count; i++)
                {
                    UvTextureData.CreateColor(i, Color.black);
                    _uvTextureData.colors.Add(Color.black);
                }



                StreamWriter writer = new StreamWriter(absPath_textureJsonFile);
                writer.WriteLine(JsonUtility.ToJson(_uvTextureData));
                writer.Close();

                isCreatingNewJson = true;
                AssetDatabase.ImportAsset(pathToEditorResource + colorJsonFileName);

            }
      

        }

        void UpdateColorJson()
        {
            if (isMeshAlreadyGeneratedByTool && uvTextureData != null)
            {
                StreamWriter writer = new StreamWriter(absPath_textureJsonFile);
                writer.WriteLine(JsonUtility.ToJson(uvTextureData));
                writer.Close();
            }
        }

        void LoadColorJsonFile()
        {
            if (Resources.Load<TextAsset>("shared_texture") != null)
                uvTextureData = JsonUtility.FromJson<UvTextureData>(Resources.Load<TextAsset>("shared_texture").text);
        }

        void AddToColorJsonFile()
        {
            if (!isCreatingNewJson && !isMeshAlreadyGeneratedByTool && uvTextureData != null)
            {
             

                for (var i = 0; i < _tempMeshData.colors.Count; i++)
                {
                    uvTextureData.colors.Add(Color.black);
                }
     
                StreamWriter writer = new StreamWriter(absPath_textureJsonFile);
                writer.WriteLine(JsonUtility.ToJson(uvTextureData));
                writer.Close();
            }
        }


        void CreateMeshJson()
        {
            if (!File.Exists(absPath_meshJsonDataFile.Replace("_shanshel_", "")))
            {
                _tempMeshData.name = targetMeshFilter.sharedMesh.name;
               
                for (var i = 0; i < _tempMeshData.colors.Count; i++)
                {
                    _tempMeshData.colors[i].indexOnTexture = (uvTextureData.colors.Count - _tempMeshData.colors.Count) + i;
                }


                StreamWriter writer2 = new StreamWriter(absPath_meshJsonDataFile.Replace("_shanshel_", ""));
                writer2.WriteLine(JsonUtility.ToJson(_tempMeshData));
                writer2.Close();
                AssetDatabase.Refresh();
                AssetDatabase.ImportAsset(pathToEditorResource + "meshes/" + targetMeshFilter.sharedMesh.name.Replace("_shanshel_", "") + ".json");

            }
        }


        void UpdateMeshJson()
        {
            if (isMeshAlreadyGeneratedByTool)
            {
                if (_tempMeshData.colors.Count == 0) return;
                StreamWriter writer2 = new StreamWriter(absPath_meshJsonDataFile.Replace("_shanshel_", ""));
                writer2.WriteLine(JsonUtility.ToJson(_tempMeshData));
                writer2.Close();
            }
        }

        void LoadMeshJson()
        {
            if (Resources.Load<TextAsset>("meshes/" + targetMeshFilter.sharedMesh.name.Replace("_shanshel_", "")) != null)
            {
                _tempMeshData = JsonUtility.FromJson<UvMesh>(Resources.Load<TextAsset>("meshes/" + targetMeshFilter.sharedMesh.name.Replace("_shanshel_", "")).text);
            }

        }
        //Change UV 

        void ChangeUV(int colorIndex)
        {

     
            var indexOnTexture = _tempMeshData.colors[colorIndex].indexOnTexture;

            float _xPos = Mathf.Repeat(indexOnTexture, devidedSize);
            float _yPos = Mathf.FloorToInt(indexOnTexture / devidedSize);


            _xPos /= devidedSize;
            _yPos /= devidedSize;

            _xPos += 0.00390625f;
            _yPos += 0.00390625f;

     

            List<Vector2> _uvs = new List<Vector2>();
            int _count = _tempMeshData.groups[colorIndex].points.Count;
            List<int> indexes = _tempMeshData.groups[colorIndex].points;
            for (var _i = 0; _i < targetMeshFilter.sharedMesh.vertexCount; _i++)
            {
                if (indexes.Contains(_i))
                {
                    _uvs.Add(new Vector2(_xPos, _yPos));
                }
                else
                {
                    _uvs.Add(targetMeshFilter.sharedMesh.uv[_i]);
                }

            }

            targetMeshFilter.sharedMesh.SetUVs(0, _uvs.ToArray());
            targetMeshFilter.sharedMesh.uv = _uvs.ToArray();
            targetMeshFilter.sharedMesh.RecalculateNormals();
            targetMeshFilter.sharedMesh.RecalculateBounds();
            targetMeshFilter.sharedMesh.Optimize();

        }


        //Check requirement before allow to use with this target
        bool canBeUsedWithSelectedTarget()
        {
            if (!hasValidMeshFilter()) return false;
            canShowAlert = true;
            if (!hasLowPointGroupCount()) return false;
            return true;
        }
   
        bool hasValidMeshFilter()
        {
           
           
            if (targetMeshFilter == null) return false;
            if (targetMeshFilter.sharedMesh == null) return false;

            return true;
        }

        bool hasLowPointGroupCount()
        {

            if (targetMeshFilter.sharedMesh.name.Contains("_shanshel_")) {
                isUVPointsSupported = true;
                return true;
            }

            Vector2[] _uvs = targetMeshFilter.sharedMesh.uv;
            


            for (var i = 0; i < _uvs.Length; i++)
            {

                bool makeANewPointGroup = true;
                for (var ai = 0; ai < _tempMeshData.groups.Count; ai++)
                {
                    Vector2 _savedPoint = _tempMeshData.groups[ai].groupVector;

                    if (Vector2.Distance(_savedPoint, _uvs[i]) < 0.02f)
                    {
                        _tempMeshData.groups[ai].points.Add(i);
                        makeANewPointGroup = false;
                        break;
                    }
                }

                if (!makeANewPointGroup) continue;
                Vector2 _point = _uvs[i];
                UvPointGroup _pointGroup = new UvPointGroup();
                _pointGroup.points.Add(i);
                _pointGroup.groupVector = _point;
                _tempMeshData.groups.Add(_pointGroup);
                _tempMeshData.colors.Add(new UvColor { color= Color.black, oldColor = Color.black });
  

            }
            


            if (_tempMeshData.groups.Count > 12) return false;

            isUVPointsSupported = true;
            return true;
        }

        bool IsMeshAlreadyGeneratedByTool()
        {
            return targetMeshFilter.sharedMesh.name.Contains("_shanshel_");
        }




        private void OnDisable()
        {
            UpdateColorJson();
            UpdateMeshJson();
            UvTextureData.SaveTexture(absPath_textureFile);
            AssetDatabase.Refresh();
        }
    }

}
