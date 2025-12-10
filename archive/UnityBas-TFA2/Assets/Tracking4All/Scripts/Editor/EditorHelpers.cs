// EditorUtilities
// (C) 2024 G8gaming Ltd.
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace Tracking4All
{
    public static class EditorHelpers
    {
        public static readonly float LINE_HEIGHT = EditorGUIUtility.singleLineHeight;

        public static bool Check(SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            Type t = obj.GetType();
            FieldInfo b = t.GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic); // reference to the target field
            if (b == null)
            {
                b = t.GetField(property.name, BindingFlags.Instance | BindingFlags.Public);
            }
            if (b == null) return false;

            return true;
        }
        public static string Type(SerializedProperty property, string defaultValue, bool requirePlayMode)
        {
            if (requirePlayMode && EditorApplication.isPlaying == false) return defaultValue;

            object obj = property.serializedObject.targetObject;
            Type t = obj.GetType();
            FieldInfo b = t.GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic); // reference to the target field
            if (b == null)
            {
                b = t.GetField(property.name, BindingFlags.Instance | BindingFlags.Public);
            }
            PropertyInfo v = b.FieldType.GetProperty("Provider");
            Type t2 = v.PropertyType;
            return FullHumanReadableTypeName(t2);
        }

        public static string FullHumanReadableTypeName(this Type type)
        {
            // https://stackoverflow.com/questions/3396300/get-type-name-without-full-namespace
            var sb = new StringBuilder();
            var name = type.Name;
            if (!type.IsGenericType) return name;
            sb.Append(name.Substring(0, name.IndexOf('`')));
            sb.Append("<");
            sb.Append(string.Join(", ", type.GetGenericArguments()
                                            .Select(t => t.FullHumanReadableTypeName())));
            sb.Append(">");
            return sb.ToString();
        }

        public static object Get(SerializedProperty property, string variableName, object defaultValue, bool requirePlayMode)
        {
            if (requirePlayMode && EditorApplication.isPlaying == false) return defaultValue;

            object obj = property.serializedObject.targetObject;
            if (obj == null) return defaultValue;
            Type t = obj.GetType();
            FieldInfo b = t.GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic); // reference to the target field
            if (b == null)
            {
                b = t.GetField(property.name, BindingFlags.Instance | BindingFlags.Public);
            }
            if (b == null) return defaultValue;
            object fieldObject = b.GetValue(obj);

            if (fieldObject == null) return defaultValue;

            PropertyInfo v = b.FieldType.GetProperty(variableName);
            if (v == null) return defaultValue;

            object value = v.GetValue(fieldObject);

            return value == null ? defaultValue : value;
        }
        public static void CallVoid(SerializedProperty property, string methodName, bool requirePlayMode)
        {
            if (requirePlayMode && EditorApplication.isPlaying == false) return;

            object obj = property.serializedObject.targetObject;
            Type t = obj.GetType();
            FieldInfo b = t.GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic); // reference to the target field
            if (b == null)
            {
                b = t.GetField(property.name, BindingFlags.Instance | BindingFlags.Public);
            }
            object fieldObject = b.GetValue(obj);
            MethodInfo v = b.FieldType.GetMethod(methodName);

            if (v != null)
            {
                v.Invoke(fieldObject, null);
            }
        }
    }
}