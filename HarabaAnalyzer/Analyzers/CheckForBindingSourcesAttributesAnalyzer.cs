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
    public class CheckForBindingSourcesAttributesAnalyzer : BaseAnalyzer
    {
        public CheckForBindingSourcesAttributesAnalyzer(Logger logger) : base(logger)
        {
        }

        public override MessageType Type => MessageType.Warning;
        
        public override Task Execute(SyntaxNode root)
        {
            var warningMessage = "Нет атрибута для явного указания источника привязки";
            var sourceBindingAttributesList = new List<string>
            {
                "FromHeader",
                "FromQuery",
                "FromRoute",
                "FromForm",
                "FromBody"
            };

            foreach (var parameter in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Where(x => x.Parent is ClassDeclarationSyntax syntax && syntax.Identifier.ValueText.EndsWith("Controller"))
                .SelectMany(method => method.ParameterList.Parameters))
            {
                if (parameter.AttributeLists.Count == 0)
                {
                    _logger.Write(parameter, Type, warningMessage);
                    continue;
                }

                if (parameter.AttributeLists.Any(x => x.Attributes.Any(attribute =>
                    attribute.Name.ToString() != "FromServices" &&
                    !sourceBindingAttributesList.Contains(attribute.Name.ToString()))))
                {
                    _logger.Write(parameter, Type, warningMessage);
                }
            }

            return Task.CompletedTask;
        }
    }
}