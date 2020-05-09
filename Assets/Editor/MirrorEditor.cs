using UnityEditor;
using UnityEngine;
using UI.Effect;

namespace UIEditor.Effect
{
    [CustomEditor(typeof(Mirror), true)]
    [CanEditMultipleObjects]
    public class MirrorEditor : Editor
    {
        protected SerializedProperty m_MirrorType;

        private GUIContent m_CorrectButtonContent;
        private GUIContent m_MirrorTypeContent;

        protected virtual void OnDisable()
        {
            
        }

        protected virtual void OnEnable()
        {
            m_CorrectButtonContent = new GUIContent("Set Native Size", "Sets the size to match the content.");

            m_MirrorTypeContent = new GUIContent("Mirror Type");

            m_MirrorType = serializedObject.FindProperty("m_MirrorType");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_MirrorType, m_MirrorTypeContent);

            if (GUILayout.Button(m_CorrectButtonContent, EditorStyles.miniButton))
            {
                int len = targets.Length;

                for (int i = 0; i < len; i++)
                {
                    if (targets[i] is Mirror)
                    {
                        Mirror mirror = targets[i] as Mirror;

                        Undo.RecordObject(mirror.rectTransform, "Set Native Size");
                        mirror.SetNativeSize();
                        EditorUtility.SetDirty(mirror);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
