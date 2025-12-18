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

public class TypescriptUeJsonTemplateExtension : ScriptObject
{
    public static string Deserialize(string fieldName, string jsonVar, TType type)
    {
        return type.Apply(JsonDeserializeVisitor.Ins, jsonVar, fieldName, 0);
    }

    public static string GenerateImports(List<DefTable> tables)
    {
        var namespaces = new HashSet<string>();
        foreach (var table in tables)
        {
            var ns = table.ValueTType.DefBean.Namespace;
            if (!string.IsNullOrEmpty(ns))
            {
                namespaces.Add(ns);
            }
        }

        if (namespaces.Count == 0)
        {
            return "";
        }

        var imports = new List<string>();
        foreach (var ns in namespaces.OrderBy(n => n))
        {
            // Get the last part of namespace as filename
            var fileName = ns.Split('.').Last();
            imports.Add($"import {{ {ns} }} from \"./{fileName}\"");
        }

        return string.Join("\n", imports) + "\n";
    }

    /// <summary>
    /// Generate union key expression from item variable
    /// e.g., `${item.Id}_${item.SubId}`
    /// </summary>
    public static string MakeUnionKey(ICodeStyle codeStyle, DefTable table, string varName)
    {
        var parts = table.IndexList.Select(idx =>
        {
            var fieldName = codeStyle.FormatField(idx.IndexField.Name);
            return $"${{{varName}.{fieldName}}}";
        });
        return "`" + string.Join("_", parts) + "`";
    }

    /// <summary>
    /// Generate union key parameters for Get function
    /// e.g., "id: number, subId: number"
    /// </summary>
    public static string UnionKeyParams(ICodeStyle codeStyle, DefTable table)
    {
        var parts = table.IndexList.Select(idx =>
        {
            var fieldName = codeStyle.FormatField(idx.IndexField.Name);
            var typeName = idx.Type.Apply(DeclaringTypeNameVisitor.Ins);
            return $"{fieldName}: {typeName}";
        });
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Generate union key expression from parameters
    /// e.g., `${id}_${subId}`
    /// </summary>
    public static string MakeUnionKeyFromParams(ICodeStyle codeStyle, DefTable table)
    {
        var parts = table.IndexList.Select(idx =>
        {
            var fieldName = codeStyle.FormatField(idx.IndexField.Name);
            return $"${{{fieldName}}}";
        });
        return "`" + string.Join("_", parts) + "`";
    }
}
