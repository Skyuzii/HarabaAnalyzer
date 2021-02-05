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

            foreach (var @class in root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where(x => x.Identifier.ValueText.EndsWith("Controller")).ToList())
            {
                foreach (var parameter in root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList().SelectMany(method => method.ParameterList.Parameters))
                {
                    if (parameter.AttributeLists.Count == 0)
                    {
                        WriteInformation(@class, parameter, warningMessage);
                        continue;
                    }

                    foreach (var attributes in parameter.AttributeLists)
                    {
                        foreach (var attribute in attributes.Attributes.Where(x => x.Name.ToString() != "FromServices" && !sourceBindingAttributesList.Contains(x.Name.ToString())))
                        {
                            WriteInformation(@class, parameter, warningMessage);
                        }
                    }
                }
            }
            
            return Task.CompletedTask;
        }
    }
}