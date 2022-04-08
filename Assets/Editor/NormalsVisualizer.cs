using UnityEditor;
using UnityEngine;

// 模型法线可视化
// 使用方法：
// 选中模型对象，在 Inspector 视图中的 MeshFilter 组件下面有个 Show Normals 
// 的选项，勾上后会显示 Normals Length 的输入框，可以输入显示的法线长度，然后
// 将鼠标移动到 Scene 视图中就能看到法线的显示。
[CustomEditor(typeof(MeshFilter))]
public class NormalsVisualizer : Editor
{
    private const string EDITOR_PREF_KEY = "_normals_length";
    private const string EDITOR_PREF_BOOL = "_show_normals";
    private Mesh mesh;
    private MeshFilter mf;
    private Vector3[] verts;
    private Vector3[] normals;
    private float normalsLength = 1f;
    private bool showNormals = false;

    private void OnEnable()
    {
        mf = target as MeshFilter;
        if (mf != null)
        {
            mesh = mf.sharedMesh;
        }

        normalsLength = EditorPrefs.GetFloat(EDITOR_PREF_KEY);
        showNormals = EditorPrefs.GetBool(EDITOR_PREF_BOOL);
    }

    private void OnSceneGUI()
    {
        if (mesh == null || !showNormals)
        {
            return;
        }

        Handles.matrix = mf.transform.localToWorldMatrix;
        Handles.color = Color.yellow;
        verts = mesh.vertices;
        normals = mesh.normals;
        int len = mesh.vertexCount;

        for (int i = 0; i < len; i++)
        {
            Handles.DrawLine(verts[i], verts[i] + normals[i] * normalsLength);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        showNormals = EditorGUILayout.Toggle("Show Normals", showNormals);
        if (showNormals)
        {
            normalsLength = EditorGUILayout.FloatField("Normals Length", normalsLength);
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool(EDITOR_PREF_BOOL, showNormals);
            EditorPrefs.SetFloat(EDITOR_PREF_KEY, normalsLength);
        }
    }
}
