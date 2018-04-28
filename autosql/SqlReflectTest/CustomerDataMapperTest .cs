using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.DataMappers;
using SqlReflectTest.Model;

namespace SqlReflectTest
{
    [TestClass]
    public class CustomerDataMapperTest : AbstractCustomerDataMapperTest
    {
        public CustomerDataMapperTest() : base(new CustomerDataMapper(typeof(Customer),NORTHWIND,false))
        {
        }

        [TestMethod]
        public new void TestCustomerGetAll() {
            base.TestCustomerGetAll();
        }

        [TestMethod]
        public new void TestCustomerGetById() {
            base.TestCustomerGetById();
        }

        [TestMethod]
        public new void TestCustomerInsertAndDelete() {
            base.TestCustomerInsertAndDelete();
        }

        [TestMethod]
        public new void TestCustomerUpdate() {
            base.TestCustomerUpdate();
        }
    }
}
