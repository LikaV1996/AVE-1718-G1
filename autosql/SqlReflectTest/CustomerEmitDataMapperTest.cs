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
        public void TestCustomerEmitGetAll() {
            base.TestCustomerGetAll();
        }

        [TestMethod]
        public void TestCustomerEmitGetById() {
            base.TestCustomerGetById();
        }

        [TestMethod]
        public void TestCustomerEmitInsertAndDelete() {
            base.TestCustomerInsertAndDelete();
        }

        [TestMethod]
        public void TestCustomerEmitUpdate() {
            base.TestCustomerUpdate();
        }
    }
}
