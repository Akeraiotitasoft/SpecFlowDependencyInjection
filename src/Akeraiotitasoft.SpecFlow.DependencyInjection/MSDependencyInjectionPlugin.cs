using Akeraiotitasoft.SpecFlow.DependencyInjection.SpecFlowPlugin;
using System;
using TechTalk.SpecFlow.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: RuntimePlugin(typeof(MSDependencyInjectionPlugin))]

namespace Akeraiotitasoft.SpecFlow.DependencyInjection.SpecFlowPlugin
{
    using BoDi;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Bindings;
    using TechTalk.SpecFlow.Bindings.Reflection;
    using TechTalk.SpecFlow.Infrastructure;
    using TechTalk.SpecFlow.UnitTestProvider;

    /// <summary>
    /// The <see cref="MSDependencyInjectionPlugin"/> specflow plugin.
    /// </summary>
    public class MSDependencyInjectionPlugin : IRuntimePlugin
    {
        private static Object _registrationLock = new Object();

        /// <summary>
        /// Initialize the plugin
        /// </summary>
        /// <param name="runtimePluginEvents">The runtime plugin events</param>
        /// <param name="runtimePluginParameters">The runtime plugin parameters</param>
        /// <param name="unitTestProviderConfiguration">The unit test provider configuration</param>
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            if (runtimePluginEvents == null)
            {
                throw new ArgumentNullException(nameof(runtimePluginEvents));
            }
            if (runtimePluginParameters == null)
            {
                throw new ArgumentNullException(nameof(runtimePluginParameters));
            }
            if (unitTestProviderConfiguration == null)
            {
                throw new ArgumentNullException(nameof(unitTestProviderConfiguration));
            }
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                // temporary fix for CustomizeGlobalDependencies called multiple times
                // see https://github.com/techtalk/SpecFlow/issues/948
                if (!args.ObjectContainer.IsRegistered<IServiceCollectionFinder>())
                {
                    // an extra lock to ensure that there are not two super fast threads re-registering the same stuff
                    lock (_registrationLock)
                    {
                        if (!args.ObjectContainer.IsRegistered<IServiceCollectionFinder>())
                        {
                            args.ObjectContainer.RegisterTypeAs<ServiceProviderTestObjectResolver, ITestObjectResolver>();
                            args.ObjectContainer.RegisterTypeAs<ServiceCollectionFinder, IServiceCollectionFinder>();
                        }
                    }

                    // workaround for parallel execution issue - this should be rather a feature in BoDi?
                    args.ObjectContainer.Resolve<IServiceCollectionFinder>();
                }
            };

            runtimePluginEvents.CustomizeScenarioDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterFactoryAs<IServiceProvider>(() =>
                {
                    var serviceCollectionFinder = args.ObjectContainer.Resolve<IServiceCollectionFinder>();
                    var createServiceCollection = serviceCollectionFinder.GetCreateServiceCollection();
                    var serviceCollection = createServiceCollection();
                    RegisterSpecflowDependecies(args.ObjectContainer, serviceCollection);
                    var bindingRegistry = args.ObjectContainer.Resolve<IBindingRegistry>();
                    RegisterStepClasses(bindingRegistry, serviceCollection);
                    
                    var serviceProvider = serviceCollection.BuildServiceProvider();
                    return serviceProvider;
                });
            };
        }

        /// <summary>
        ///     Fix for https://github.com/gasparnagy/SpecFlow.Autofac/issues/11 Cannot resolve ScenarioInfo
        ///     Extracted from
        ///     https://github.com/techtalk/SpecFlow/blob/master/TechTalk.SpecFlow/Infrastructure/ITestObjectResolver.cs
        ///     The test objects might be dependent on particular SpecFlow infrastructure, therefore the implemented
        ///     resolution logic should support resolving the following objects (from the provided SpecFlow container):
        ///     <see cref="ScenarioContext" />, <see cref="FeatureContext" />, <see cref="TestThreadContext" /> and
        ///     <see cref="IObjectContainer" /> (to be able to resolve any other SpecFlow infrastucture). So basically
        ///     the resolution of these classes has to be forwarded to the original container.
        /// </summary>
        /// <param name="objectContainer">SpecFlow DI container.</param>
        /// <param name="serviceCollection">Microsoft Dependnecy Injection Service Collection.</param>
        private void RegisterSpecflowDependecies(
            IObjectContainer objectContainer,
            IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IObjectContainer>(provider => objectContainer);
            serviceCollection.AddTransient<ScenarioContext>(
                provider =>
                {
                    var specflowContainer = provider.GetRequiredService<IObjectContainer>();
                    var scenarioContext = specflowContainer.Resolve<ScenarioContext>();
                    return scenarioContext;
                }
                );
            serviceCollection.AddTransient<FeatureContext>(
                provider =>
                {
                    var specflowContainer = provider.GetRequiredService<IObjectContainer>();
                    var featureContext = specflowContainer.Resolve<FeatureContext>();
                    return featureContext;
                }
                );
            serviceCollection.AddTransient<TestThreadContext>(
                provider =>
                {
                    var specflowContainer = provider.GetRequiredService<IObjectContainer>();
                    var testThreadContext = specflowContainer.Resolve<TestThreadContext>();
                    return testThreadContext;
                }
                );
        }

        private void RegisterStepClasses(
            IBindingRegistry bindingRegistry,
            IServiceCollection serviceCollection
            )
        {
            foreach (var type in bindingRegistry.GetBindingTypes().OfType<RuntimeBindingType>())
            {
                serviceCollection.AddSingleton(type.Type);
            }
        }
    }
}