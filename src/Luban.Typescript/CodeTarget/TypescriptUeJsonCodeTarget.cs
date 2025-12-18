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

[CodeTarget("typescript-ue-json")]
public class TypescriptUeJsonCodeTarget : TemplateCodeTargetBase
{
    private static readonly ICodeStyle s_ueCodeStyle = new ConfigurableCodeStyle(
        "pascal",  // namespace
        "pascal",  // type
        "pascal",  // method
        "pascal",  // property
        "pascal",  // field
        "none"     // enumItem
    );

    public override string FileHeader => CommonFileHeaders.AUTO_GENERATE_C_LIKE;

    protected override string FileSuffixName => "ts";

    protected override ICodeStyle DefaultCodeStyle => s_ueCodeStyle;

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
        ctx.PushGlobal(new TypescriptUeJsonTemplateExtension());
    }

    public override void Handle(GenerationContext ctx, OutputFileManifest manifest)
    {
        // Generate UCfgMgr file
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

                // Use namespace as filename, or "Common" if namespace is empty
                string fileName = string.IsNullOrEmpty(group.Namespace)
                    ? "Common"
                    : group.Namespace.Split('.').Last();

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
        // Sort enums by name for consistent output
        var sortedEnums = group.Enums.OrderBy(e => e.Name).ToList();
        // Sort beans by name for consistent output
        var sortedBeans = group.Beans.OrderBy(b => b.Name).ToList();

        // Collect dependencies from other namespaces
        var dependencies = CollectDependencies(group, sortedBeans);

        // Generate import statements for dependencies
        foreach (var dep in dependencies.OrderBy(d => d.Key))
        {
            string fileName = string.IsNullOrEmpty(dep.Key) ? "Common" : dep.Key.Split('.').Last();
            if (string.IsNullOrEmpty(dep.Key))
            {
                // Import from Common.ts (no namespace wrapper)
                var typeNames = string.Join(", ", dep.Value.OrderBy(t => t));
                writer.Write($"import {{ {typeNames} }} from \"./{fileName}\"");
            }
            else
            {
                // Import namespace from file
                writer.Write($"import {{ {dep.Key} }} from \"./{fileName}\"");
            }
        }

        if (dependencies.Count > 0)
        {
            writer.Write("");
        }

        // Add namespace wrapper if namespace is not empty
        bool hasNamespace = !string.IsNullOrEmpty(group.Namespace);
        if (hasNamespace)
        {
            writer.Write($"export namespace {group.Namespace} {{");
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

        // Then generate beans
        for (int i = 0; i < sortedBeans.Count; i++)
        {
            GenerateBean(ctx, sortedBeans[i], writer);
            if (i < sortedBeans.Count - 1)
            {
                writer.Write("");
            }
        }

        // Close namespace
        if (hasNamespace)
        {
            writer.Write("}");
        }
    }

    private class NamespaceGroup
    {
        public string Namespace { get; set; } = "";
        public List<DefEnum> Enums { get; set; } = new();
        public List<DefBean> Beans { get; set; } = new();
    }

    /// <summary>
    /// Collect dependencies from other namespaces used in beans
    /// </summary>
    /// <param name="group">Current namespace group</param>
    /// <param name="beans">Beans in this group</param>
    /// <returns>Dictionary of namespace -> set of type names</returns>
    private Dictionary<string, HashSet<string>> CollectDependencies(NamespaceGroup group, List<DefBean> beans)
    {
        var dependencies = new Dictionary<string, HashSet<string>>();

        foreach (var bean in beans)
        {
            CollectTypeDependencies(bean, group.Namespace, dependencies);
        }

        return dependencies;
    }

    private void CollectTypeDependencies(DefBean bean, string currentNamespace, Dictionary<string, HashSet<string>> dependencies)
    {
        // Check parent type
        if (bean.ParentDefType != null)
        {
            AddDependencyIfNeeded(bean.ParentDefType.Namespace, bean.ParentDefType.Name, currentNamespace, dependencies);
        }

        // Check all fields
        foreach (var field in bean.HierarchyExportFields)
        {
            CollectFieldTypeDependencies(field.CType, currentNamespace, dependencies);
        }
    }

    private void CollectFieldTypeDependencies(TType type, string currentNamespace, Dictionary<string, HashSet<string>> dependencies)
    {
        switch (type)
        {
            case TBean beanType:
                AddDependencyIfNeeded(beanType.DefBean.Namespace, beanType.DefBean.Name, currentNamespace, dependencies);
                break;
            case TEnum enumType:
                AddDependencyIfNeeded(enumType.DefEnum.Namespace, enumType.DefEnum.Name, currentNamespace, dependencies);
                break;
            case TArray arrayType:
                CollectFieldTypeDependencies(arrayType.ElementType, currentNamespace, dependencies);
                break;
            case TList listType:
                CollectFieldTypeDependencies(listType.ElementType, currentNamespace, dependencies);
                break;
            case TSet setType:
                CollectFieldTypeDependencies(setType.ElementType, currentNamespace, dependencies);
                break;
            case TMap mapType:
                CollectFieldTypeDependencies(mapType.KeyType, currentNamespace, dependencies);
                CollectFieldTypeDependencies(mapType.ValueType, currentNamespace, dependencies);
                break;
        }
    }

    private void AddDependencyIfNeeded(string typeNamespace, string typeName, string currentNamespace, Dictionary<string, HashSet<string>> dependencies)
    {
        // Normalize empty namespace
        typeNamespace = typeNamespace ?? "";
        currentNamespace = currentNamespace ?? "";

        // Don't add dependency if same namespace
        if (typeNamespace == currentNamespace)
        {
            return;
        }

        if (!dependencies.TryGetValue(typeNamespace, out var typeSet))
        {
            typeSet = new HashSet<string>();
            dependencies[typeNamespace] = typeSet;
        }

        typeSet.Add(typeName);
    }
}
