using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class VariableWatching : MonoBehaviour
{
    private const int maxDepth = 5;
    
    private static readonly HashSet<Type> UnityPrimitiveTypes = new()
    {
        typeof(Vector2), typeof(Vector3), typeof(Vector4),
        typeof(Quaternion), typeof(Color), typeof(Rect),
        typeof(Matrix4x4), typeof(LayerMask)
    };

    public static string Watch(string targetVariable, GameObject targetGameObject)
    {
        if (!targetGameObject) return "No GameObject Attached";
        
        object value = GetVariableValue(targetVariable, targetGameObject);
        string formattedValue = FormatValue(value);
        return formattedValue;
    }

    private static object GetVariableValue(string variableName, GameObject target)
    {
        foreach (Component component in target.GetComponents<Component>())
        {
            Type type = component.GetType();
            FieldInfo field = type.GetField(variableName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            
            if (field != null)
            {
                return field.GetValue(component);
            }
        }
    
        return null;
    }

    #region Format Values 
    
    private static string FormatValue(object value, int depth = 0)
    {
        if (depth > maxDepth) return "...";
        if (value == null) return "Variable not found";

        Type type = value.GetType();
        
        if (type.IsPrimitive || type == typeof(decimal)) return value.ToString();
        if (type == typeof(string)) return $"\"{value}\"";
        if (type.IsEnum) return value.ToString();

        if (UnityPrimitiveTypes.Contains(type) || type.Namespace?.StartsWith("UnityEngine") == true)
        {
            return FormatUnityType(value, depth);
        }

        if (IsDictionary(type))
        {
            return FormatDictionary(value as IDictionary, depth + 1);
        }
        
        if (IsCollection(type))
        {
            return FormatCollection(value as IEnumerable, depth + 1);
        }

        return FormatComplexType(value, depth + 1);
    }

    private static string FormatUnityType(object value, int depth)
    {
        Type type = value.GetType();
        List<string> fields = new();

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            object fieldValue = field.GetValue(value);
            fields.Add($"{field.Name}: {FormatValue(fieldValue, depth + 1)}");
        }

        return $"{type.Name}({string.Join(", ", fields)})";
    }

    private static string FormatComplexType(object value, int depth)
    {
        Type type = value.GetType();
        List<string> fields = new();

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            object fieldValue = field.GetValue(value);
            fields.Add($"{field.Name}: {FormatValue(fieldValue, depth + 1)}");
        }

        return $"{type.Name} {{ {string.Join(", ", fields)} }}";
    }

    private static string FormatCollection(IEnumerable collection, int depth)
    {
        List<string> elements = new();
        foreach (object item in collection)
        {
            elements.Add(FormatValue(item, depth + 1));
        }
        return $"[ {string.Join(", ", elements)} ]";
    }

    private static string FormatDictionary(IDictionary dictionary, int depth)
    {
        List<string> pairs = new();
        foreach (DictionaryEntry entry in dictionary)
        {
            string key = FormatValue(entry.Key, depth + 1);
            string value = FormatValue(entry.Value, depth + 1);
            pairs.Add($"{key}: {value}");
        }
        return $"{{ {string.Join(", ", pairs)} }}";
    }
    
    #endregion

    private static bool IsCollection(Type type)
    {
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static bool IsDictionary(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
}