using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/*
How to use?
    1. Put this script in your "Editor" folder.
    2. Select the menu item from "Tools->Export".
*/

struct ObjMaterial
{
    public string name;
    public string textureName;
}

public class ObjExporter : EditorWindow
{
    public List<MeshFilter> meshFilters;
    public static string targetFolder = "ExportedObj";

    protected SerializedObject serializedObject;
    protected SerializedProperty assetLstProperty;

    private Vector2 scrollPos;

    ObjExporter()
    {
        titleContent = new GUIContent("ObjExporter");
    }

    [MenuItem("Tools/Export/ObjExporter Window")]
    static void CreateWindows()
    {
        GetWindow(typeof(ObjExporter));
    }

    void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        assetLstProperty = serializedObject.FindProperty("meshFilters");

        targetFolder = Application.dataPath + "/" + targetFolder;
    }

    void OnGUI()
    {
        serializedObject.Update();
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
        GUILayout.BeginVertical();
        EditorGUILayout.HelpBox("Please select the MeshFilter of the mesh you want to export.", MessageType.Info);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(assetLstProperty, new GUIContent("MeshFilters : "), true);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUILayout.HelpBox("Please input the target folder.", MessageType.Info);
        targetFolder = EditorGUILayout.TextField(new GUIContent("TargetFolder : "), targetFolder);
        EditorGUILayout.HelpBox("The OBJs will be saved in your project root directory.", MessageType.Info);
        if (GUILayout.Button("Export OBJs"))
        {
            ExportOBJsByObjExporterWindow();
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    void ExportOBJsByObjExporterWindow()
    {
        if (!CreateTargetFolder())
            return;

        if (meshFilters.Count == 0)
        {
            EditorUtility.DisplayDialog("No MeshFilters in the list!", "Please select one or more MeshFilters", "OK");
            return;
        }

        int exportedObjects = 0;

        for (int i = 0; i < meshFilters.Count; i++)
        {
            exportedObjects++;
            MeshToFile(meshFilters[i], targetFolder, meshFilters[i].name);
        }

        if (exportedObjects > 0)
            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "OK");
        else
            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "OK");

        meshFilters.Clear();
        GetWindow(typeof(ObjExporter)).Repaint();
    }

    private static int vertexOffset = 0;
    private static int normalOffset = 0;
    private static int uvOffset = 0;

    private static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList)
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 lv in m.vertices)
        {
            Vector3 wv = mf.transform.TransformPoint(lv);
            sb.Append(string.Format("v {0} {1} {2}\n", -wv.x, wv.y, wv.z));
        }
        sb.Append("\n");

        foreach (Vector3 lv in m.normals)
        {
            Vector3 wv = mf.transform.TransformDirection(lv);

            sb.Append(string.Format("vn {0} {1} {2}\n", -wv.x, wv.y, wv.z));
        }
        sb.Append("\n");

        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }

        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            sb.Append("usemap ").Append(mats[material].name).Append("\n");

            try
            {
                ObjMaterial objMaterial = new ObjMaterial
                {
                    name = mats[material].name
                };

                if (mats[material].mainTexture)
                    objMaterial.textureName = AssetDatabase.GetAssetPath(mats[material].mainTexture);
                else
                    objMaterial.textureName = null;

                materialList.Add(objMaterial.name, objMaterial);
            }
            catch (ArgumentException)
            {

            }


            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                    triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
            }
        }

        vertexOffset += m.vertices.Length;
        normalOffset += m.normals.Length;
        uvOffset += m.uv.Length;

        return sb.ToString();
    }

    private static void Clear()
    {
        vertexOffset = 0;
        normalOffset = 0;
        uvOffset = 0;
    }

    private static Dictionary<string, ObjMaterial> PrepareFileWrite()
    {
        Clear();
        return new Dictionary<string, ObjMaterial>();
    }

    private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string folder, string filename)
    {
        using (StreamWriter sw = new StreamWriter(folder + Path.DirectorySeparatorChar + filename + ".mtl"))
        {
            foreach (KeyValuePair<string, ObjMaterial> kvp in materialList)
            {
                sw.Write("\n");
                sw.Write("newmtl {0}\n", kvp.Key);
                sw.Write("Ka  0.6 0.6 0.6\n");
                sw.Write("Kd  0.6 0.6 0.6\n");
                sw.Write("Ks  0.9 0.9 0.9\n");
                sw.Write("d  1.0\n");
                sw.Write("Ns  0.0\n");
                sw.Write("illum 2\n");

                if (kvp.Value.textureName != null)
                {
                    string destinationFile = kvp.Value.textureName;


                    int stripIndex = destinationFile.LastIndexOf(Path.DirectorySeparatorChar);

                    if (stripIndex >= 0)
                        destinationFile = destinationFile.Substring(stripIndex + 1).Trim();


                    string relativeFile = destinationFile;

                    destinationFile = folder + Path.DirectorySeparatorChar + destinationFile;

                    Debug.Log("Copying texture from " + kvp.Value.textureName + " to " + destinationFile);

                    try
                    {
                        File.Copy(kvp.Value.textureName, destinationFile);
                    }
                    catch
                    {

                    }


                    sw.Write("map_Kd {0}", relativeFile);
                }

                sw.Write("\n\n\n");
            }
        }
    }

    private static void MeshToFile(MeshFilter mf, string folder, string filename)
    {
        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

        using (StreamWriter sw = new StreamWriter(folder + Path.DirectorySeparatorChar + filename + ".obj"))
        {
            sw.Write("mtllib ./" + filename + ".mtl\n");

            sw.Write(MeshToString(mf, materialList));
        }

        MaterialsToFile(materialList, folder, filename);
    }

    private static void MeshesToFile(MeshFilter[] mf, string folder, string filename)
    {
        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

        using (StreamWriter sw = new StreamWriter(folder + Path.DirectorySeparatorChar + filename + ".obj"))
        {
            sw.Write("mtllib ./" + filename + ".mtl\n");

            for (int i = 0; i < mf.Length; i++)
            {
                sw.Write(MeshToString(mf[i], materialList));
            }
        }

        MaterialsToFile(materialList, folder, filename);
    }

    private static bool CreateTargetFolder()
    {
        try
        {
            Directory.CreateDirectory(targetFolder);
        }
        catch
        {
            EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "OK");
            return false;
        }

        return true;
    }

    [MenuItem("Tools/Export/Quickly export all MeshFilters in selection to separate OBJs")]
    static void ExportSelectionToSeparate()
    {
        if (!CreateTargetFolder())
            return;

        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "OK");
            return;
        }

        int exportedObjects = 0;

        for (int i = 0; i < selection.Length; i++)
        {
            Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

            for (int m = 0; m < meshfilter.Length; m++)
            {
                exportedObjects++;
                MeshToFile((MeshFilter)meshfilter[m], targetFolder, selection[i].name + "_" + i + "_" + m);
            }
        }

        if (exportedObjects > 0)
            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "OK");
        else
            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "OK");
    }

    [MenuItem("Tools/Export/Quickly export each selected to single OBJ")]
    static void ExportEachSelectionToSingle()
    {
        if (!CreateTargetFolder())
            return;

        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "OK");
            return;
        }

        int exportedObjects = 0;

        for (int i = 0; i < selection.Length; i++)
        {
            Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

            MeshFilter[] mf = new MeshFilter[meshfilter.Length];

            for (int m = 0; m < meshfilter.Length; m++)
            {
                exportedObjects++;
                mf[m] = (MeshFilter)meshfilter[m];
            }

            MeshesToFile(mf, targetFolder, selection[i].name + "_" + i);
        }

        if (exportedObjects > 0)
        {
            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "OK");
        }
        else
            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "OK");
    }
}