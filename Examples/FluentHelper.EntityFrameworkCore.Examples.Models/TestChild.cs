using System;

namespace EntityFramework.FluentHelperCore.Examples.Models
{
    public class TestChild
    {
        public Guid Id { get; set; }
        public Guid IdParent { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Active { get; set; }

        public virtual TestData Parent { get; set; }
    }
}
