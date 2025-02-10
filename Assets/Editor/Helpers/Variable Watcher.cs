using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class VariableWatcher : MonoBehaviour
{
    private static readonly HashSet<Type> UnityPrimitiveTypes = new()
    {
        typeof(Vector2), typeof(Vector3), typeof(Vector4),
        typeof(Quaternion), typeof(Color), typeof(Rect),
        typeof(Matrix4x4), typeof(LayerMask)
    };
    
    private const int maxDepth = 5;

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
        string typeColor = GetColorFrom(type);
        List<string> fields = new();
        
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            List<string> fieldLines = FormatValue(field.GetValue(value), depth);
            
            string fieldValue = fieldLines.Count > 0 ? fieldLines[0] : "null";
            fields.Add(fieldValue);
        }
        
        lines.Add($"{prefix}<color={typeColor}>{type.Name}</color>({string.Join(", ", fields)})");
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
        string collectionName = GetFormattedTypeName(collectionType);
        string collectionColor = GetColorFrom(collectionType);

        lines.Add($"{prefix}<color={collectionColor}>{collectionName}</color> = [");
        
        List<object> items = collection.Cast<object>().ToList();
        for (int i = 0; i < items.Count; i++)
        {
            object item = items[i];
            List<string> itemLines = FormatValue(item);
            bool isLastItem = i == items.Count - 1;

            for (int j = 0; j < itemLines.Count; j++)
            {
                string line = itemLines[j];
                bool isLastLineOfItem = j == itemLines.Count - 1;

                string suffix = isLastLineOfItem && !isLastItem ? "," : "";
                lines.Add($"{prefix}    {line}{suffix}");
            }
        }
        lines.Add($"{prefix}]");
    }

    private static void FormatDictionary(IDictionary dictionary, List<string> lines, int depth, string prefix)
    {
        Type dictType = dictionary.GetType();
        string dictName = GetFormattedTypeName(dictType);
        string dictColor = GetColorFrom(dictType);

        lines.Add($"{prefix}<color={dictColor}>{dictName}</color> = {{");

        List<KeyValuePair<object, object>> entries = new();

        if (dictType.IsGenericType)
        {
            dynamic genericDict = dictionary;
            foreach (dynamic entry in genericDict)
            {
                entries.Add(new KeyValuePair<object, object>(entry.Key, entry.Value));
            }
        }
        else
        {
            entries.AddRange(from DictionaryEntry entry in dictionary select new KeyValuePair<object, object>(entry.Key, entry.Value));
        }

        for (int i = 0; i < entries.Count; i++)
        {
            KeyValuePair<object, object> entry = entries[i];
            List<string> keyLines = FormatValue(entry.Key, depth, prefix);
            List<string> valueLines = FormatValue(entry.Value, depth, prefix);
            bool isLastItem = i == entries.Count - 1;

            lines.Add($"{prefix}  {{");
            lines.AddRange(keyLines.Select(keyLine => $"{prefix}    {keyLine}"));
            lines.AddRange(valueLines.Select(valueLine => $"{prefix}    {valueLine}"));
            lines.Add($"{prefix}  }}" + (isLastItem ? "" : ","));
        }

        lines.Add($"{prefix}}}");
    }
    
    private static string GetFormattedTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        string baseName = type.Name.Split('`')[0];
        string baseColor = GetColorFrom(type);
        
        Type[] genericArgs = type.GetGenericArguments();
        string[] formattedArgs = new string[genericArgs.Length];
        for (int i = 0; i < genericArgs.Length; i++)
        {
            string formattedArg = GetFormattedTypeName(genericArgs[i]);
            string formattedArgColor = GetColorFrom(genericArgs[i]);
            
            formattedArgs[i] = $"<color={formattedArgColor}>{formattedArg}</color>";
        }
        return $"<color={baseColor}>{baseName}</color>{ChangeColorOf("<")}{string.Join($"{ChangeColorOf(",")} ", formattedArgs)}{ChangeColorOf(">")}";
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

    private static string ChangeColorOf(string text, string color = "#bdbdbd")
    {
        return $"<color={color}>{text}</color>";
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