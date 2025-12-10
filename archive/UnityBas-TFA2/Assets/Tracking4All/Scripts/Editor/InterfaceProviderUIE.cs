// LandmarkProviderUIE
// (C) 2024 G8gaming Ltd.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tracking4All
{
    // NOTE: reflection based approach is perhaps not too great in specific cases of duplicate names.

    /// <summary>
    /// Experimental custom status drawer for interface providers.
    /// <para>Currently only works fully when serialized at a surface level.</para>
    /// </summary>
    [CustomPropertyDrawer(typeof(InterfaceProvider<>))]
    public class InterfaceProviderUIE : PropertyDrawer
    {
        public static float LINE_HEIGHT => EditorHelpers.LINE_HEIGHT;
        public static float DOWN_BIAS = 0;
        bool PlayMode => EditorApplication.isPlaying;

        protected SerializedProperty provider;

        private string message = "The selected gameobject does not impliment the interface.";
        private Color messageColor = Color.gray;
        public void SetMessage(string msg, Color msgColor)
        {
            message = msg;
            messageColor = msgColor;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (provider == null)
            {
                provider = property.FindPropertyRelative("provider");
            }

            if (PlayMode)
            {
                return LINE_HEIGHT * 2.1f + DOWN_BIAS * 2f;
            }

            return LINE_HEIGHT * 1.1f + DOWN_BIAS * 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Color c = GUI.backgroundColor;
            GUI.backgroundColor = Color.Lerp(c, messageColor, .5f);

            float y = position.y + DOWN_BIAS;

            /*Rect labelRect = new Rect(position.x, y, position.width, LINE_HEIGHT);
            EditorGUI.LabelField(labelRect, new GUIContent(property.displayName), EditorStyles.label);*/
            Rect propertyFieldRect = new Rect(position.x, y, position.width, LINE_HEIGHT);
            EditorGUI.PropertyField(propertyFieldRect, property.FindPropertyRelative("provider"), new GUIContent(property.displayName));

            Rect messageRect = new Rect(position.x, y + LINE_HEIGHT * 1f, position.width, LINE_HEIGHT);

            if (EditorHelpers.Check(property) && PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                EditorGUI.LabelField(propertyFieldRect,
                    new GUIContent("", "Status: " + message + "\n\nRequires: " + EditorHelpers.Type(property, "?", false)));

                // Set message.
                UpdateMessageOnGUI(position, property, label);

                // Display message.
                if (message != null)
                {
                    var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                    style.normal.textColor = messageColor;
                    style.fontSize = Mathf.CeilToInt(style.fontSize * 1f);
                    style.alignment = TextAnchor.MiddleLeft;
                    style.clipping = TextClipping.Clip;

                    if (PlayMode)
                        EditorGUI.LabelField(messageRect, message, style);
                }
            }
            else
            {
                if (PlayMode)
                    EditorGUI.LabelField(messageRect, "Interface provider.", EditorStyles.centeredGreyMiniLabel);
            }

            GUI.backgroundColor = c;

            EditorGUI.EndProperty();
        }

        public virtual void UpdateMessageOnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DefaultInterfaceProviderMessageUpdate(position, property, label);
        }

        // default implementations deriving from interfaceprovider.
        public virtual void DefaultInterfaceProviderMessageUpdate(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!EditorApplication.isPlaying) EditorHelpers.CallVoid(property, "Reset", false); // do not reset when playing.

            bool hasInterface = (bool)EditorHelpers.Get(property, "HasInterface", false, false);

            if (hasInterface)
            {
                SetMessage("Valid provider assigned.", Color.green);
            }
            else
            {
                SetMessage("Valid provider could not be generated from what is currently assigned.", Color.red);
            }
        }
        public virtual void DefaultProviderMessageUpdate(Rect position, SerializedProperty property, GUIContent label)
        {
            bool u = (bool)EditorHelpers.Get(property, "EditorOnly_HasInterfaceRaw", false, true);
            // Debug.Log(u);
            if (PlayMode && u)
            {
                float f = (float)EditorHelpers.Get(property, "TimeSinceLastUpdate", Mathf.Infinity, true);
                bool isAlive = (bool)EditorHelpers.Get(property, "IsAlive", false, true);// Tracking4All.Instance.IsProviderLive(f);

                if (isAlive)
                {
                    SetMessage("Alive (Last Update " + (f) + "s ago)", Color.green);
                }
                else
                {
                    SetMessage("Not Live (Last Update >" + Tracking4All.Instance.ProviderAliveThreshold + "s ago)", Color.red);
                }

                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
            else
            {
                DefaultInterfaceProviderMessageUpdate(position, property, label);
            }
        }
    }
}