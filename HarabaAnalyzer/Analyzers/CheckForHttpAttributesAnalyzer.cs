using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarabaAnalyzer.Abstracts;
using HarabaAnalyzer.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HarabaAnalyzer.Analyzers
{
    public class CheckForHttpAttributesAnalyzer : BaseAnalyzer
    {
        public CheckForHttpAttributesAnalyzer(Logger logger) : base(logger)
        {
        }

        public override MessageType Type => MessageType.Warning;
        
        public override Task Execute(SyntaxNode root)
        {
            const string warningMessage = "Нет HTTP атрибута";
            var httpAttributesList = new List<string>
            {
                "HttpPost",
                "HttpPut",
                "HttpDelete",
                "HttpHead",
                "HttpOptions",
                "HttpPatch",
                "HttpGet"
            };

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Where(x => x.Parent is ClassDeclarationSyntax syntax && syntax.Identifier.ValueText.EndsWith("Controller"))
                .Where(x => !x.AttributeLists.Any(y => y.Attributes.Any(attribute => !httpAttributesList.Contains(attribute.Name.ToString())))))
            {
                _logger.Write(method, Type, warningMessage);
            }

            return Task.CompletedTask;
        }
    }
}