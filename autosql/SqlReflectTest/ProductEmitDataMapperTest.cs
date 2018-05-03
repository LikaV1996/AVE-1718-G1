using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflect;
using SqlReflectTest.DataMappers;
using SqlReflectTest.Model;

namespace SqlReflectTest
{
    [TestClass]
    public class ProductEmitDataMapperTest : AbstractProductDataMapperTest
    {
        public ProductEmitDataMapperTest() : base(
            EmitDataMapper.Build(typeof(Product), NORTHWIND, false),
            EmitDataMapper.Build(typeof(Category), NORTHWIND, false),
            EmitDataMapper.Build(typeof(Supplier), NORTHWIND, false) )
        {
        }

        [TestMethod]
        public void TestProductEmitGetAll() {
            base.TestProductGetAll();
        }

        [TestMethod]
        public void TestProducEmitGetById() {
            base.TestProductGetById();
        }

        [TestMethod]
        public void TestProductEmitInsertAndDelete()
        {
            base.TestProductInsertAndDelete();
        }
    }
}
