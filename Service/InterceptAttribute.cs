using System;
using System.Diagnostics;
using KingAOP.Aspects;

//using KingAOP.Aspects;


namespace Emroy.Vfs.Service
{
    // [PSerializable]
    class InterceptAttribute2 : Attribute
    {
        /* public override void OnInvoke(MethodInterceptionArgs args)
         {
             VfsService.DebugInfo($"Call {args.Method.Name} {string.Join(",", args.Arguments)} init.");
             //base.OnInvoke(args);
             VfsService.DebugInfo($"Call {args.Method.Name} finished.");
         }
         */
    }
    
    public class InterceptAttribute :  OnMethodBoundaryAspect
    {
        private readonly Stopwatch _stopwatch;

        public InterceptAttribute()
        {
            _stopwatch = new Stopwatch();
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            Console.WriteLine($"Method  {args.Method.Name} started");
            _stopwatch.Restart();

        }

        public override void OnExit(MethodExecutionArgs args)
        {
            _stopwatch.Stop();
            Console.WriteLine($"Method  {args.Method.Name} finished - Elapsed Ticks: {_stopwatch.ElapsedTicks}");
        }
    }
    
}
