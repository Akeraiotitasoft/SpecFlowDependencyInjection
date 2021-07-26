using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akeraiotitasoft.SpecFlow.DependencyInjection.SpecFlowPlugin
{
    /// <summary>
    /// Decorate a method with this attribute to get the constructed and configured <see cref="ServiceCollection"/> returned as <see cref="IServiceCollection"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ScenarioDependenciesAttribute : Attribute
    {
    }
}
