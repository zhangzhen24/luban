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
using Luban.CodeFormat.CodeStyles;
using Luban.CodeTarget;
using Luban.Defs;
using Luban.Types;
using Luban.Typescript.TemplateExtensions;
using Scriban;

namespace Luban.Typescript.CodeTarget;

/// <summary>
/// TypeScript code target for Puerts (UE TypeScript plugin)
/// Generates clean, UE-style TypeScript code with:
/// - PascalCase for types/classes
/// - camelCase for fields/properties
/// - Simple CfgMgr similar to AngelScript style
/// </summary>
[CodeTarget("typescript-puerts")]
public class TypescriptPuertsCodeTarget : TemplateCodeTargetBase
{
    private static readonly ICodeStyle s_puertsCodeStyle = new ConfigurableCodeStyle(
        "camel",   // namespace - camelCase (用于文件名)
        "pascal",  // type - PascalCase (类名)
        "pascal",  // method - PascalCase (方法名)
        "camel",   // property - camelCase (属性)
        "camel",   // field - camelCase (字段)
        "none"     // enumItem - 保持原样
    );

    public override string FileHeader => CommonFileHeaders.AUTO_GENERATE_C_LIKE;

    protected override string FileSuffixName => "ts";

    protected override ICodeStyle DefaultCodeStyle => s_puertsCodeStyle;

    private static readonly HashSet<string> s_preservedKeyWords = new()
    {
        "abstract", "as", "any", "boolean", "break", "case", "catch", "class", "const", "continue", "debugger", "declare",
        "default", "delete", "do", "else", "enum", "export", "extends", "false", "finally", "for", "from", "function", "get",
        "if", "implements", "import", "in", "instanceof", "interface", "let", "module", "namespace", "new", "null", "number",
        "object", "package", "private", "protected", "public", "require", "return", "set", "static", "string", "super", "switch",
        "symbol", "this", "throw", "true", "try", "typeof", "undefined", "var", "void", "while", "with", "yield"
    };

    protected override IReadOnlySet<string> PreservedKeyWords => s_preservedKeyWords;

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        ctx.PushGlobal(new TypescriptCommonTemplateExtension());
        ctx.PushGlobal(new TypescriptPuertsTemplateExtension());
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

    public override void Handle(GenerationContext ctx, OutputFileManifest manifest)
    {
        // Generate CfgMgr file
        string outputFile = EnvManager.Current.GetOptionOrDefault(Name, "outputFile", true, $"CfgMgr.{FileSuffixName}");
        var tablesTask = Task.Run(() =>
        {
            var writer = new CodeWriter();
            GenerateTables(ctx, ctx.ExportTables, writer);
            return CreateOutputFile(outputFile, writer.ToResult(FileHeader));
        });

        // Group types by namespace
        var namespaceGroups = new Dictionary<string, NamespaceGroup>();

        // Group enums
        foreach (var @enum in ctx.ExportEnums)
        {
            string ns = string.IsNullOrEmpty(@enum.Namespace) ? "" : @enum.Namespace;
            if (!namespaceGroups.TryGetValue(ns, out var group))
            {
                group = new NamespaceGroup { Namespace = ns };
                namespaceGroups[ns] = group;
            }
            group.Enums.Add(@enum);
        }

        // Group beans
        foreach (var bean in ctx.ExportBeans)
        {
            string ns = string.IsNullOrEmpty(bean.Namespace) ? "" : bean.Namespace;
            if (!namespaceGroups.TryGetValue(ns, out var group))
            {
                group = new NamespaceGroup { Namespace = ns };
                namespaceGroups[ns] = group;
            }
            group.Beans.Add(bean);
        }

        // Generate files for each namespace group
        var tasks = new List<Task<OutputFile>> { tablesTask };
        foreach (var kvp in namespaceGroups)
        {
            var group = kvp.Value;
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                GenerateNamespaceGroup(ctx, group, writer);

                string fileName = GetFileName(group.Namespace);
                return CreateOutputFile($"{fileName}.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
        }

        Task.WaitAll(tasks.ToArray());
        foreach (var task in tasks)
        {
            manifest.AddFile(task.Result);
        }
    }

    private void GenerateNamespaceGroup(GenerationContext ctx, NamespaceGroup group, CodeWriter writer)
    {
        var sortedEnums = group.Enums.OrderBy(e => e.Name).ToList();
        var sortedBeans = group.Beans.OrderBy(b => b.Name).ToList();

        // Collect all imports from beans first
        var imports = CollectImports(group.Namespace, sortedBeans);
        if (imports.Count > 0)
        {
            foreach (var import in imports.OrderBy(i => i.Key))
            {
                var typeNames = string.Join(", ", import.Value.OrderBy(t => t));
                writer.Write($"import {{ {typeNames} }} from \"./{import.Key}\"");
            }
            writer.Write("");
        }

        // Generate enums first
        for (int i = 0; i < sortedEnums.Count; i++)
        {
            GenerateEnum(ctx, sortedEnums[i], writer);
            if (i < sortedEnums.Count - 1 || sortedBeans.Count > 0)
            {
                writer.Write("");
            }
        }

        // Then generate beans (without individual imports)
        for (int i = 0; i < sortedBeans.Count; i++)
        {
            GenerateBeanWithoutImports(ctx, sortedBeans[i], writer);
            if (i < sortedBeans.Count - 1)
            {
                writer.Write("");
            }
        }
    }

    private void GenerateBeanWithoutImports(GenerationContext ctx, DefBean bean, CodeWriter writer)
    {
        // Generate bean comment
        if (!string.IsNullOrEmpty(bean.Comment))
        {
            writer.Write("/**");
            writer.Write($" * {bean.Comment}");
            writer.Write(" */");
        }

        writer.Write($"export class {bean.Name} {{");

        foreach (var field in bean.HierarchyExportFields)
        {
            var fieldName = CodeStyle.FormatField(field.Name);
            var fieldType = field.CType.Apply(Luban.Typescript.TypeVisitors.PuertsDeclaringTypeNameVisitor.Ins);
            var fieldDefault = field.CType.Apply(Luban.Typescript.TypeVisitors.PuertsDefaultValueVisitor.Ins);

            if (!string.IsNullOrEmpty(field.Comment))
            {
                writer.Write($"    /** {field.Comment} */");
            }
            writer.Write($"    {fieldName}: {fieldType} = {fieldDefault}");
        }

        writer.Write("}");
    }

    private Dictionary<string, HashSet<string>> CollectImports(string currentNs, List<DefBean> beans)
    {
        var imports = new Dictionary<string, HashSet<string>>();
        var currentFileName = GetFileName(currentNs);

        foreach (var bean in beans)
        {
            foreach (var field in bean.HierarchyExportFields)
            {
                CollectExternalTypes(field.CType, currentNs, currentFileName, imports);
            }
        }

        return imports;
    }

    private void CollectExternalTypes(TType type, string currentNs, string currentFileName, Dictionary<string, HashSet<string>> imports)
    {
        switch (type)
        {
            case TBean beanType:
                var beanNs = beanType.DefBean.Namespace ?? "";
                if (beanNs != currentNs)
                {
                    var fileName = GetFileName(beanNs);
                    if (fileName != currentFileName)
                    {
                        AddImport(imports, fileName, beanType.DefBean.Name);
                    }
                }
                break;
            case TEnum enumType:
                var enumNs = enumType.DefEnum.Namespace ?? "";
                if (enumNs != currentNs)
                {
                    var fileName = GetFileName(enumNs);
                    if (fileName != currentFileName)
                    {
                        AddImport(imports, fileName, enumType.DefEnum.Name);
                    }
                }
                break;
            case TArray arrayType:
                CollectExternalTypes(arrayType.ElementType, currentNs, currentFileName, imports);
                break;
            case TList listType:
                CollectExternalTypes(listType.ElementType, currentNs, currentFileName, imports);
                break;
            case TSet setType:
                CollectExternalTypes(setType.ElementType, currentNs, currentFileName, imports);
                break;
            case TMap mapType:
                CollectExternalTypes(mapType.KeyType, currentNs, currentFileName, imports);
                CollectExternalTypes(mapType.ValueType, currentNs, currentFileName, imports);
                break;
        }
    }

    private static void AddImport(Dictionary<string, HashSet<string>> imports, string fileName, string typeName)
    {
        if (!imports.TryGetValue(fileName, out var types))
        {
            types = new HashSet<string>();
            imports[fileName] = types;
        }
        types.Add(typeName);
    }

    private class NamespaceGroup
    {
        public string Namespace { get; set; } = "";
        public List<DefEnum> Enums { get; set; } = new();
        public List<DefBean> Beans { get; set; } = new();
    }
}
