using FluentHelper.EntityFrameworkCore.Examples.Models;
using FluentHelper.EntityFrameworkCore.Examples.Repositories;
using System;
using System.Linq;

namespace FluentHelper.EntityFrameworkCore.Examples.Runner
{
    class Program
    {
        TestData ExampleData { get; set; }
        TestChild ExampleChild { get; set; }
        TestDataAttr ExampleAttr { get; set; }

        TestDataRepository TestDataRepository { get; set; }

        static void Main()
        {
            Program p = new();
            p.StartProgram();
        }

        public Program()
        {
            ExampleData = new TestData
            {
                Id = Guid.NewGuid(),
                Name = "ExampleData",
                CreationDate = DateTime.UtcNow,
                Active = true
            };
            ExampleChild = new TestChild
            {
                Id = Guid.NewGuid(),
                IdParent = ExampleData.Id,
                Name = "ExampleData",
                CreationDate = DateTime.UtcNow,
                Active = true
            };
            ExampleAttr = new TestDataAttr
            {
                Id = ExampleData.Id,
                IsBeautiful = true
            };

            TestDataRepository = new TestDataRepository();
        }

        void StartProgram()
        {
            try
            {
                var testDataList = TestDataRepository.GetAll().ToList();
                Console.WriteLine($"Table contains {testDataList.Count} rows");

                Console.WriteLine($"Adding 1 row..");
                PressToContinue();

                TestDataRepository.Add(ExampleData);

                testDataList = TestDataRepository.GetAll().ToList();
                Console.WriteLine($"Table contains {testDataList.Count} rows");
                Console.WriteLine($"Adding 1 child and 1 attr..");
                PressToContinue();

                TestDataRepository.AddChild(ExampleChild);
                TestDataRepository.AddAttr(ExampleAttr);

                var testDataInstance = TestDataRepository.GetById(ExampleData.Id);
                Console.WriteLine($"TestData is null:{testDataInstance == null}, contains {testDataInstance?.ChildList?.Count} children and IsBeautiful:{testDataInstance?.Attr?.IsBeautiful}");
                Console.WriteLine($"Removing 1 child..");
                PressToContinue();

                TestDataRepository.RemoveChild(ExampleChild.Id);

                testDataInstance = TestDataRepository.GetById(ExampleData.Id);
                Console.WriteLine($"TestData is null:{testDataInstance == null}, contains {testDataInstance?.ChildList?.Count} children and IsBeautiful:{testDataInstance?.Attr?.IsBeautiful}");
                Console.WriteLine($"Removing 1 attr..");
                PressToContinue();

                TestDataRepository.RemoveAttr(ExampleAttr.Id);

                testDataInstance = TestDataRepository.GetById(ExampleData.Id);
                Console.WriteLine($"TestData is null:{testDataInstance == null}, contains {testDataInstance?.ChildList?.Count} children and IsBeautiful:{testDataInstance?.Attr?.IsBeautiful}");
                Console.WriteLine($"Removing 1 row..");
                PressToContinue();

                TestDataRepository.Remove(ExampleData.Id);

                testDataList = TestDataRepository.GetAll().ToList();
                Console.WriteLine($"Table contains {testDataList.Count} rows");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            PressToContinue();
        }

        static void PressToContinue()
        {
            Console.WriteLine("Enter any key to continue..");
            Console.ReadLine();
        }
    }
}
