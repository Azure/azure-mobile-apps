// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Azure.Mobile.Server.TestModels
{
    public static class TestData
    {
        public static Collection<Person> Persons
        {
            get
            {
                return CreatePersons();
            }
        }

        private static Collection<Person> CreatePersons()
        {
            return new Collection<Person>
            {
                new Person { FirstName = "Henrik", LastName = "Nielsen", Age = 10 },
                new Person { FirstName = "你好世界", LastName = "Nielsen", Age = 20 },
                new Person { FirstName = "Ben", LastName = "Nielsen", Age = 30 },
                new Person { FirstName = "Nora", LastName = "Nielsen", Age = 40 },
                new Person { FirstName = "Mathew", LastName = "Charles", Age = 50 },
            };
        }

        public static Collection<Person> CreatePersons(int numberPersons)
        {
            Collection<Person> people = new Collection<Person>();

            for (int i = 1; i <= numberPersons; i++)
            {
                Person person = new Person
                { 
                    FirstName = string.Format("Person{0}".FormatInvariant(i)), 
                    LastName = "Smith", 
                    Age = i 
                };
                people.Add(person);
            }

            return people;
        }
    }
}
