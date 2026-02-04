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

namespace Luban.Typescript.TypeVisitors;

/// <summary>
/// Type name visitor for Puerts - returns simple type names without namespace
/// </summary>
public class PuertsTypeNameVisitor : ITypeFuncVisitor<string>
{
    public static PuertsTypeNameVisitor Ins { get; } = new();

    public string Accept(TBool type) => "boolean";
    public string Accept(TByte type) => "number";
    public string Accept(TShort type) => "number";
    public string Accept(TInt type) => "number";
    public string Accept(TLong type) => type.IsBigInt ? "bigint" : "number";
    public string Accept(TFloat type) => "number";
    public string Accept(TDouble type) => "number";
    public string Accept(TString type) => "string";
    public string Accept(TDateTime type) => "number";

    // Return simple name without namespace
    public string Accept(TEnum type) => type.DefEnum.Name;
    public string Accept(TBean type) => type.DefBean.Name;

    public string Accept(TArray type) => $"{type.ElementType.Apply(this)}[]";
    public string Accept(TList type) => $"{type.ElementType.Apply(this)}[]";
    public string Accept(TSet type) => $"Set<{type.ElementType.Apply(this)}>";
    public string Accept(TMap type) => $"Map<{type.KeyType.Apply(this)}, {type.ValueType.Apply(this)}>";
}

/// <summary>
/// Declaring type name visitor for Puerts - handles nullable types
/// </summary>
public class PuertsDeclaringTypeNameVisitor : DecoratorFuncVisitor<string>
{
    public static PuertsDeclaringTypeNameVisitor Ins { get; } = new();

    public override string DoAccept(TType type)
    {
        var typeName = type.Apply(PuertsTypeNameVisitor.Ins);
        return type.IsNullable ? $"{typeName} | undefined" : typeName;
    }
}
