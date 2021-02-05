using System;
using System.Threading.Tasks;
using HarabaAnalyzer.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HarabaAnalyzer.Abstracts
{
    public abstract class BaseAnalyzer
    {
        public abstract MessageType Type { get; }

        public abstract Task Execute(SyntaxNode root);

        protected void WriteInformation(SyntaxNode @class, CSharpSyntaxNode node, string message)
        {
            var location = node.GetLocation();
            var filePath = location.SourceTree.FilePath;
            var spanStart = location.GetLineSpan().Span.Start;

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"\n\n\n\n{Type}: {filePath}:{spanStart.Line + 1}:{spanStart.Character} - {message}\n");

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
                
            Console.Write($"{spanStart.Line + 1}");
            Console.ResetColor();
                
            Console.Write($"{@class.SyntaxTree.GetText().Lines[spanStart.Line],10}");
            Console.ResetColor();
        }
    }
}