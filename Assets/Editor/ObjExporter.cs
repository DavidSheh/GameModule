using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

// 将 Mesh 导出成 obj 格式模型
public class ObjExporter : EditorWindow {

    [MenuItem("Tools/ObjExporter")]
    private static void ExportMesh()
    {
        GameObject go = Selection.activeGameObject;
        List<Material> mats = new List<Material>();
        string dir = Path.Combine(Application.dataPath, "ObjExport/" + go.name);
        Directory.CreateDirectory(dir);
        BuildObj(go, ref mats, dir);
        BuildMtl(mats, dir, go.name);
    }

    private static void BuildObj(GameObject go, ref List<Material> mtls, string dir)
    {
        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
        StringBuilder vs = new StringBuilder("mtllib " + go.name + ".mtl").AppendLine();
        StringBuilder vts = new StringBuilder();
        StringBuilder vns = new StringBuilder();
        StringBuilder fs = new StringBuilder();

        int o = 1;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            var mf = meshFilters[i];
            var m = mf.sharedMesh;
            if (mf.gameObject.GetComponent<Renderer>() == null)
            {
                continue;
            }
            var mats = mf.gameObject.GetComponent<Renderer>().sharedMaterials;
            for (int j = 0; j < m.vertexCount; j++) 
            {
                var v = m.vertices[j];
                v = mf.transform.TransformPoint(v);
                vs.AppendFormat("v {0} {1} {2}", v.x, v.y, v.z).AppendLine(); // 顶点数据
                v = m.normals[j];
                vns.AppendFormat("vn {0} {1} {2}", v.x, v.y, v.z).AppendLine(); // 顶点法线数据
                v = m.uv[j];
                vts.AppendFormat("vt {0} {1}", v.x, v.y).AppendLine(); // 贴图坐标数据
            }
            for (int u = 0; u < m.subMeshCount; u++)
            {
                var mat = mats[u];
                if (!mtls.Contains(mat))
                {
                    mtls.Add(mat);
                }
                fs.AppendFormat("usemtl {0}", mat.name).AppendLine();
                var tr = m.GetTriangles(u);
                for (int k = 0; k < tr.Length; k += 3)
                {
                    fs.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                        tr[k] + o, tr[k + 1] + o, tr[k + 2] + o).AppendLine(); // 网格面数据
                }
            }
            o += m.vertexCount;
        }

        StringBuilder meshBuilder = new StringBuilder();
        meshBuilder.Append(vs.ToString());
        meshBuilder.Append(vns.ToString());
        meshBuilder.Append(vts.ToString());
        meshBuilder.Append(fs.ToString());

        using (StreamWriter sw = new StreamWriter(Path.Combine(dir, go.name + ".obj"), false))
        {
            sw.Write(meshBuilder.ToString());
        }
    }

    private static void BuildMtl(List<Material> mats, string dir, string name)
    {
        StringBuilder mtl = new StringBuilder();
        foreach (Material m in mats)
        {
            mtl.AppendFormat("newmtl {0}", m.name).AppendLine();
            if (m.HasProperty("_Color"))
            {
                Color c = m.GetColor("_Color");
                mtl.AppendFormat("Kd {0} {1} {2}", c.r, c.g, c.b).AppendLine();
            }
            if (m.HasProperty("_MainTex"))
            {
                string assetPath = AssetDatabase.GetAssetPath(m.GetTexture("_MainTex"));
                string texName = Path.GetFileName(assetPath);
                string exportPath = Path.Combine(dir, texName);
                mtl.AppendFormat("map_Kd {0}", texName).AppendLine();
                if (!File.Exists(exportPath))
                {
                    File.Copy(assetPath, exportPath);
                }
            }

        }
        using (StreamWriter sw = new StreamWriter(Path.Combine(dir, name + ".mtl"), false))
        {
            sw.Write(mtl.ToString());
        }
    }
 }