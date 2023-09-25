﻿using FluentHelper.EntityFrameworkCore.Tests.Support;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
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

            Assert.NotNull(retrievedModelBuilder);
            Assert.AreEqual(modelBuilder, retrievedModelBuilder);
        }

        [Test]
        public void Verify_GetModelBuilder_Throws_When_Null()
        {
            var modelBuilder = Substitute.For<ModelBuilder>();

            var testEntityMap = new TestEntityMap();

            Assert.Throws<ArgumentNullException>(() => testEntityMap.GetModelBuilder());
        }

        [Test]
        public void Verify_EntityProperty_Works()
        {
            var modelBuilder = Substitute.For<ModelBuilder>();
            modelBuilder.Entity<TestEntity>().Returns(x => null);

            var testEntityMap = new TestEntityMap();
            testEntityMap.SetModelBuilder(modelBuilder);

            _ = testEntityMap.Entity;
            modelBuilder.Received(1).Entity<TestEntity>();
        }

        [Test]
        public void Verify_GetMappedType_Works()
        {
            var testEntityMap = new TestEntityMap();

            var mappedType = testEntityMap.GetMappedType();

            Assert.NotNull(mappedType);
            Assert.AreEqual(typeof(TestEntity), mappedType);
        }
    }
}