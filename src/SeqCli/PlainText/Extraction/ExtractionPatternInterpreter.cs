﻿using System;
using System.Collections.Generic;
using System.Linq;
using SeqCli.PlainText.Patterns;

namespace SeqCli.PlainText.Extraction
{
    static class ExtractionPatternInterpreter
    {
        public static NameValueExtractor MultilineMessageExtractor { get; } = new NameValueExtractor(new[]
        {
            new SimplePatternElement(Matchers.MultiLineMessage, ReifiedProperties.Message)
        });

        static PatternElement[] CreatePatternElements(ExtractionPattern pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            var patternElements = new PatternElement[pattern.Elements.Count];
            for (var i = pattern.Elements.Count - 1; i >= 0; --i)
            {
                var element = pattern.Elements[i];
                switch (element)
                {
                    case LiteralTextPatternExpression text:
                        patternElements[i] = new SimplePatternElement(Matchers.LiteralText(text.Text));
                        break;
                    case CapturePatternExpression capture
                    when capture.Content is NonGreedyContentExpression ngc:
                        patternElements[i] = new SimplePatternElement(
                            Matchers.NonGreedyContent(patternElements.Skip(i + 1).Take(ngc.Lookahead).ToArray()),
                            capture.Name);
                        break;
                    case CapturePatternExpression capture
                    when capture.Content is MatchTypeContentExpression mtc:
                        patternElements[i] = new SimplePatternElement(
                            mtc.Type == null ? Matchers.Token : Matchers.GetByType(mtc.Type),
                            capture.Name);
                        break;
                    case CapturePatternExpression capture
                    when capture.Content is GroupedContentExpression gc:
                        patternElements[i] = new GroupedPatternElement(
                            CreatePatternElements(gc.ExtractionPattern),
                            capture.Name);
                        break;
                    default:
                        throw new InvalidOperationException($"Element `{element}` not recognized.");
                }
            }

            return patternElements;
        }

        public static NameValueExtractor CreateNameValueExtractor(ExtractionPattern pattern)
        {
            var patternElements = CreatePatternElements(pattern);
            return new NameValueExtractor(patternElements);
        }
    }
}