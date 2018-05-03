using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.DataMappers;
using SqlReflectTest.Model;

namespace SqlReflectTest
{
    [TestClass]
    public class EmployeeDataMapperTest : AbstractEmployeeDataMapperTest
    {
        public EmployeeDataMapperTest() : base(new EmployeeDataMapper(typeof(Employee),NORTHWIND,false))
        {
        }

        [TestMethod]
        public new void TestEmployeeGetAll() {
            base.TestEmployeeGetAll();
        }

        [TestMethod]
        public new void TestEmployeeGetById() {
            base.TestEmployeeGetById();
        }

        [TestMethod]
        public new void TestEmployeeInsertAndDelete() {
            base.TestEmployeeInsertAndDelete();
        }

        [TestMethod]
        public new void TestEmployeeUpdate() {
            base.TestEmployeeUpdate();
        }
    }
}
