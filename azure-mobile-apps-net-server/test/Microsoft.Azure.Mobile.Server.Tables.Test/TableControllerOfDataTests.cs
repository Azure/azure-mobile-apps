// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server.Tables;
using Microsoft.Azure.Mobile.Server.TestModels;
using Moq;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class TableControllerOfDataTests
    {
        private const string Id = "1234";

        private Delta<TestEntity> patch;
        private TestEntity data;
        private HttpResponseMessage response;
        private Exception exception;
        private HttpResponseException responseException;
        private Mock<DomainManager<TestEntity>> domainManagerMock;
        private IDomainManager<TestEntity> domainManager;
        private TableControllerMock controller;

        public TableControllerOfDataTests()
        {
            this.patch = new Delta<TestEntity>();
            this.data = new TestEntity { BooleanValue = true, Id = Id, IntValue = 42 };
            this.exception = new Exception("Catch this!");
            this.response = new HttpResponseMessage(HttpStatusCode.Conflict);
            this.responseException = new HttpResponseException(this.response);
            this.domainManagerMock = new Mock<DomainManager<TestEntity>>(new HttpRequestMessage(), false);
            this.domainManager = this.domainManagerMock.Object;
            this.controller = new TableControllerMock(this.domainManager);
        }

        [Fact]
        public void DomainManager_ThrowsIfNotSet()
        {
            // Arrange
            TableControllerMock ctrl = new TableControllerMock();
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => ctrl.DomainManager);

            // Assert
            Assert.Equal("The 'DomainManager' property has not been initialized. Please initialize the property with a 'IDomainManager' instance.", ex.Message);
        }

        [Fact]
        public void DomainManager_Roundtrips()
        {
            Mock<IDomainManager<TestEntity>> roundtripMock = new Mock<IDomainManager<TestEntity>>();
            PropertyAssert.Roundtrips(this.controller, c => c.DomainManager, PropertySetter.NullThrows, defaultValue: this.domainManager, roundtripValue: roundtripMock.Object);
        }

        [Fact]
        public void DomainManager_SoftDeleteIsEnabled_IfParameterIsTrue()
        {
            // Arrange
            var controller1 = new TableControllerMock<TestEntity>();
            var domainManager1 = new Mock<DomainManager<TestEntity>>(new HttpRequestMessage(), true);
            controller1.DomainManager = domainManager1.Object;

            // Assert
            Assert.Equal(true, domainManager1.Object.EnableSoftDelete);
            Assert.Equal(false, domainManager1.Object.IncludeDeleted);
        }

        [Fact]
        public void DomainManager_SoftDeleteIsDisabled_IfParameterIsFalse()
        {
            // Arrange
            var controller1 = new TableControllerMock<TestEntity>();
            var domainManager1 = new Mock<DomainManager<TestEntity>>(new HttpRequestMessage(), false);
            controller1.DomainManager = domainManager1.Object;

            // Assert
            Assert.Equal(false, domainManager1.Object.EnableSoftDelete);
            Assert.Equal(true, domainManager1.Object.IncludeDeleted);
        }

        [Fact]
        public void ReadAll_CallsDomainManager()
        {
            this.controller.Query();
            this.domainManagerMock.Verify(d => d.Query(), Times.Once());
        }

        [Fact]
        public void ReadAll_Throws_IfDomainManagerThrows_ResponseException()
        {
            // Arrange
            this.domainManagerMock.Setup(d => d.Query())
                .Throws(this.responseException)
                .Verifiable();

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => this.controller.Query());

            // Assert
            Assert.Equal(this.responseException, ex);
        }

        [Fact]
        public void ReadAll_Throws_IfDomainManagerThrows_Exception()
        {
            // Arrange
            this.domainManagerMock.Setup(d => d.Query())
                .Throws(this.exception)
                .Verifiable();

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => this.controller.Query());
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("Catch this!", error.ExceptionMessage);
        }

        [Fact]
        public void Lookup_CallsDomainManager()
        {
            this.controller.Lookup(Id);
            this.domainManagerMock.Verify(d => d.Lookup(Id), Times.Once());
        }

        [Fact]
        public void Lookup_Throws_IfDomainManagerThrowsResponseException()
        {
            // Arrange
            this.domainManagerMock.Setup(d => d.Lookup(Id))
                .Throws(this.responseException)
                .Verifiable();

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => this.controller.Lookup(Id));

            // Assert
            Assert.Equal(this.responseException, ex);
        }

        [Fact]
        public void Lookup_Throws_IfDomainManagerThrowsException()
        {
            // Arrange
            this.domainManagerMock.Setup(d => d.Lookup(Id))
                .Throws(this.exception)
                .Verifiable();

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => this.controller.Lookup(Id));
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("Catch this!", error.ExceptionMessage);
        }

        [Fact]
        public async Task Insert_Throws_IfModelStateError()
        {
            // Arrange
            this.controller.ModelState.AddModelError("error", this.exception);

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.InsertAsync(this.data));
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("The request is invalid.", error.Message);
            Assert.NotNull(error.ModelState);
        }

        [Fact]
        public async Task Insert_CallsDomainManager()
        {
            await this.controller.InsertAsync(this.data);
            this.domainManagerMock.Verify(d => d.InsertAsync(this.data), Times.Once());
        }

        [Fact]
        public async Task Insert_Throws_IfDomainManagerThrowsResponseException()
        {
            // Arrange
            this.domainManagerMock.Setup(d => d.InsertAsync(this.data))
                .Throws(this.responseException)
                .Verifiable();

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.InsertAsync(this.data));

            // Assert
            Assert.Equal(this.responseException, ex);
        }

        [Fact]
        public async Task Insert_Throws_IfDomainManagerThrowsException()
        {
            // Arrange
            this.domainManagerMock.Setup(d => d.InsertAsync(this.data))
                .Throws(this.exception)
                .Verifiable();

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.InsertAsync(this.data));
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("Catch this!", error.ExceptionMessage);
        }

        [Fact]
        public async Task Update_Throws_IfModelStateError()
        {
            // Arrange
            this.controller.ModelState.AddModelError("error", this.exception);

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.UpdateAsync(Id, this.patch));
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("The request is invalid.", error.Message);
            Assert.NotNull(error.ModelState);
        }

        [Fact]
        public async Task Update_CallsDomainManager()
        {
            await this.controller.UpdateAsync(Id, this.patch);
            this.domainManagerMock.Verify(d => d.UpdateAsync(Id, this.patch), Times.Once());
        }

        [Fact]
        public async Task Update_CallsDomainManagerWithNonWildcardETag()
        {
            // Arrange
            string order = string.Empty;
            this.controller.Request.Headers.IfMatch.Add(EntityTagHeaderValue.Parse("\"SGVsbG8=\""));
            this.domainManagerMock.Setup(d => d.UpdateAsync(Id, this.patch))
                .Callback<string, Delta<TestEntity>>((id, patch) =>
                {
                    object versionObj;
                    patch.TryGetPropertyValue("Version", out versionObj);
                    Assert.IsType<byte[]>(versionObj);
                    byte[] version = (byte[])versionObj;
                    Assert.Equal(Encoding.UTF8.GetBytes("Hello"), version);
                    order += "1";
                })
                .ReturnsAsync(() => null);

            // Act
            await this.controller.UpdateAsync(Id, this.patch);

            // Assert
            Assert.Equal("1", order);
        }

        [Fact]
        public async Task Update_Throws_IfDomainManagerThrowsResponseException()
        {
            // Arrange
            this.domainManagerMock.Setup(d => d.UpdateAsync(Id, this.patch))
                .Throws(this.responseException)
                .Verifiable();

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.UpdateAsync(Id, this.patch));

            // Assert
            Assert.Equal(this.responseException, ex);
        }

        [Fact]
        public async Task Update_Throws_IfDomainManagerThrowsException()
        {
            // Arrange
            this.domainManagerMock.Setup(d => d.UpdateAsync(Id, this.patch))
                .Throws(this.exception)
                .Verifiable();

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.UpdateAsync(Id, this.patch));
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("Catch this!", error.ExceptionMessage);
        }

        [Fact]
        public async Task Replace_Throws_IfModelStateError()
        {
            // Arrange
            this.controller.ModelState.AddModelError("error", this.exception);

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.ReplaceAsync(Id, this.data));
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("The request is invalid.", error.Message);
            Assert.NotNull(error.ModelState);
        }

        [Fact]
        public async Task Replace_CallsDomainManager()
        {
            await this.controller.ReplaceAsync(Id, this.data);
            this.domainManagerMock.Verify(d => d.ReplaceAsync(Id, this.data), Times.Once());
        }

        [Fact]
        public async Task Replace_CallsDomainManagerWithNonWildcardETag()
        {
            // Arrange
            string order = string.Empty;
            this.controller.Request.Headers.IfMatch.Add(EntityTagHeaderValue.Parse("\"SGVsbG8=\""));
            this.domainManagerMock.Setup(d => d.ReplaceAsync(Id, this.data))
                .Callback<string, TestEntity>((id, data) =>
                {
                    Assert.Equal(Encoding.UTF8.GetBytes("Hello"), data.Version);
                    order += "1";
                })
                .ReturnsAsync(() => null);

            // Act
            await this.controller.ReplaceAsync(Id, this.data);

            // Assert
            Assert.Equal("1", order);
        }

        [Fact]
        public async Task Replace_Throws_IfDomainManagerThrowsResponseException()
        {
            // Arrange
            this.domainManagerMock.Setup(d => d.ReplaceAsync(Id, this.data))
                .Throws(this.responseException)
                .Verifiable();

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.ReplaceAsync(Id, this.data));

            // Assert
            Assert.Equal(this.responseException, ex);
        }

        [Fact]
        public async Task Replace_Throws_IfDomainManagerThrowsException()
        {
            // Arrange
            this.domainManagerMock.Setup(d => d.ReplaceAsync(Id, this.data))
                .Throws(this.exception)
                .Verifiable();

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.ReplaceAsync(Id, this.data));
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("Catch this!", error.ExceptionMessage);
        }

        [Fact]
        public async Task Delete_CallsDomainManager()
        {
            this.domainManagerMock.Setup<Task<bool>>(d => d.DeleteAsync(Id))
                .ReturnsAsync(true)
                .Verifiable();

            await this.controller.DeleteAsync(Id);
            this.domainManagerMock.Verify();
        }

        [Fact]
        public async Task Delete_Throws_IfDomainManagerThrowsResponseException()
        {
            // Arrange
            this.domainManagerMock.Setup<Task>(d => d.DeleteAsync(Id))
                .Throws(this.responseException)
                .Verifiable();

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.DeleteAsync(Id));

            // Assert
            Assert.Equal(this.responseException, ex);
        }

        [Fact]
        public async Task Delete_Throws_IfDomainManagerThrowsException()
        {
            // Arrange
            this.domainManagerMock.Setup<Task>(d => d.DeleteAsync(Id))
                .Throws(this.exception)
                .Verifiable();

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.controller.DeleteAsync(Id));
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("Catch this!", error.ExceptionMessage);
        }

        public class TableControllerMock<TData> : TableController<TData>
            where TData : class, ITableData
        {
            private HttpConfiguration config = new HttpConfiguration()
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };

            public TableControllerMock()
            {
                this.RequestContext = new HttpRequestContext() { IncludeErrorDetail = true };
                this.Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
                this.Request.SetRequestContext(this.RequestContext);
                this.Configuration = this.config;
            }

            public TableControllerMock(IDomainManager<TData> domainManager)
                : base(domainManager)
            {
                this.RequestContext = new HttpRequestContext() { IncludeErrorDetail = true };
                this.Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
                this.Request.SetRequestContext(this.RequestContext);
                this.Configuration = this.config;
            }

            public new IDomainManager<TData> DomainManager
            {
                get
                {
                    return base.DomainManager;
                }

                set
                {
                    base.DomainManager = value;
                }
            }

            public new IQueryable<TData> Query()
            {
                return base.Query();
            }

            public new SingleResult<TData> Lookup(string id)
            {
                return base.Lookup(id);
            }

            public new Task<TData> InsertAsync(TData item)
            {
                return base.InsertAsync(item);
            }

            public new Task<TData> UpdateAsync(string id, Delta<TData> patch)
            {
                return base.UpdateAsync(id, patch);
            }

            public new Task<TData> ReplaceAsync(string id, TData item)
            {
                return base.ReplaceAsync(id, item);
            }

            public new Task DeleteAsync(string id)
            {
                return base.DeleteAsync(id);
            }
        }

        public class TableControllerMock : TableControllerMock<TestEntity>
        {
            public TableControllerMock()
                : base()
            {
            }

            public TableControllerMock(IDomainManager<TestEntity> domainManager)
                : base(domainManager)
            {
            }
        }
    }
}