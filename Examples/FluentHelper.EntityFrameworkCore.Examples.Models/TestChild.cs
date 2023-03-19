using System;

namespace FluentHelper.EntityFrameworkCore.Examples.Models
{
    public class TestChild
    {
        public Guid Id { get; set; }
        public Guid IdParent { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public bool Active { get; set; }

        public virtual TestData? Parent { get; set; }
    }
}
