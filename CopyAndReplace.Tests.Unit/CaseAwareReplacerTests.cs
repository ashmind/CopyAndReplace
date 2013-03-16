using System;
using System.Collections.Generic;
using System.Linq;
using CopyAndReplace.Implementation;
using Xunit;
using Xunit.Extensions;

namespace CopyAndReplace.Tests.Unit {
    public class CaseAwareReplacerTests {
        [Theory]
        [InlineData("OldClass", "NewClass", "OldClass", "NewClass")]
        [InlineData("OldClass", "NewClass", "oldClass", "newClass")]
        [InlineData("OldClass", "NewClass", "oldclass", "newclass")]
        [InlineData("OldClass", "NewClass", "OLDCLASS", "NEWCLASS")]
        public void Replace_WorksAsExpected_WhenUsingCaseMatch(string pattern, string replacement, string source, string expected) {
            var replacer = new StringCaseAwareReplacer(pattern, replacement);
            var result = replacer.ReplaceAllIn(source);

            Assert.Equal(expected, result);
        }
    }
}
