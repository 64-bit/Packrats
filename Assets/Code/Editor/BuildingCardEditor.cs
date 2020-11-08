using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Packrats
{
    [CustomEditor(typeof(BuildingCard))]
    public class BuildingCardEditor : Editor
    {

        SerializedProperty cardHeight;

        void OnEnable()
        {
            cardHeight = serializedObject.FindProperty("CardHeight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var targetCard = target as BuildingCard;

            var oldTexture = targetCard.CardTexture;

            EditorGUILayout.PropertyField(cardHeight);
            targetCard.CardTexture = EditorGUILayout.ObjectField(targetCard.CardTexture, typeof(Texture), false) as Texture;

            if (targetCard.CardTexture != oldTexture)
            {
                //OnTextureChanged
                targetCard.OnTextureChanged(CaveSystemSettings.DefaultSettings);
            }

            targetCard.OnEdited();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}