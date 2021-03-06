using System.Threading.Tasks;
using Xunit;

namespace DebounceMonitoring.Tests
{
    public class IntervalTest
    {
        [Fact]
        public void Should_Return_True_For_Delay_Withing_Interval()
        {
            var sample = new DebouncingSample();

            Assert.False(sample.DebounceDuring100ms());
            Assert.True(sample.DebounceDuring100ms());
        }

        [Fact]
        public async Task Should_Return_False_For_Delay_Over_Interval()
        {
            var sample = new DebouncingSample();

            Assert.False(sample.DebounceDuring100ms());

            await Task.Delay(110).ConfigureAwait(false);

            Assert.False(sample.DebounceDuring100ms());
        }

        [Fact]
        public async Task Interval_Should_Not_Slide_With_Each_Call()
        {
            var sample = new DebouncingSample();

            Assert.False(sample.DebounceDuring100ms());

            await Task.Delay(50).ConfigureAwait(false);
            Assert.True(sample.DebounceDuring100ms());

            await Task.Delay(60).ConfigureAwait(false);
            Assert.False(sample.DebounceDuring100ms());

            await Task.Delay(50).ConfigureAwait(false);
            Assert.True(sample.DebounceDuring100ms());

            await Task.Delay(60).ConfigureAwait(false);
            Assert.False(sample.DebounceDuring100ms());
        }
    }
}
