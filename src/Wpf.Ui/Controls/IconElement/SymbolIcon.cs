// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents a text element containing an icon glyph.
/// </summary>
public class SymbolIcon : FontIcon
{
    /// <summary>Identifies the <see cref="Symbol"/> dependency property.</summary>
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
        nameof(Symbol),
        typeof(SymbolRegular),
        typeof(SymbolIcon),
        new PropertyMetadata(SymbolRegular.Empty, static (o, _) => ((SymbolIcon)o).OnGlyphChanged())
    );

    /// <summary>Identifies the <see cref="Filled"/> dependency property.</summary>
    public static readonly DependencyProperty FilledProperty = DependencyProperty.Register(
        nameof(Filled),
        typeof(bool),
        typeof(SymbolIcon),
        new PropertyMetadata(false, OnFilledChanged)
    );

    /// <summary>
    /// Gets or sets displayed <see cref="SymbolRegular"/>.
    /// </summary>
    public SymbolRegular Symbol
    {
        get => (SymbolRegular)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not we should use the <see cref="SymbolFilled"/>.
    /// </summary>
    public bool Filled
    {
        get => (bool)GetValue(FilledProperty);
        set => SetValue(FilledProperty, value);
    }

    public SymbolIcon() { }

    public SymbolIcon(SymbolRegular symbol, double fontSize = 14, bool filled = false)
    {
        Symbol = symbol;
        Filled = filled;
        FontSize = fontSize;
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        SetFontReference();
    }

    private void OnGlyphChanged()
    {
        if (Filled)
        {
            SetCurrentValue(GlyphProperty, Symbol.Swap().GetString());
        }
        else
        {
            SetCurrentValue(GlyphProperty, Symbol.GetString());
        }
    }

    private void SetFontReference()
    {
        SetResourceReference(FontFamilyProperty, Filled ? "FluentSystemIconsFilled" : "FluentSystemIcons");
    }

    private static void OnFilledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (SymbolIcon)d;
        self.SetFontReference();
        self.OnGlyphChanged();
    }
    public ImageSource ToImageSource(int size, Brush backgroundBrush,
        double fontSize)
    {
        return ConvertFontIconToImageSource(this, Foreground, backgroundBrush, fontSize, size, size);
    }
    public ImageSource ToImageSource(int width, int height, Brush backgroundBrush,
        double fontSize)
    {
        return ConvertFontIconToImageSource(this, Foreground, backgroundBrush, fontSize, width, height);
    }
    public static ImageSource ConvertFontIconToImageSource(FontIcon fontIcon,
        Brush foregroundBrush,
        Brush backgroundBrush,
        double fontSize,
        double width, double height)
    {
        // Create a Canvas to host the FontIcon
        var canvas = new Canvas();
        canvas.Width = width;
        canvas.Height = height;

        canvas.Background = backgroundBrush;

        // Set the foreground color of the FontIcon
        fontIcon.Foreground = foregroundBrush;
        fontIcon.VerticalAlignment = VerticalAlignment.Stretch;
        fontIcon.HorizontalAlignment = HorizontalAlignment.Stretch;
        // Add the FontIcon to the Canvas
        canvas.Children.Add(fontIcon);
        fontIcon.Width = width;
        fontIcon.Height = height;
        fontIcon.FontSize = fontSize;

        // Measure and arrange the Canvas to properly position the FontIcon
        canvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        canvas.Arrange(new Rect(0, 0, width, height));

        // Render the Canvas to a RenderTargetBitmap
        var renderTargetBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
        renderTargetBitmap.Render(canvas);

        return renderTargetBitmap;
    }
}
