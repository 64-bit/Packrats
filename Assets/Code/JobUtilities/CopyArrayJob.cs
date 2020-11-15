using System;
using Unity.Collections;
using Unity.Jobs;

namespace Packrats
{
    public struct CopyArrayJob<T> : IJobParallelFor where T : struct
    {
        [ReadOnly]
        private NativeArray<T> _source;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        private NativeArray<T> _destination;

        private int _readStartIndex;
        private int _writeStartIndex;

        public static JobHandle CopyArray(NativeArray<T> source, NativeArray<T> destination,
            JobHandle jobToWaitOn = default)
        {
            return CopyArray(source, destination, 0, 0, source.Length, jobToWaitOn);
        }

        public static JobHandle CopyArray(NativeArray<T> source, NativeArray<T> destination, int readIndex, int writeIndex, int count, JobHandle jobToWaitOn = default)
        {
            if (!source.IsCreated)
            {
                throw new ArgumentException("Array must be allocated", nameof(source));
            }

            if (!destination.IsCreated)
            {
                throw new ArgumentException("Array must be allocated", nameof(source));
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Must copy 1 or more elements");
            }

            CheckBounds(ref source, readIndex, count, nameof(source));
            CheckBounds(ref destination, writeIndex, count, nameof(destination));

            var job = new CopyArrayJob<T>()
            {
                _source = source,
                _destination = destination,
                _readStartIndex = readIndex,
                _writeStartIndex = writeIndex
            };

            return job.Schedule(count, 256, jobToWaitOn);
        }

        private static void CheckBounds(ref NativeArray<T> array, int startIndex, int count, string arrayName)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"out of bounds read on {arrayName} array at {startIndex}");
            }

            if (startIndex + count >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"out of bounds read on {arrayName} array (length {array.Length}) at {startIndex + count}");
            }
        }

        public void Execute(int index)
        {
            _destination[index + _writeStartIndex] = _source[index + _readStartIndex];
        }
    }
}