// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zumo.Server.Test
{
    [TestClass]
    public class TableControllerOptions_Tests
    {
        [TestMethod]
        public void CTOR_DefaultDataView()
        {
            var actual = new TableControllerOptions<Movie>();
            Assert.IsNotNull(actual.DataView);
        }

        [TestMethod]
        public void CTOR_SoftDelete_DisabledByDefault()
        {
            var actual = new TableControllerOptions<Movie>();
            Assert.IsFalse(actual.SoftDeleteEnabled);
        }

        [TestMethod]
        public void CTOR_ODataSettings_NonZeroByDefault()
        {
            var actual = new TableControllerOptions<Movie>();
            Assert.IsTrue(actual.MaxTop > 1);
            Assert.IsTrue(actual.PageSize > 1);
        }

        [TestMethod]
        public void DataView_CanRoundtrip()
        {
            var actual = new TableControllerOptions<Movie>();
            static bool t(Movie movie) => true;
            actual.DataView = t;
            Assert.AreEqual(t, actual.DataView);
        }

        [TestMethod]
        public void ODataSettings_CanRoundtrip()
        {
            var actual = new TableControllerOptions<Movie>
            {
                MaxTop = 10,
                PageSize = 20
            };
            Assert.AreEqual(10, actual.MaxTop);
            Assert.AreEqual(20, actual.PageSize);
        }
    }
}
