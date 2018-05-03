using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflect;
using SqlReflectTest.DataMappers;
using SqlReflectTest.Model;

namespace SqlReflectTest
{
    [TestClass]
    public class EmployeeEmitDataMapperTest : AbstractEmployeeDataMapperTest
    {
        public EmployeeEmitDataMapperTest() : base(EmitDataMapper.Build(typeof(Employee),NORTHWIND,false))
        {
        }

        [TestMethod]
        public void TestEmployeeEmitGetAll() {
            base.TestEmployeeGetAll();
        }

        [TestMethod]
        public void TestEmployeeEmitGetById() {
            base.TestEmployeeGetById();
        }

        [TestMethod]
        public void TestEmployeeEmitInsertAndDelete() {
            base.TestEmployeeInsertAndDelete();
        }

        [TestMethod]
        public void TestEmployeeEmitUpdate() {
            base.TestEmployeeUpdate();
        }
    }
}
