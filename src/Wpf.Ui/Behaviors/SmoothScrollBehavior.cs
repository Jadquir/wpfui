// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Behaviors
{
    public static class SmoothScrollBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(SmoothScrollBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static readonly DependencyProperty AnimatedVerticalOffsetProperty = DependencyProperty.RegisterAttached(
            "AnimatedVerticalOffset", typeof(double), typeof(SmoothScrollBehavior),
            new PropertyMetadata(0.0, OnAnimatedVerticalOffsetChanged));

        private static readonly DependencyProperty TargetVerticalOffsetProperty = DependencyProperty.RegisterAttached(
            "TargetVerticalOffset", typeof(double), typeof(SmoothScrollBehavior),
            new PropertyMetadata(0.0));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer scrollViewer) return;

            if ((bool)e.NewValue)
            {
                scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
                scrollViewer.SetValue(TargetVerticalOffsetProperty, scrollViewer.VerticalOffset);
                scrollViewer.SetValue(AnimatedVerticalOffsetProperty, scrollViewer.VerticalOffset);
            }
            else
            {
                scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            }
        }

        private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;

            e.Handled = true;

            // Get current target (where we're animating TO)
            double currentTarget = (double)scrollViewer.GetValue(TargetVerticalOffsetProperty);
            double currentAnimated = (double)scrollViewer.GetValue(AnimatedVerticalOffsetProperty);
            double actualOffset = scrollViewer.VerticalOffset;

            // Sync check: if user manually scrolled (grabbed scrollbar), reset everything
            if (Math.Abs(actualOffset - currentAnimated) > 5)
            {
                currentTarget = actualOffset;
                scrollViewer.SetValue(AnimatedVerticalOffsetProperty, actualOffset);
            }

            // Calculate scroll delta (3 lines per wheel notch, 48px per line)
            double scrollAmount = (e.Delta / 120.0) * 48.0 * 3.0;

            // NEW TARGET: accumulate on the existing target (allows momentum)
            double newTarget = currentTarget - scrollAmount;
            newTarget = Math.Max(0, Math.Min(scrollViewer.ScrollableHeight, newTarget));

            scrollViewer.SetValue(TargetVerticalOffsetProperty, newTarget);

            // Calculate distance to determine optimal animation duration
            double distance = Math.Abs(newTarget - currentAnimated);

            // Longer, more relaxed duration for smoother feel
            // Scale with distance but keep it substantial
            double duration = Math.Min(500, Math.Max(300, distance * 0.8));

            // Create smooth animation with exponential ease out for gentler deceleration
            // CRITICAL: Set From to current position to avoid jump to 0
            var animation = new DoubleAnimation
            {
                From = currentAnimated, // Start from where we currently are
                To = newTarget,
                Duration = TimeSpan.FromMilliseconds(duration),
                // ExponentialEase with EaseOut creates a much gentler, more natural deceleration
                // Power of 7 gives it a smooth, luxurious feel without abrupt ending
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3 },
                FillBehavior = FillBehavior.HoldEnd
            };

            scrollViewer.BeginAnimation(AnimatedVerticalOffsetProperty, animation);
        }

        private static void OnAnimatedVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }
}