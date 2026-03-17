using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using BetterCommentsPlus.Options;

namespace BetterCommentsPlus.CommentsViewCustomization
{
    internal sealed class BackgroundAdorner
    {
        private const double CornerRadius = 2.0;

        private readonly IAdornmentLayer layer;
        private readonly IWpfTextView view;
        private readonly Thickness tBlur = new(2, -3, 2, -3);
        private readonly Thickness tNone = new(0, 0, 0, 0);
        private char[] firstChars;
        private List<CommentToken> tokens;
        private Performance performance = Performance.Normal;

        public BackgroundAdorner(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            BackgroundHelper.InitDefaults();

            RefreshCriteria();

            layer = view.GetAdornmentLayer("BackgroundAdorner");
            this.view = view;
            this.view.LayoutChanged += OnLayoutChanged;
            this.view.ViewportWidthChanged += OnViewportWidthChanged;

            Settings.Instance.ConfigurationChanged += OnConfigurationChanged;
        }

        private void OnConfigurationChanged(object sender, EventArgs e)
        {
            layer.RemoveAllAdornments();
            RefreshCriteria();

            try
            {
                foreach (ITextViewLine line in view.TextViewLines)
                {
                    if (line.VisibilityState == VisibilityState.FullyVisible)
                        CreateVisuals(line);
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        private void OnViewportWidthChanged(object sender, EventArgs e)
        {
            layer.RemoveAllAdornments();

            try
            {
                foreach (ITextViewLine line in view.TextViewLines)
                {
                    if (line.VisibilityState == VisibilityState.FullyVisible)
                        CreateVisuals(line);
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        private void RefreshCriteria()
        {
            performance = Performance.Normal;

            tokens = Settings.Instance.CommentTokens
                .Where(x => x.BackgroundStyle != null && x.BackgroundStyle.IsActive && !string.IsNullOrEmpty(x.BackgroundStyle.ColorHex))
                .ToList();

            List<char> chars = new List<char>();
            chars.AddRange(tokens.Select(y => y.Criteria[0]));
            chars.AddRange(tokens.Where(x => !x.BackgroundStyle.IsCaseSensitive).Select(y => y.Criteria.ToUpper()[0]));
            firstChars = chars.ToArray();
        }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (tokens == null || !tokens.Any())
                return;

            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                CreateVisuals(line);
            }
        }

        private void CreateVisuals(ITextViewLine line)
        {
            IWpfTextViewLineCollection textViewLines = view.TextViewLines;
            int start = line.Start;
            int end = line.End;
            List<Geometry> geometries = new();

            for (int i = start; i < end; i++)
            {
                if (firstChars.Contains(view.TextSnapshot[i]))
                {
                    foreach (var token in tokens)
                    {
                        string keyword = token.Criteria.Trim();
                        if (FirstCharacterEquals(view.TextSnapshot[i], keyword[0], token.BackgroundStyle.IsCaseSensitive) &&
                            i <= end - keyword.Length &&
                            CompareWords(view.TextSnapshot.GetText(i, keyword.Length), keyword, token.BackgroundStyle.IsCaseSensitive) &&
                            CheckWholeWordsMatch(view.TextSnapshot, i, keyword, token.BackgroundStyle.AllowPartialMatch))
                        {
                            SnapshotSpan span;
                            TagShape shape = (TagShape)Enum.Parse(typeof(TagShape), token.BackgroundStyle.Shape);

                            if (shape == TagShape.Line || shape == TagShape.LineUnder)
                            {
                                span = new SnapshotSpan(view.TextSnapshot, Span.FromBounds(i, line.End));
                            }
                            else
                            {
                                span = new SnapshotSpan(view.TextSnapshot, Span.FromBounds(i, i + keyword.Length));
                            }

                            BlurIntensity blur = (BlurIntensity)Enum.Parse(typeof(BlurIntensity), token.BackgroundStyle.Blur);
                            Geometry markerGeometry = textViewLines.GetMarkerGeometry(span, false,
                                blur == BlurIntensity.None ? tNone : tBlur);

                            if (markerGeometry != null)
                            {
                                if (!geometries.Any(g => g.FillContainsWithDetail(markerGeometry) > IntersectionDetail.Empty))
                                {
                                    geometries.Add(markerGeometry);
                                    AddMarker(span, markerGeometry, token);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool FirstCharacterEquals(char text, char keyword, bool isCaseSensitive)
        {
            return isCaseSensitive ? text == keyword : char.ToUpperInvariant(text) == char.ToUpperInvariant(keyword);
        }

        private static bool CompareWords(string text, string keyword, bool isCaseSensitive)
        {
            return isCaseSensitive ? text == keyword : string.Equals(text, keyword, StringComparison.OrdinalIgnoreCase);
        }

        public bool CheckWholeWordsMatch(ITextSnapshot textSnapshot, int i, string keyword, bool allowPartialMatch)
        {
            if (allowPartialMatch)
            {
                return true;
            }
            else
            {
                return BackgroundHelper.Escapes.Contains(Convert.ToChar(textSnapshot.GetText(Math.Max(0, i - 1), 1))) &&
                       BackgroundHelper.Escapes.Contains(Convert.ToChar(textSnapshot.GetText(i + keyword.Length, 1)));
            }
        }

        private double CalculateDynamicWidth(Geometry markerGeometry, CommentToken token)
        {
            double baseWidth = markerGeometry.Bounds.Width;

            if (baseWidth < 0)
                baseWidth = 0;

            TagShape shape = (TagShape)Enum.Parse(typeof(TagShape), token.BackgroundStyle.Shape);

            if (shape == TagShape.Tag || shape == TagShape.TagUnder)
            {
                return baseWidth + 3;
            }
            else if (shape == TagShape.Line || shape == TagShape.LineUnder)
            {
                return baseWidth + 5;
            }
            else
            {
                return baseWidth + 3;
            }
        }

        private void AddMarker(SnapshotSpan span, Geometry markerGeometry, CommentToken token)
        {
            double width = CalculateDynamicWidth(markerGeometry, token);
            double height = markerGeometry.Bounds.Height;

            TagShape shape = (TagShape)Enum.Parse(typeof(TagShape), token.BackgroundStyle.Shape);
            BlurIntensity blur = (BlurIntensity)Enum.Parse(typeof(BlurIntensity), token.BackgroundStyle.Blur);
            FillAlpha alpha = (FillAlpha)Enum.Parse(typeof(FillAlpha), token.BackgroundStyle.Alpha);
            Color color = BackgroundHelper.HexToColor(token.BackgroundStyle.ColorHex);

            Rectangle r = new()
            {
                Fill = new SolidColorBrush(color.ChangeAlpha(60)),
                RadiusX = CornerRadius,
                RadiusY = CornerRadius,
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(color.ChangeAlpha(255))
            };

            bool isLine = shape == TagShape.Line;
            bool isTag = shape == TagShape.Tag;

            if (shape == TagShape.TagUnder)
            {
                r.Height = 1.5;
            }
            else if (shape == TagShape.LineUnder)
            {
                r.Height = 1.5;
            }
            else if (shape == TagShape.Line)
            {
                if (performance != Performance.NoEffects && blur != BlurIntensity.None)
                    r.Height = markerGeometry.Bounds.Height + 5.0;
            }
            else if (shape == TagShape.Tag)
            {
                if (performance != Performance.NoEffects && blur != BlurIntensity.None)
                    r.Height = markerGeometry.Bounds.Height + 5.0;
            }

            if (performance != Performance.NoEffects && blur != BlurIntensity.None)
            {
                r.Effect = new BlurEffect
                {
                    KernelType = performance == Performance.Normal ? KernelType.Gaussian : KernelType.Box,
                    RenderingBias = RenderingBias.Performance
                };

                switch (blur)
                {
                    case BlurIntensity.Low:
                        ((BlurEffect)r.Effect).Radius = isLine ? 1 : 1.0;
                        break;

                    case BlurIntensity.Medium:
                        ((BlurEffect)r.Effect).Radius = isLine ? 2 : 2.0;
                        break;

                    case BlurIntensity.High:
                        ((BlurEffect)r.Effect).Radius = isLine ? 4 : 4.0;
                        break;

                    case BlurIntensity.Ultra:
                        ((BlurEffect)r.Effect).Radius = isLine ? 6 : 10.0;
                        break;
                }

                r.Stroke = null;

                if (r.Effect.CanFreeze)
                    r.Effect.Freeze();
            }

            r.Fill = new SolidColorBrush(((SolidColorBrush)r.Fill).Color.ChangeAlpha((byte)((255 * (byte)alpha) / 10)));

            if (shape == TagShape.TagUnder)
                Canvas.SetLeft(r, markerGeometry.Bounds.Left);
            else if (shape == TagShape.LineUnder)
                Canvas.SetLeft(r, markerGeometry.Bounds.Left);
            else
            {
                if (performance != Performance.NoEffects && blur != BlurIntensity.None)
                    Canvas.SetLeft(r, markerGeometry.Bounds.Left);
                else
                    Canvas.SetLeft(r, markerGeometry.Bounds.Left - 1.5);
            }

            if (r.Fill.CanFreeze)
                r.Fill.Freeze();

            if (r.Stroke is { CanFreeze: true })
                r.Stroke.Freeze();

            if (shape == TagShape.TagUnder)
            {
                Canvas.SetTop(r, markerGeometry.Bounds.Top + markerGeometry.Bounds.Height - 2);
            }
            else if (shape == TagShape.LineUnder)
            {
                Canvas.SetTop(r, markerGeometry.Bounds.Top + markerGeometry.Bounds.Height - 2);
            }
            else
            {
                if (performance != Performance.NoEffects && blur != BlurIntensity.None)
                    Canvas.SetTop(r, markerGeometry.Bounds.Top - 1.5);
                else
                    Canvas.SetTop(r, markerGeometry.Bounds.Top);
            }

            layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, r, null);
        }
    }
}
