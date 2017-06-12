using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NConcern;


namespace Emroy.Vfs.Service
{
    public class Injector : IAspect
    {

        public IEnumerable<IAdvice> Advise(MethodBase method)
        {
          
            yield return Advice.Basic.Before(() => Console.WriteLine($"{method.Name} started."));
            yield return Advice.Basic.After(() => Console.WriteLine($"{method.Name} finished."));
        
        }
    }

}
