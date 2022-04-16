using System;
using System.Collections.Generic;

namespace FluentHelper.EntityFrameworkCore.Examples.Models
{
    public class TestData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Active { get; set; }

        public virtual TestDataAttr Attr { get; set; }
        public virtual ICollection<TestChild> ChildList { get; set; }
    }
}
