using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AshMind.Extensions;

namespace CopyAndReplace.Implementation {
    // not thread safe
    public class StringCaseAwareReplacer {
        public event EventHandler<ReplacementUsedEventArgs> ReplacementUsed = delegate { };

        private readonly IDictionary<string, string> map = new Dictionary<string, string>();
        private readonly Regex patternRegex;
        private readonly string replacement;

        public StringCaseAwareReplacer(string pattern, string replacement) {
            this.patternRegex = new Regex(Regex.Escape(pattern), RegexOptions.IgnoreCase);
            this.replacement = replacement;

            this.map.Add(pattern, replacement);
        }

        public string ReplaceAllIn(string source) {
            return this.patternRegex.Replace(source, Replace);
        }

        private string Replace(Match match) {
            var cached = this.map.GetValueOrDefault(match.Value);
            if (cached != null) {
                this.ReplacementUsed(this, new ReplacementUsedEventArgs(match.Value, cached));
                return cached;
            }

            var newReplacement = this.GetNewReplacement(match);
            this.map.Add(match.Value, newReplacement);

            if (newReplacement != match.Value) // == is a special case -- no actual replacement found
                this.ReplacementUsed(this, new ReplacementUsedEventArgs(match.Value, newReplacement));

            return newReplacement;
        }

        private string GetNewReplacement(Match match) {
            var matchCase = this.GetCase(match.Value);
            if (matchCase == StringCase.Unknown)
                return match.Value;

            return this.ConvertToCase(this.replacement, matchCase);
        }

        private StringCase GetCase(string value) {
            var possibleCases = new HashSet<StringCase> {
                StringCase.AllLower,
                StringCase.AllUpper,
                StringCase.Camel,
                StringCase.Pascal
            };

            var index = -1;
            var previousWasUpper = false;
            var hadAnyUpper = false;
            foreach (var @char in value) {
                index += 1;

                if (@char.IsUpper()) {
                    possibleCases.Remove(StringCase.AllLower);
                    if (index == 0)
                        possibleCases.Remove(StringCase.Camel);

                    if (previousWasUpper)
                        possibleCases.Remove(StringCase.Pascal);

                    previousWasUpper = true;
                    hadAnyUpper = true;
                    continue;
                }

                if (@char.IsLower()) {
                    possibleCases.Remove(StringCase.AllUpper);
                    if (index == 0)
                        possibleCases.Remove(StringCase.Pascal);
                }

                previousWasUpper = false;
            }

            if (!hadAnyUpper)
                possibleCases.Remove(StringCase.Camel);

            if (possibleCases.Count != 1)
                return StringCase.Unknown;

            return possibleCases.Single();
        }

        private string ConvertToCase(string value, StringCase @case) {
            switch (@case) {
                case StringCase.AllUpper: return value.ToUpper();
                case StringCase.AllLower: return value.ToLower();
                case StringCase.Pascal:   return value[0].ToUpper() + value.Substring(1);
                case StringCase.Camel:    return value[0].ToLower() + value.Substring(1);
                default: throw new NotSupportedException();
            }
        }
    }
}
