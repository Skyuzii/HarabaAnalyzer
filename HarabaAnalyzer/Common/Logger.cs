using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HarabaAnalyzer.Common
{
    public class Logger
    {
        public void Write(CSharpSyntaxNode node, MessageType type, string message)
        {
            var location = node.GetLocation();
            var filePath = location.SourceTree.FilePath;
            var spanStart = location.GetLineSpan().Span.Start;
            var spanEnd = location.GetLineSpan().Span.End;

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"\n\n\n\n{type}: {filePath}:{spanStart.Line + 1}:{spanStart.Character} - {message}\n");

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
                
            Console.Write($"{spanStart.Line + 1}");
            Console.ResetColor();
                
            Console.Write($"{node.SyntaxTree.GetText().Lines[spanStart.Line],10}");
            Console.Write("\n" + new string(' ', spanStart.Character + 2));

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(new string('~', spanEnd.Character - spanStart.Character));
            Console.ResetColor();
        }
        
        public void CompleteAnalyze()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Анализ успешно завершен");
            Console.ResetColor();
        }
    }
}