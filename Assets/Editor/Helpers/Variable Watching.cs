using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    public static List<string> Watch(string targetVariable, GameObject targetGameObject)
    {
        if (!targetGameObject) return new List<string> { "No GameObject Attached" };
        
        object value = GetVariableValue(targetVariable, targetGameObject);
        return FormatValue(value);
    }

    private static object GetVariableValue(string variableName, GameObject target)
    {
        foreach (Component component in target.GetComponents<Component>())
        {
            Type type = component.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(variableName, 
                    BindingFlags.Public | 
                    BindingFlags.NonPublic | 
                    BindingFlags.Instance | 
                    BindingFlags.Static);
                
                if (field != null)
                {
                    object instance = field.IsStatic ? null : component;
                    return field.GetValue(instance);
                }
                type = type.BaseType;
            }
        }
        return null;
    }

    private static List<string> FormatValue(object value, int depth = -1, string prefix = "")
    {
        List<string> lines = new();
        
        if (depth > maxDepth)
        {
            lines.Add($"{prefix}...");
            return lines;
        }

        if (value == null)
        {
            lines.Add($"{prefix}<color=#6c95eb>null</color>");
            return lines;
        }

        Type type = value.GetType();
        string color = GetColorFrom(type);
        
        if (type == typeof(string))
        {
            lines.Add($"{prefix}<color={color}>\"{value}\"</color>");
            return lines;
        }
        
        if (type == typeof(char))
        {
            lines.Add($"{prefix}<color={color}>'{value}'</color>");
            return lines;
        }
        
        if (type.IsPrimitive || type.IsEnum)
        {
            lines.Add($"{prefix}<color={color}>{value}</color>");
            return lines;
        }

        if (UnityPrimitiveTypes.Contains(type) || type.Namespace?.StartsWith("UnityEngine") == true)
        {
            FormatUnityType(value, lines, depth, prefix);
            return lines;
        }

        if (IsCollection(type))
        {
            FormatCollection(value as IEnumerable, lines, depth, prefix);
            return lines;
        }

        if (IsDictionary(type))
        {
            FormatDictionary(value as IDictionary, lines, depth, prefix);
            return lines;
        }

        FormatComplexType(value, lines, depth, prefix);
        return lines;
    }

    #region Formatting Helpers

    private static void FormatUnityType(object value, List<string> lines, int depth, string prefix)
    {
        Type type = value.GetType();
        lines.Add($"{prefix}{type.Name}(");
        
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            List<string> fieldLines = FormatValue(field.GetValue(value), depth + 1, $"{prefix}  {field.Name}: ");
            lines.AddRange(fieldLines);
        }
        
        lines.Add($"{prefix})");
    }

    private static void FormatComplexType(object value, List<string> lines, int depth, string prefix)
    {
        Type type = value.GetType();
        lines.Add($"{prefix}{type.Name} {{");
        
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            List<string> fieldLines = FormatValue(field.GetValue(value), depth + 1, $"{prefix}  {field.Name}: ");
            lines.AddRange(fieldLines);
        }
        
        lines.Add($"{prefix}}}");
    }

    private static void FormatCollection(IEnumerable collection, List<string> lines, int depth, string prefix)
    {
        Type collectionType = collection.GetType();
        string collectionName = collectionType.Name.Trim('1').Trim('`');
        Type[] genericArgs = collectionType.GetGenericArguments();

        string collectionColor = GetColorFrom(collectionType);
        string argColor = GetColorFrom(genericArgs[0]);
        
        lines.Add(
            $"{prefix}<color={collectionColor}>{collectionName}</color>" +
            $"<<color={argColor}>{genericArgs[0].Name}</color>> = [" );
        
        foreach (object item in collection)
        {
            List<string> itemLines = FormatValue(item, depth, $"{prefix}");
            lines.Add($"{prefix}    {itemLines[0]},");
        }
        lines.Add($"{prefix}]");
    }

    private static void FormatDictionary(IDictionary dictionary, List<string> lines, int depth, string prefix)
    {
        Type dictType = dictionary.GetType();
        string dictName = dictType.Name.Trim('2').Trim('`');
        Type[] genericArgs = dictType.GetGenericArguments();

        string dictColor = GetColorFrom(dictType);
        string arg1Color = GetColorFrom(genericArgs[0]);
        string arg2Color = GetColorFrom(genericArgs[1]);

        lines.Add(
            $"{prefix}<color={dictColor}>{dictName}</color>" +
            $"<<color={arg1Color}>{genericArgs[0].Name}</color>, " +
            $"<color={arg2Color}>{genericArgs[1].Name}</color>> = {{" );
        
        foreach (DictionaryEntry entry in dictionary)
        {
            List<string> keyLines = FormatValue(entry.Key, depth, $"{prefix}");
            List<string> valueLines = FormatValue(entry.Value, depth, $"{prefix}");
            lines.Add($"{prefix}  {{  {keyLines[0]},    {valueLines[0]}  }},");
        }
        lines.Add($"{prefix}}}");
    }

    private static string GetColorFrom(Type type)
    {
        if (type == null || type == typeof(bool))
            return "#6c95eb";

        if (type == typeof(char) || type == typeof(string))
            return "#c9a26d";
        
        if (type.IsClass)
            return "#c191ff";

        if (type.IsPrimitive)
            return "#ed94c0";

        if (type.IsEnum)
            return "#eibfff";
        
        return "#bdbdbd";
    }

    #endregion

    private static bool IsCollection(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }

    private static bool IsDictionary(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
}