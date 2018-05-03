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
        const string SQL_INSERT = "INSERT INTO Employees (" + COLUMNS + ") OUTPUT INSERTED." + PK + " VALUES ";
        const string SQL_DELETE = "DELETE FROM Employees WHERE " + PK +" = ";
        const string SQL_UPDATE = "UPDATE Employees SET LastName={1}, FirstName={2} ,Title={3}, TitleOfCourtesy={4}, Address={5}, City={6} ,Region={7}, PostalCode={8}, Country={9}, HomePhone={10}, Extension={11} WHERE " + PK + " = {0}";

        public EmployeeDataMapper(Type klass, string connStr, bool withCache) : base(klass,connStr,false)
        {
        }

        protected override object Load(IDataReader dr)
        {
            Employee e = new Employee()
            {
                EmployeeID = (int)dr["EmployeeID"],
                LastName = dr["LastName"] as string,
                FirstName = dr["FirstName"] as string,
                Title = dr["Title"] as string,
                TitleOfCourtesy = dr["TitleOfCourtesy"] as string,
                Address = dr["Address"] as string,
                City = dr["City"] as string,
                Region = dr["Region"] as string,
                PostalCode = dr["PostalCode"] as string,
                Country = dr["Country"] as string,
                HomePhone = dr["HomePhone"] as string,
                Extension = dr["Extension"] as string
            };
            return e;
        }

        protected override string SqlInsert(object target)
        {
            Employee e = (Employee)target;
            string values = String.Concat(new string[]{
                "'", e.LastName, "' , '", e.FirstName, "' , '"
                , e.Title, "' , '", e.TitleOfCourtesy, "' , '", e.Address, "' , '"
                , e.City, "' , '", e.Region, "' , '", e.PostalCode, "' , '"
                , e.Country, "' , '", e.HomePhone, "' , '", e.Extension, "'"
            });
            return base.insertStmt + "(" + values + ")";
        }

        protected override string SqlUpdate(object target)
        {
            Employee e = (Employee)target;
            
            string setString = String.Concat(
                new string[] {
                    "LastName='", e.LastName, "', ",
                    "FirstName='", e.FirstName, "', ",
                    "Title='" , e.Title, "', ",
                    "TitleOfCourtesy='", e.TitleOfCourtesy, "', ",
                    "Address='", e.Address, "', ",
                    "City='", e.City, "', ",
                    "Region='", e.Region, "', ",
                    "PostalCode='", e.PostalCode, "', ",
                    "Country='", e.Country, "', ",
                    "HomePhone='", e.HomePhone, "', ",
                    "Extension='", e.Extension, "'"
                }
            );

            return String.Format(base.updateStmt,
                    setString,
                    e.EmployeeID
                );
        }

        protected override string SqlDelete(object target)
        {
            Employee e = (Employee)target;
            return base.deleteStmt + e.EmployeeID;
        }
    }
}
