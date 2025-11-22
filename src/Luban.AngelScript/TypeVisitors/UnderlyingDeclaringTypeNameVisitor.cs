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

using Luban.Types;
using Luban.TypeVisitors;
using Luban.Utils;

namespace Luban.AngelScript.TypeVisitors;

public class UnderlyingDeclaringTypeNameVisitor : ITypeFuncVisitor<string>
{
    public static UnderlyingDeclaringTypeNameVisitor Ins { get; } = new();

    public string Accept(TBool type)
    {
        return "bool";
    }

    public string Accept(TByte type)
    {
        return "int8";
    }

    public string Accept(TShort type)
    {
        return "int16";
    }

    public string Accept(TInt type)
    {
        return "int32";
    }

    public string Accept(TLong type)
    {
        return "int64";
    }

    public string Accept(TFloat type)
    {
        return "float";
    }

    public string Accept(TDouble type)
    {
        return "double";
    }

    public virtual string Accept(TEnum type)
    {
        // In Angelscript, when enum and struct/class are in the same file (same namespace),
        // we use the simple enum name without namespace prefix
        return type.DefEnum.TypeNameWithTypeMapper() ?? type.DefEnum.Name;
    }

    public string Accept(TString type)
    {
        return "FString";
    }

    public string Accept(TBean type)
    {
        // In Angelscript, when bean and struct/class are in the same file (same namespace),
        // we use the simple bean name without namespace prefix
        // For value types (structs), add "F" prefix
        string name = type.DefBean.TypeNameWithTypeMapper() ?? type.DefBean.Name;
        if (type.DefBean.IsValueType && !name.StartsWith("F"))
        {
            return "F" + name;
        }
        return name;
    }

    public string Accept(TArray type)
    {
        return $"TArray<{type.ElementType.Apply(this)}>";
    }

    public string Accept(TList type)
    {
        return $"TArray<{type.ElementType.Apply(this)}>";
    }

    public string Accept(TSet type)
    {
        return $"TSet<{type.ElementType.Apply(this)}>";
    }

    public string Accept(TMap type)
    {
        return $"TMap<{type.KeyType.Apply(this)}, {type.ValueType.Apply(this)}>";
    }

    public virtual string Accept(TDateTime type)
    {
        return "int64";
    }
}

