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
        public new void TestProductEmitGetAll() {
            base.TestProductGetAll();
        }

        [TestMethod]
        public new void TestProducEmittGetById() {
            base.TestProductGetById();
        }

        [TestMethod]
        public new void TestProductEmitInsertAndDelete()
        {
            base.TestProductInsertAndDelete();
        }
    }
}
