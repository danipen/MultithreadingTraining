using System;
using System.Collections.Generic;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace MultithreadingTraining
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            DockPanel mainPanel = new DockPanel();

            Button calculateButton = new Button
            {
                Content = "Calculate",
                Margin = new Thickness(5)
            };
            calculateButton.Click += CalculateButton_Click;

            Button cancelButton = new Button
            {
                Content = "Cancel",
                Margin = new Thickness(5)
            };
            cancelButton.Click += CancelButton_Click;

            StackPanel buttonsPanel = new StackPanel();
            buttonsPanel.Orientation = Orientation.Horizontal;
            buttonsPanel.Children.Add(calculateButton);
            buttonsPanel.Children.Add(cancelButton);

            DockPanel.SetDock(buttonsPanel, Dock.Top);
            mainPanel.Children.Add(buttonsPanel);

            ScrollViewer scrollViewer = new ScrollViewer();

            this.Content = mainPanel;

            mainPanel.Children.Add(scrollViewer);

            mStackPanel = new StackPanel();
            scrollViewer.Content = mStackPanel;
        }

        void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            mOperation.Cancel();
        }

        void Timer_Tick(object? sender, EventArgs e)
        {
            // the dispatcher thread notifies you here in the UI thread
            // so now we're safe to upadte the UI

            List<long> numbers = mOperation.GetCalculatedValues();

            foreach (var value in numbers)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = value.ToString(),
                    Margin = new Thickness(5)
                };
                mStackPanel.Children.Add(textBlock);
            }
        }

        private void CalculateButton_Click(object? sender, RoutedEventArgs e)
        {
            mOperation = new CalculateCapicuaOperation();
            mOperation.Start();
            mTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, Timer_Tick);
            mTimer.Start();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        class CalculateCapicuaOperation
        {
            internal void Cancel()
            {
                mIsCancelled = true;
            }

            public void Start()
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(Calculate));
            }

            public List<long> GetCalculatedValues()
            {
                // this method is going be to accessed from the UI thread
                List<long> calculatedValues = new List<long>();

                lock (mLock)
                {
                    while (mQueue.Count > 0)
                    {
                        calculatedValues.Add(mQueue.Dequeue());
                    }
                }

                return calculatedValues;
            }

            void Calculate(object state)
            {
                // this method is executed in the thread

                long nextNumber = 10000000000000;
                while (!mIsCancelled)
                {
                    try
                    {
                        // we're on the thred
                        nextNumber = Capicua.Next(nextNumber);
                    }
                    catch (Exception ex)
                    {
                        // log me
                    }

                    lock (mLock)
                    {
                        mQueue.Enqueue(nextNumber);
                    }
                }
            }

            volatile bool mIsCancelled = false;
            object mLock = new object();
            Queue<long> mQueue = new Queue<long>();
        }

        StackPanel mStackPanel;
        DispatcherTimer mTimer;
        CalculateCapicuaOperation mOperation;
    }
}