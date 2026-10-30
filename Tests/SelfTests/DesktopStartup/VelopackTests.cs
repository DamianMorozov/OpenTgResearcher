namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Проверяем фактические точки входа готовых приложений без загрузки WinUI и доступа к пользовательской БД
    private static int CheckVelopackEntryPoints(string[] args)
    {
        if (args.Length != 3 || args[0] != "--velopack")
        {
            Console.WriteLine("Ожидается --velopack <Desktop.dll> <Console.dll>");
            return 1;
        }
        var failures = 0;
        foreach (var path in args.Skip(1))
        {
            try { CheckVelopackEntryPoint(path); Console.WriteLine($"PASS: {Path.GetFileName(path)}: Velopack до запуска приложения"); }
            catch (Exception ex) { failures++; Console.WriteLine($"FAIL: {Path.GetFileName(path)}: {ex.GetBaseException().Message}"); }
        }
        Console.WriteLine($"Результат проверки точек входа: {2 - failures}/2");
        return failures == 0 ? 0 : 1;
    }

    // Читаем IL, чтобы отсутствие вызова, поздний вызов или повторный запуск Velopack не давали ложного успеха
    private static void CheckVelopackEntryPoint(string path)
    {
        using var stream = File.OpenRead(path);
        using var image = new PEReader(stream);
        var metadata = image.GetMetadataReader();
        var entry = (MethodDefinitionHandle)MetadataTokens.Handle(image.PEHeaders.CorHeader!.EntryPointTokenOrRelativeVirtualAddress);
        var calls = ReadMethodCalls(image, metadata, entry);
        var run = calls.IndexOf("Velopack.VelopackApp.Run");
        Assert(run >= 0, "Точка входа не вызывает VelopackApp.Run напрямую до запуска приложения");
        Assert(calls[0] == "Velopack.VelopackApp.Build", $"До Velopack вызывается {calls[0]}");
        Assert(calls.Take(run).All(call => call.StartsWith("Velopack.VelopackApp.", StringComparison.Ordinal) || call == "Velopack.VelopackHook..ctor"),
            "До VelopackApp.Run выполняется код приложения");
        Assert(calls.Count > run + 1, "После Velopack отсутствует запуск приложения");

        var runCalls = metadata.MethodDefinitions.Sum(method => ReadMethodCalls(image, metadata, method).Count(call => call == "Velopack.VelopackApp.Run"));
        Assert(runCalls == 1, $"Ожидался один вызов VelopackApp.Run в сборке, обнаружено {runCalls}");
        if (Path.GetFileNameWithoutExtension(path) == "OpenTgResearcherDesktop")
        {
            Assert(calls.Contains("WinRT.ComWrappersSupport.InitializeComWrappers"), "Потеряна инициализация WinRT");
            Assert(calls.Contains("Microsoft.UI.Xaml.Application.Start"), "Потерян запуск WinUI");
            var attributes = metadata.GetMethodDefinition(entry).GetCustomAttributes();
            Assert(attributes.Any(attribute => GetMethodName(metadata, metadata.GetCustomAttribute(attribute).Constructor) == "System.STAThreadAttribute..ctor"),
                "Точка входа Desktop должна выполняться в STA");
        }
        else Assert(calls.Contains("OpenTgResearcherConsole.Program.RunAsync"), "Потерян запуск обычной работы Console");
    }

    // Декодируем инструкции, пропуская операнды, чтобы байты токенов не принимались за вызовы методов
    private static List<string> ReadMethodCalls(PEReader image, MetadataReader metadata, MethodDefinitionHandle method)
    {
        var definition = metadata.GetMethodDefinition(method);
        if (definition.RelativeVirtualAddress == 0) return [];
        var reader = image.GetMethodBody(definition.RelativeVirtualAddress).GetILReader();
        var calls = new List<string>();
        while (reader.RemainingBytes > 0)
        {
            var value = (ushort)reader.ReadByte();
            if (value == 0xfe) value = (ushort)(0xfe00 | reader.ReadByte());
            var instruction = _ilOpCodes[value];
            switch (instruction.OperandType)
            {
                case OperandType.InlineMethod:
                    var handle = MetadataTokens.EntityHandle(reader.ReadInt32());
                    if (instruction == OpCodes.Call || instruction == OpCodes.Callvirt || instruction == OpCodes.Newobj)
                        calls.Add(GetMethodName(metadata, handle));
                    break;
                case OperandType.InlineSwitch:
                    var targets = reader.ReadInt32();
                    reader.Offset += targets * sizeof(int);
                    break;
                case OperandType.InlineI8:
                case OperandType.InlineR: reader.Offset += 8; break;
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineI:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR: reader.Offset += 4; break;
                case OperandType.InlineVar: reader.Offset += 2; break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar: reader.Offset++; break;
            }
        }
        return calls;
    }

    private static readonly Dictionary<ushort, OpCode> _ilOpCodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!).ToDictionary(op => (ushort)op.Value);

    // Разрешаем имена вызовов через метаданные без выполнения кода проверяемой сборки
    private static string GetMethodName(MetadataReader metadata, EntityHandle handle)
    {
        if (handle.Kind == HandleKind.MethodSpecification) return GetMethodName(metadata, metadata.GetMethodSpecification((MethodSpecificationHandle)handle).Method);
        if (handle.Kind == HandleKind.MethodDefinition)
        {
            var method = metadata.GetMethodDefinition((MethodDefinitionHandle)handle);
            var type = metadata.GetTypeDefinition(method.GetDeclaringType());
            return $"{metadata.GetString(type.Namespace)}.{metadata.GetString(type.Name)}.{metadata.GetString(method.Name)}";
        }
        var member = metadata.GetMemberReference((MemberReferenceHandle)handle);
        if (member.Parent.Kind != HandleKind.TypeReference) return metadata.GetString(member.Name);
        var reference = metadata.GetTypeReference((TypeReferenceHandle)member.Parent);
        return $"{metadata.GetString(reference.Namespace)}.{metadata.GetString(reference.Name)}.{metadata.GetString(member.Name)}";
    }
}
