// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;

using Xunit;

namespace MobileClient.Tests
{
    public class MobileServiceFeatures_Tests
    {
        [Fact]
        public void Validate_TT_FeatureCode()
            => Assert.Equal("TT", EnumValueAttribute.GetValue(MobileServiceFeatures.TypedTable));

        [Fact]
        public void Validate_TU_FeatureCode()
            => Assert.Equal("TU", EnumValueAttribute.GetValue(MobileServiceFeatures.UntypedTable));

        [Fact]
        public void Validate_AT_FeatureCode()
            => Assert.Equal("AT", EnumValueAttribute.GetValue(MobileServiceFeatures.TypedApiCall));

        [Fact]
        public void Validate_AJ_FeatureCode()
            => Assert.Equal("AJ", EnumValueAttribute.GetValue(MobileServiceFeatures.JsonApiCall));

        [Fact]
        public void Validate_AG_FeatureCode()
            => Assert.Equal("AG", EnumValueAttribute.GetValue(MobileServiceFeatures.GenericApiCall));

        [Fact]
        public void Validate_TC_FeatureCode()
            => Assert.Equal("TC", EnumValueAttribute.GetValue(MobileServiceFeatures.TableCollection));

        [Fact]
        public void Validate_OL_FeatureCode()
            => Assert.Equal("OL", EnumValueAttribute.GetValue(MobileServiceFeatures.Offline));

        [Fact]
        public void Validate_QS_FeatureCode()
            => Assert.Equal("QS", EnumValueAttribute.GetValue(MobileServiceFeatures.AdditionalQueryParameters));

        [Fact]
        public void Validate_RT_FeatureCode()
            => Assert.Equal("RT", EnumValueAttribute.GetValue(MobileServiceFeatures.RefreshToken));

        [Fact]
        public void Validate_FeatureCode_None()
            => Assert.Null(EnumValueAttribute.GetValue(MobileServiceFeatures.None));
    }
}
