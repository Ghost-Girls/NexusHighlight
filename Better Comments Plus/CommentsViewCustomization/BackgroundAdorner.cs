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
        private const double cornerRadius = 2.0;

        private readonly IAdornmentLayer layer;
        private readonly IWpfTextView view;
        private readonly Thickness tBlur = new(2, -3, 2, -3);
        private readonly Thickness tNone = new(0, 0, 0, 0);
        private char[] firstChars;
        private List<CommentRule> tags;
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

        private void RefreshCriteria()
        {
            performance = Performance.Normal;

            tags = Settings.Instance.AllRules
                .Where(x => x.Background != null && x.Background.IsActive)
                .ToList();

            List<char> chars = new List<char>();
            foreach (var tag in tags)
            {
                if (!string.IsNullOrEmpty(tag.Criteria))
                {
                    chars.Add(tag.Criteria[0]);
                    if (!tag.Background.IsCaseSensitive)
                    {
                        chars.Add(char.ToUpperInvariant(tag.Criteria[0]));
                    }
                }
            }
            firstChars = chars.ToArray();
        }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (tags == null || !tags.Any())
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
                    foreach (var tag in tags)
                    {
                        if (string.IsNullOrEmpty(tag.Criteria))
                            continue;

                        string keyword = tag.Criteria.Trim();
                        if (string.IsNullOrEmpty(keyword))
                            continue;

                        if (FirstCharacterEquals(view.TextSnapshot[i], keyword[0], tag.Background.IsCaseSensitive) &&
                            i <= end - keyword.Length &&
                            CompareWords(view.TextSnapshot.GetText(i, keyword.Length), keyword, tag.Background.IsCaseSensitive) &&
                            CheckWholeWordsMatch(view.TextSnapshot, i, keyword, tag.Background.AllowPartialMatch))
                        {
                            SnapshotSpan span;
                            TagShape shape;
                            try
                            {
                                shape = (TagShape)Enum.Parse(typeof(TagShape), tag.Background.Shape ?? "Tag");
                            }
                            catch
                            {
                                shape = TagShape.Tag;
                            }

                            if (shape == TagShape.Line || shape == TagShape.LineUnder)
                            {
                                span = new SnapshotSpan(view.TextSnapshot, Span.FromBounds(i, line.End));
                            }
                            else
                            {
                                span = new SnapshotSpan(view.TextSnapshot, Span.FromBounds(i, i + keyword.Length));
                            }

                            BlurIntensity blur;
                            try
                            {
                                blur = (BlurIntensity)Enum.Parse(typeof(BlurIntensity), tag.Background.Blur ?? "None");
                            }
                            catch
                            {
                                blur = BlurIntensity.None;
                            }

                            Geometry markerGeometry = textViewLines.GetMarkerGeometry(span, false,
                                blur == BlurIntensity.None ? tNone : tBlur);

                            if (markerGeometry != null)
                            {
                                if (!geometries.Any(g => g.FillContainsWithDetail(markerGeometry) > IntersectionDetail.Empty))
                                {
                                    geometries.Add(markerGeometry);
                                    AddMarker(span, markerGeometry, tag);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool FirstCharacterEquals(char text, char keyword, bool isCaseSensitive)
        {
            if (isCaseSensitive)
            {
                return text == keyword;
            }
            else
            {
                return char.ToUpperInvariant(text) == char.ToUpperInvariant(keyword);
            }
        }

        private static bool CompareWords(string text, string keyword, bool isCaseSensitive)
        {
            if (isCaseSensitive)
            {
                return text == keyword;
            }
            else
            {
                return string.Equals(text, keyword, StringComparison.OrdinalIgnoreCase);
            }
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

        private double CalculateDynamicWidth(Geometry markerGeometry, CommentRule rule)
        {
            double baseWidth = markerGeometry.Bounds.Width;

            if (baseWidth < 0)
                baseWidth = 0;

            TagShape shape;
            try
            {
                shape = (TagShape)Enum.Parse(typeof(TagShape), rule.Background?.Shape ?? "Tag");
            }
            catch
            {
                shape = TagShape.Tag;
            }

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

        private string ConvertAlphaToEnumFormat(string alphaDisplay)
        {
            if (alphaDisplay == "0/10") return "Alpha_0_10";
            if (alphaDisplay == "1/10") return "Alpha_1_10";
            if (alphaDisplay == "2/10") return "Alpha_2_10";
            if (alphaDisplay == "3/10") return "Alpha_3_10";
            if (alphaDisplay == "4/10") return "Alpha_4_10";
            if (alphaDisplay == "5/10") return "Alpha_5_10";
            if (alphaDisplay == "6/10") return "Alpha_6_10";
            if (alphaDisplay == "7/10") return "Alpha_7_10";
            if (alphaDisplay == "8/10") return "Alpha_8_10";
            if (alphaDisplay == "9/10") return "Alpha_9_10";
            if (alphaDisplay == "10/10") return "Alpha_10_10";
            return "Alpha_1_10";
        }

        private void AddMarker(SnapshotSpan span, Geometry markerGeometry, CommentRule rule)
        {
            double width = CalculateDynamicWidth(markerGeometry, rule);
            double height = markerGeometry.Bounds.Height;

            TagShape shape;
            try
            {
                shape = (TagShape)Enum.Parse(typeof(TagShape), rule.Background?.Shape ?? "Tag");
            }
            catch
            {
                shape = TagShape.Tag;
            }

            BlurIntensity blur;
            try
            {
                blur = (BlurIntensity)Enum.Parse(typeof(BlurIntensity), rule.Background.Blur ?? "None");
            }
            catch
            {
                blur = BlurIntensity.None;
            }

            FillAlpha alpha;
            try
            {
                string alphaEnumFormat = ConvertAlphaToEnumFormat(rule.Background.Alpha ?? "1/10");
                alpha = (FillAlpha)Enum.Parse(typeof(FillAlpha), alphaEnumFormat);
            }
            catch
            {
                alpha = FillAlpha.Alpha_1_10;
            }

            Color color;
            if (!string.IsNullOrEmpty(rule.Background.ColorHex))
            {
                color = BackgroundHelper.HexToColor(rule.Background.ColorHex);
            }
            else if (!string.IsNullOrEmpty(rule.ColorHex))
            {
                color = BackgroundHelper.HexToColor(rule.ColorHex);
            }
            else
            {
                color = Colors.Red;
            }

            Rectangle r = new()
            {
                Fill = new SolidColorBrush(color.ChangeAlpha(60)),
                RadiusX = cornerRadius,
                RadiusY = cornerRadius,
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
