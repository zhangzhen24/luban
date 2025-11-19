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
using Luban.CodeTarget;
using Scriban;

namespace Luban.AngelScript.CodeTarget;

public abstract class AngelScriptCodeTargetBase : TemplateCodeTargetBase
{
    public override string FileHeader => CommonFileHeaders.AUTO_GENERATE_C_LIKE;

    protected override string FileSuffixName => "as";

    protected override ICodeStyle DefaultCodeStyle => CodeFormatManager.Ins.NoneCodeStyle;

    private static readonly HashSet<string> s_preservedKeyWords = new()
    {
        // Angelscript preserved keywords
        "and", "auto", "bool", "break", "case", "cast", "class", "const", "continue", "default", "do", "double",
        "else", "enum", "false", "final", "float", "for", "from", "funcdef", "function", "get", "if", "import",
        "in", "inout", "int", "int8", "int16", "int32", "int64", "interface", "is", "mixin", "namespace", "not",
        "null", "or", "out", "override", "private", "protected", "return", "set", "shared", "super", "switch",
        "this", "true", "typedef", "uint", "uint8", "uint16", "uint32", "uint64", "void", "while", "xor"
    };

    protected override IReadOnlySet<string> PreservedKeyWords => s_preservedKeyWords;

    protected override string GetFileNameWithoutExtByTypeName(string name)
    {
        return name.Replace('.', '/');
    }

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        // Base implementation, can be overridden
    }
}

