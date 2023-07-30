using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Default
{
    public static class MermaidMakerSample
    {
        //Namespaces shown in the Mermaid class diagram
        private static readonly List<string> NameSpaces = new List<string>
        {
            //Just a sample
            nameof(Default),
            nameof(Line),
            nameof(Math)
        };

        //　Classes you want to ignore in the Mermaid class diagram
        private static readonly List<string> IgnoreClasses = new List<string>()
        {
            nameof(UnityEngine.Object),
            nameof(Enum),
            nameof(MonoBehaviour),
            nameof(ValueType)
        };
        
        [MenuItem("Original/ClassDiagram")]
        private static void CreateStringText()
        {
            var fileText = "";
            var arrowText = "";

            var types = Assembly.Load("Assembly-CSharp")
                .GetTypes()
                .Where(type => NameSpaces.Contains(type.Namespace))
                .Where(type =>
                    !type.Name.Contains("<") && !type.Name.Contains("+") && !type.Name.Contains("Extensions"));
            const string br = "\n";

            var memberInfos = types.ToList();
            foreach (var type in memberInfos)
            {
                var words = type.BaseType?.Name.Split("`");
                if (words != null && type.BaseType != null && !IgnoreClasses.Contains(words[0]))
                    arrowText += $"{type.Name} --|> {words[0]}{br}";

                arrowText = type.GetInterfaces().Where(interfaceType => memberInfos.Contains(interfaceType)).Aggregate(arrowText, (current, item) => current + $"{type.Name} ..|> {item.Name}{br}");

                fileText += $"{br}    class {type.Name.Split("`")[0]}";
                fileText += "{";
                fileText += $"{br}";

                var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(fieldInfo => !fieldInfo.Name.Contains("<"))
                    .Where(field => field.DeclaringType == type);

                foreach (var field in fields)
                {
                    fileText += "       ";
                    var attributeText = GetFieldAttributeText(field);
                    if (attributeText == "") continue;
                    fileText += attributeText;

                    var typeText = GetTypeText(field.FieldType);
                    fileText += $"{typeText} ";

                    fileText += $"{field.Name}{br}";
                }


                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(fieldInfo => !fieldInfo.Name.Contains("<"))
                    .Where(method => method.DeclaringType == type);
                foreach (var method in methods)
                {
                    fileText += "       ";
                    var attributeText = GetMethodAttributeText(method);
                    if (attributeText == "") continue;
                    fileText += attributeText;

                    var typeText = GetTypeText(method.ReturnType);
                    fileText += $"{typeText} ";
                    fileText += $"{method.Name}{br}";
                }

                fileText += @"   }";
                fileText += $"{br}";
            }

            fileText += arrowText;

            Debug.Log($"```mermaid{br}    classDiagram{br}{fileText}```");
        }

        private static string GetFieldAttributeText(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPrivate) return "-";
            if (fieldInfo.IsFamily) return "#";
            if (fieldInfo.IsPublic) return "+";
            throw new Exception("Attribute not found");
        }

        private static string GetMethodAttributeText(MethodBase methodInfo)
        {
            if (methodInfo.IsPrivate) return "-";
            if (methodInfo.IsFamily) return "#";
            if (methodInfo.IsPublic) return "+";
            throw new Exception("Attribute not found");
        }

        private static string GetTypeText(Type fieldType)
        {
            var words = fieldType.Name.Split("`");

            if (words.Length == 1)
            {
                var word = words[0];
                return word switch
                {
                    "Int32" => "int",
                    "Single" => "float",
                    "Boolean" => "bool",
                    "Void" => "void",
                    "Int32[]" => "int[]",
                    "Single[]" => "float[]",
                    "Boolean[]" => "bool[]",
                    "Double" => "double",
                    "Double[]" => "double[]",
                    _ => fieldType.Name
                };
            }

            if (words.Length == 2)
            {
                var genericTypeList = fieldType.GenericTypeArguments.Select(GetTypeText).ToList();
                
                var genericText = $"{words[0]}<{genericTypeList[0]}";
                genericTypeList.RemoveAt(0);
                genericTypeList.ForEach(text => { genericText += $",{text}"; });
                genericText += ">";
                return genericText;
            }

            return "";
        }
    }
}