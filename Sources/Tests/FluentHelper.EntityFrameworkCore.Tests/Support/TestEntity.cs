using System;

namespace FluentHelper.EntityFrameworkCore.Tests.Support
{
    public class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
