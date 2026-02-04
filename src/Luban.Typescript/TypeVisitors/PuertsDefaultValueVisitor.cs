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
/// Default value visitor for TypeScript Puerts
/// </summary>
public class PuertsDefaultValueVisitor : ITypeFuncVisitor<string>
{
    public static PuertsDefaultValueVisitor Ins { get; } = new();

    public string Accept(TBool type) => "false";
    public string Accept(TByte type) => "0";
    public string Accept(TShort type) => "0";
    public string Accept(TInt type) => "0";
    public string Accept(TLong type) => type.IsBigInt ? "0n" : "0";
    public string Accept(TFloat type) => "0";
    public string Accept(TDouble type) => "0";
    public string Accept(TString type) => "\"\"";
    public string Accept(TDateTime type) => "0";

    public string Accept(TEnum type) => "0";
    public string Accept(TBean type) => $"new {type.DefBean.Name}()";

    public string Accept(TArray type) => "[]";
    public string Accept(TList type) => "[]";
    public string Accept(TSet type) => "new Set()";
    public string Accept(TMap type) => "new Map()";
}
