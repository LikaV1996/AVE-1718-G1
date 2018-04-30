using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflect;
using SqlReflectTest.DataMappers;
using SqlReflectTest.Model;

namespace SqlReflectTest
{
    [TestClass]
    public class CustomerEmitDataMapperTest : AbstractCustomerDataMapperTest
    {
        public CustomerEmitDataMapperTest() : base(EmitDataMapper.Build(typeof(Customer), NORTHWIND, false))
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
