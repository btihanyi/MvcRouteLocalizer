using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides automatic conversions into lowercase, hyphen-separated (kebab case) names.
    /// </summary>
    public class HyphenatedRouteDictionaryProvider : MonolingualRouteDictionaryProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HyphenatedRouteDictionaryProvider"/> class.
        /// </summary>
        /// <param name="ignoreUrlNameAttribute">If <see langword="true"/>, the <see cref="UrlNameAttribute"/>
        /// overwrites will be ignored. Sets the value of <see cref="IgnoreUrlNameAttribute"/>.</param>
        public HyphenatedRouteDictionaryProvider(bool ignoreUrlNameAttribute = false)
        {
            this.IgnoreUrlNameAttribute = ignoreUrlNameAttribute;
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="UrlNameAttribute"/>s should be ignored upon generating naming maps.
        /// </summary>
        public bool IgnoreUrlNameAttribute { get; }

        /// <summary>
        /// Gets the lowercase, hyphen-separated name of the <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller">The controller to be localized.</param>
        /// <returns>The lowercase, hyphen-separated name of the <paramref name="controller"/>,
        /// or <see cref="UrlNameAttribute.Name"/>'s value, if present, and <see cref="IgnoreUrlNameAttribute"/>
        /// is set to <see langword="false"/>.</returns>
        protected override string GetLocalizationForController(ControllerInfo controller)
        {
            if (controller.UrlName != null && !IgnoreUrlNameAttribute)
            {
                return controller.UrlName;
            }
            else
            {
                return PascalToKebabCase(controller.Name);
            }
        }

        /// <summary>
        /// Gets the lowercase, hyphen-separated name of the <paramref name="action"/>.
        /// </summary>
        /// <param name="controller">The controller of the <paramref name="action"/>.</param>
        /// <param name="action">The action to be localized.</param>
        /// <returns>The lowercase, hyphen-separated name of the <paramref name="action"/>,
        /// or <see cref="UrlNameAttribute.Name"/>'s value, if present, and <see cref="IgnoreUrlNameAttribute"/>
        /// is set to <see langword="false"/>.</returns>
        protected override string GetLocalizationForAction(ControllerInfo controller, ActionInfo action)
        {
            if (action.UrlName != null && !IgnoreUrlNameAttribute)
            {
                return action.UrlName;
            }
            else
            {
                return PascalToKebabCase(action.Name);
            }
        }

        private static string PascalToKebabCase(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            int upperCaseCount = text.Skip(1).Count(c => char.IsUpper(c));
            var buffer = new char[text.Length + upperCaseCount];

            int index = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        buffer[index++] = '-';
                    }
                    buffer[index++] = char.ToLowerInvariant(c);
                }
                else
                {
                    buffer[index++] = c;
                }
            }

            return new string(buffer);
        }

        [ExcludeFromCodeCoverage]
        private static string KebabToPascalCase(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            int hyphenCount = text.Count(c => c == '-');
            var builder = new StringBuilder(text.Length - hyphenCount);
            int startIndex = 0;
            int hyphenIndex;

            while ((hyphenIndex = text.IndexOf('-', startIndex)) >= 0)
            {
                if (hyphenIndex > startIndex)
                {
                    AppendSegmentToBuilder(startIndex, hyphenIndex);
                }
                startIndex = hyphenIndex + 1;
            }

            if (startIndex < text.Length && hyphenIndex < 0)
            {
                AppendSegmentToBuilder(startIndex, text.Length);
            }

            return builder.ToString();

            void AppendSegmentToBuilder(int segmentStart, int segmentEnd)
            {
                int builderLength = builder.Length;
                builder.Append(text, segmentStart, segmentEnd - segmentStart);
                builder[builderLength] = char.ToUpperInvariant(builder[builderLength]);
            }
        }
    }
}
