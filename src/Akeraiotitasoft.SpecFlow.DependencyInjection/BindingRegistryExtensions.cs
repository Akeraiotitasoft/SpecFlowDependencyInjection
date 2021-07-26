using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Bindings.Reflection;

namespace Akeraiotitasoft.SpecFlow.DependencyInjection.SpecFlowPlugin
{
    /// <summary>
    /// Extensions for the binding registry
    /// </summary>
    public static class BindingRegistryExtensions
    {
        /// <summary>
        /// Gets the binding types
        /// </summary>
        /// <param name="bindingRegistry">The binding registry</param>
        /// <returns>An enumerable of <see cref="IBindingType"/></returns>
        public static IEnumerable<IBindingType> GetBindingTypes(this IBindingRegistry bindingRegistry)
        {
            if (bindingRegistry == null)
            {
                throw new ArgumentNullException(nameof(bindingRegistry));
            }
            return bindingRegistry.GetStepDefinitions().Cast<IBinding>()
                .Concat(bindingRegistry.GetHooks().Cast<IBinding>())
                .Concat(bindingRegistry.GetStepTransformations())
                .Select(b => b.Method.Type)
                .Distinct();
        }

        /// <summary>
        /// Gets the assemblies of the binding types
        /// </summary>
        /// <param name="bindingRegistry">The binding registry</param>
        /// <returns>An enumerable of <see cref="Assembly"/></returns>
        public static IEnumerable<Assembly> GetBindingAssemblies(this IBindingRegistry bindingRegistry)
        {
            return bindingRegistry.GetBindingTypes().OfType<RuntimeBindingType>()
                .Select(t => t.Type.Assembly)
                .Distinct();
        }
    }
}
