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

using Luban;
using Luban.AngelScript.TemplateExtensions;
using Luban.CodeTarget;
using Luban.Defs;
using Scriban;

namespace Luban.AngelScript.CodeTarget;

[CodeTarget("angelscript-json")]
public class AngelScriptJsonCodeTarget : AngelScriptCodeTargetBase
{
    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new AngelScriptJsonTemplateExtension());
    }

    public override void Handle(GenerationContext ctx, OutputFileManifest manifest)
    {
        // Generate CfgMgr file - use outputFile option if provided, otherwise use default "CfgMgr.{FileSuffixName}"
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

        // Tables are generated separately, not grouped by namespace

        // Generate files for each namespace group
        var tasks = new List<Task<OutputFile>> { tablesTask };
        foreach (var kvp in namespaceGroups)
        {
            var group = kvp.Value;
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                GenerateNamespaceGroup(ctx, group, writer);

                // Use namespace as filename with "Cfg" suffix, or a default name if namespace is empty
                string fileName = string.IsNullOrEmpty(group.Namespace)
                    ? "CommonCfg"
                    : group.Namespace.Split('.').Last() + "Cfg";

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

        // Generate enums first
        for (int i = 0; i < sortedEnums.Count; i++)
        {
            GenerateEnum(ctx, sortedEnums[i], writer);
            if (i < sortedEnums.Count - 1 || sortedBeans.Count > 0)
            {
                writer.Write("");
                writer.Write("");
            }
        }

        // Then generate beans (skip beans with TypeMapper as they are external types)
        var beansToGenerate = new List<DefBean>();
        string targetName = ctx.Target.Name;
        string codeTargetName = Name;

        foreach (var bean in sortedBeans)
        {
            // Skip beans that have TypeMapper for the current target/codeTarget
            // These are external types (like UE's native FVector, FVector2D, FVector4)
            bool shouldSkip = false;
            if (bean.TypeMappers != null && bean.TypeMappers.Count > 0)
            {
                foreach (var mapper in bean.TypeMappers)
                {
                    if (mapper.Targets.Contains(targetName) && mapper.CodeTargets.Contains(codeTargetName))
                    {
                        shouldSkip = true;
                        break;
                    }
                }
            }

            if (!shouldSkip)
            {
                beansToGenerate.Add(bean);
            }
        }

        for (int i = 0; i < beansToGenerate.Count; i++)
        {
            GenerateBean(ctx, beansToGenerate[i], writer);
            if (i < beansToGenerate.Count - 1)
            {
                writer.Write("");
                writer.Write("");
            }
        }

        // Tables are generated separately, not in namespace groups
    }

    private class NamespaceGroup
    {
        public string Namespace { get; set; } = "";
        public List<DefEnum> Enums { get; set; } = new();
        public List<DefBean> Beans { get; set; } = new();
    }
}
