using Azure.Mobile.Server.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Mobile.Server.Test
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
            Func<Movie, bool> t = movie => true;
            actual.DataView = t;
            Assert.AreEqual(t, actual.DataView);
        }

        [TestMethod]
        public void ODataSettings_CanRoundtrip()
        {
            var actual = new TableControllerOptions<Movie>();
            actual.MaxTop = 10;
            actual.PageSize = 20;
            Assert.AreEqual(10, actual.MaxTop);
            Assert.AreEqual(20, actual.PageSize);
        }
    }
}
