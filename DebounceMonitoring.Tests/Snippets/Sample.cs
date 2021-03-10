using System;
using System.Reactive.Linq;
using System.Windows.Input;

#pragma warning disable CS0067 // The event is never used

namespace DebounceMonitoring.Tests.Snippets
{
    internal class ViewModel
    {
        public void OnButtonClick()
        {
            if (this.DebounceHere()) return;

            // Handle the click
        }

        public Command ClickCommand { get; }

        public ViewModel()
        {
            ClickCommand = new Command(() =>
            {
                if (this.DebounceHere()) return;

                // Handle the click
            });
        }

        public void ObservableDebounce(Button button)
        {
            button.ClickAsObservable()
                .Debounce()
                .Subscribe(_ => OnButtonClick());
        }

        public void MultipleObjectsOneMethod()
        {
            new Button().Click += OnClick;
            new Button().Click += OnClick;
            new Button().Click += OnClick;

            void OnClick(object? sender, EventArgs e)
            {
                if (this.DebounceHere()) return; // NOT RECOMMENDED!
                if (sender!.DebounceHere()) return;

                // Handle the click
            }
        }
    }

    internal class Analytics
    {
        public static void TrackEvent()
        {
            if (DebounceMonitor.DebounceHereStatic<Analytics>()) return;

            // The logic
        }
    }

    internal static class UnitTestGlobalSetup
    {
        //[System.Runtime.CompilerServices.ModuleInitializer]
        internal static void SetupDebounceMonitor() => DebounceMonitor.Disabled = true;
    }

    internal class Button
    {
        public event EventHandler? Click;
    }

    internal static class ButtonRxExtensions
    {
        public static IObservable<EventArgs> ClickAsObservable(this Button self)
        {
            return Observable.FromEvent<EventHandler, EventArgs>(
                h => (s, e) => h(e),
                h => self.Click += h,
                h => self.Click -= h);
        }
    }

    internal class Command : ICommand
    {
        private readonly Action _action;

        public Command(Action action)
        {
            _action = action;
        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _action.Invoke();
    }
}