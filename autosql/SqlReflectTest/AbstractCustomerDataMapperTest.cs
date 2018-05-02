using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.Model;
using SqlReflect;
using System.Collections;

namespace SqlReflectTest
{
    public abstract class AbstractCustomerDataMapperTest
    {
        protected static readonly string NORTHWIND = @"
                    Server=(LocalDB)\MSSQLLocalDB;
                    Integrated Security=true;
                    AttachDbFileName=" +
                        Environment.CurrentDirectory +
                        "\\data\\NORTHWND.MDF";

        readonly IDataMapper customers;

        public AbstractCustomerDataMapperTest(IDataMapper customers)
        {
            this.customers = customers;
        }

        public void TestCustomerGetAll()
        {
            IEnumerable res = customers.GetAll();
            int count = 0;
            foreach (object p in res)
            {
                Console.WriteLine(p);
                count++;
            }
            Assert.AreEqual(91, count);
        }
        public void TestCustomerGetById()
        {
            Customer c = (Customer)customers.GetById("'" + "ANTON" + "'");
            Assert.AreEqual("Antonio Moreno Taquería", c.CompanyName);
            Assert.AreEqual("Antonio Moreno", c.ContactName);
            Assert.AreEqual("Owner", c.ContactTitle);
            Assert.AreEqual("Mataderos  2312", c.Address);
            Assert.AreEqual("México D.F.", c.City);
            Assert.AreEqual(null, c.Region);
            Assert.AreEqual("05023", c.PostalCode);
            Assert.AreEqual("Mexico", c.Country);
            Assert.AreEqual("(5) 555-3932", c.Phone);
            Assert.AreEqual(null, c.Fax);
        }

        public void TestCustomerInsertAndDelete()
        {
            //
            // Create and Insert new Customer
            // 
            Customer c = new Customer()
            {
                CustomerID = "HELLO",
                CompanyName = "HelloWorld",
                ContactName = "Hi",
                ContactTitle = "Owner",
                Address = "Nowhere",
                City = "Nope",
                Region = null,
                PostalCode = "non-nono",
                Country = "Far Away Land",
                Phone = "9177(dance dance)",
                Fax = null
            };

            //customers.Delete(c);
            object id = "'" + customers.Insert(c) + "'";
            //
            // Get the new customer object from database
            //
            Customer actual = (Customer)customers.GetById(id);
            Assert.AreEqual(c.CompanyName, actual.CompanyName);
            Assert.AreEqual(c.ContactName, actual.ContactName);
            Assert.AreEqual(c.ContactTitle, actual.ContactTitle);
            Assert.AreEqual(c.Address, actual.Address);
            Assert.AreEqual(c.City, actual.City);
            Assert.AreEqual(c.Region ?? "", actual.Region);
            Assert.AreEqual(c.PostalCode, actual.PostalCode);
            Assert.AreEqual(c.Country, actual.Country);
            Assert.AreEqual(c.Phone, actual.Phone);
            Assert.AreEqual(c.Fax ?? "", actual.Fax);
            //
            // Delete the created customer from database
            //
            customers.Delete(actual);
            object res = customers.GetById(id);
            actual = res != null ? (Customer)res : default(Customer);
            Assert.IsNull(actual.CompanyName);
            Assert.IsNull(actual.ContactName);
            Assert.IsNull(actual.ContactTitle);
            Assert.IsNull(actual.Address);
            Assert.IsNull(actual.City);
            Assert.IsNull(actual.Region);
            Assert.IsNull(actual.PostalCode);
            Assert.IsNull(actual.Country);
            Assert.IsNull(actual.Phone);
            Assert.IsNull(actual.Fax);
        }

        public void TestCustomerUpdate()
        {
            string id = "'" + "FRANK" + "'";
            Customer original = (Customer)customers.GetById(id);
            Customer modified = new Customer()
            {
                CustomerID = "FRANK",
                CompanyName = "Modified",
                ContactName = "Mod",
                ContactTitle = "Owner",
                Address = "IDK",
                City = "IDC",
                Region = null,
                PostalCode = "FFS",
                Country = "IDGAF",
                Phone = "(+351) 770 300 300",
                Fax = null
            };
            /*  //Just in case DataBase goes Bananas
            modified = new Customer()
            {
                CustomerID = "FRANK",
                CompanyName = "Frankenversand",
                ContactName = "Peter Franken",
                ContactTitle = "Marketing Manager",
                Address = "Berliner Platz 43",
                City = "München",
                Region = null,
                PostalCode = "80805",
                Country = "Germany",
                Phone = "089-0877310",
                Fax = "089-0877451"
            };
            */
            customers.Update(modified);
            Customer actual = (Customer)customers.GetById(id);
            Assert.AreEqual(modified.CompanyName, actual.CompanyName);
            Assert.AreEqual(modified.ContactName, actual.ContactName);
            Assert.AreEqual(modified.ContactTitle, actual.ContactTitle);
            Assert.AreEqual(modified.Address, actual.Address);
            Assert.AreEqual(modified.City, actual.City);
            Assert.AreEqual(modified.Region ?? "", actual.Region);
            Assert.AreEqual(modified.PostalCode, actual.PostalCode);
            Assert.AreEqual(modified.Country, actual.Country);
            Assert.AreEqual(modified.Phone, actual.Phone);
            Assert.AreEqual(modified.Fax ?? "", actual.Fax);

            customers.Update(original);
            actual = (Customer)customers.GetById(id);
            Assert.AreEqual(original.CompanyName, actual.CompanyName);
            Assert.AreEqual(original.ContactName, actual.ContactName);
            Assert.AreEqual(original.ContactTitle, actual.ContactTitle);
            Assert.AreEqual(original.Address, actual.Address);
            Assert.AreEqual(original.City, actual.City);
            Assert.AreEqual(original.Region ?? "", actual.Region);
            Assert.AreEqual(original.PostalCode, actual.PostalCode);
            Assert.AreEqual(original.Country, actual.Country);
            Assert.AreEqual(original.Phone, actual.Phone);
            Assert.AreEqual(original.Fax ?? "", actual.Fax);
            /*
            Assert.AreEqual("Frankenversand", actual.CompanyName);
            Assert.AreEqual("Peter Franken", actual.ContactName);
            Assert.AreEqual("Marketing Manager", actual.ContactTitle);
            Assert.AreEqual("Berliner Platz 43", actual.Address);
            Assert.AreEqual("München", actual.City);
            Assert.AreEqual(null, actual.Region);
            Assert.AreEqual("80805", actual.PostalCode);
            Assert.AreEqual("Germany", actual.Country);
            Assert.AreEqual("089-0877310", actual.Phone);
            Assert.AreEqual("089-0877451", actual.Fax);
            */
        }
        
    }
}
