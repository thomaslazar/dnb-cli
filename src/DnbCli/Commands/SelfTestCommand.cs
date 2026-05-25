using System.CommandLine;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using DnbCli.Marc;
using DnbCli.Models;

namespace DnbCli.Commands;

public static class SelfTestCommand
{
    public static Command Create()
    {
        var command = new Command("self-test", "Verify AOT binary integrity — exercises parse + serialize without network");
        command.AddExamples("dnb self-test");

        command.SetAction(parseResult =>
        {
            var pass = 0;
            var fail = 0;
            void Check(string label, Action test)
            {
                try { test(); Console.Error.WriteLine($"  PASS: {label}"); pass++; }
                catch (Exception ex) { Console.Error.WriteLine($"  FAIL: {label} — {ex.Message}"); fail++; }
            }

            Console.Error.WriteLine("=== Embedded fixture parsing ===");

            Check("Embedded MARC fixture loads", () =>
            {
                var xml = ReadEmbeddedString("selftest-record.xml");
                Assert(!string.IsNullOrWhiteSpace(xml), "fixture empty");
                Assert(xml.Contains("<record"), "fixture has no <record> element");
            });

            Check("ParseRecord populates expected fields", () =>
            {
                var xml = ReadEmbeddedString("selftest-record.xml");
                var rec = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1314588753");
                Assert(rec.DnbId == "1314588753", $"dnbId: {rec.DnbId}");
                Assert(rec.Isbns.Contains("9783837165890"), "ISBN missing");
                Assert(rec.Languages.Publication.Contains("ger"), "language ger missing");
                Assert(rec.Languages.Original.Contains("eng"), "original language eng missing");
                Assert(rec.Contributors.Any(c => c.Role == "trl"), "translator not surfaced");
                Assert(rec.Series.Count == 1 && rec.Series[0].Volume == "15", $"series mismatch");
            });

            Check("DnbRecord serializes via source-gen", () =>
            {
                var xml = ReadEmbeddedString("selftest-record.xml");
                var rec = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1314588753");
                var json = JsonSerializer.Serialize(rec, JsonContext.Default.DnbRecord);
                Assert(json.Contains("\"dnbId\":\"1314588753\""), "dnbId not in output");
                JsonDocument.Parse(json); // throws on invalid JSON
            });

            Check("SearchEnvelope serializes via source-gen", () =>
            {
                var env = new SearchEnvelope { Query = "TIT=test", TotalResults = 0, Results = new List<DnbRecord>() };
                var json = JsonSerializer.Serialize(env, JsonContext.Default.SearchEnvelope);
                Assert(json.Contains("\"query\""), "query missing");
                JsonDocument.Parse(json);
            });

            Console.Error.WriteLine();
            Console.Error.WriteLine($"========================================");
            Console.Error.WriteLine($"Results: {pass} passed, {fail} failed");
            Console.Error.WriteLine($"========================================");
            if (fail > 0) Environment.Exit(1);
            return 0;
        });

        return command;
    }

    private static string ReadEmbeddedString(string logicalName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(logicalName)
            ?? throw new InvalidOperationException($"Embedded resource not found: {logicalName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
