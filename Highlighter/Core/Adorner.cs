﻿using Microsoft.VisualStudio.Text;
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

namespace Highlighter.Core
{
    internal sealed class Adorner
    {
        private const double cornerRadius = 2.0;

        private readonly IAdornmentLayer layer;

        private readonly IWpfTextView view;

        private Thickness tBlur = new(2, -3, 2, -3);
        private Thickness tNone = new(0, 0, 0, 0);
        private Highlighter.Options.Options options;
        private char[] firstChars;
        private List<HighlightTag> tags;
        private Performance performance = Performance.Normal;

        public Adorner(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            Helper.InitDefaults();

            options = Highlighter.Options.Options.Instance;

            if (options.ColorTags == null || !options.ColorTags.Any())
            {
                options.ColorTags = Helper.GetFillerTags().ToArray();
                options.Save();
            }

            // 订阅实例事件
            options.Saved += HighlighterOptions_Saved;

            RefreshCriteria();

            layer = view.GetAdornmentLayer("Highlighter");

            this.view = view;
            this.view.LayoutChanged += OnLayoutChanged;
            this.view.ViewportWidthChanged += OnViewportWidthChanged;
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

        private void HighlighterOptions_Saved(object sender, Highlighter.Options.Options e)
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
            performance = options.Performance;

            // Recreate Tags list
            tags = options.ColorTags.Where(x => x.IsActive).ToList();

            // Add Solution-scoped Tags, if any
            if (options.SolutionTags != null && options.SolutionTags.Any())
            {
                tags.AddRange(options.SolutionTags.Where(x => x.IsActive));
            }

            List<char> chars = new List<char>();

            chars.AddRange(tags.Select(y => y.Criteria[0]));
            chars.AddRange(tags.Where(x => !x.IsCaseSensitive).Select(y => y.Criteria.ToUpper()[0]));
            firstChars = chars.ToArray();
            //firstChars = options.ColorTags.Select(k => (k.IsCaseSensitive ? k.Criteria[0] : char.ToUpperInvariant(k.Criteria[0]), k.IsCaseSensitive)).Distinct().ToArray();
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
            //# Grab a reference to the lines in the current TextView
            IWpfTextViewLineCollection textViewLines = view.TextViewLines;
            int start = line.Start;
            int end = line.End;
            List<Geometry> geometries = new();

            //## Main Loop
            for (int i = start; i < end; i++)
            {
                if (firstChars.Contains(view.TextSnapshot[i]))
                {
                    foreach (var tag in tags)
                    {
                        string keyword = tag.Criteria.Trim();
                        if (FirstCharacterEquals(view.TextSnapshot[i], keyword[0], tag.IsCaseSensitive) &&
                            i <= end - keyword.Length &&
                            CompareWords(view.TextSnapshot.GetText(i, keyword.Length), keyword, tag.IsCaseSensitive)
                            && CheckWholeWordsMatch(view.TextSnapshot, i, keyword, tag.AllowPartialMatch))
                        {
                            SnapshotSpan span;
                            if (tag.IsLine() || tag.IsLineUnder())
                            {
                                // 对于Line和LineUnder类型，创建从标签开始到行尾的span
                                span = new(view.TextSnapshot, Span.FromBounds(i, line.End));
                            }
                            else
                            {
                                // 对于其他类型，只创建标签本身的span
                                span = new(view.TextSnapshot, Span.FromBounds(i, i + keyword.Length));
                            }

                            Geometry markerGeometry = textViewLines.GetMarkerGeometry(span, false,
                                tag.Blur == BlurIntensity.None ? tNone : tBlur);

                            if (markerGeometry != null)
                            {
                                if (!geometries.Any(g => g.FillContainsWithDetail(markerGeometry) >
                                                         IntersectionDetail.Empty))
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
                return Helper.escapes.Contains(Convert.ToChar(textSnapshot.GetText(Math.Max(0, i - 1), 1))) &&
                       Helper.escapes.Contains(Convert.ToChar(textSnapshot.GetText(i + keyword.Length, 1)));
            }
        }

        private double CalculateDynamicWidth(Geometry markerGeometry, HighlightTag tag)
        {
            double baseWidth = markerGeometry.Bounds.Width;
            
            if (baseWidth < 0)
                baseWidth = 0;
            
            if (tag.IsTag() || tag.IsTagUnder())
            {
                return baseWidth + 3;
            }
            else if (tag.IsLine() || tag.IsLineUnder())
            {
                // 为Line和LineUnder类型添加一个小的宽度调整，确保覆盖到行尾
                return baseWidth + 5;
            }
            else
            {
                return baseWidth + 3;
            }
        }

        private void AddMarker(SnapshotSpan span, Geometry markerGeometry, HighlightTag tag)
        {
            double width = CalculateDynamicWidth(markerGeometry, tag);
            double height = markerGeometry.Bounds.Height;

            Rectangle r = new()
            {
                Fill = new SolidColorBrush(tag.Color.ChangeAlpha(60)),
                RadiusX = cornerRadius,
                RadiusY = cornerRadius,
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(tag.Color.ChangeAlpha(255))
            };

            bool isLine = tag.IsLine();
            bool isTag = tag.IsTag();
            if (tag.IsTagUnder())
            {
                r.Height = 1.5;
            }
            else if (tag.IsLineUnder())
            {
                r.Height = 1.5;
            }
            else if (tag.IsLine())
            {
                if (performance != Performance.NoEffects && tag.Blur != BlurIntensity.None)
                    r.Height = markerGeometry.Bounds.Height + 5.0;
            }
            else if (tag.IsTag())
            {
                if (performance != Performance.NoEffects && tag.Blur != BlurIntensity.None)
                    r.Height = markerGeometry.Bounds.Height + 5.0;
            }

            /* 无效果或者是模糊为空，则不会添加模糊效果*/
            if (performance != Performance.NoEffects && tag.Blur != BlurIntensity.None)
            {
                r.Effect = new BlurEffect
                {
                    KernelType = performance == Performance.Normal ? KernelType.Gaussian : KernelType.Box,
                    RenderingBias = RenderingBias.Performance
                };
                //!? #INFO
                //##
                //###
                //####
                //#####
                //######
                //#STUB ABCDEFGHIJKLMNOPQRSTUVWXYZ
                //#INFO
                //#ABC
                //2
                //3
                //4
                //5
                //6

                switch (tag.Blur)
                {
                    case BlurIntensity.Low:
                        //((SolidColorBrush)r.Fill).Color.ChangeAlpha(80);
                        ((BlurEffect)r.Effect).Radius = isLine ? 1 : 1.0;
                        break;

                    case BlurIntensity.Medium:
                        //((SolidColorBrush)r.Fill).Color.ChangeAlpha(120);
                        ((BlurEffect)r.Effect).Radius = isLine ? 2 : 2.0;
                        break;

                    case BlurIntensity.High:
                        //((SolidColorBrush)r.Fill).Color.ChangeAlpha(170);
                        ((BlurEffect)r.Effect).Radius = isLine ? 4 : 4.0;
                        break;

                    case BlurIntensity.Ultra:
                        //((SolidColorBrush)r.Fill).Color.ChangeAlpha(255);
                        ((BlurEffect)r.Effect).Radius = isLine ? 6 : 10.0;
                        break;
                }

                r.Stroke = null;

                if (r.Effect.CanFreeze)
                    r.Effect.Freeze();
            }

            //修改Fill的透明度
            r.Fill = new SolidColorBrush(((SolidColorBrush)r.Fill).Color.ChangeAlpha((byte)((255 * (byte)tag.Alpha) / 10)));

            // Align the image with the top of the bounds of the text geometry
            if (tag.IsTagUnder())
                Canvas.SetLeft(r, markerGeometry.Bounds.Left);
            else if (tag.IsLineUnder())
                Canvas.SetLeft(r, markerGeometry.Bounds.Left);
            else
            {
                if (performance != Performance.NoEffects && tag.Blur != BlurIntensity.None)
                    Canvas.SetLeft(r, markerGeometry.Bounds.Left);
                else
                    Canvas.SetLeft(r, markerGeometry.Bounds.Left - 1.5);
            }

            if (r.Fill.CanFreeze)
                r.Fill.Freeze();

            if (r.Stroke is { CanFreeze: true })
                r.Stroke.Freeze();

            if (tag.IsTagUnder())
            {
                Canvas.SetTop(r, markerGeometry.Bounds.Top + markerGeometry.Bounds.Height - 2);
            }
            else if (tag.IsLineUnder())
            {
                Canvas.SetTop(r, markerGeometry.Bounds.Top + markerGeometry.Bounds.Height - 2);
            }
            else
            {
                if (performance != Performance.NoEffects && tag.Blur != BlurIntensity.None)
                    Canvas.SetTop(r, markerGeometry.Bounds.Top - 1.5);
                else
                    Canvas.SetTop(r, markerGeometry.Bounds.Top);
            }

            layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, r, null);
        }
    }
}