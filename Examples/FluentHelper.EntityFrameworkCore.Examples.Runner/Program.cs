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

        TestDataRepository testDataRepository { get; set; }

        static void Main(string[] args)
        {
            Program p = new Program();
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

            testDataRepository = new TestDataRepository();
        }

        void StartProgram()
        {
            try
            {
                var testDataList = testDataRepository.GetAll().ToList();
                Console.WriteLine($"Table contains {testDataList.Count} rows");

                PressToContinue();

                Console.WriteLine($"Adding 1 row..");
                testDataRepository.Add(ExampleData);

                testDataList = testDataRepository.GetAll().ToList();
                Console.WriteLine($"Table contains {testDataList.Count} rows");

                PressToContinue();

                Console.WriteLine($"Adding 1 child..");
                testDataRepository.AddChild(ExampleChild);

                var testDataInstance = testDataRepository.GetById(ExampleData.Id);
                Console.WriteLine($"TestData is null:{testDataInstance == null} and contains {testDataInstance?.ChildList.Count} children");

                PressToContinue();

                Console.WriteLine($"Removing 1 row..");
                testDataRepository.Remove(ExampleData.Id);

                testDataList = testDataRepository.GetAll().ToList();
                Console.WriteLine($"Table contains {testDataList.Count} rows");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            PressToContinue();
        }

        void PressToContinue()
        {
            Console.WriteLine("Enter any key to continue..");
            Console.ReadLine();
        }
    }
}
