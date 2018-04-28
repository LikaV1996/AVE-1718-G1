﻿using SqlReflect;
using SqlReflectTest.Model;
using System;
using System.Data;

namespace SqlReflectTest.DataMappers
{
    class CustomerDataMapper : DynamicDataMapper
    {
        const string PK = "CustomerID";
        const string COLUMNS = "CompanyName, ContactName, ContactTitle, Address, City, Region, PostalCode, Country, Phone, Fax";
        const string SQL_GET_ALL = @"SELECT " + PK + ", " + COLUMNS + " FROM Customers";
        const string SQL_GET_BY_ID = SQL_GET_ALL + " WHERE " + PK + "=";
        const string SQL_INSERT = "INSERT INTO Customers (" + PK + ", " + COLUMNS + ") OUTPUT INSERTED." + PK + " VALUES ";
        const string SQL_DELETE = "DELETE FROM Customers WHERE " + PK +" = ";
        const string SQL_UPDATE = "UPDATE Customers SET CompanyName={1}, ContactName={2}, ContactTitle={3}, Address={4}, City={5}, Region={6}, PostalCode={7}, Country={8}, Phone={9}, Fax={10} WHERE " + PK + " = {0}";

        public CustomerDataMapper(Type klass, string connStr, bool withCache) : base(klass,connStr,false)
        {
        }
        /*
        protected override string SqlGetAll()
        {
            return SQL_GET_ALL;
        }
        */
        protected override string SqlGetById(object id)
        {
            return SQL_GET_BY_ID + "'" + id + "'";
        }
        
        protected override object Load(IDataReader dr)
        {
            Customer c = new Customer();
            c.CustomerID = (string)dr["CustomerID"];
            c.CompanyName = (string)dr["CompanyName"];
            c.ContactName = (string)dr["ContactName"];
            c.ContactTitle = (string)dr["ContactTitle"];
            c.Address = (string)dr["Address"];
            c.City = (string)dr["City"];
            c.Region = dr["Region"] as string;
            c.PostalCode = dr["PostalCode"] as string;
            c.Country = (string)dr["Country"];
            c.Phone = (string)dr["Phone"];
            c.Fax = dr["Fax"] as string;
            return c;
        }

        protected override string SqlInsert(object target)
        {
            Customer c = (Customer)target;
            string values = "'" + c.CustomerID + "' , '" + c.CompanyName + "' , '"
                + c.ContactName + "' , '" + c.ContactTitle + "' , '" + c.Address + "' , '"
                + c.City + "' , '" + c.Region + "' , '" + c.PostalCode + "' , '"
                + c.Country + "' , '" + c.Phone + "' , '" + c.Fax + "'";
            return SQL_INSERT + "(" + values + ")";
        }

        protected override string SqlUpdate(object target)
        {
            Customer c = (Customer)target;
            return String.Format(SQL_UPDATE,
                "'" + c.CustomerID + "'",
                "'" + c.CompanyName + "'",
                "'" + c.ContactName + "'",
                "'" + c.ContactTitle + "'",
                "'" + c.Address + "'",
                "'" + c.City + "'",
                "'" + c.Region + "'",
                "'" + c.PostalCode + "'",
                "'" + c.Country + "'",
                "'" + c.Phone + "'",
                "'" + c.Fax + "'");
        }

        protected override string SqlDelete(object target)
        {
            Customer c = (Customer)target;
            return SQL_DELETE + "'" + c.CustomerID + "'";
        }
    }
}