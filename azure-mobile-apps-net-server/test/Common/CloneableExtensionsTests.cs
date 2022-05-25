// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Xunit;

namespace System
{
    public class CloneableExtensionsTests
    {
        [Fact]
        public void Clone_CallsClone_OnExplicitInterfaceDefinition()
        {
            // Arrange
            ExplicitInterfaceDefinition source = new ExplicitInterfaceDefinition { Age = 2, Name = "Henrik" };

            // Act
            ExplicitInterfaceDefinition result = source.Clone();

            // Assert
            Assert.NotSame(source, result);
            Assert.Equal(source.Age, result.Age);
            Assert.Equal(source.Name, result.Name);
        }

        [Fact]
        public void Clone_CallsClone_OnImplicitInterfaceDefinition()
        {
            // Arrange
            ImplicitInterfaceDefinition source = new ImplicitInterfaceDefinition { Age = 2, Name = "Henrik" };

            // Act
            ImplicitInterfaceDefinition result = source.Clone<ImplicitInterfaceDefinition>();

            // Assert
            Assert.NotSame(source, result);
            Assert.Equal(source.Age, result.Age);
            Assert.Equal(source.Name, result.Name);
        }

        private class ExplicitInterfaceDefinition : ICloneable
        {
            public ExplicitInterfaceDefinition()
            {
            }

            protected ExplicitInterfaceDefinition(ExplicitInterfaceDefinition source)
            {
                this.Name = source.Name;
                this.Age = source.Age;
            }

            public string Name { get; set; }

            public int Age { get; set; }

            object ICloneable.Clone()
            {
                return new ExplicitInterfaceDefinition(this);
            }
        }

        private class ImplicitInterfaceDefinition : ICloneable
        {
            public ImplicitInterfaceDefinition()
            {
            }

            protected ImplicitInterfaceDefinition(ImplicitInterfaceDefinition source)
            {
                this.Name = source.Name;
                this.Age = source.Age;
            }

            public string Name { get; set; }

            public int Age { get; set; }

            public object Clone()
            {
                return new ImplicitInterfaceDefinition(this);
            }
        }
    }
}
