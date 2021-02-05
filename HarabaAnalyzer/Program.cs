using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace HarabaAnalyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var instance = MSBuildLocator.QueryVisualStudioInstances().ToArray().First();

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);
                
                if (args.Length > 1 && args[1] == "--project")
                {
                    await StartProjectAnalyze(workspace, args[0]);
                }
                else
                {
                    await StartSolutionAnalyze(workspace, args[0]);
                }
            }

            Console.WriteLine("Готово");
            Console.ReadLine();
        }

        private static async Task StartSolutionAnalyze(MSBuildWorkspace workspace, string solutionPath)
        {
            Console.WriteLine($"Loading solution '{solutionPath}'");

            var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
            Console.WriteLine($"Finished loading solution '{solutionPath}'");

            foreach (var project in solution.Projects)
            {
                await ProjectAnalyze(project);
            }
        }

        private static async Task StartProjectAnalyze(MSBuildWorkspace workspace, string projectPath)
        {
            Console.WriteLine($"Loading project '{projectPath}'");

            var project = await workspace.OpenProjectAsync(projectPath, new ConsoleProgressReporter());
            Console.WriteLine($"Finished loading project '{projectPath}'");

            await ProjectAnalyze(project);
        }

        private static async Task ProjectAnalyze(Project project)
        {
            foreach (var file in project.Documents)
            {
                var tree = await file.GetSyntaxTreeAsync();
                var root = await tree.GetRootAsync();

                AreThereBindingSources(root);
            }
        }

        private static void AreThereBindingSources(SyntaxNode root)
        {
            var sourceBindingAttributesList = new List<string>
            {
                "FromHeader",
                "FromQuery",
                "FromRoute",
                "FromForm",
                "FromBody"
            };

            foreach (var @class in root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where(x => x.Identifier.ValueText.EndsWith("Controller")).ToList())
            {
                foreach (var parameter in root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList().SelectMany(method => method.ParameterList.Parameters))
                {
                    if (parameter.AttributeLists.Count == 0)
                    {
                        WriteNonAttribute(@class, parameter);
                        continue;
                    }

                    foreach (var attributes in parameter.AttributeLists)
                    {
                        foreach (var attribute in attributes.Attributes.Where(x => x.Name.ToString() != "FromServices" && !sourceBindingAttributesList.Contains(x.Name.ToString())))
                        {
                            WriteNonAttribute(@class, parameter);
                        }
                    }
                }
            }

            void WriteNonAttribute(ClassDeclarationSyntax @class, ParameterSyntax parameter)
            {
                var location = parameter.GetLocation();
                var filePath = location.SourceTree.FilePath;
                var spanStart = location.GetLineSpan().Span.Start;

                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine($"\n\n\n\nWarning: {filePath}:{spanStart.Line + 1}:{spanStart.Character} - Нет атрибута для явного указания источника привязки '{parameter.ToString()}'\n");

                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Red;
                
                Console.Write($"{spanStart.Line + 1}");
                Console.ResetColor();
                
                Console.Write($"{@class.SyntaxTree.GetText().Lines[spanStart.Line],10}");
                Console.ResetColor();
            }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }
}
