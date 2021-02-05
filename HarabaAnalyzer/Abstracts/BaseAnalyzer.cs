using System;
using System.Threading.Tasks;
using HarabaAnalyzer.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HarabaAnalyzer.Abstracts
{
    public abstract class BaseAnalyzer
    {
        protected readonly Logger _logger;

        public BaseAnalyzer(Logger logger)
        {
            _logger = logger;
        }
        
        public abstract MessageType Type { get; }

        public abstract Task Execute(SyntaxNode root);
    }
}