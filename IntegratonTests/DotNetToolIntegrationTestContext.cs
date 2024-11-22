using System.Collections.Concurrent;
using System.Diagnostics;
using NoeticTools.Common.Tools;
using NoeticTools.Common.Tools.DotnetCli;
using NUnit.Framework.Internal;


namespace NoeticTools.Git2SemVer.Core.IntegrationTests;

internal sealed class DotNetToolIntegrationTestContext : IDisposable
{
    private const int ConcurrentContextsLimit = 100;
    private static int _nextContextId = 1;
    private static readonly ConcurrentDictionary<TestExecutionContext, DirectoryInfo> TestDirectories = [];

    public DotNetToolIntegrationTestContext()
    {
        Logger = new NUnitLogger(false);
        TestDirectory = GetTestDirectory();
        TestFolderName = TestDirectory.Name;
        var processCli = new ProcessCli(Logger) {WorkingDirectory = TestDirectory.FullName};
        DotNetCli = new DotNetTool(processCli);
    }

    public string TestFolderName { get; }

    public DotNetTool DotNetCli { get; }

    public NUnitLogger Logger { get; }

    public DirectoryInfo TestDirectory { get; }

    private static void ReleaseTestDirectory()
    {
        var currentContext = TestExecutionContext.CurrentContext;
        if (!TestDirectories.Remove(currentContext, out var directory))
        {
            Assert.Fail("Context does not have a test directory to release.");
        }
        directory!.Delete(true);
        if (!WaitUntil(() => !directory.Exists))
        {
            Assert.Fail($"Unable to release a {directory.FullName}.");
        }
    }

    private static bool WaitUntil(Func<bool> predicate)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!predicate())
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(30))
            {
                return false;
            }

            Thread.Sleep(5);
        }

        return true;
    }

    private DirectoryInfo GetTestDirectory()
    {
        if (TestDirectories.Count > ConcurrentContextsLimit)
        {
            Assert.Fail("The number of test directories has exceeded the maximum allowed.");
        }

        var currentContext = TestExecutionContext.CurrentContext;
        if (TestDirectories.TryGetValue(currentContext, out var directory))
        {
            return directory;
        }

        var testId = _nextContextId++;
        if (testId == ConcurrentContextsLimit)
        {
            _nextContextId = 1;
        }

        var testFolderName = $"{currentContext.CurrentTest.MethodName}_TestFolder{testId}";
        var testFolderPath = Path.Combine(TestContext.CurrentContext.TestDirectory, testFolderName);
        Assert.That(testFolderPath, Does.Not.Exist, "The test directory '{0}' already exists.", testFolderPath);
        directory = Directory.CreateDirectory(testFolderPath);

        Logger.LogInfo("Created test directory {0}.", directory.FullName);
        if (!TestDirectories.TryAdd(currentContext, directory))
        {
            Assert.Fail("The context already has an assigned test directory.");
        }
        return directory;
    }

    public void Dispose()
    {
        ReleaseTestDirectory();
    }
}