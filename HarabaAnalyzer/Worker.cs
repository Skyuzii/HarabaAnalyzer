﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HarabaAnalyzer.Abstracts;
using HarabaAnalyzer.Common;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace HarabaAnalyzer
{
    public class Worker
    {
        private readonly Logger _logger;
        private readonly IEnumerable<BaseAnalyzer> _analyzers;

        public Worker(IEnumerable<BaseAnalyzer> analyzers, Logger logger)
        {
            _logger = logger;
            _analyzers = analyzers.ToList();
        }

        public async Task Start(string path)
        {
            var instance = MSBuildLocator.QueryVisualStudioInstances().ToArray().First();

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                if (path.EndsWith(".csproj"))
                {
                    await StartProjectAnalyze(workspace, path);
                }
                else
                {
                    await StartSolutionAnalyze(workspace, path);
                }
            }

            _logger.CompleteAnalyze();
        }

        private async Task StartSolutionAnalyze(MSBuildWorkspace workspace, string solutionPath)
        {
            Console.WriteLine($"Loading solution '{solutionPath}'");

            var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
            Console.WriteLine($"Finished loading solution '{solutionPath}'");

            foreach (var project in solution.Projects)
            {
                await ProjectAnalyze(project);
            }
        }

        private async Task StartProjectAnalyze(MSBuildWorkspace workspace, string projectPath)
        {
            Console.WriteLine($"Loading project '{projectPath}'");

            var project = await workspace.OpenProjectAsync(projectPath, new ConsoleProgressReporter());
            Console.WriteLine($"Finished loading project '{projectPath}'");

            await ProjectAnalyze(project);
        }

        private async Task ProjectAnalyze(Project project)
        {
            foreach (var file in project.Documents)
            {
                var tree = await file.GetSyntaxTreeAsync();
                var root = await tree.GetRootAsync();
                
                _analyzers.ForEach(async x => await x.Execute(root));
            }
        }
    }
}