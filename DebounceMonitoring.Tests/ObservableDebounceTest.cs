using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DebounceMonitoring.Tests
{
    public class ObservableDebounceTest
    {
        [Fact]
        public async void Interval_Should_Not_Slide_With_Each_Call()
        {
            Assert.Equal(
                new[] { 0, 110, 220 },
                await Observable.Create<int>(async o =>
                    {
                        o.OnNext(0);
                        await Task.Delay(50);
                        o.OnNext(50); // too fast
                        await Task.Delay(60);
                        o.OnNext(110); // 50 + 60 > 100, OK
                        await Task.Delay(50);
                        o.OnNext(160);
                        await Task.Delay(60);
                        o.OnNext(220);
                    })
                    .Debounce(intervalMs: 100)
                    .ToList());
        }
    }
}