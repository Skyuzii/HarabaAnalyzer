using HarabaAnalyzer.Abstracts;
using HarabaAnalyzer.Analyzers;
using Microsoft.Extensions.DependencyInjection;

namespace HarabaAnalyzer.Common
{
    public static class IoC
    {
        private static readonly ServiceProvider _provider;

        static IoC()
        {
            var services = new ServiceCollection();

            services.AddTransient(typeof(Worker));
            services.AddAnalyzers();
            
            _provider = services.BuildServiceProvider();
        }

        public static T Resolve<T>() => _provider.GetRequiredService<T>();

        private static void AddAnalyzers(this IServiceCollection services)
        {
            services.AddTransient(typeof(BaseAnalyzer), typeof(CheckForBindingSourcesAttributesAnalyzer));
        }
    }
}