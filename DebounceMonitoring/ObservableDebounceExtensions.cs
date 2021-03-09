using System;

namespace DebounceMonitoring
{
    /// <summary>
    /// Provides a set of static methods for debouncing observable sequences.
    /// </summary>
    public static class ObservableDebounceExtensions
    {
        /// <summary>
        /// Emits only the first item emitted by the source within periodic time intervals.
        /// </summary>
        /// <remarks>
        /// Marble diagram
        /// (<see href="https://raw.githubusercontent.com/SIDOVSKY/DebounceMonitoring/master/Assets/debounce_marble.png">image</see>):
        /// <code>
        /// -A----B-C-------D-----E-|--><br/>
        /// -a~~~~~~~~~1s---d~~~~~~~|--><br/>
        /// -A--------------D-------|--><br/>
        /// </code>
        /// <br/>
        /// <see href="https://rxmarbles.com/#throttle">Interactive</see>
        /// (<c>throttle</c> in RxJs)
        /// </remarks>
        /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An observable sequence whose elements to debounce.</param>
        /// <param name="intervalMs">
        /// Time to wait before emitting another item after emitting the last one.
        /// Defaults to <see cref="DebounceMonitor.DefaultInterval"/> if not specified.
        /// </param>
        /// <returns>an <see cref="IObservable{T}"/> that performs the debounce operation</returns>
        public static IObservable<T> Debounce<T>(this IObservable<T> source, int? intervalMs = null)
        {
            var interval = intervalMs.HasValue
                ? TimeSpan.FromMilliseconds(intervalMs.Value)
                : DebounceMonitor.DefaultInterval;

            return new ObservableDebounceDecorator<T>(source, interval);
        }
    }
}