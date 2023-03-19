using System;

namespace FluentHelper.EntityFrameworkCore.Examples.Models
{
    public class TestDataAttr
    {
        public Guid Id { get; set; }
        public bool IsBeautiful { get; set; }
        public virtual TestData? Data { get; set; }
    }
}
