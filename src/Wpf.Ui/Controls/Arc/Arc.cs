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
    /// <summary>Identifies the <see cref="StartAngle"/> dependency property.</summary>
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

    /// <summary>Identifies the <see cref="SweepDirection"/> dependency property.</summary>
    public static readonly DependencyProperty SweepDirectionProperty = DependencyProperty.Register(
        nameof(SweepDirection),
        typeof(SweepDirection),
        typeof(Arc),
        new PropertyMetadata(SweepDirection.Clockwise, PropertyChangedCallback)
    );

    static Arc()
    {
        // Modify the metadata of the StrokeStartLineCap dependency property.
        StrokeStartLineCapProperty.OverrideMetadata(
            typeof(Arc),
            new FrameworkPropertyMetadata(PenLineCap.Round, PropertyChangedCallback)
        );

        // Modify the metadata of the StrokeEndLineCap dependency property.
        StrokeEndLineCapProperty.OverrideMetadata(
            typeof(Arc),
            new FrameworkPropertyMetadata(PenLineCap.Round, PropertyChangedCallback)
        );
    }

    /// <summary>
    /// Gets or sets the initial angle from which the arc will be drawn.
    /// </summary>
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

    /// <summary>
    /// Gets a value indicating whether one of the two larger arc sweeps is chosen; otherwise, if is <see langword="false"/>, one of the smaller arc sweeps is chosen.
    /// </summary>
    public bool IsLargeArc { get; internal set; } = false;

    /// <inheritdoc />
    protected override Geometry DefiningGeometry => DefinedGeometry();

    /// <summary>
    /// Get the geometry that defines this shape.
    /// <para><see href="https://stackoverflow.com/a/36756365/13224348">Based on Mark Feldman implementation.</see></para>
    /// </summary>
    protected Geometry DefinedGeometry()
    {
        var geometryStream = new StreamGeometry();
        var arcSize = new Size(
            Math.Max(0, (RenderSize.Width - StrokeThickness) / 2),
            Math.Max(0, (RenderSize.Height - StrokeThickness) / 2)
        );

        using StreamGeometryContext context = geometryStream.Open();
        context.BeginFigure(PointAtAngle(Math.Min(StartAngle, EndAngle)), false, false);

        context.ArcTo(
            PointAtAngle(Math.Max(StartAngle, EndAngle)),
            arcSize,
            0,
            IsLargeArc,
            SweepDirection,
            true,
            false
        );

        geometryStream.Transform = new TranslateTransform(StrokeThickness / 2, StrokeThickness / 2);

        return geometryStream;
    }

    /// <summary>
    /// Draws a point on the coordinates of the given angle.
    /// <para><see href="https://stackoverflow.com/a/36756365/13224348">Based on Mark Feldman implementation.</see></para>
    /// </summary>
    /// <param name="angle">The angle at which to create the point.</param>
    protected Point PointAtAngle(double angle)
    {
        if (SweepDirection == SweepDirection.Counterclockwise)
        {
            angle += 90;
            angle %= 360;
            if (angle < 0)
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

        control.IsLargeArc = Math.Abs(control.EndAngle - control.StartAngle) > 180;
        control.InvalidateVisual();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Geometry calculations depend on RenderSize, so we need to invalidate visual when size changes.
        // The base Shape class doesn't do this automatically for custom-sized geometries.
        InvalidateVisual();

        return base.ArrangeOverride(finalSize);
    }
}
