using SqlReflect;
using SqlReflectTest.Model;
using System;
using System.Data;

namespace SqlReflectTest.DataMappers
{
    class SupplierDataMapper : AbstractDataMapper
    {
        const string SQL_GET_ALL = @"SELECT SupplierID, CompanyName, ContactName, ContactTitle, Address, City, Region, PostalCode, Country, Phone, Fax
                                     FROM Suppliers";
        const string SQL_GET_BY_ID = SQL_GET_ALL + " WHERE SupplierID=";

        public SupplierDataMapper(string connStr) : base(connStr)
        {
        }

        protected override string SqlGetAll()
        {
            return SQL_GET_ALL;
        }
        protected override string SqlGetById(object id)
        {
            return SQL_GET_BY_ID + id; 
        }

        protected override string SqlInsert(object target)
        {
            throw new NotImplementedException();
        }
        protected override string SqlUpdate(object target)
        {
            throw new NotImplementedException();
        }

        protected override string SqlDelete(object target)
        {
            throw new NotImplementedException();
        }

        

        protected override object Load(IDataReader dr)
        {
            Supplier s = new Supplier();
            s.SupplierID = (int)dr["SupplierID"];
            s.CompanyName = dr["CompanyName"] as string;
            s.ContactName = dr["ContactName"] as string;
            s.ContactTitle = dr["ContactTitle"] as string;
            s.Address = dr["Address"] as string;
            s.City = dr["City"] as string;
            s.Region = dr["Region"] as string;
            s.PostalCode = dr["PostalCode"] as string;
            s.Country = dr["Country"] as string;
            s.Phone = dr["Phone"] as string;
            s.Fax = dr["Fax"] as string;
            return s;
        }
    }
}
