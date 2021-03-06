using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DebounceMonitoring
{
    /// <summary>
    /// Attaches a debounce countdown to a reference object (a type for static calls)
    /// and the call location (member + line number).
    /// </summary>
    public static class DebounceMonitor
    {
        /// <summary>
        /// The initial interval used for all debounce calls when no corresponding parameter is specified.
        /// </summary>
        public const int InitialDefaultIntervalMs = 500;

        private static readonly ConcurrentDictionary<string, ObjectTimeStamps> _debounceTimeStamps = new();

        /// <summary>
        /// An interval used in <see cref="DebounceHere"/> and <see cref="DebounceHereStatic"/>
        /// when the corresponding parameter is not specified. <br/>
        /// </summary>
        /// <remarks>
        /// Initial value is <see cref="InitialDefaultIntervalMs"/>.
        /// </remarks>
        public static TimeSpan DefaultInterval { get; set; } = TimeSpan.FromMilliseconds(InitialDefaultIntervalMs);

        /// <summary>
        /// Make <see cref="DebounceHere"/> and <see cref="DebounceHereStatic"/> always return <c>false</c>.
        /// </summary>
        /// <remarks>
        /// Useful for automated tests. May be set globally in ModuleInitializer
        /// with <see href="https://docs.microsoft.com/dotnet/api/system.runtime.compilerservices.moduleinitializerattribute">ModuleInitializerAttribute</see>
        /// or <see href="https://github.com/Fody/ModuleInit">Fody.ModuleInit</see>.
        /// </remarks>
        public static bool Disabled { get; set; }

        /// <summary>
        /// Сhecks whether the call of the current line for the <paramref name="debounceReference"/>
        /// has been repeated within the <paramref name="intervalMs"/> since the last non-repeating call.
        /// </summary>
        /// <remarks>
        /// Usage:
        /// <code>
        /// if (this.DebounceHere()) return;
        /// </code>
        /// </remarks>
        /// <param name="debounceReference">
        /// Debounce countdown reference.
        /// Usually a caller member owner.
        /// It is held weakly.
        /// </param>
        /// <param name="intervalMs">Defaults to <see cref="DefaultInterval"/> if not specified.</param>
        /// <param name="memberName">A compiler-supplied identifier for the call location</param>
        /// <param name="lineNumber">A compiler-supplied identifier for the call location</param>
        public static bool DebounceHere<TReference>(
                this TReference debounceReference,
                int? intervalMs = null,
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = default)
            where TReference : class
        {
            if (Disabled)
                return false;

            var locationKey = $"{memberName}_{lineNumber}";

            var now = DateTime.UtcNow;

            var objectTimeStamps = _debounceTimeStamps.GetOrAdd(locationKey, valueFactory: _ => new());
            ref var lastStamp = ref objectTimeStamps.GetOrAddRef(debounceReference);

            var interval = intervalMs.HasValue
                ? TimeSpan.FromMilliseconds(intervalMs.Value)
                : DefaultInterval;

            if (now < lastStamp + interval)
                return true;

            lastStamp = now;
            return false;
        }

        /// <summary>
        /// Сhecks whether the call of the current line for the <typeparamref name="TReferenceType"/>
        /// has been repeated within the <paramref name="intervalMs"/> since the last non-repeating call.
        /// </summary>
        /// <remarks>
        /// Usage:
        /// <code>
        /// if (DebounceMonitor.DebounceHereStatic&lt;SomeStaticClass&gt;()) return;
        /// </code>
        /// or
        /// <code>
        /// using static DebounceMonitoring.DebounceMonitor; <br/>
        /// ... <br/>
        /// if (DebounceHereStatic&lt;SomeStaticClass&gt;()) return;
        /// </code>
        /// </remarks>
        /// <typeparam name="TReferenceType">Debounce countdown reference as a Type.</typeparam>
        /// <param name="intervalMs">Defaults to <see cref="DefaultInterval"/> if not specified.</param>
        /// <param name="memberName">A compiler-supplied identifier for the call location</param>
        /// <param name="lineNumber">A compiler-supplied identifier for the call location</param>
        public static bool DebounceHereStatic<TReferenceType>(
                int? intervalMs = null,
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = default)
            where TReferenceType : class =>
                DebounceHereStatic(typeof(TReferenceType), intervalMs, memberName, lineNumber);

        /// <summary>
        /// Сhecks whether the call of the current line for the <paramref name="debounceReference"/>
        /// has been repeated within the <paramref name="intervalMs"/> since the last non-repeating call.
        /// </summary>
        /// <remarks>
        /// Usage:
        /// <code>
        /// if (DebounceMonitor.DebounceHereStatic(typeof(SomeStaticClass))) return;
        /// </code>
        /// or
        /// <code>
        /// using static DebounceMonitoring.DebounceMonitor; <br/>
        /// ... <br/>
        /// if (DebounceHereStatic(typeof(SomeStaticClass))) return;
        /// </code>
        /// </remarks>
        /// <param name="debounceReference">Debounce countdown reference.</param>
        /// <param name="intervalMs">Defaults to <see cref="DefaultInterval"/> if not specified.</param>
        /// <param name="memberName">A compiler-supplied identifier for the call location</param>
        /// <param name="lineNumber">A compiler-supplied identifier for the call location</param>
        public static bool DebounceHereStatic(
                Type debounceReference,
                int? intervalMs = null,
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = default) =>
            DebounceHere(debounceReference, intervalMs, memberName, lineNumber);
    }
}