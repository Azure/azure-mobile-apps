// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Query;
using MobileClient.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace MobileClient.Tests.Table
{
    public class ZumoQuery_Test
    {
        private MobileServiceTableQueryDescription Compile<T, U>(Func<IMobileServiceTable<T>, IMobileServiceTableQuery<U>> getQuery)
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            IMobileServiceTable<T> table = service.GetTable<T>();
            IMobileServiceTableQuery<U> query = getQuery(table);
            MobileServiceTableQueryProvider provider = new MobileServiceTableQueryProvider();
            MobileServiceTableQueryDescription compiledQuery = provider.Compile((MobileServiceTableQuery<U>)query);
            return compiledQuery;
        }

        [Fact]
        public void BasicQuery()
        {
            // Query syntax
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                select p);
            Assert.Equal("Product", query.TableName);
            Assert.Null(query.Filter);
            Assert.Equal(2, query.Selection.Count);
            Assert.Equal(0, query.Ordering.Count);
        }

        [Fact]
        public void Ordering()
        {
            // Query syntax
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                orderby p.Price ascending
                select p);
            Assert.Equal(1, query.Ordering.Count);
            Assert.Equal("Price", ((MemberAccessNode)query.Ordering[0].Expression).MemberName);
            Assert.Equal(OrderByDirection.Ascending, query.Ordering[0].Direction);

            // Chaining
            query = Compile<Product, Product>(table => table.OrderBy(p => p.Price));
            Assert.Equal(1, query.Ordering.Count);
            Assert.Equal("Price", ((MemberAccessNode)query.Ordering[0].Expression).MemberName);
            Assert.Equal(OrderByDirection.Ascending, query.Ordering[0].Direction);

            // Query syntax descending
            query = Compile<Product, Product>(table =>
                from p in table
                orderby p.Price descending
                select p);
            Assert.Equal(1, query.Ordering.Count);
            Assert.Equal("Price", ((MemberAccessNode)query.Ordering[0].Expression).MemberName);
            Assert.Equal(OrderByDirection.Descending, query.Ordering[0].Direction);

            // Chaining descending
            query = Compile<Product, Product>(table => table.OrderByDescending(p => p.Price));
            Assert.Equal(1, query.Ordering.Count);
            Assert.Equal("Price", ((MemberAccessNode)query.Ordering[0].Expression).MemberName);
            Assert.Equal(OrderByDirection.Descending, query.Ordering[0].Direction);

            // Query syntax with multiple
            query = Compile<Product, Product>(table =>
                from p in table
                orderby p.Price ascending, p.Name descending
                select p);
            Assert.Equal(2, query.Ordering.Count);
            Assert.Equal("Price", ((MemberAccessNode)query.Ordering[0].Expression).MemberName);
            Assert.Equal(OrderByDirection.Ascending, query.Ordering[0].Direction);
            Assert.Equal("Name", ((MemberAccessNode)query.Ordering[1].Expression).MemberName);
            Assert.Equal(OrderByDirection.Descending, query.Ordering[1].Direction);

            // Chaining with multiple
            query = Compile<Product, Product>(table =>
                table
                .OrderBy(p => p.Price)
                .ThenByDescending(p => p.Name));
            Assert.Equal(2, query.Ordering.Count);
            Assert.Equal("Price", ((MemberAccessNode)query.Ordering[0].Expression).MemberName);
            Assert.Equal(OrderByDirection.Ascending, query.Ordering[0].Direction);
            Assert.Equal("Name", ((MemberAccessNode)query.Ordering[1].Expression).MemberName);
            Assert.Equal(OrderByDirection.Descending, query.Ordering[1].Direction);
        }

        [Fact]
        public void Projection()
        {
            // Query syntax
            MobileServiceTableQueryDescription query = Compile<Product, string>(table =>
                from p in table
                select p.Name);
            Assert.Equal(3, query.Selection.Count);
            Assert.Equal("Name", query.Selection[0]);
            Assert.Equal("Weight", query.Selection[1]);
            Assert.Equal("WeightInKG", query.Selection[2]);
            Assert.Equal(typeof(Product), query.ProjectionArgumentType);
            Assert.Equal(
                "ZUMO",
                query.Projections.First().DynamicInvoke(
                    new Product { Name = "ZUMO", Price = 0, InStock = true }));

            // Chaining
            query = Compile<Product, string>(table => table.Select(p => p.Name));
            Assert.Equal(3, query.Selection.Count);
            Assert.Equal("Name", query.Selection[0]);
            Assert.Equal("Weight", query.Selection[1]);
            Assert.Equal("WeightInKG", query.Selection[2]);
            Assert.Equal(typeof(Product), query.ProjectionArgumentType);
            Assert.Equal(
                "ZUMO",
                query.Projections.First().DynamicInvoke(
                    new Product { Name = "ZUMO", Price = 0, InStock = true }));

            // Verify that we don't blow up by trying to include the Foo
            // property in the compiled query
            Compile((IMobileServiceTable<Product> table) =>
                from p in table
                select new { Foo = p.Name });
        }

        [Fact]
        public void MutlipleProjection()
        {
            // Chaining
            MobileServiceTableQueryDescription query = Compile<Product, string>(table =>
                table.Select(p => new { Foo = p.Name })
                     .Select(f => f.Foo.ToLower()));
            Assert.Equal(3, query.Selection.Count);
            Assert.Equal("Name", query.Selection[0]);
            Assert.Equal("Weight", query.Selection[1]);
            Assert.Equal("WeightInKG", query.Selection[2]);
            Assert.Equal(typeof(Product), query.ProjectionArgumentType);
            Assert.Equal(
                "zumo",
                query.Projections[1].DynamicInvoke(
                    query.Projections[0].DynamicInvoke(
                    new Product { Name = "ZUMO", Price = 0, InStock = true })));

            // Verify that we don't blow up by trying to include the Foo
            // property in the compiled query
            Compile((IMobileServiceTable<Product> table) =>
                table.Select(p => new { Foo = p.Name })
                     .Select(f => new { LowerFoo = f.Foo.ToLower() }));
        }

        [Fact]
        public void SkipTake()
        {
            // Query syntax
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Skip(2).Take(5));
            Assert.Equal(2, query.Skip);
            Assert.Equal(5, query.Top);

            // Chaining
            query = Compile<Product, Product>(table => table.Select(p => p).Skip(2).Take(5));
            Assert.Equal(2, query.Skip);
            Assert.Equal(5, query.Top);

            // Allow New operations
            query = Compile<Product, Product>(table => table.Select(p => p).Skip(new Product() { SmallId = 2 }.SmallId).Take(5));
            Assert.Equal(2, query.Skip);
            Assert.Equal(5, query.Top);
        }

        [Fact]
        public void WithParameters()
        {
            var userParmeters1 = new Dictionary<string, string>() { { "state", "PA" } };
            var userParmeters2 = new Dictionary<string, string>() { { "country", "USA" } };

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            IMobileServiceTable<Product> t = service.GetTable<Product>();

            IMobileServiceTableQuery<Product> originalQuery = from p in t select p;
            IMobileServiceTableQuery<Product> query = originalQuery.WithParameters(userParmeters1)
                                                                   .Skip(2)
                                                                   .WithParameters(userParmeters2);

            Assert.Equal("PA", originalQuery.Parameters["state"]); // , "original query should also have parameters"
            Assert.Equal("USA", originalQuery.Parameters["country"]); // , "original query should also have parameters"
            Assert.Equal("PA", query.Parameters["state"]);
            Assert.Equal("USA", query.Parameters["country"]);
        }

        [Fact]
        public void Filtering()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Price > 50
                select p);
            AssertFilter(query.Filter, "(Price gt 50M)");

            query = Compile<Product, Product>(table => table.Where(p => p.Price > 50));
            AssertFilter(query.Filter, "(Price gt 50M)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight <= 10
                select p);
            AssertFilter(query.Filter, "(Weight le 10f)");

            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= 10));
            AssertFilter(query.Filter, "(Weight le 10f)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight <= 10 && p.InStock == true
                select p);
            AssertFilter(query.Filter, "((Weight le 10f) and (InStock eq true))");

            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= 10 && p.InStock == true));
            AssertFilter(query.Filter, "((Weight le 10f) and (InStock eq true))");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight <= 10 || p.InStock == true
                select p);
            AssertFilter(query.Filter, "((Weight le 10f) or (InStock eq true))");

            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= 10 || p.InStock == true));
            AssertFilter(query.Filter, "((Weight le 10f) or (InStock eq true))");

            query = Compile<Product, Product>(table =>
                from p in table
                where !p.InStock
                select p);
            AssertFilter(query.Filter, "not(InStock)");

            query = Compile<Product, Product>(table => table.Where(p => !p.InStock));
            AssertFilter(query.Filter, "not(InStock)");
        }

        [Fact]
        public void Filtering_PartialEval()
        {
            // Allow New Operations
            float foo = 10;
            var query = Compile<Product, Product>(table => table.Where(p => p.Weight <= new Product() { Weight = foo }.Weight || p.InStock == true));
            AssertFilter(query.Filter, "((Weight le 10f) or (InStock eq true))");

            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= new Product(15) { Weight = foo }.Weight || p.InStock == true));
            AssertFilter(query.Filter, "((Weight le 10f) or (InStock eq true))");

            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= new Product(15) { Weight = 10 }.Weight || p.InStock == true));
            AssertFilter(query.Filter, "((Weight le 10f) or (InStock eq true))");

            long id = 15;
            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= new Product(id) { Weight = 10 }.Weight || p.InStock == true));
            AssertFilter(query.Filter, "((Weight le 10f) or (InStock eq true))");

            query = Compile<Product, Product>(table => table.Where(p => p.Created == new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc)));
            AssertFilter(query.Filter, "(Created eq datetime'1994-10-14T00%3A00%3A00.000Z')");

            query = Compile<ProductWithDateTimeOffset, ProductWithDateTimeOffset>(table => table.Where(p => p.Created == new DateTimeOffset(1994, 10, 13, 17, 0, 0, TimeSpan.FromHours(7))));
            AssertFilter(query.Filter, "(Created eq datetimeoffset'1994-10-13T17%3A00%3A00.0000000%2B07%3A00')");
        }

        [Fact]
        public void CombinedQuery()
        {
            MobileServiceTableQueryDescription query = Compile((IMobileServiceTable<Product> table) =>
                (from p in table
                 where p.Price <= 10M && p.Weight > 10f
                 where !p.InStock
                 orderby p.Price descending, p.Name
                 select new { p.Name, p.Price })
                .Skip(20)
                .Take(10));
            Assert.Equal("$filter=(((Price le 10M) and (Weight gt 10f)) and not(InStock))&$orderby=Price desc,Name&$skip=20&$top=10&$select=Name,Price,Weight,WeightInKG", query.ToODataString());
        }

        [Fact]
        public void Bug466610UsingShorts()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                table.Where(p => p.DisplayAisle == 2));
            AssertFilter(query.Filter, "(DisplayAisle eq 2)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.DisplayAisle == (short)3
                select p);
            AssertFilter(query.Filter, "(DisplayAisle eq 3)");

            short closedOverVariable = (short)5;
            query = Compile<Product, Product>(table =>
                from p in table
                where p.DisplayAisle == closedOverVariable
                select p);
            AssertFilter(query.Filter, "(DisplayAisle eq 5)");

            query = Compile<Product, Product>(table =>
                table.Where(p => p.OptionFlags == 7));
            AssertFilter(query.Filter, "(OptionFlags eq 7)");

            // Verify non-numeric conversions still aren't supported
            Assert.Throws<NotSupportedException>(() =>
            {
                object aisle = 12.0;
                Compile<Product, Product>(table =>
                        from p in table
                        where (object)p.DisplayAisle == aisle
                        select p
                );
            });
        }

        [Fact]
        public void Bug466610BareSkipTake()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table => table.Skip(3));
            Assert.Equal(3, query.Skip);
            Assert.False(query.Top.HasValue);

            query = Compile<Product, Product>(table => table.Take(5));
            Assert.Equal(5, query.Top);
            Assert.False(query.Skip.HasValue);

            query = Compile<Product, Product>(table => table.Skip(7).Take(9));
            Assert.Equal(7, query.Skip);
            Assert.Equal(9, query.Top);

            query = Compile<Product, Product>(table => table.Take(11).Skip(13));
            Assert.Equal(13, query.Skip);
            Assert.Equal(11, query.Top);
        }

        [Fact]
        public void FilterOperators()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Name + "x" == "mx"
                select p);
            AssertFilter(query.Filter, "(concat(Name,'x') eq 'mx')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight + 1.0 == 10.0
                select p);
            AssertFilter(query.Filter, "((Weight add 1.0) eq 10.0)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight + 1 == 10
                select p);
            AssertFilter(query.Filter, "((Weight add 1f) eq 10f)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight - 1.0 == 10.0
                select p);
            AssertFilter(query.Filter, "((Weight sub 1.0) eq 10.0)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight * 2.0 == 10.0
                select p);
            AssertFilter(query.Filter, "((Weight mul 2.0) eq 10.0)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight / 2.0 == 10.0
                select p);
            AssertFilter(query.Filter, "((Weight div 2.0) eq 10.0)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Id % 2 == 1
                select p);
            AssertFilter(query.Filter, "((id mod 2L) eq 1L)");

            query = Compile<Product, Product>(table =>
                from p in table
                where (p.Weight * 2.0) / 3.0 + 1.0 == 10.0
                select p);
            AssertFilter(query.Filter, "((((Weight mul 2.0) div 3.0) add 1.0) eq 10.0)");
        }

        [Fact]
        public void FilterMethods_1()
        {
            // Methods that look like properties
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Length == 7
                select p);
            AssertFilter(query.Filter, "(length(Name) eq 7)");
        }

        [Fact]
        public void FilterMethods_2()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Day == 7
                select p);
            AssertFilter(query.Filter, "(day(Created) eq 7)");
        }

        [Fact]
        public void FilterMethods_3()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Month == 7
                select p);
            AssertFilter(query.Filter, "(month(Created) eq 7)");
        }

        [Fact]
        public void FilterMethods_4()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Year == 7
                select p);
            AssertFilter(query.Filter, "(year(Created) eq 7)");
        }

        [Fact]
        public void FilterMethods_5()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Hour == 7
                select p);
            AssertFilter(query.Filter, "(hour(Created) eq 7)");
        }

        [Fact]
        public void FilterMethods_6()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Minute == 7
                select p);
            AssertFilter(query.Filter, "(minute(Created) eq 7)");
        }

        [Fact]
        public void FilterMethods_7()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Second == 7
                select p);
            AssertFilter(query.Filter, "(second(Created) eq 7)");
        }

        [Fact]
        public void FilterMethods_8()
        {
            // Static methods
            var query = Compile<Product, Product>(table =>
                from p in table
                where Math.Floor(p.Weight) == 10
                select p);
            AssertFilter(query.Filter, "(floor(Weight) eq 10.0)");
        }

        [Fact]
        public void FilterMethods_9()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where Decimal.Floor(p.Price) == 10
                select p);
            AssertFilter(query.Filter, "(floor(Price) eq 10M)");
        }

        [Fact]
        public void FilterMethods_10()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where Math.Ceiling(p.Weight) == 10
                select p);
            AssertFilter(query.Filter, "(ceiling(Weight) eq 10.0)");
        }

        [Fact]
        public void FilterMethods_11()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where Decimal.Ceiling(p.Price) == 10
                select p);
            AssertFilter(query.Filter, "(ceiling(Price) eq 10M)");
        }

        [Fact]
        public void FilterMethods_12()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where Math.Round(p.Weight) == 10
                select p);
            AssertFilter(query.Filter, "(round(Weight) eq 10.0)");
        }

        [Fact]
        public void FilterMethods_13()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where Math.Round(p.Price) == 10
                select p);
            AssertFilter(query.Filter, "(round(Price) eq 10M)");
        }

        [Fact]
        public void FilterMethods_14()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where string.Concat(p.Name, "x") == "mx"
                select p);
            AssertFilter(query.Filter, "(concat(Name,'x') eq 'mx')");
        }

#pragma warning disable RCS1155 // Use StringComparison when comparing strings.
        [Fact]
        public void FilterMethods_15()
        {
            // Instance methods
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.ToLower() == "a"
                select p);
            AssertFilter(query.Filter, "(tolower(Name) eq 'a')");
        }
#pragma warning restore RCS1155 // Use StringComparison when comparing strings.

#pragma warning disable RCS1155 // Use StringComparison when comparing strings.
        [Fact]
        public void FilterMethods_16()
        {
            // Instance methods
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.ToLowerInvariant() == "a"
                select p);
            AssertFilter(query.Filter, "(tolower(Name) eq 'a')");
        }
#pragma warning restore RCS1155 // Use StringComparison when comparing strings.

#pragma warning disable RCS1155 // Use StringComparison when comparing strings.
        [Fact]
        public void FilterMethods_17()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.ToUpper() == "A"
                select p);
            AssertFilter(query.Filter, "(toupper(Name) eq 'A')");
        }
#pragma warning restore RCS1155 // Use StringComparison when comparing strings.

#pragma warning disable RCS1155 // Use StringComparison when comparing strings.
        [Fact]
        public void FilterMethods_18()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.ToUpperInvariant() == "A"
                select p);
            AssertFilter(query.Filter, "(toupper(Name) eq 'A')");
        }
#pragma warning restore RCS1155 // Use StringComparison when comparing strings.

        [Fact]
        public void FilterMethods_19()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Trim() == "A"
                select p);
            AssertFilter(query.Filter, "(trim(Name) eq 'A')");
        }

        [Fact]
        public void FilterMethods_20()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.StartsWith("x")
                select p);
            AssertFilter(query.Filter, "startswith(Name,'x')");
        }

        [Fact]
        public void FilterMethods_21()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.EndsWith("x")
                select p);
            AssertFilter(query.Filter, "endswith(Name,'x')");
        }

        [Fact]
        public void FilterMethods_22()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.IndexOf("x") == 2
                select p);
            AssertFilter(query.Filter, "(indexof(Name,'x') eq 2)");
        }

        [Fact]
        public void FilterMethods_23()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.IndexOf('x') == 2
                select p);
            AssertFilter(query.Filter, "(indexof(Name,'x') eq 2)");
        }

        [Fact]
        public void FilterMethods_24()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Contains("x")
                select p);
            AssertFilter(query.Filter, "substringof('x',Name)");
        }

        [Fact]
        public void FilterMethods_25()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Replace("a", "A") == "A"
                select p);
            AssertFilter(query.Filter, "(replace(Name,'a','A') eq 'A')");
        }

        [Fact]
        public void FilterMethods_26()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Replace('a', 'A') == "A"
                select p);
            AssertFilter(query.Filter, "(replace(Name,'a','A') eq 'A')");
        }

        [Fact]
        public void FilterMethods_27()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Substring(2) == "A"
                select p);
            AssertFilter(query.Filter, "(substring(Name,2) eq 'A')");
        }

        [Fact]
        public void FilterMethods_28()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Substring(2, 3) == "A"
                select p);
            AssertFilter(query.Filter, "(substring(Name,2,3) eq 'A')");
        }

        [Fact]
        public void FilterMethods_29()
        {
            // Verify each type works on nested expressions too
            var query = Compile<Product, Product>(table =>
                from p in table
                where (p.Name + "x").Length == 7
                select p);
            AssertFilter(query.Filter, "(length(concat(Name,'x')) eq 7)");
        }

        [Fact]
        public void FilterMethods_30()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where string.Concat(p.Name + "x", "x") == "mx"
                select p);
            AssertFilter(query.Filter, "(concat(concat(Name,'x'),'x') eq 'mx')");
        }

#pragma warning disable RCS1155 // Use StringComparison when comparing strings.
        [Fact]
        public void FilterMethods_31()
        {
            var query = Compile<Product, Product>(table =>
                from p in table
                where (p.Name + "x").ToLower() == "ax"
                select p);
            AssertFilter(query.Filter, "(tolower(concat(Name,'x')) eq 'ax')");
        }
#pragma warning restore RCS1155 // Use StringComparison when comparing strings.

        [Fact]
        public void FilterContainsMethod()
        {
            string[] names = new[] { "name1", "name2" };
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where names.Contains(p.Name)
                select p);
            AssertFilter(query.Filter, "((Name eq 'name1') or (Name eq 'name2'))");

            IEnumerable<string> namesEnum = new[] { "name1", "name2" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesEnum.Contains(p.Name)
                select p);
            AssertFilter(query.Filter, "((Name eq 'name1') or (Name eq 'name2'))");

            IEnumerable<string> empty = Enumerable.Empty<string>();
            query = Compile<Product, Product>(table =>
                from p in table
                where empty.Contains(p.Name)
                select p);
            AssertFilter(query.Filter, string.Empty);

            //test Contains on List<T>
            List<string> namesList = new List<string>() { "name1", "name2" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesList.Contains(p.Name)
                select p);
            AssertFilter(query.Filter, "((Name eq 'name1') or (Name eq 'name2'))");


            //test Contains on Collection<T>
            Collection<string> coll = new Collection<string>() { "name1", "name2" };
            query = Compile<Product, Product>(table =>
                from p in table
                where coll.Contains(p.Name)
                select p);
            AssertFilter(query.Filter, "((Name eq 'name1') or (Name eq 'name2'))");

            //test duplicates
            namesList = new List<string>() { "name1", "name1" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesList.Contains(p.Name)
                select p);
            AssertFilter(query.Filter, "((Name eq 'name1') or (Name eq 'name1'))");

            //test single
            namesList = new List<string>() { "name1" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesList.Contains(p.Name)
                select p);
            AssertFilter(query.Filter, "(Name eq 'name1')");

            //test multiples
            namesList = new List<string>() { "name1", "name2", "name3" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesList.Contains(p.Name)
                select p);
            AssertFilter(query.Filter, "(((Name eq 'name1') or (Name eq 'name2')) or (Name eq 'name3'))");


            IEnumerable<string> productNames = new Product[] { new Product { Name = "name1" }, new Product { Name = "name2" } }.Select(pr => pr.Name);
            query = Compile<Product, Product>(table =>
                from p in table
                where productNames.Contains(p.Name)
                select p);
            AssertFilter(query.Filter, "((Name eq 'name1') or (Name eq 'name2'))");

            IEnumerable<bool> booleans = new[] { false, true };
            query = Compile<Product, Product>(table =>
                from p in table
                where booleans.Contains(p.InStock)
                select p);
            AssertFilter(query.Filter, "((InStock eq false) or (InStock eq true))");

            IEnumerable<int> numbers = new[] { 13, 6 };
            query = Compile<Product, Product>(table =>
                from p in table
                where numbers.Contains(p.DisplayAisle)
                select p);
            AssertFilter(query.Filter, "((DisplayAisle eq 13) or (DisplayAisle eq 6))");

            IEnumerable<double> doubles = new[] { 4.6, 3.9089 };
            query = Compile<Product, Product>(table =>
                from p in table
                where doubles.Contains(p.Weight)
                select p);
            AssertFilter(query.Filter, "((Weight eq 4.6) or (Weight eq 3.9089))");
        }

        [Fact]
        public void FilterContainsMethodNegative()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                //Contains on parameter members is not supported
                Compile<Product, Product>(table =>
                        from p in table
                        where p.Tags.Contains(p.Name)
                        select p);
            });
        }

        [Fact]
        public void TestSelectInsideWhere()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                //test Select inside Where
                IEnumerable<Product> products = new Product[] { new Product { Name = "name1" }, new Product { Name = "name2" } };
                Compile<Product, Product>(table =>
                        from p in table
                        where products.Select(pr => pr.Name).Contains(p.Name)
                        select p);
            });
        }

        [Fact]
        public void MultipleParameterExpressionsThrowInContains()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                IEnumerable<int> numbers = new[] { 13, 6, 5 };
                //multiple parameter expressions throw in Contains
                Compile<Product, Product>(table =>
                        from p in table
                        where numbers.Select(n => p.Id).Contains(p.DisplayAisle)
                        select p);
            });
        }

        [Fact]
        public void ContainsOnListOfTWhereTNotMember()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                IEnumerable<object> objects = new object[] { 4.6, "string" };
                //Contains on list of T where T != typeof(p.Member)
                Compile<Product, Product>(table =>
                        from p in table
                        where objects.Contains(p.Weight)
                        select p);
            });
        }

        [Fact]
        public void FilterEnums()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Type == ProductType.Food
                select p);
            AssertFilter(query.Filter, "(Type eq 'Food')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum1 == Enum1.Enum1Value2
                select p);
            AssertFilter(query.Filter, "(Enum1 eq 'Enum1Value2')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum2 == Enum2.Enum2Value2
                select p);
            AssertFilter(query.Filter, "(Enum2 eq 'Enum2Value2')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum3 == (Enum3.Enum3Value2 | Enum3.Enum3Value1)
                select p);
            AssertFilter(query.Filter, "(Enum3 eq 'Enum3Value1%2C%20Enum3Value2')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum4 == Enum4.Enum4Value2
                select p);
            AssertFilter(query.Filter, "(Enum4 eq 'Enum4Value2')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum5 == Enum5.Enum5Value2
                select p);
            AssertFilter(query.Filter, "(Enum5 eq 'Enum5Value2')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum6 == Enum6.Enum6Value1
                select p);
            AssertFilter(query.Filter, "(Enum6 eq 'Enum6Value1')");

            //check if toString works
            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum6.ToString() == Enum6.Enum6Value1.ToString()
                select p);
            AssertFilter(query.Filter, "(Enum6 eq 'Enum6Value1')");
        }

        [Fact]
        public void ToStringTest()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Tags.ToString() == "Test"
                select p);
            AssertFilter(query.Filter, "(Tags eq 'Test')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Price.ToString() == "1.56"
                select p);
            AssertFilter(query.Filter, "(Price eq '1.56')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.ToString() == "January 23"
                select p);
            AssertFilter(query.Filter, "(Created eq 'January%2023')");

            IList<string> namesList = new List<string>() { "name1", "name1" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesList.Contains(p.DisplayAisle.ToString())
                select p);
            AssertFilter(query.Filter, "((DisplayAisle eq 'name1') or (DisplayAisle eq 'name1'))");
        }

        [Fact]
        public void FilterReturnsTrueConstant()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where true
                select p);
            AssertFilter(query.Filter, "true");

            query = Compile<Product, Product>(table => table.Where(p => true));
            AssertFilter(query.Filter, "true");

            query = Compile<Product, Product>(table => table.Where(item => !item.InStock || item.InStock));
            AssertFilter(query.Filter, "(not(InStock) or InStock)");
        }

        [Fact]
        public void FilterNullable()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Updated == null
                select p);
            AssertFilter(query.Filter, "(Updated eq null)");

            var minDate = new DateTime(1, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            query = Compile<Product, Product>(table =>
               from p in table
               where p.Updated == minDate
               select p);
            AssertFilter(query.Filter, "(Updated eq datetime'0001-01-01T08%3A00%3A00.000Z')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.WeightInKG == null
                select p);
            AssertFilter(query.Filter, "(WeightInKG eq null)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.WeightInKG == 4.0
                select p);
            AssertFilter(query.Filter, "(WeightInKG eq 4.0)");
        }

        [Fact]
        public void MultipleSkipShouldAddUp()
        {
            // Query syntax
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Skip(2).Skip(5));
            Assert.Equal(7, query.Skip);

            query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Skip(5).Skip(5));
            Assert.Equal(10, query.Skip);

            query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Skip(20).Skip(6).Skip(5).Skip(2).Skip(9).Skip(5));
            Assert.Equal(47, query.Skip);
        }

        [Fact]
        public void MultipleTakeShouldChooseSmallest()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Take(2).Take(5));
            Assert.Equal(2, query.Top);

            query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Take(5).Take(2));
            Assert.Equal(2, query.Top);

            query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Take(5).Take(5));
            Assert.Equal(5, query.Top);
        }

        [Fact]
        public void TableNameShouldPropagateCorrectly()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                select p);
            Assert.Equal("Product", query.TableName);
            Assert.Null(query.Filter);
            Assert.Equal(2, query.Selection.Count);
            Assert.Equal(0, query.Ordering.Count);

            query = Compile<Product, string>(table =>
                from p in table
                select p.Name);
            Assert.Equal("Product", query.TableName);
            Assert.Null(query.Filter);
            Assert.Equal(3, query.Selection.Count);
            Assert.Equal(0, query.Ordering.Count);
        }

        [Fact]
        public void BooleanMemberValuesShouldBeHandled()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.InStock
                select p);
            Assert.Equal("Product", query.TableName);
            AssertFilter(query.Filter, "InStock");
            Assert.Equal(0, query.Selection.Count);
            Assert.Equal(0, query.Ordering.Count);
        }

        [Fact]
        public void UnsignedDataTypesTest()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.UnsignedId == 12UL
                select p);
            Assert.Equal("Product", query.TableName);
            AssertFilter(query.Filter, "(UnsignedId eq 12L)");

            //unsigned ints should be sent as long
            query = Compile<Product, Product>(table =>
                from p in table
                where p.UnsignedSmallId == 12U //uint
                select p);
            Assert.Equal("Product", query.TableName);
            AssertFilter(query.Filter, "(UnsignedSmallId eq 12L)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.UnsignedDisplayAisle == (ushort)12
                select p);
            Assert.Equal("Product", query.TableName);
            AssertFilter(query.Filter, "(UnsignedDisplayAisle eq 12)");
        }

        [Fact]
        public void FilterBasedOnTimeSpanShouldResultInString()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.AvailableTime > TimeSpan.FromDays(1)
                select p);
            Assert.Equal("Product", query.TableName);
            AssertFilter(query.Filter, "(AvailableTime gt '1.00%3A00%3A00')");
            Assert.Equal(0, query.Selection.Count);
            Assert.Equal(0, query.Ordering.Count);
        }

        [Fact]
        public void SkipZero()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table => table.Skip(0));
            Assert.Equal("Product", query.TableName);
            Assert.Equal(0, query.Skip);
            Assert.False(query.Top.HasValue);
            Assert.Equal("$skip=0", query.ToODataString());
        }

        [Fact]
        public void TopZero()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table => table.Take(0));
            Assert.Equal("Product", query.TableName);
            Assert.Equal(0, query.Top);
            Assert.False(query.Skip.HasValue);
            Assert.Equal("$top=0", query.ToODataString());
        }

        [Fact]
        public void DoublesSerializedUsingInvariantCulture()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight > 1.3f
                select p);
            AssertFilter(query.Filter, "(Weight gt 1.3f)");
        }

        [Fact]
        public void DoublesSerializedAsDoubles()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where (p.SmallId / 100.0) == 2
                select p);
            AssertFilter(query.Filter, "((SmallId div 100.0) eq 2.0)");

            query = Compile<Product, Product>(table =>
                from p in table
                where (p.Weight * 31.213) == 60200000000000000000000000.0
                select p);
            AssertFilter(query.Filter, "((Weight mul 31.213) eq 6.02E+25)");
        }

        private static void AssertFilter(QueryNode filter, string expectedFilterStr)
        {
            string filterStr = ODataExpressionVisitor.ToODataString(filter);
            Assert.Equal(expectedFilterStr, filterStr);
        }
    }
}
