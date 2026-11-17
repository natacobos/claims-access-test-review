// Scoring for the interviewer: dotnet run --project check
// Runs the tests in tests/Claims.Tests/MyTests.cs against several versions of ClaimsService.
// Prints one line per task. Do not edit this folder: its fingerprint is printed at the end.
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

var root = FindRoot();
var myTests = Path.Combine(root, "tests", "Claims.Tests", "MyTests.cs");
Console.WriteLine("Checking your tests (about a minute)...\n");

var runs = new Dictionary<string, (int Total, int Passed, int Failed)>();
Parallel.ForEach(Variants.All, new ParallelOptions { MaxDegreeOfParallelism = 4 }, v =>
{
    var r = RunAgainst(v.Source, myTests);
    lock (runs) runs[v.Name] = r;
});
var reference = runs["reference"];
var onAgentCode = RunTests(Path.Combine(root, "tests", "Claims.Tests", "Claims.Tests.csproj"), "FullyQualifiedName~Claims.Tests.MyTests");

bool Caught(string name) => runs[name].Total > 0 && runs[name].Failed > reference.Failed;

if (reference.Total == 0)
{
    Console.WriteLine("T1 Your tests: 0 tests found in MyTests.cs (or they do not compile)");
}
else
{
    Console.WriteLine($"T1 Your tests: {reference.Total} tests | pass on code that follows the rules in FINDING.md: {reference.Passed}/{reference.Total}");
    var mutants = Variants.All.Select(v => v.Name).Where(n => n.StartsWith("m")).ToList();
    Console.WriteLine($"T2 Broken versions your tests catch: {mutants.Count(Caught)} of {mutants.Count}");
    Console.WriteLine($"T3 Problems in the agent's fix your tests show: {(Caught("bug_list") ? 1 : 0) + (Caught("bug_household") ? 1 : 0)} of 2");
}

var notes = File.Exists(Path.Combine(root, "NOTES.md")) ? File.ReadAllText(Path.Combine(root, "NOTES.md")) : "";
var sections = Regex.Split(notes, @"^## ", RegexOptions.Multiline).Skip(1)
    .Count(s => Regex.Matches(string.Join(" ", s.Split('\n').Skip(1)), @"[A-Za-z]{2,}").Count >= 10);
Console.WriteLine($"T4 Notes: {Math.Min(sections, 3)}/3 sections written");

Console.WriteLine();
Console.WriteLine($"Your tests on the agent's code (src/): {onAgentCode.Passed} passed, {onAgentCode.Failed} failed");
Console.WriteLine($"Check files: {Fingerprint()}");
return 0;

// ---------- helpers ----------
static string FindRoot()
{
    var d = new DirectoryInfo(AppContext.BaseDirectory);
    while (d != null && !File.Exists(Path.Combine(d.FullName, "Directory.Build.props"))) d = d.Parent;
    return d?.FullName ?? Directory.GetCurrentDirectory();
}

static (int Total, int Passed, int Failed) RunTests(string project, string filter)
{
    var results = Path.Combine(Path.GetTempPath(), "check-" + Guid.NewGuid().ToString("N"));
    var p = Process.Start(new ProcessStartInfo("dotnet",
        $"test \"{project}\" --nologo --filter \"{filter}\" --logger \"trx;LogFileName=r.trx\" --results-directory \"{results}\"")
        { RedirectStandardOutput = true, RedirectStandardError = true })!;
    p.StandardOutput.ReadToEnd(); p.StandardError.ReadToEnd(); p.WaitForExit();
    var trx = Path.Combine(results, "r.trx");
    if (!File.Exists(trx)) return (0, 0, 0);
    var m = Regex.Match(File.ReadAllText(trx), "<Counters total=\"(\\d+)\" executed=\"\\d+\" passed=\"(\\d+)\" failed=\"(\\d+)\"");
    return m.Success ? (int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value)) : (0, 0, 0);
}

static (int Total, int Passed, int Failed) RunAgainst(string source, string myTests)
{
    var dir = Path.Combine(Path.GetTempPath(), "variant-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(dir);
    File.WriteAllText(Path.Combine(dir, "ClaimsService.cs"), Encoding.UTF8.GetString(Convert.FromBase64String(source)));
    File.WriteAllText(Path.Combine(dir, "Db.cs"), Encoding.UTF8.GetString(Convert.FromBase64String(Variants.Db)));
    if (File.Exists(myTests)) File.Copy(myTests, Path.Combine(dir, "MyTests.cs"));
    File.WriteAllText(Path.Combine(dir, "VariantRun.csproj"), """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework><RollForward>Major</RollForward>
            <Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.10" />
            <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
            <PackageReference Include="xunit" Version="2.9.2" />
            <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
          </ItemGroup>
          <ItemGroup><Using Include="Xunit" /><Using Include="Claims" /></ItemGroup>
        </Project>
        """);
    return RunTests(Path.Combine(dir, "VariantRun.csproj"), "FullyQualifiedName~MyTests");
}

string Fingerprint()
{
    var files = new[] { "check/Program.cs", "check/Check.csproj", "check/Variants.cs" };
    var all = string.Concat(files.Select(f => File.ReadAllText(Path.Combine(root, f)).Replace("\r\n", "\n")));
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(all)))[..8].ToLowerInvariant();
}
