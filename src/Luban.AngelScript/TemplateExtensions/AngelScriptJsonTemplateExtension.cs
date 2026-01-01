// Copyright 2025 Code Philosophy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Luban.AngelScript.TypeVisitors;
using Luban.CodeFormat;
using Luban.Defs;
using Luban.Types;
using Scriban.Runtime;

namespace Luban.AngelScript.TemplateExtensions;

public class AngelScriptJsonTemplateExtension : ScriptObject
{
    public static string Deserialize(string fieldName, string jsonVar, TType type)
    {
        // Unreal Angelscript doesn't support nullable types like C#
        return type.Apply(JsonDeserializeVisitor.Ins, jsonVar, fieldName, 0);
    }

    public static string DeserializeField(string fieldName, string jsonVar, string jsonFieldName, TType type)
    {
        // Unreal JSON API: JsonObject->GetObjectField("fieldName")
        return type.Apply(JsonDeserializeVisitor.Ins, $"{jsonVar}->GetObjectField(\"{jsonFieldName}\")", fieldName, 0);
    }

    public static string DeclaringTypeName(TType type)
    {
        return type.Apply(DeclaringTypeNameVisitor.Ins);
    }

    public static string DefaultValue(TType type)
    {
        return type.Apply(DefaultValueVisitor.Ins);
    }

    /// <summary>
    /// 获取 TMap 的 Key 类型
    /// 联合主键使用 FString，单主键根据类型决定
    /// </summary>
    public static string GetMapKeyType(DefTable table)
    {
        // 联合主键始终使用 FString
        if (IsUnionKey(table))
        {
            return "FString";
        }

        // 单主键 - 对于 int32/int64 使用原生类型，其他使用 FString
        var keyType = table.KeyTType;
        return keyType switch
        {
            TInt => "int32",
            TLong => "int64",
            TString => "FString",
            TEnum enumType => enumType.DefEnum.Name,
            _ => "FString"
        };
    }

    /// <summary>
    /// 判断是否为联合主键（多个字段组成的主键）
    /// </summary>
    public static bool IsUnionKey(DefTable table)
    {
        return table.IsUnionIndex && table.IndexList.Count > 1;
    }

    /// <summary>
    /// 生成单主键的访问表达式
    /// </summary>
    public static string MakeSingleKeyExpr(DefTable table, string varName, ICodeStyle codeStyle)
    {
        var indexField = table.IndexField;
        var keyType = table.KeyTType;
        var fieldName = codeStyle.FormatProperty(indexField.Name);
        var accessor = $"{varName}.{fieldName}";

        // 对于原生类型直接返回，不需要转换
        if (keyType is TInt or TLong or TString or TEnum)
        {
            return accessor;
        }

        // 其他类型转换为字符串
        return $"f\"{{{accessor}}}\"";
    }

    /// <summary>
    /// 生成联合主键的拼接表达式
    /// </summary>
    public static string MakeUnionKeyExpr(DefTable table, string varName, ICodeStyle codeStyle)
    {
        var parts = table.IndexList.Select(idx =>
        {
            var fieldName = codeStyle.FormatProperty(idx.IndexField.Name);
            var fieldType = idx.Type;
            var accessor = $"{varName}.{fieldName}";

            if (fieldType is TString)
            {
                return accessor;
            }
            else
            {
                return $"f\"{{{accessor}}}\"";
            }
        });
        return string.Join(" + \"_\" + ", parts);
    }

    /// <summary>
    /// 生成联合主键函数的参数列表
    /// </summary>
    public static string UnionKeyParams(DefTable table, ICodeStyle codeStyle)
    {
        var parts = table.IndexList.Select(idx =>
        {
            var typeName = idx.Type.Apply(DeclaringTypeNameVisitor.Ins);
            var fieldName = codeStyle.FormatProperty(idx.IndexField.Name);
            return $"{typeName} {fieldName}";
        });
        return string.Join(", ", parts);
    }

    /// <summary>
    /// 生成联合主键函数的参数名列表
    /// </summary>
    public static string UnionKeyParamNames(DefTable table, ICodeStyle codeStyle)
    {
        var names = table.IndexList.Select(idx => codeStyle.FormatProperty(idx.IndexField.Name));
        return string.Join(", ", names);
    }

    /// <summary>
    /// 从参数生成联合主键的拼接表达式
    /// </summary>
    public static string MakeUnionKeyFromParams(DefTable table, ICodeStyle codeStyle)
    {
        var parts = table.IndexList.Select(idx =>
        {
            var fieldName = codeStyle.FormatProperty(idx.IndexField.Name);
            var fieldType = idx.Type;

            if (fieldType is TString)
            {
                return fieldName;
            }
            else
            {
                return $"f\"{{{fieldName}}}\"";
            }
        });
        return string.Join(" + \"_\" + ", parts);
    }

    /// <summary>
    /// 获取单主键字段名
    /// </summary>
    public static string GetSingleKeyFieldName(DefTable table, ICodeStyle codeStyle)
    {
        return codeStyle.FormatProperty(table.IndexField.Name);
    }
}

