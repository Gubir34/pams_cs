using System;
using System.Diagnostics;
using System.IO;

// Resolve File name conflict (COM vs System.IO)
using FileIO = System.IO.File;

class Program
{
    static readonly string BaseAppsDir = @"C:\PAMS\apps";

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }

        Directory.CreateDirectory(BaseAppsDir);

        string command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "install":
                Install(args);
                break;

            case "run":
                Run(args);
                break;

            default:
                Console.WriteLine("Unknown command.");
                PrintUsage();
                break;
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  pams install <owner/repo>");
        Console.WriteLine("  pams run <app-name>");
    }

    // ---------------- INSTALL ----------------

    static void Install(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Missing repository name.");
            return;
        }

        string repo = args[1];

        if (!repo.Contains("/"))
        {
            Console.WriteLine("Repository format must be owner/repo");
            return;
        }

        string repoName = repo.Split('/')[1];
        string targetPath = Path.Combine(BaseAppsDir, repoName);

        if (Directory.Exists(targetPath))
        {
            Console.WriteLine("Application is already installed.");
            return;
        }

        Console.WriteLine($"Installing {repo}...");

        Process git = new Process();
        git.StartInfo.FileName = "git";
        git.StartInfo.Arguments =
            $"clone https://github.com/{repo}.git \"{targetPath}\"";
        git.StartInfo.UseShellExecute = false;

        git.Start();
        git.WaitForExit();

        if (git.ExitCode != 0)
        {
            Console.WriteLine("Git clone failed.");
            return;
        }

        Console.WriteLine("Install completed.");
    }

    // ---------------- RUN ----------------

    static void Run(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Missing application name.");
            return;
        }

        string appName = args[1];
        string appPath = Path.Combine(BaseAppsDir, appName);

        if (!Directory.Exists(appPath))
        {
            Console.WriteLine("Application is not installed.");
            return;
        }

        string mainPy = Path.Combine(appPath, "main.py");

        if (FileIO.Exists(mainPy))
        {
            RunPython(mainPy, appPath);
            return;
        }

        string[] pyFiles = Directory.GetFiles(appPath, "*.py");

        if (pyFiles.Length == 0)
        {
            Console.WriteLine("No Python files found.");
            return;
        }

        Console.WriteLine("Select a Python file to run:");
        for (int i = 0; i < pyFiles.Length; i++)
        {
            Console.WriteLine($"{i + 1}) {Path.GetFileName(pyFiles[i])}");
        }

        Console.Write("Choice: ");
        string input = Console.ReadLine() ?? "";

        if (!int.TryParse(input, out int choice))
        {
            Console.WriteLine("Invalid input.");
            return;
        }

        choice--;

        if (choice < 0 || choice >= pyFiles.Length)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        RunPython(pyFiles[choice], appPath);
    }

    // ---------------- PYTHON ----------------

    static void RunPython(string filePath, string workingDir)
    {
        Process p = new Process();
        p.StartInfo.FileName = "python";
        p.StartInfo.Arguments = $"\"{filePath}\"";
        p.StartInfo.WorkingDirectory = workingDir;
        p.StartInfo.UseShellExecute = false;

        p.Start();
        p.WaitForExit();
    }
}
