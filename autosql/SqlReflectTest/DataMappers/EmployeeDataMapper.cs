using SqlReflect;
using SqlReflectTest.Model;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace SqlReflectTest.DataMappers
{
    class EmployeeDataMapper : DynamicDataMapper
    {

        const string PK = "EmployeeID";
        const string COLUMNS = "LastName,FirstName,Title,TitleOfCourtesy,Address,City,Region,PostalCode,Country,HomePhone,Extension";
        const string SQL_GET_ALL = @"SELECT " + PK + ", " + COLUMNS + " FROM Employees";
        const string SQL_GET_BY_ID = SQL_GET_ALL + " WHERE " + PK + "=";
        const string SQL_INSERT = "INSERT INTO Employees (" + PK + ", " + COLUMNS + ") OUTPUT INSERTED." + PK + " VALUES ";
        const string SQL_DELETE = "DELETE FROM Employees WHERE " + PK +" = ";
        const string SQL_UPDATE = "UPDATE Employees SET LastName={1}, FirstName={2} ,Title={3}, TitleOfCourtesy={4}, Address={5}, City={6} ,Region={7}, PostalCode={8}, Country={9}, HomePhone={10}, Extension={11} WHERE " + PK + " = {0}";

        public EmployeeDataMapper(Type klass, string connStr, bool withCache) : base(klass,connStr,false)
        {
        }

        protected override object Load(IDataReader dr)
        {
            Employee c = new Employee()
            {
                CustomerID = dr["CustomerID"] as string,
                CompanyName = dr["CompanyName"] as string,
                ContactName = dr["ContactName"] as string,
                ContactTitle = dr["ContactTitle"] as string,
                Address = dr["Address"] as string,
                City = dr["City"] as string,
                Region = dr["Region"] as string,
                PostalCode = dr["PostalCode"] as string,
                Country = dr["Country"] as string,
                Phone = dr["Phone"] as string,
                Fax = dr["Fax"] as string
            };
            return c;
        }

        protected override string SqlInsert(object target)
        {
            Employee c = (Employee)target;
            string values = String.Concat(new string[]{
                "'", c.CustomerID, "' , '", c.CompanyName, "' , '"
                , c.ContactName, "' , '", c.ContactTitle, "' , '", c.Address, "' , '"
                , c.City, "' , '", c.Region, "' , '", c.PostalCode, "' , '"
                , c.Country, "' , '", c.Phone, "' , '", c.Fax, "'"
            });
            return base.insertStmt + "(" + values + ")";
        }

        protected override string SqlUpdate(object target)
        {
            Employee c = (Employee)target;
            
            string setString = String.Concat(
                new string[] {
                    "CompanyName='", c.CompanyName, "', ",
                    "ContactName='" , c.ContactName, "', ",
                    "ContactTitle='", c.ContactTitle, "', ",
                    "Address='", c.Address, "', ",
                    "City='", c.City, "', ",
                    "Region='", c.Region, "', ",
                    "PostalCode='", c.PostalCode, "', ",
                    "Country='", c.Country, "', ",
                    "Phone='", c.Phone, "', ",
                    "Fax='", c.Fax, "'"
                }
            );

            return String.Format(base.updateStmt,
                    setString,
                    "'" + c.EmployeeID + "'"
                );
        }

        protected override string SqlDelete(object target)
        {
            Employee c = (Employee)target;
            return base.deleteStmt + "'" + c.EmployeeID + "'";
        }
    }
}
