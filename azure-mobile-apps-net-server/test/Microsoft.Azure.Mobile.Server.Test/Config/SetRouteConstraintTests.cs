// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using System.Web.Http.Routing;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public class SetRouteConstraintTests
    {
        private static Dictionary<string, object> includedValues = new Dictionary<string, object> { { "k", "a" } };
        private static Dictionary<string, object> excludedValues = new Dictionary<string, object> { { "k", "z" } };

        private HashSet<string> testSet = new HashSet<string> { "a", "b", "c" };

        public static TheoryDataCollection<string, IDictionary<string, object>, bool, bool> MatchData
        {
            get
            {
                return new TheoryDataCollection<string, IDictionary<string, object>, bool, bool>
                {
                    { "k", includedValues, false, true },
                    { "k", excludedValues, false, false },
                    { "k", includedValues, true, false },
                    { "k", excludedValues, true, true },
                    { "w", includedValues, false, false },
                    { "w", excludedValues, false, false },
                    { "w", includedValues, true, false },
                    { "w", excludedValues, true, false },
                };
            }
        }

        [Theory]
        [MemberData("BoolDataSet", MemberType = typeof(TestDataSets))]
        public void SetRouteConstraint_InitializesParameters(bool excluded)
        {
            var constraint = new SetRouteConstraint<string>(this.testSet, excluded);
            Assert.Equal(excluded, constraint.Excluded);
            Assert.Same(this.testSet, constraint.Set);
        }

        [Theory]
        [MemberData("MatchData")]
        public void Match_FindsMatches(string parameterName, IDictionary<string, object> values, bool excluded, bool match)
        {
            // Arrange
            var constraint = new SetRouteConstraint<string>(this.testSet, matchOnExcluded: excluded);

            // Act
            var result = constraint.Match(request: null, route: null, parameterName: parameterName, values: values, routeDirection: HttpRouteDirection.UriResolution);

            // Assert
            Assert.Equal(match, result);
        }
    }
}