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
    /// 获取字段的默认值，优先使用用户定义的默认值
    /// </summary>
    public static string GetFieldDefaultValue(DefField field)
    {
        if (!string.IsNullOrEmpty(field.DefaultValue))
        {
            return ParseDefaultValue(field.DefaultValue, field.CType);
        }
        return field.CType.Apply(DefaultValueVisitor.Ins);
    }

    /// <summary>
    /// 将用户定义的默认值字符串转换为AngelScript格式
    /// </summary>
    private static string ParseDefaultValue(string defaultValue, TType type)
    {
        if (string.IsNullOrEmpty(defaultValue))
        {
            return type.Apply(DefaultValueVisitor.Ins);
        }

        return type switch
        {
            TBool => defaultValue.ToLower() == "true" || defaultValue == "1" ? "true" : "false",
            TByte or TShort or TInt or TLong => defaultValue,
            TFloat => defaultValue.Contains('.') ? $"{defaultValue}f" : $"{defaultValue}.0f",
            TDouble => defaultValue.Contains('.') ? defaultValue : $"{defaultValue}.0",
            TString => $"\"{defaultValue}\"",
            TEnum enumType => ParseEnumDefault(defaultValue, enumType),
            TBean beanType when beanType.DefBean.IsValueType => ParseValueTypeDefault(defaultValue, beanType),
            _ => type.Apply(DefaultValueVisitor.Ins)
        };
    }

    /// <summary>
    /// 解析枚举类型的默认值
    /// </summary>
    private static string ParseEnumDefault(string defaultValue, TEnum enumType)
    {
        // 如果是数字，查找对应的枚举项
        if (int.TryParse(defaultValue, out var intValue))
        {
            var item = enumType.DefEnum.Items.FirstOrDefault(i => i.IntValue == intValue);
            if (item != null)
            {
                return $"{enumType.DefEnum.Name}::{item.Name}";
            }
        }
        // 否则假设是枚举名称
        return $"{enumType.DefEnum.Name}::{defaultValue}";
    }

    /// <summary>
    /// 解析值类型(如vector3, vector4)的默认值
    /// </summary>
    private static string ParseValueTypeDefault(string defaultValue, TBean beanType)
    {
        var typeName = beanType.DefBean.Name;

        // 检查是否有TypeMapper定义的类型映射
        // 优先检查 "all" target (通用映射)，然后检查 angelscript 特定映射
        var mapper = beanType.DefBean.TypeMappers?.FirstOrDefault(m =>
            (m.Targets.Contains("all") || m.Targets.Contains("angelscript")) &&
            m.CodeTargets.Contains("angelscript-json"));

        if (mapper != null)
        {
            // 优先使用 "type" 选项，然后是 "name" 选项
            if (mapper.Options.TryGetValue("type", out var mappedType))
            {
                typeName = mappedType;
            }
            else if (mapper.Options.TryGetValue("name", out var mappedName))
            {
                typeName = mappedName;
            }
        }

        // 处理vector3: "0,0,0" -> FVector(0, 0, 0)
        if (typeName == "FVector" || typeName == "vector3")
        {
            var parts = defaultValue.Split(',');
            if (parts.Length == 3)
            {
                return $"FVector({parts[0].Trim()}, {parts[1].Trim()}, {parts[2].Trim()})";
            }
        }
        // 处理vector4: "0,0,0,0" -> FVector4(0, 0, 0, 0)
        else if (typeName == "FVector4" || typeName == "vector4")
        {
            var parts = defaultValue.Split(',');
            if (parts.Length == 4)
            {
                return $"FVector4({parts[0].Trim()}, {parts[1].Trim()}, {parts[2].Trim()}, {parts[3].Trim()})";
            }
        }

        // 其他值类型返回空字符串，让编译器使用默认构造函数
        return "";
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

