using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;

namespace Packrats
{
    public static class JobUtilitiyExtensions
    {

        public static JobHandle CopyTo<T>(
            this NativeArray<T> source,
            NativeArray<T> destination,
            JobHandle jobToWaitOn = default) where T : struct
        {
            return CopyArrayJob<T>.CopyArray(source, destination, jobToWaitOn);
        }


    }
}
