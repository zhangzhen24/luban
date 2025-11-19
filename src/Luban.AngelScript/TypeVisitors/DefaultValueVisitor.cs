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

namespace Luban.AngelScript.TypeVisitors;

public class DefaultValueVisitor : ITypeFuncVisitor<string>
{
    public static DefaultValueVisitor Ins { get; } = new();

    public string Accept(TBool type)
    {
        return "false";
    }

    public string Accept(TByte type)
    {
        return "0";
    }

    public string Accept(TShort type)
    {
        return "0";
    }

    public string Accept(TInt type)
    {
        return "0";
    }

    public string Accept(TLong type)
    {
        return "0";
    }

    public string Accept(TFloat type)
    {
        return "0.0f";
    }

    public string Accept(TDouble type)
    {
        return "0.0";
    }

    public string Accept(TString type)
    {
        return "\"\"";
    }

    public string Accept(TEnum type)
    {
        // Use the first enum item as default, or None if available
        if (type.DefEnum.Items.Count > 0)
        {
            // Check if there's a "None" item
            var noneItem = type.DefEnum.Items.FirstOrDefault(item => item.Name.Equals("None", StringComparison.OrdinalIgnoreCase));
            if (noneItem != null)
            {
                return $"{type.DefEnum.Name}::{noneItem.Name}";
            }
            var firstItem = type.DefEnum.Items[0];
            return $"{type.DefEnum.Name}::{firstItem.Name}";
        }
        return $"{type.DefEnum.Name}::None";
    }

    public string Accept(TBean type)
    {
        // For structs, return default constructor with F prefix
        if (type.DefBean.IsValueType)
        {
            return $"F{type.DefBean.Name}()";
        }
        return "null";
    }

    public string Accept(TArray type)
    {
        // TArray doesn't need default value, it's initialized by default
        return "";
    }

    public string Accept(TList type)
    {
        // TArray doesn't need default value, it's initialized by default
        return "";
    }

    public string Accept(TSet type)
    {
        // TSet doesn't need default value, it's initialized by default
        return "";
    }

    public string Accept(TMap type)
    {
        // TMap doesn't need default value, it's initialized by default
        return "";
    }

    public string Accept(TDateTime type)
    {
        return "0";
    }
}

