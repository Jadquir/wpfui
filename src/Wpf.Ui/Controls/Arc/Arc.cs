// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

// ReSharper disable CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Control that draws a symmetrical arc with rounded edges.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:Arc
///     EndAngle="359"
///     StartAngle="0"
///     Stroke="{ui:ThemeResource SystemAccentColorSecondaryBrush}"
///     StrokeThickness="2"
///     Visibility="Visible" /&gt;
/// </code>
/// </example>
public class Arc : Shape
{
    public static readonly DependencyProperty StartAngleProperty = DependencyProperty.Register(
        nameof(StartAngle),
        typeof(double),
        typeof(Arc),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty EndAngleProperty = DependencyProperty.Register(
        nameof(EndAngle),
        typeof(double),
        typeof(Arc),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public double StartAngle
    {
        get => (double)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public double EndAngle
    {
        get => (double)GetValue(EndAngleProperty);
        set => SetValue(EndAngleProperty, value);
    }

    // We override the metadata to ensure the base Shape class knows to use Round caps
    static Arc()
    {
        StrokeStartLineCapProperty.OverrideMetadata(typeof(Arc), new FrameworkPropertyMetadata(PenLineCap.Round));
        StrokeEndLineCapProperty.OverrideMetadata(typeof(Arc), new FrameworkPropertyMetadata(PenLineCap.Round));
    }

    protected override Geometry DefiningGeometry
    {
        get
        {
            double width = RenderSize.Width;
            double height = RenderSize.Height;

            if (width <= 0 || height <= 0 || StrokeThickness >= width / 2)
            {
                return Geometry.Empty;
            }

            double xRadius = (width - StrokeThickness) / 2;
            double yRadius = (height - StrokeThickness) / 2;

            bool isLargeArc = Math.Abs(EndAngle - StartAngle) > 180;

            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(Arc.PointAtAngle(StartAngle, xRadius, yRadius), false, false);
                context.ArcTo(
                    Arc.PointAtAngle(EndAngle, xRadius, yRadius),
                    new Size(xRadius, yRadius),
                    0,
                    isLargeArc,
                    SweepDirection.Clockwise,
                    true,
                    false);
            }

            // Move the geometry so it accounts for the StrokeThickness offset
            geometry.Transform = new TranslateTransform(StrokeThickness / 2, StrokeThickness / 2);

            // CRITICAL: Freezing makes the geometry read-only and much faster to render
            geometry.Freeze();

            return geometry;
        }
    }

    private static Point PointAtAngle(double angle, double xRadius, double yRadius)
    {
        // Adjust angle so 0 starts at the top (12 o'clock)
        double radAngle = (angle - 90) * (Math.PI / 180.0);

        return new Point(
            xRadius + (xRadius * Math.Cos(radAngle)),
            yRadius + (yRadius * Math.Sin(radAngle))
        );
    }
}
