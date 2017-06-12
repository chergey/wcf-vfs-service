using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using Emroy.Vfs.Service.Impl;
using NConcern;



namespace Emroy.Vfs.Service
{
    public class Injector : IAspect
    {

        public IEnumerable<IAdvice> Advise(MethodBase method)
        {
            yield return Advice.Basic.Around((instance, args, body) =>
            {
                var sw = Stopwatch.StartNew();
                var js = new JavaScriptSerializer();
                var str = js.Serialize(args);
                sw.Start();
                DebugInfo($"{method.Name} ({str}) started.");

                body();
                DebugInfo($"{method.Name}  ({str}) finished. Elapsed: {sw.Elapsed}");
                sw.Stop();
            });


        }

        public static void DebugInfo(string info)
        {
            Console.WriteLine(info);
            VfsService.AppLogger.Info(info);
        }
    }

}
