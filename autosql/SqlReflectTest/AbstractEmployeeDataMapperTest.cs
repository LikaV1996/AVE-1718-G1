using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.Model;
using SqlReflect;
using System.Collections;

namespace SqlReflectTest
{
    public abstract class AbstractEmployeeDataMapperTest
    {
        protected static readonly string NORTHWIND = @"
                    Server=(LocalDB)\MSSQLLocalDB;
                    Integrated Security=true;
                    AttachDbFileName=" +
                        Environment.CurrentDirectory +
                        "\\data\\NORTHWND.MDF";

        readonly IDataMapper employees;

        public AbstractEmployeeDataMapperTest(IDataMapper employees)
        {
            this.employees = employees;
        }

        public void TestEmployeeGetAll()
        {
            IEnumerable res = employees.GetAll();
            int count = 0;
            foreach (object p in res)
            {
                Console.WriteLine(p);
                count++;
            }
            Assert.AreEqual(9, count);
        }
        public void TestEmployeeGetById()
        {
            Employee e = (Employee)employees.GetById(3);
            Assert.AreEqual("Leverling", e.LastName);
            Assert.AreEqual("Janet", e.FirstName);
            Assert.AreEqual("Sales Representative", e.Title);
            Assert.AreEqual("Ms.", e.TitleOfCourtesy);
            Assert.AreEqual("722 Moss Bay Blvd.", e.Address);
            Assert.AreEqual("Kirkland", e.City);
            Assert.AreEqual("WA", e.Region);
            Assert.AreEqual("98033", e.PostalCode);
            Assert.AreEqual("USA", e.Country);
            Assert.AreEqual("(206) 555-3412", e.HomePhone);
            Assert.AreEqual("3355", e.Extension);
        }

        public void TestEmployeeInsertAndDelete()
        {
            //
            // Create and Insert new Employee
            // 
            Employee e = new Employee()
            {
                LastName = "LastN",
                FirstName = "FirstN",
                Title = "Representative",
                TitleOfCourtesy = "Thing",
                Address = "None",
                City = "Nowhere",
                Region = "Nope",
                PostalCode = "non-nono",
                Country = "NON",
                HomePhone = "999 999 999",
                Extension = "1234"
            };

            //employees.Delete(e);
            object id = employees.Insert(e);
            //
            // Get the new employee object from database
            //
            Employee actual = (Employee)employees.GetById(id);
            Assert.AreEqual(e.LastName, actual.LastName);
            Assert.AreEqual(e.FirstName, actual.FirstName);
            Assert.AreEqual(e.Title, actual.Title);
            Assert.AreEqual(e.TitleOfCourtesy, actual.TitleOfCourtesy);
            Assert.AreEqual(e.Address, actual.Address);
            Assert.AreEqual(e.City, actual.City);
            Assert.AreEqual(e.Region, actual.Region);
            Assert.AreEqual(e.PostalCode, actual.PostalCode);
            Assert.AreEqual(e.Country, actual.Country);
            Assert.AreEqual(e.HomePhone, actual.HomePhone);
            Assert.AreEqual(e.Extension, actual.Extension);
            //
            // Delete the created employee from database
            //
            employees.Delete(actual);
            object res = employees.GetById(id);
            actual = res != null ? (Employee)res : default(Employee);
            Assert.IsNull(actual.LastName);
            Assert.IsNull(actual.FirstName);
            Assert.IsNull(actual.Title);
            Assert.IsNull(actual.TitleOfCourtesy);
            Assert.IsNull(actual.Address);
            Assert.IsNull(actual.City);
            Assert.IsNull(actual.Region);
            Assert.IsNull(actual.PostalCode);
            Assert.IsNull(actual.Country);
            Assert.IsNull(actual.HomePhone);
            Assert.IsNull(actual.Extension);
        }

        public void TestEmployeeUpdate()
        {
            int id = 5;
            Employee original = (Employee)employees.GetById(id);
            Employee modified = new Employee()
            {
                EmployeeID = id,
                LastName = "LastNMod",
                FirstName = "FirstNMod",
                Title = "Modified",
                TitleOfCourtesy = "Mod",
                Address = "None",
                City = "Nowhere",
                Region = "Nope",
                PostalCode = "non-nono",
                Country = "NON",
                HomePhone = "999 666 999",
                Extension = "4321"
            };
            /*  //Just in case DataBase goes Bananas
            modified = new Employee()
            {
                EmployeeID = id,
                LastName = "Buchanan",
                FirstName = "Steven",
                Title = "Sales Manager",
                TitleOfCourtesy = "Mr.",
                Address = "Coventry HouseMiner Rd.",
                City = "London",
                Region = null,
                PostalCode = "EC2 7JR",
                Country = "UK",
                HomePhone = "(71) 555-7773",
                Extension = "428"
            };
            */
            employees.Update(modified);
            Employee actual = (Employee)employees.GetById(id);
            Assert.AreEqual(modified.LastName, actual.LastName);
            Assert.AreEqual(modified.FirstName, actual.FirstName);
            Assert.AreEqual(modified.Title, actual.Title);
            Assert.AreEqual(modified.TitleOfCourtesy, actual.TitleOfCourtesy);
            Assert.AreEqual(modified.Address, actual.Address);
            Assert.AreEqual(modified.City, actual.City);
            Assert.AreEqual(modified.Region, actual.Region);
            Assert.AreEqual(modified.PostalCode, actual.PostalCode);
            Assert.AreEqual(modified.Country, actual.Country);
            Assert.AreEqual(modified.HomePhone, actual.HomePhone);
            Assert.AreEqual(modified.Extension, actual.Extension);

            employees.Update(original);
            actual = (Employee)employees.GetById(id);
            Assert.AreEqual(original.LastName, actual.LastName);
            Assert.AreEqual(original.FirstName, actual.FirstName);
            Assert.AreEqual(original.Title, actual.Title);
            Assert.AreEqual(original.TitleOfCourtesy, actual.TitleOfCourtesy);
            Assert.AreEqual(original.Address, actual.Address);
            Assert.AreEqual(original.City, actual.City);
            Assert.AreEqual(original.Region ?? "", actual.Region);
            Assert.AreEqual(original.PostalCode, actual.PostalCode);
            Assert.AreEqual(original.Country, actual.Country);
            Assert.AreEqual(original.HomePhone, actual.HomePhone);
            Assert.AreEqual(original.Extension, actual.Extension);
            /*
            Assert.AreEqual(id, actual.LastName);
            Assert.AreEqual("Buchanan", actual.FirstName);
            Assert.AreEqual("Steven", actual.Title);
            Assert.AreEqual("Sales Manager", actual.TitleOfCourtesy);
            Assert.AreEqual("Mr.", actual.Address);
            Assert.AreEqual("Coventry HouseMiner Rd.", actual.City);
            Assert.AreEqual("London", actual.Region);
            Assert.AreEqual(null, actual.PostalCode);
            Assert.AreEqual("EC2 7JR", actual.Country);
            Assert.AreEqual("UK", actual.HomePhone);
            Assert.AreEqual("(71) 555-7773", actual.Extension);
            Assert.AreEqual("428", actual.Extension);
            */
        }

    }
}
