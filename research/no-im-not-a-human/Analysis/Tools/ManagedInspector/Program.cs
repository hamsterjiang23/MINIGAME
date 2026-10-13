using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Dictionary<ushort, OpCode> Opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!).ToDictionary(o => unchecked((ushort)o.Value));
    private static readonly HashSet<string> CallOps = ["call", "callvirt", "newobj", "ldftn", "ldvirtftn"];
    private static string Token(Handle h) => $"0x{MetadataTokens.GetToken(h):X8}";
    private static string Token(EntityHandle h) => $"0x{MetadataTokens.GetToken(h):X8}";
    private static string Csv(string? s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
    private static string Dot(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "") + "\"";
    private static void WriteJson(string p, object value) => File.WriteAllText(p, JsonSerializer.Serialize(value, JsonOptions), Encoding.UTF8);

    public static int Main(string[] args)
    {
        try
        {
            string Value(string key) { int n = Array.IndexOf(args, key); return n >= 0 && n + 1 < args.Length ? args[n + 1] : throw new ArgumentException($"Missing {key}"); }
            string input = Path.GetFullPath(Value("--input"));
            string output = Path.GetFullPath(Value("--output"));
            Directory.CreateDirectory(output);
            bool smoke = args.Contains("--smoke");
            Inspect(input, output, smoke);
            if (!smoke && args.Contains("--inventory-dir"))
            {
                List<object> inventory = [];
                foreach (string path in Directory.GetFiles(Value("--inventory-dir"), "*.dll").OrderBy(p => p, StringComparer.Ordinal))
                {
                    try { inventory.Add(Summary(path)); }
                    catch (Exception ex) { inventory.Add(new { path, error = ex.GetType().Name, detail = ex.Message }); }
                }
                WriteJson(Path.Combine(output, "assemblies-inventory.json"), inventory);
            }
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.ToString()); return 1; }
    }

    private static object Summary(string path)
    {
        using FileStream fs = File.OpenRead(path);
        using PEReader pe = new(fs);
        if (!pe.HasMetadata) return new { path, managed = false, sha256 = Hash(path) };
        MetadataReader md = pe.GetMetadataReader();
        int bodyCount = 0, noRva = 0, abstractCount = 0, pinvoke = 0, native = 0, tinyRet = 0, errors = 0;
        long ilBytes = 0;
        foreach (MethodDefinitionHandle h in md.MethodDefinitions)
        {
            MethodDefinition m = md.GetMethodDefinition(h);
            if ((m.Attributes & MethodAttributes.Abstract) != 0) abstractCount++;
            if ((m.Attributes & MethodAttributes.PinvokeImpl) != 0) pinvoke++;
            if ((m.ImplAttributes & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL) native++;
            if (m.RelativeVirtualAddress == 0) { noRva++; continue; }
            try { byte[] il = pe.GetMethodBody(m.RelativeVirtualAddress).GetILBytes() ?? []; bodyCount++; ilBytes += il.Length; if (il.Length == 1 && il[0] == 0x2a) tinyRet++; }
            catch (BadImageFormatException) { errors++; }
        }
        string name = md.IsAssembly ? md.GetString(md.GetAssemblyDefinition().Name) : md.GetString(md.GetModuleDefinition().Name);
        return new { path, managed = true, sha256 = Hash(path), assembly_name = name, mvid = md.GetGuid(md.GetModuleDefinition().Mvid).ToString(),
            type_def_count = md.TypeDefinitions.Count, method_def_count = md.MethodDefinitions.Count, field_def_count = md.FieldDefinitions.Count,
            methods_with_readable_il_body = bodyCount, methods_with_no_rva = noRva, abstract_methods = abstractCount, pinvoke_methods = pinvoke,
            non_il_implementation_methods = native, one_byte_ret_methods = tinyRet, total_il_bytes = ilBytes, unreadable_rva_bodies = errors,
            body_classification_limits = "An IL body is observed implementation, not proof it matches the shipped native build. A one-byte ret is an empty body candidate, not proof of artificial stub generation. No-RVA includes abstract/runtime/PInvoke methods." };
    }

    private static string Hash(string path) { using FileStream fs = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant(); }

    private static void Inspect(string path, string output, bool smoke)
    {
        object summary = Summary(path);
        WriteJson(Path.Combine(output, smoke ? "smoke-summary.json" : "summary.json"), summary);
        Console.WriteLine(JsonSerializer.Serialize(summary, JsonOptions));
        if (smoke) return;
        using FileStream fs = File.OpenRead(path);
        using PEReader pe = new(fs);
        MetadataReader md = pe.GetMetadataReader();
        Names names = new(md);
        string assembly = md.GetString(md.GetAssemblyDefinition().Name);
        SignatureProvider sig = new(names);
        List<object> types = [], methods = [], fields = [], edges = [], dependencies = [], bodyErrors = [];
        Dictionary<MethodDefinitionHandle, MethodInfoRow> methodMap = [];
        Dictionary<string, int> typeEdges = [];
        foreach (TypeDefinitionHandle th in md.TypeDefinitions)
        {
            TypeDefinition td = md.GetTypeDefinition(th);
            string tn = names.Type(th);
            string ns = names.Namespace(th);
            TypeDefinitionHandle parent = td.GetDeclaringType();
            string[] interfaces = td.GetInterfaceImplementations().Select(h => names.Type(md.GetInterfaceImplementation(h).Interface)).ToArray();
            types.Add(new { token = Token(th), name = md.GetString(td.Name), @namespace = ns, full_name = tn, declaring_type = parent.IsNil ? null : names.Type(parent),
                attributes = td.Attributes.ToString(), base_type = td.BaseType.IsNil ? null : names.Type(td.BaseType), interfaces,
                method_count = td.GetMethods().Count, field_count = td.GetFields().Count, generic_parameters = td.GetGenericParameters().Select(h => md.GetString(md.GetGenericParameter(h).Name)).ToArray() });
            foreach (MethodDefinitionHandle mh in td.GetMethods())
            {
                MethodDefinition m = md.GetMethodDefinition(mh);
                MethodSignature<string> ms = m.DecodeSignature(sig, null);
                string mn = md.GetString(m.Name);
                string display = $"{tn}::{mn}({string.Join(", ", ms.ParameterTypes)}) -> {ms.ReturnType}";
                methodMap.Add(mh, new MethodInfoRow(Token(mh), tn, ns, mn, display, ms));
            }
            foreach (FieldDefinitionHandle fh in td.GetFields())
            {
                FieldDefinition f = md.GetFieldDefinition(fh);
                ConstantHandle ch = f.GetDefaultValue();
                fields.Add(new { token = Token(fh), declaring_type = tn, @namespace = ns, name = md.GetString(f.Name), field_type = f.DecodeSignature(sig, null),
                    attributes = f.Attributes.ToString(), rva = f.GetRelativeVirtualAddress(), constant = ch.IsNil ? null : ConstantValue(md, ch) });
            }
        }
        using StreamWriter cilOut = new(Path.Combine(output, "method-il.jsonl"), false, Encoding.UTF8);
        using StreamWriter textOut = new(Path.Combine(output, "method-il.txt"), false, Encoding.UTF8);
        foreach ((MethodDefinitionHandle mh, MethodInfoRow mi) in methodMap)
        {
            MethodDefinition m = md.GetMethodDefinition(mh);
            Dictionary<int, string> parameterNames = m.GetParameters().Select(h => md.GetParameter(h)).Where(p => p.SequenceNumber > 0)
                .ToDictionary(p => p.SequenceNumber, p => md.GetString(p.Name));
            var parameters = mi.Signature.ParameterTypes.Select((p, i) => new { position = i + 1, name = parameterNames.GetValueOrDefault(i + 1, ""), type = p }).ToArray();
            string[] genericParams = m.GetGenericParameters().Select(h => md.GetString(md.GetGenericParameter(h).Name)).ToArray();
            byte[] il = [];
            MethodBodyBlock? body = null;
            string classification = m.RelativeVirtualAddress == 0 ? "no_rva" : "unreadable_rva";
            if (m.RelativeVirtualAddress != 0)
            {
                try { body = pe.GetMethodBody(m.RelativeVirtualAddress); il = body.GetILBytes() ?? []; classification = il.Length == 1 && il[0] == 0x2a ? "real_cil_one_byte_ret_empty_candidate" : "real_cil_body"; }
                catch (BadImageFormatException ex) { bodyErrors.Add(new { token = mi.Token, signature = mi.Display, rva = m.RelativeVirtualAddress, error = ex.Message }); }
            }
            string[] locals = body is not null && !body.LocalSignature.IsNil ? md.GetStandaloneSignature(body.LocalSignature).DecodeLocalSignature(sig, null).ToArray() : [];
            methods.Add(new { token = mi.Token, declaring_type = mi.Type, @namespace = mi.Namespace, name = mi.Name, full_signature = mi.Display,
                return_type = mi.Signature.ReturnType, parameters, generic_parameters = genericParams, calling_convention = mi.Signature.Header.CallingConvention.ToString(),
                attributes = m.Attributes.ToString(), implementation_attributes = m.ImplAttributes.ToString(), rva = m.RelativeVirtualAddress,
                body_classification = classification, il_length = il.Length, il_sha256 = body is null ? null : Convert.ToHexString(SHA256.HashData(il)).ToLowerInvariant(),
                max_stack = body?.MaxStack, local_variables_initialized = body?.LocalVariablesInitialized, locals,
                exception_regions = body?.ExceptionRegions.Select(r => new { kind = r.Kind.ToString(), try_offset = r.TryOffset, try_length = r.TryLength,
                    handler_offset = r.HandlerOffset, handler_length = r.HandlerLength, filter_offset = r.FilterOffset, catch_type = r.CatchType.IsNil ? null : names.Type(r.CatchType) }).ToArray() });
            if (body is null) continue;
            List<object> instructions = [];
            textOut.WriteLine($"// {mi.Token} RVA 0x{m.RelativeVirtualAddress:X8} {mi.Display}");
            foreach (Instruction ins in DecodeIL(il))
            {
                object? resolved = null;
                string? operandDescription = null;
                if (ins.MetadataToken is int mt)
                {
                    try
                    {
                        Handle h = MetadataTokens.Handle(mt);
                        if (h.Kind == HandleKind.UserString) { operandDescription = md.GetUserString((UserStringHandle)h); resolved = new { user_string = operandDescription }; }
                        else { Target target = Resolve(md, names, sig, (EntityHandle)h, methodMap); resolved = target; operandDescription = target.Signature; }
                    }
                    catch (Exception ex) when (ex is BadImageFormatException or ArgumentException or InvalidOperationException)
                    { resolved = new { token = $"0x{mt:X8}", resolution_error = ex.Message }; }
                }
                instructions.Add(new { offset = ins.Offset, opcode = ins.Opcode, operand_hex = ins.OperandHex, metadata_token = ins.MetadataToken is int t ? $"0x{t:X8}" : null, resolved_operand = resolved });
                textOut.WriteLine($"IL_{ins.Offset:X4}: {ins.Opcode} {ins.OperandHex} {operandDescription}");
                if (CallOps.Contains(ins.Opcode) && resolved is Target callee)
                {
                    edges.Add(new { caller_token = mi.Token, caller_type = mi.Type, caller_signature = mi.Display, il_offset = ins.Offset, opcode = ins.Opcode,
                        callee_operand_token = callee.Token, callee_definition_token = callee.DefinitionToken, callee_type = callee.DeclaringType, callee_name = callee.Name,
                        callee_signature = callee.Signature, callee_assembly = callee.AssemblyName,
                        relation = ins.Opcode is "ldftn" or "ldvirtftn" ? "function_pointer_reference" : ins.Opcode == "newobj" ? "constructor_call" : "call_instruction",
                        dispatch = ins.Opcode is "callvirt" or "ldvirtftn" ? "declared_target_only_runtime_receiver_unresolved" : "encoded_target",
                        scope = callee.DefinitionToken is null ? "external_or_unresolved_member_reference" : "same_assembly_method_definition" });
                    if (callee.DefinitionToken is not null)
                    {
                        string key = mi.Type + "\t" + callee.DeclaringType;
                        typeEdges[key] = typeEdges.GetValueOrDefault(key) + 1;
                    }
                }
            }
            textOut.WriteLine();
            cilOut.WriteLine(JsonSerializer.Serialize(new { token = mi.Token, signature = mi.Display, rva = m.RelativeVirtualAddress, il_hex = Convert.ToHexString(il), instructions }));
        }
        foreach (AssemblyReferenceHandle ah in md.AssemblyReferences)
        {
            AssemblyReference a = md.GetAssemblyReference(ah);
            dependencies.Add(new { source_assembly = assembly, token = Token(ah), target_assembly = md.GetString(a.Name), version = a.Version.ToString(),
                culture = a.Culture.IsNil ? "" : md.GetString(a.Culture), public_key_or_token = Convert.ToHexString(md.GetBlobBytes(a.PublicKeyOrToken)), flags = a.Flags.ToString() });
        }
        WriteJson(Path.Combine(output, "types.json"), types); WriteJson(Path.Combine(output, "methods.json"), methods); WriteJson(Path.Combine(output, "fields.json"), fields);
        WriteJson(Path.Combine(output, "call-edges.json"), edges); WriteJson(Path.Combine(output, "assembly-dependencies.json"), dependencies); WriteJson(Path.Combine(output, "body-errors.json"), bodyErrors);
        var aggregate = typeEdges.Select(kv => new { caller_type = kv.Key.Split('\t')[0], callee_type = kv.Key.Split('\t')[1], il_reference_count = kv.Value }).ToArray();
        WriteJson(Path.Combine(output, "type-call-edges.json"), aggregate);
        StringBuilder dot = new("digraph TypeCalls {\n  rankdir=LR;\n");
        foreach (var e in aggregate) dot.AppendLine($"  {Dot(e.caller_type)} -> {Dot(e.callee_type)} [label={Dot(e.il_reference_count.ToString())}];");
        dot.AppendLine("}"); File.WriteAllText(Path.Combine(output, "type-call-graph.dot"), dot.ToString(), Encoding.UTF8);
        StringBuilder depsDot = new("digraph AssemblyDependencies {\n  rankdir=LR;\n");
        StringBuilder depsMermaid = new("flowchart LR\n  source[\"" + assembly + "\"]\n");
        int depIndex = 0;
        foreach (AssemblyReferenceHandle ah in md.AssemblyReferences)
        { string target = md.GetString(md.GetAssemblyReference(ah).Name); depsDot.AppendLine($"  {Dot(assembly)} -> {Dot(target)};"); depsMermaid.AppendLine($"  source --> dep{depIndex++}[\"{target}\"]"); }
        depsDot.AppendLine("}"); File.WriteAllText(Path.Combine(output, "assembly-dependencies.dot"), depsDot.ToString(), Encoding.UTF8); File.WriteAllText(Path.Combine(output, "assembly-dependencies.mmd"), depsMermaid.ToString(), Encoding.UTF8);
        WriteCsv(Path.Combine(output, "types.csv"), types, ["token", "namespace", "full_name", "declaring_type", "base_type", "method_count", "field_count"]);
        WriteCsv(Path.Combine(output, "methods.csv"), methods, ["token", "declaring_type", "name", "full_signature", "rva", "body_classification", "il_length"]);
        WriteCsv(Path.Combine(output, "fields.csv"), fields, ["token", "declaring_type", "name", "field_type", "attributes", "rva"]);
        WriteCsv(Path.Combine(output, "call-edges.csv"), edges, ["caller_token", "caller_type", "caller_signature", "il_offset", "opcode", "callee_operand_token", "callee_definition_token", "callee_type", "callee_name", "callee_signature", "callee_assembly", "relation", "dispatch", "scope"]);
        WriteJson(Path.Combine(output, "coverage.json"), new { type_rows = types.Count, method_rows = methods.Count, field_rows = fields.Count, call_edges = edges.Count,
            type_call_edges = aggregate.Length, assembly_references = dependencies.Count, body_errors = bodyErrors.Count,
            read_target_execution = false, target_assembly_loaded_via_reflection = false, decoder = "System.Reflection.Metadata PEReader with framework opcode table",
            limitations = new[] { "callvirt/ldvirtftn targets are declared methods, not a points-to or virtual-dispatch analysis.", "Unity serialization/events, reflection, native callbacks and dynamically loaded dependencies are not resolved by this graph.", "ldftn/ldvirtftn are function-pointer references, not evidence the pointed-to function was called.", "IL2CPP runtime equivalence with shipped native implementation requires C++/native correlation.", "No-RVA and one-byte-ret entries are explicitly distinguished; no inferred C# implementation generated." } });
    }

    private static void WriteCsv(string path, List<object> values, string[] keys)
    {
        using StreamWriter w = new(path, false, Encoding.UTF8); w.WriteLine(string.Join(",", keys.Select(Csv)));
        foreach (object row in values) { JsonElement j = JsonSerializer.SerializeToElement(row); w.WriteLine(string.Join(",", keys.Select(k => Csv(j.TryGetProperty(k, out JsonElement p) ? p.ToString() : "")))); }
    }

    private static string ConstantValue(MetadataReader md, ConstantHandle h)
    {
        Constant c = md.GetConstant(h); BlobReader r = md.GetBlobReader(c.Value);
        return c.TypeCode switch { ConstantTypeCode.Boolean => r.ReadBoolean().ToString(), ConstantTypeCode.Char => ((char)r.ReadUInt16()).ToString(),
            ConstantTypeCode.SByte => r.ReadSByte().ToString(), ConstantTypeCode.Byte => r.ReadByte().ToString(), ConstantTypeCode.Int16 => r.ReadInt16().ToString(),
            ConstantTypeCode.UInt16 => r.ReadUInt16().ToString(), ConstantTypeCode.Int32 => r.ReadInt32().ToString(), ConstantTypeCode.UInt32 => r.ReadUInt32().ToString(),
            ConstantTypeCode.Int64 => r.ReadInt64().ToString(), ConstantTypeCode.UInt64 => r.ReadUInt64().ToString(), ConstantTypeCode.Single => r.ReadSingle().ToString(System.Globalization.CultureInfo.InvariantCulture),
            ConstantTypeCode.Double => r.ReadDouble().ToString(System.Globalization.CultureInfo.InvariantCulture), ConstantTypeCode.String => r.ReadUTF16(r.Length),
            ConstantTypeCode.NullReference => "null", _ => Convert.ToHexString(md.GetBlobBytes(c.Value)) };
    }

    private static IEnumerable<Instruction> DecodeIL(byte[] il)
    {
        int offset = 0;
        while (offset < il.Length)
        {
            int start = offset; ushort op = il[offset++]; if (op == 0xfe) { if (offset >= il.Length) throw new BadImageFormatException("Truncated IL opcode"); op = (ushort)(0xfe00 | il[offset++]); }
            if (!Opcodes.TryGetValue(op, out OpCode code)) throw new BadImageFormatException($"Unknown IL opcode {op:X4} at {start}");
            int length = code.OperandType switch { OperandType.InlineNone => 0, OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2, OperandType.InlineI8 or OperandType.InlineR => 8, OperandType.InlineSwitch => offset + 4 <= il.Length ? checked(4 + BitConverter.ToInt32(il, offset) * 4) : throw new BadImageFormatException("Truncated switch"), _ => 4 };
            if (length < 0 || offset + length > il.Length) throw new BadImageFormatException($"Truncated IL operand at {start}");
            int? token = code.OperandType is OperandType.InlineMethod or OperandType.InlineField or OperandType.InlineType or OperandType.InlineTok or OperandType.InlineString or OperandType.InlineSig ? BitConverter.ToInt32(il, offset) : null;
            string hex = Convert.ToHexString(il.AsSpan(offset, length)); offset += length;
            yield return new Instruction(start, code.Name ?? $"opcode_{op:X4}", hex, token);
        }
    }

    private static Target Resolve(MetadataReader md, Names names, SignatureProvider sig, EntityHandle h, Dictionary<MethodDefinitionHandle, MethodInfoRow> methods)
    {
        if (h.Kind == HandleKind.MethodDefinition)
        { MethodInfoRow m = methods[(MethodDefinitionHandle)h]; return new Target(Token(h), m.Token, m.Type, m.Name, m.Display, names.AssemblyName); }
        if (h.Kind == HandleKind.MemberReference)
        {
            MemberReference m = md.GetMemberReference((MemberReferenceHandle)h); string parent = names.Type(m.Parent); string name = md.GetString(m.Name);
            string? definition = null;
            string display;
            if (m.GetKind() == MemberReferenceKind.Method)
            {
                MethodSignature<string> declaredSignature = m.DecodeMethodSignature(sig, null);
                display = FormatMethod(parent, name, declaredSignature);
                EntityHandle root = names.RootType(m.Parent);
                if (root.Kind == HandleKind.TypeDefinition)
                {
                    // Bind only a unique, exact declaration in the actual metadata root type.
                    // This is static MemberRef-to-MethodDef identity, not runtime virtual dispatch.
                    MethodInfoRow[] candidates = md.GetTypeDefinition((TypeDefinitionHandle)root).GetMethods()
                        .Select(handle => methods[handle]).Where(row => row.Name == name && SameSignature(row.Signature, declaredSignature)).ToArray();
                    if (candidates.Length == 1) definition = candidates[0].Token;
                }
            }
            else display = $"{parent}::{name} : {m.DecodeFieldSignature(sig, null)}";
            return new Target(Token(h), definition, parent, name, display, names.AssemblyScope(m.Parent));
        }
        if (h.Kind == HandleKind.MethodSpecification)
        {
            MethodSpecification ms = md.GetMethodSpecification((MethodSpecificationHandle)h); Target inner = Resolve(md, names, sig, ms.Method, methods);
            string generic = string.Join(", ", ms.DecodeSignature(sig, null)); return inner with { Token = Token(h), Signature = inner.Signature + $" [generic<{generic}>]" };
        }
        if (h.Kind == HandleKind.FieldDefinition)
        { FieldDefinition f = md.GetFieldDefinition((FieldDefinitionHandle)h); string type = names.Type(f.GetDeclaringType()); string name = md.GetString(f.Name); return new Target(Token(h), null, type, name, $"{type}::{name} : {f.DecodeSignature(sig, null)}", names.AssemblyName); }
        return new Target(Token(h), null, names.Type(h), "", names.Type(h), names.AssemblyScope(h));
    }
    private static bool SameSignature(MethodSignature<string> left, MethodSignature<string> right) =>
        left.Header.RawValue == right.Header.RawValue && left.GenericParameterCount == right.GenericParameterCount &&
        left.RequiredParameterCount == right.RequiredParameterCount && left.ReturnType == right.ReturnType &&
        left.ParameterTypes.SequenceEqual(right.ParameterTypes);
    private static string FormatMethod(string type, string name, MethodSignature<string> m) => $"{type}::{name}({string.Join(", ", m.ParameterTypes)}) -> {m.ReturnType}";
    private sealed record Instruction(int Offset, string Opcode, string OperandHex, int? MetadataToken);
    private sealed record MethodInfoRow(string Token, string Type, string Namespace, string Name, string Display, MethodSignature<string> Signature);
    private sealed record Target(string Token, string? DefinitionToken, string DeclaringType, string Name, string Signature, string? AssemblyName);

    private sealed class Names(MetadataReader md)
    {
        public string AssemblyName => md.IsAssembly ? md.GetString(md.GetAssemblyDefinition().Name) : "<module>";
        public string Namespace(TypeDefinitionHandle h) { TypeDefinition t = md.GetTypeDefinition(h); return t.GetDeclaringType().IsNil ? md.GetString(t.Namespace) : Namespace(t.GetDeclaringType()); }
        public string Type(EntityHandle h)
        {
            if (h.IsNil) return "<nil>";
            if (h.Kind == HandleKind.TypeDefinition) { TypeDefinition t = md.GetTypeDefinition((TypeDefinitionHandle)h); string n = md.GetString(t.Name); TypeDefinitionHandle p = t.GetDeclaringType(); return p.IsNil ? Join(md.GetString(t.Namespace), n) : Type(p) + "+" + n; }
            if (h.Kind == HandleKind.TypeReference) { TypeReference t = md.GetTypeReference((TypeReferenceHandle)h); string n = md.GetString(t.Name); return t.ResolutionScope.Kind == HandleKind.TypeReference ? Type(t.ResolutionScope) + "+" + n : Join(md.GetString(t.Namespace), n); }
            if (h.Kind == HandleKind.TypeSpecification) return md.GetTypeSpecification((TypeSpecificationHandle)h).DecodeSignature(new SignatureProvider(this), null);
            if (h.Kind == HandleKind.ModuleReference) return md.GetString(md.GetModuleReference((ModuleReferenceHandle)h).Name);
            if (h.Kind == HandleKind.MethodDefinition) return Type(md.GetMethodDefinition((MethodDefinitionHandle)h).GetDeclaringType());
            return h.Kind + ":" + Token(h);
        }
        public string? AssemblyScope(EntityHandle h)
        {
            if (h.Kind is HandleKind.TypeDefinition or HandleKind.MethodDefinition or HandleKind.FieldDefinition) return AssemblyName;
            if (h.Kind == HandleKind.TypeReference) { EntityHandle scope = md.GetTypeReference((TypeReferenceHandle)h).ResolutionScope; if (scope.Kind == HandleKind.AssemblyReference) return md.GetString(md.GetAssemblyReference((AssemblyReferenceHandle)scope).Name); return scope.Kind == HandleKind.TypeReference ? AssemblyScope(scope) : AssemblyName; }
            if (h.Kind == HandleKind.TypeSpecification)
            {
                EntityHandle root = RootType(h);
                return root.IsNil ? null : AssemblyScope(root);
            }
            return null;
        }
        public EntityHandle RootType(EntityHandle h) => h.Kind == HandleKind.TypeSpecification
            ? md.GetTypeSpecification((TypeSpecificationHandle)h).DecodeSignature(new GenericRootProvider(), null)
            : h.Kind is HandleKind.TypeDefinition or HandleKind.TypeReference ? h : default;
        private static string Join(string ns, string n) => ns.Length == 0 ? n : ns + "." + n;
    }

    // Preserve the generic declaration's metadata handle, not an argument's namespace or assembly.
    // Non-nominal signatures (array/pointer/primitive/generic-parameter) deliberately remain unknown.
    private sealed class GenericRootProvider : ISignatureTypeProvider<EntityHandle, object?>
    {
        public EntityHandle GetArrayType(EntityHandle elementType, ArrayShape shape) => default;
        public EntityHandle GetByReferenceType(EntityHandle elementType) => default;
        public EntityHandle GetFunctionPointerType(MethodSignature<EntityHandle> signature) => default;
        public EntityHandle GetGenericInstantiation(EntityHandle genericType, ImmutableArray<EntityHandle> typeArguments) => genericType;
        public EntityHandle GetGenericMethodParameter(object? genericContext, int index) => default;
        public EntityHandle GetGenericTypeParameter(object? genericContext, int index) => default;
        public EntityHandle GetModifiedType(EntityHandle modifier, EntityHandle unmodifiedType, bool isRequired) => unmodifiedType;
        public EntityHandle GetPinnedType(EntityHandle elementType) => elementType;
        public EntityHandle GetPointerType(EntityHandle elementType) => default;
        public EntityHandle GetPrimitiveType(PrimitiveTypeCode typeCode) => default;
        public EntityHandle GetSZArrayType(EntityHandle elementType) => default;
        public EntityHandle GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => handle;
        public EntityHandle GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => handle;
        public EntityHandle GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
    }

    private sealed class SignatureProvider(Names names) : ISignatureTypeProvider<string, object?>
    {
        public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[" + new string(',', Math.Max(0, shape.Rank - 1)) + "]";
        public string GetByReferenceType(string elementType) => elementType + "&";
        public string GetFunctionPointerType(MethodSignature<string> signature) => "methodptr(" + string.Join(", ", signature.ParameterTypes) + ") -> " + signature.ReturnType;
        public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) => genericType + "<" + string.Join(", ", typeArguments) + ">";
        public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;
        public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;
        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType + (isRequired ? " modreq(" : " modopt(") + modifier + ")";
        public string GetPinnedType(string elementType) => elementType + " pinned";
        public string GetPointerType(string elementType) => elementType + "*";
        public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode switch { PrimitiveTypeCode.Void => "void", PrimitiveTypeCode.Boolean => "bool", PrimitiveTypeCode.Char => "char", PrimitiveTypeCode.SByte => "sbyte", PrimitiveTypeCode.Byte => "byte", PrimitiveTypeCode.Int16 => "short", PrimitiveTypeCode.UInt16 => "ushort", PrimitiveTypeCode.Int32 => "int", PrimitiveTypeCode.UInt32 => "uint", PrimitiveTypeCode.Int64 => "long", PrimitiveTypeCode.UInt64 => "ulong", PrimitiveTypeCode.Single => "float", PrimitiveTypeCode.Double => "double", PrimitiveTypeCode.String => "string", PrimitiveTypeCode.Object => "object", PrimitiveTypeCode.IntPtr => "nint", PrimitiveTypeCode.UIntPtr => "nuint", PrimitiveTypeCode.TypedReference => "typedref", _ => typeCode.ToString() };
        public string GetSZArrayType(string elementType) => elementType + "[]";
        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => names.Type(handle);
        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => names.Type(handle);
        public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
    }
}
