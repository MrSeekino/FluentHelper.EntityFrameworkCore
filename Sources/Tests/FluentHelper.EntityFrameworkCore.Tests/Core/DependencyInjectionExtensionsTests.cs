using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Tests.Core
{
    [TestFixture]
    public class DependencyInjectionExtensionsTests
    {
        [Test]
        public void Verify_AddFluentDbContext_WorksCorrectly_WithContextBuilder()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddFluentDbContext(dbConfigBuilder =>
            {
                dbConfigBuilder
                    .WithDbConfiguration(x => { })
                    .WithDbProvider(x => { })
                    .WithLazyLoadingProxies()
                    .WithLogAction((x, y, z) => { }, true)
                    .WithMappingFromAssemblyOf<TestEntityMap>();
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var dbConfig = serviceProvider.GetRequiredService<IDbConfig>();
            Assert.That(dbConfig, Is.Not.Null);
            Assert.That(dbConfig.DbConfiguration, Is.Not.Null);
            Assert.That(dbConfig.DbProvider, Is.Not.Null);
            Assert.That(dbConfig.LogAction, Is.Not.Null);
            Assert.That(dbConfig.EnableSensitiveDataLogging, Is.True);
            Assert.That(dbConfig.EnableLazyLoadingProxies, Is.True);
            Assert.That(dbConfig.MappingAssemblies.Count, Is.EqualTo(1));

            var dbMaps = serviceProvider.GetServices<IDbMap>();
            Assert.That(dbMaps, Is.Not.Null);
            Assert.That(dbMaps.Count(), Is.EqualTo(1));
            Assert.That(dbMaps.First().GetType(), Is.EqualTo(typeof(TestEntityMap)));

            var dbContext = serviceProvider.GetRequiredService<IDbContext>();
            Assert.That(dbContext, Is.Not.Null);
        }

        [Test]
        public void Verify_AddFluentDbContext_WorksCorrectly_WithDirectContext()
        {
            var dbContext = Substitute.For<IDbContext>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddFluentDbContext(dbContext);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var dbContextRetrieved = serviceProvider.GetRequiredService<IDbContext>();
            Assert.That(dbContextRetrieved, Is.Not.Null);
            Assert.AreEqual(dbContextRetrieved, dbContext);
        }
    }
}
