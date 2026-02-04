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

using Luban.CodeFormat;
using Luban.Defs;
using Luban.Types;
using Luban.Typescript.TypeVisitors;
using Scriban.Runtime;

namespace Luban.Typescript.TemplateExtensions;

/// <summary>
/// Template extension for TypeScript Puerts code generation
/// </summary>
public class TypescriptPuertsTemplateExtension : ScriptObject
{
    public static string Deserialize(string fieldName, string jsonVar, TType type)
    {
        return type.Apply(JsonDeserializeVisitor.Ins, jsonVar, fieldName, 0);
    }

    /// <summary>
    /// Get type name without namespace (for Puerts)
    /// </summary>
    public static string DeclaringTypeName(TType type)
    {
        return type.Apply(PuertsDeclaringTypeNameVisitor.Ins);
    }

    /// <summary>
    /// Get default value for a type
    /// </summary>
    public static string DefaultValue(TType type)
    {
        return type.Apply(PuertsDefaultValueVisitor.Ins);
    }

    /// <summary>
    /// Get the map key type for a table
    /// </summary>
    public static string GetMapKeyType(DefTable table)
    {
        if (IsUnionKey(table))
        {
            return "string";
        }

        var keyType = table.KeyTType;
        return keyType switch
        {
            TInt or TLong => "number",
            TString => "string",
            TEnum enumType => enumType.DefEnum.Name,
            _ => "string"
        };
    }

    /// <summary>
    /// Check if table has union key (multiple fields)
    /// </summary>
    public static bool IsUnionKey(DefTable table)
    {
        return table.IsUnionIndex && table.IndexList.Count > 1;
    }

    /// <summary>
    /// Generate single key expression
    /// </summary>
    public static string MakeSingleKeyExpr(DefTable table, string varName, ICodeStyle codeStyle)
    {
        var indexField = table.IndexField;
        var fieldName = codeStyle.FormatField(indexField.Name);
        return $"{varName}.{fieldName}";
    }

    /// <summary>
    /// Generate union key expression
    /// </summary>
    public static string MakeUnionKeyExpr(DefTable table, string varName, ICodeStyle codeStyle)
    {
        var parts = table.IndexList.Select(idx =>
        {
            var fieldName = codeStyle.FormatField(idx.IndexField.Name);
            return $"{varName}.{fieldName}";
        });
        return "`" + string.Join("_", parts.Select(p => "${" + p + "}")) + "`";
    }

    /// <summary>
    /// Generate union key parameters for Get function
    /// </summary>
    public static string UnionKeyParams(DefTable table, ICodeStyle codeStyle)
    {
        var parts = table.IndexList.Select(idx =>
        {
            var fieldName = codeStyle.FormatField(idx.IndexField.Name);
            var typeName = idx.Type.Apply(PuertsTypeNameVisitor.Ins);
            return $"{fieldName}: {typeName}";
        });
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Generate union key from parameters
    /// </summary>
    public static string MakeUnionKeyFromParams(DefTable table, ICodeStyle codeStyle)
    {
        var parts = table.IndexList.Select(idx =>
        {
            var fieldName = codeStyle.FormatField(idx.IndexField.Name);
            return "${" + fieldName + "}";
        });
        return "`" + string.Join("_", parts) + "`";
    }

    /// <summary>
    /// Get single key field name
    /// </summary>
    public static string GetSingleKeyFieldName(DefTable table, ICodeStyle codeStyle)
    {
        return codeStyle.FormatField(table.IndexField.Name);
    }

    /// <summary>
    /// Get filename for a namespace (PascalCase + Cfg suffix)
    /// </summary>
    private static string GetFileName(string ns)
    {
        if (string.IsNullOrEmpty(ns))
        {
            return "CommonCfg";
        }
        var name = ns.Split('.').Last();
        // Use PascalCase: first letter uppercase
        return char.ToUpper(name[0]) + name.Substring(1) + "Cfg";
    }

    /// <summary>
    /// Generate imports for tables file - only import table value types
    /// </summary>
    public static string GenerateImports(List<DefTable> tables)
    {
        // Only collect table value types, grouped by their source file
        var typesByFile = new Dictionary<string, HashSet<string>>();

        foreach (var table in tables)
        {
            var bean = table.ValueTType.DefBean;
            // Only add the table's value type, not its field types
            var fileName = GetFileName(bean.Namespace);
            if (!typesByFile.TryGetValue(fileName, out var types))
            {
                types = new HashSet<string>();
                typesByFile[fileName] = types;
            }
            types.Add(bean.Name);
        }

        if (typesByFile.Count == 0)
        {
            return "";
        }

        var imports = new List<string>();
        foreach (var kvp in typesByFile.OrderBy(k => k.Key))
        {
            var typeNames = string.Join(", ", kvp.Value.OrderBy(t => t));
            imports.Add($"import {{ {typeNames} }} from \"./{kvp.Key}\"");
        }

        return string.Join("\n", imports) + "\n";
    }
}
