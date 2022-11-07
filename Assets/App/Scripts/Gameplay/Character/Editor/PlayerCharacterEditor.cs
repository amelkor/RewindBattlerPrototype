using System;
using UnityEditor;
using UnityEngine;

namespace Game.Gameplay.Character
{
    [Obsolete("Was intended for early prototyping, get rid of this")]
    [CustomEditor(typeof(PlayerCharacter))]
    public class PlayerCharacterEditor : Editor
    {
        private const string BODY_RENDERER_PROP = "bodyRenderer";
        private readonly GUIContent _characterColorLabel = new("Character Color");

        private SerializedProperty _bodyRendererProp;
        private PlayerCharacter _target;
        private Color _bodyColor;

        private void OnEnable()
        {
            _target = (PlayerCharacter)target;
            _bodyRendererProp = serializedObject.FindProperty(BODY_RENDERER_PROP);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Extra settings");
            
            var renderer = (SkinnedMeshRenderer)_bodyRendererProp?.objectReferenceValue;
            if (renderer)
            {
                EditorGUI.BeginChangeCheck();
                _bodyColor = EditorGUILayout.ColorField(_characterColorLabel, renderer.sharedMaterial.color);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(renderer.sharedMaterial, "Change Character color");
                    _target.SetBodyColor(_bodyColor);
                }
            }
        }
    }
}