using FluentHelper.EntityFrameworkCore.Tests.Support;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;

namespace FluentHelper.EntityFrameworkCore.Tests.Core
{
    [TestFixture]
    public class EfDbMapTests
    {
        [Test]
        public void Verify_GetSetModelBuilder_Works()
        {
            var modelBuilder = Substitute.For<ModelBuilder>();

            var testEntityMap = new TestEntityMap();
            testEntityMap.SetModelBuilder(modelBuilder);

            var retrievedModelBuilder = testEntityMap.GetModelBuilder();

            ClassicAssert.NotNull(retrievedModelBuilder);
            ClassicAssert.AreEqual(modelBuilder, retrievedModelBuilder);
        }

        [Test]
        public void Verify_GetSetModelBuilder_Throws_When_CalledTwice()
        {
            var modelBuilder = Substitute.For<ModelBuilder>();

            var testEntityMap = new TestEntityMap();
            testEntityMap.SetModelBuilder(modelBuilder);

            Assert.Throws<InvalidOperationException>(() => testEntityMap.SetModelBuilder(modelBuilder));
        }

        [Test]
        public void Verify_GetModelBuilder_Throws_When_Null()
        {
            var modelBuilder = Substitute.For<ModelBuilder>();

            var testEntityMap = new TestEntityMap();

            Assert.Throws<InvalidOperationException>(() => testEntityMap.GetModelBuilder());
        }

        [Test]
        public void Verify_GetMappedType_Works()
        {
            var testEntityMap = new TestEntityMap();

            var mappedType = testEntityMap.GetMappedType();

            ClassicAssert.NotNull(mappedType);
            ClassicAssert.AreEqual(typeof(TestEntity), mappedType);
        }
    }
}
