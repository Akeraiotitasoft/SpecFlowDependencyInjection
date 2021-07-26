using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow.Bindings;

namespace Akeraiotitasoft.SpecFlow.DependencyInjection.SpecFlowPlugin
{
    /// <summary>
    /// Finds the <see cref="IServiceCollection"/> method marked with <see cref="ScenarioDependenciesAttribute"/>
    /// </summary>
    public class ServiceCollectionFinder : IServiceCollectionFinder
    {
        private readonly IBindingRegistry bindingRegistry;
        private readonly Lazy<Func<IServiceCollection>> createServiceCollection;

        /// <summary>
        /// The constructor for <see cref="ServiceCollectionFinder"/>
        /// </summary>
        /// <param name="bindingRegistry">The binding registry</param>
        public ServiceCollectionFinder(IBindingRegistry bindingRegistry)
        {
            this.bindingRegistry = bindingRegistry;
            createServiceCollection = new Lazy<Func<IServiceCollection>>(FindCreateServiceCollection, true);
        }

        /// <summary>
        /// Gets the method that creates the <see cref="ServiceCollection"/>
        /// </summary>
        /// <returns>A function that returns a <see cref="IServiceCollection"/></returns>
        public Func<IServiceCollection> GetCreateServiceCollection()
        {
            var collection = createServiceCollection.Value;
            if (collection == null)
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new Exception("Unable to find scenario dependencies! Mark a static method that returns a IServiceCollection with [ScenarioDependencies]!");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            return collection;
        }

        /// <summary>
        /// Find the method with the <see cref="ScenarioDependenciesAttribute"/>
        /// </summary>
        /// <returns>A function that returns a <see cref="IServiceCollection"/></returns>
        protected virtual Func<IServiceCollection> FindCreateServiceCollection()
        {
            var assemblies = bindingRegistry.GetBindingAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Where(m => Attribute.IsDefined((MemberInfo)m, typeof(ScenarioDependenciesAttribute)) && typeof(IServiceCollection).IsAssignableFrom(m.ReturnType)))
                    {
                        return () => (IServiceCollection)methodInfo.Invoke(null, null);
                    }
                }
            }
            return null;
        }
    }
}
