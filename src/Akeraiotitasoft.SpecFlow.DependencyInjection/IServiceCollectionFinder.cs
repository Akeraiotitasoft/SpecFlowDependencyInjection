using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akeraiotitasoft.SpecFlow.DependencyInjection.SpecFlowPlugin
{
    interface IServiceCollectionFinder
    {
        Func<IServiceCollection> GetCreateServiceCollection();
    }
}
