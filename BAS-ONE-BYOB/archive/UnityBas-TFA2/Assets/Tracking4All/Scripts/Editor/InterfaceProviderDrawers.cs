// InterfaceProviderDrawers
// (C) 2024 G8gaming Ltd.
using UnityEditor;
using UnityEngine;

namespace Tracking4All
{
    // Bunch of property drawers for derived types.
    [CustomPropertyDrawer(typeof(AdapterSettingsProvider))]
    public class AdapterSettingsProviderUIE : InterfaceProviderUIE
    {
        public override void UpdateMessageOnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DefaultInterfaceProviderMessageUpdate(position, property, label);
        }
    }

    [CustomPropertyDrawer(typeof(PuppetJointProvider<,>))]
    public class JointProviderUIE : InterfaceProviderUIE
    {
        public override void UpdateMessageOnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DefaultProviderMessageUpdate(position, property, label);
        }
    }

    [CustomPropertyDrawer(typeof(LandmarkProvider<>))]
    public class LandmarkProviderUIE : InterfaceProviderUIE
    {
        public override void UpdateMessageOnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DefaultProviderMessageUpdate(position, property, label);
        }
    }

    [CustomPropertyDrawer(typeof(NormalizedLandmarkProvider<>))]
    public class NormalizedLandmarkProviderUIE : InterfaceProviderUIE
    {
        public override void UpdateMessageOnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DefaultProviderMessageUpdate(position, property, label);
        }
    }
}