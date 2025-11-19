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

using Luban.AngelScript;
using Luban.Types;
using Luban.TypeVisitors;
using Luban.Utils;

namespace Luban.AngelScript.TypeVisitors;

// JSON反序列化Visitor，用于Unreal Angelscript
// 使用TSharedPtr<FJsonObject>和FJsonObject进行JSON解析
public class JsonDeserializeVisitor : ITypeFuncVisitor<string, string, int, string>
{
    public static JsonDeserializeVisitor Ins { get; } = new();

    public string Accept(TBool type, string json, string x, int depth)
    {
        // For basic types, JSON value might be directly in the field or wrapped in "value"
        return $"{x} = {json}->GetBoolField(\"value\");";
    }

    public string Accept(TByte type, string json, string x, int depth)
    {
        return $"{x} = {json}->GetIntegerField(\"value\");";
    }

    public string Accept(TShort type, string json, string x, int depth)
    {
        return $"{x} = {json}->GetIntegerField(\"value\");";
    }

    public string Accept(TInt type, string json, string x, int depth)
    {
        return $"{x} = {json}->GetIntegerField(\"value\");";
    }

    public string Accept(TLong type, string json, string x, int depth)
    {
        return $"{x} = {json}->GetNumberField(\"value\");";
    }

    public string Accept(TFloat type, string json, string x, int depth)
    {
        return $"{x} = {json}->GetNumberField(\"value\");";
    }

    public string Accept(TDouble type, string json, string x, int depth)
    {
        return $"{x} = {json}->GetNumberField(\"value\");";
    }

    public string Accept(TEnum type, string json, string x, int depth)
    {
        return $"{x} = ({type.Apply(DeclaringTypeNameVisitor.Ins)}){json}->GetIntegerField(\"value\");";
    }

    public string Accept(TString type, string json, string x, int depth)
    {
        return $"{x} = {json}->GetStringField(\"value\");";
    }

    public string Accept(TDateTime type, string json, string x, int depth)
    {
        return $"{x} = {json}->GetNumberField(\"value\");";
    }

    public string Accept(TBean type, string json, string x, int depth)
    {
        string src = $"{AngelScriptUtil.GetFullNameWithGlobalQualifier(type.DefBean)}.Deserialize{type.DefBean.Name}({json})";
        string constructor = type.DefBean.TypeConstructorWithTypeMapper();
        return $"{x} = {(string.IsNullOrEmpty(constructor) ? src : $"{constructor}({src})")};";
    }

    public string Accept(TArray type, string json, string x, int depth)
    {
        string __v = $"__v{depth}";
        string __json = $"__json{depth}";
        string __arr = $"__arr{depth}";
        string __index = $"__index{depth}";
        return $"{{ TArray<TSharedPtr<FJsonValue>> {__arr} = {json}->GetArrayField(\"value\"); {x}.Empty(); {x}.Reserve({__arr}.Num()); for (int {__index} = 0; {__index} < {__arr}.Num(); {__index}++) {{ TSharedPtr<FJsonObject> {__json} = {__arr}[{__index}]->AsObject(); {type.ElementType.Apply(DeclaringTypeNameVisitor.Ins)} {__v}; {type.ElementType.Apply(this, __json, __v, depth + 1)} {x}.Add({__v}); }} }}";
    }

    public string Accept(TList type, string json, string x, int depth)
    {
        string __v = $"__v{depth}";
        string __json = $"__json{depth}";
        string __arr = $"__arr{depth}";
        string __index = $"__index{depth}";
        return $"{{ TArray<TSharedPtr<FJsonValue>> {__arr} = {json}->GetArrayField(\"value\"); {x}.Empty(); {x}.Reserve({__arr}.Num()); for (int {__index} = 0; {__index} < {__arr}.Num(); {__index}++) {{ TSharedPtr<FJsonObject> {__json} = {__arr}[{__index}]->AsObject(); {type.ElementType.Apply(DeclaringTypeNameVisitor.Ins)} {__v}; {type.ElementType.Apply(this, __json, __v, depth + 1)} {x}.Add({__v}); }} }}";
    }

    public string Accept(TSet type, string json, string x, int depth)
    {
        string __v = $"__v{depth}";
        string __json = $"__json{depth}";
        string __arr = $"__arr{depth}";
        string __index = $"__index{depth}";
        return $"{{ TArray<TSharedPtr<FJsonValue>> {__arr} = {json}->GetArrayField(\"value\"); {x}.Empty(); {x}.Reserve({__arr}.Num()); for (int {__index} = 0; {__index} < {__arr}.Num(); {__index}++) {{ TSharedPtr<FJsonObject> {__json} = {__arr}[{__index}]->AsObject(); {type.ElementType.Apply(DeclaringTypeNameVisitor.Ins)} {__v}; {type.ElementType.Apply(this, __json, __v, depth + 1)} {x}.Add({__v}); }} }}";
    }

    public string Accept(TMap type, string json, string x, int depth)
    {
        string __k = $"_k{depth}";
        string __v = $"_v{depth}";
        string __json = $"__json{depth}";
        string __arr = $"__arr{depth}";
        string __index = $"__index{depth}";
        string __pair = $"__pair{depth}";
        return $"{{ TArray<TSharedPtr<FJsonValue>> {__arr} = {json}->GetArrayField(\"value\"); {x}.Empty(); {x}.Reserve({__arr}.Num()); for (int {__index} = 0; {__index} < {__arr}.Num(); {__index}++) {{ TSharedPtr<FJsonObject> {__pair} = {__arr}[{__index}]->AsObject(); {type.KeyType.Apply(DeclaringTypeNameVisitor.Ins)} {__k}; {type.KeyType.Apply(this, $"{__pair}->GetObjectField(\"key\")", __k, depth + 1)} {type.ValueType.Apply(DeclaringTypeNameVisitor.Ins)} {__v}; {type.ValueType.Apply(this, $"{__pair}->GetObjectField(\"value\")", __v, depth + 1)} {x}.Add({__k}, {__v}); }} }}";
    }
}

