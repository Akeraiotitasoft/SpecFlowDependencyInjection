using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoDi;
using TechTalk.SpecFlow.Infrastructure;

namespace Akeraiotitasoft.SpecFlow.DependencyInjection.SpecFlowPlugin
{
    /// <summary>
    /// The resolver that gets the requested service
    /// </summary>
    public class ServiceProviderTestObjectResolver : ITestObjectResolver
    {
        /// <summary>
        /// Get a service from the <see cref="IServiceProvider"/>
        /// </summary>
        /// <param name="bindingType"></param>
        /// <param name="scenarioContainer"></param>
        /// <returns>The <see cref="object"/> instance of the type requested or null</returns>
        public object ResolveBindingInstance(Type bindingType, IObjectContainer scenarioContainer)
        {
            if (scenarioContainer == null)
            {
                throw new ArgumentNullException(nameof(scenarioContainer));
            }
            var serviceProvider = scenarioContainer.Resolve<IServiceProvider>();
            return serviceProvider.GetService(bindingType);
        }
    }
}
