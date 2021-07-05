using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DebounceMonitoring.Tests
{
    public class DebounceHereTest
    {
        [Fact]
        public void Should_Hold_Debounce_References_Weakly()
        {
            WeakReference? sampleRef = null;
            WeakReference? sampleRef2 = null;

            new Action(() =>
            {
                var sample = new DebouncingSample();
                var sample2 = new DebouncingSample();

                sampleRef = new WeakReference(sample);
                sampleRef2 = new WeakReference(sample2);

                sample.DebounceHere();
                sample2.DebounceHere();
            }).Invoke();

            GC.Collect();

            Assert.False(sampleRef!.IsAlive);
            Assert.False(sampleRef2!.IsAlive);
        }

        [Fact]
        public void Should_Work_From_Local_Functions()
        {
            var sample = new DebouncingSample();

            Assert.False(LocalDebounce(sample));
            Assert.True(LocalDebounce(sample));

            static bool LocalDebounce(DebouncingSample sample) => sample.DebounceHere();
        }

        [Fact]
        public void Should_Work_For_A_Single_Object_At_Multiple_Locations()
        {
            var sample = new DebouncingSample();

            Assert.False(DebounceLocation1(sample));
            Assert.False(DebounceLocation2(sample));

            Assert.True(DebounceLocation1(sample));
            Assert.True(DebounceLocation2(sample));

            static bool DebounceLocation1(DebouncingSample sample) => sample.DebounceHere();
            static bool DebounceLocation2(DebouncingSample sample) => sample.DebounceHere();
        }

        [Fact]
        public void Should_Work_For_Multiple_Objects_At_Same_Location()
        {
            var sample = new DebouncingSample();
            var sample2 = new DebouncingSample();

            Assert.False(SameLocationDebounce(sample));
            Assert.False(SameLocationDebounce(sample2));
            Assert.True(SameLocationDebounce(sample));
            Assert.True(SameLocationDebounce(sample2));

            static bool SameLocationDebounce(DebouncingSample sample) => sample.DebounceHere();
        }

        [Fact]
        public void Should_Be_Thread_Safe()
        {
            var testOk = true;

            var thread1 = new Thread(Test);
            var thread2 = new Thread(Test);

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            Assert.True(testOk);

            void Test()
            {
                for (int i = 0; i < 100000; i++)
                {
                    var sample = new DebouncingSample();
                    var sample2 = new DebouncingSample();

                    testOk &= !sample.DebounceHere();
                    testOk &= !sample2.DebounceHere();

                    testOk &= !SameLocationDebounce(sample);
                    testOk &= !SameLocationDebounce(sample2);

                    testOk &= SameLocationDebounce(sample);
                    testOk &= SameLocationDebounce(sample2);
                }
            }

            static bool SameLocationDebounce(DebouncingSample sample) => sample.DebounceHere();
        }

        [Fact]
        public void Should_Be_Thread_Safe_In_Debouncing_Second_Object_At_The_Same_Location()
        {
            var allDebounceResultsAsExpected = true;

            var sample = new DebouncingSample();

            for (int i = 0; i < 10000; i++)
            {
                SameLocationDebounce(sample);

                Parallel.For(0, 10, new ParallelOptions { MaxDegreeOfParallelism = 10 }, _ =>
                {
                    var sample2 = new DebouncingSample();
                    allDebounceResultsAsExpected &= !SameLocationDebounce(sample2);
                    allDebounceResultsAsExpected &= SameLocationDebounce(sample2);
                });

                bool SameLocationDebounce(DebouncingSample sample) => sample.DebounceHere(
                    memberName: nameof(Should_Be_Thread_Safe_In_Debouncing_Second_Object_At_The_Same_Location),
                    lineNumber: i);
            }

            Assert.True(allDebounceResultsAsExpected, "All debounce results appeared as expected");
        }
    }
}
