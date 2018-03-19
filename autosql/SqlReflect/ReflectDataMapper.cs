using SqlReflect.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace SqlReflect
{
    public class ReflectDataMapper : AbstractDataMapper
    {
        Type klass;

        string TABLE_NAME = "";
        string PRIMARY_KEY = "";
        string SQL_GET_ALL = "";
        string SQL_GET_BY_ID = "";
        string SQL_INSERT = "";
        string SQL_DELETE = "";
        string SQL_UPDATE = "";
        

        public ReflectDataMapper(Type klass, string connStr) : base(connStr)
        {
            this.klass = klass;
            
            PropertyInfo[] pi = klass.GetProperties();

            Attribute att = klass.GetCustomAttribute(typeof(TableAttribute));
            TABLE_NAME = att.GetType().GetProperty("Name").GetValue(att).ToString();

            PRIMARY_KEY = "";
            string COLUMNS = "";
            string[] columns_arr = new string[pi.Length - 1];
            for(int i = 0; i < pi.Length; i++)
            {
                string col = pi[i].Name;
                if (col.Equals(klass.Name + "ID"))
                    PRIMARY_KEY = col;
                else
                {
                    columns_arr[i-1] = col;
                    COLUMNS += col;
                    if (i < pi.Length - 1) COLUMNS += ", ";
                }
            }
            
            SQL_GET_ALL = "SELECT " + PRIMARY_KEY + ", " + COLUMNS + " FROM " + TABLE_NAME;
            SQL_GET_BY_ID = SQL_GET_ALL + " WHERE " + PRIMARY_KEY + " = ";
            SQL_INSERT = "INSERT INTO " + TABLE_NAME + " (" + COLUMNS + ") OUTPUT INSERTED." + PRIMARY_KEY + " VALUES ";
            SQL_DELETE = "DELETE FROM " + TABLE_NAME + " WHERE " + PRIMARY_KEY + " = ";

            SQL_UPDATE = "UPDATE " + TABLE_NAME + " SET ";
            
        }

        protected override object Load(SqlDataReader dr)
        {
            Object obj = Activator.CreateInstance(klass);
            PropertyInfo[] PIarr = klass.GetProperties();

            foreach(PropertyInfo p in PIarr)
            {
                p.SetValue(obj, dr[p.Name]);
            }
            return obj;
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
            PropertyInfo[] pi = target.GetType().GetProperties();

            string values = "";
            for(int i = 0; i < pi.Length; i++)
            {
                string val = pi[i].GetValue(target).ToString();
                if(!pi[i].Name.Equals(PRIMARY_KEY))
                    values += " '" + pi[i].GetValue(target) + "' " + ((i < pi.Length - 1) ? ", " : "");
            }
            return SQL_INSERT + "(" + values + ")";
        }

        protected override string SqlDelete(object target)
        {
            PropertyInfo[] pi = target.GetType().GetProperties();

            string PKvalue = "";
            for (int i = 0; i < pi.Length; i++)
            {
                if (pi[i].Name.Equals(PRIMARY_KEY))
                {
                    PKvalue = pi[i].GetValue(target).ToString();
                    break;
                }
            }

            return SQL_DELETE + PKvalue;
        }

        protected override string SqlUpdate(object target)
        {
            PropertyInfo[] pi = target.GetType().GetProperties();
            PropertyInfo[] columns = klass.GetProperties();


            string PKvalue = "";
            string SETvalues = "";
            for (int i = 0; i < pi.Length && i < columns.Length; i++)
            {
                string colName = columns[i].Name, colValue = pi[i].GetValue(target).ToString();

                if (colName.Equals(PRIMARY_KEY))
                {
                    PKvalue = colValue;
                }
                else
                {
                    SETvalues += colName + "= '" + colValue + "'";
                    SETvalues += i < pi.Length - 1 ? ", " : "";
                }
            }
            return SQL_UPDATE + SETvalues + " WHERE " + PRIMARY_KEY + " = " + PKvalue;
        }
    }

/*
UPDATE Categories SET CategoryName='Beverages', Description='Soft drinks, coffees, teas, beers, and ales' WHERE CategoryID = 1
UPDATE Categories SET CategoryName='Condiments', Description='Sweet and savory sauces, relishes, spreads, and seasonings' WHERE CategoryID = 2
UPDATE Categories SET CategoryName='Confections', Description='Desserts, candies, and sweet breads' WHERE CategoryID = 3
UPDATE Categories SET CategoryName='Dairy Products', Description='Cheeses' WHERE CategoryID = 4
UPDATE Categories SET CategoryName='Grains/Cereals', Description='Breads, crackers, pasta, and cereal' WHERE CategoryID = 5
UPDATE Categories SET CategoryName='Meat/Poultry', Description='Prepared meats' WHERE CategoryID = 6
UPDATE Categories SET CategoryName='Produce	Dried', Description='Dried fruit and bean curd' WHERE CategoryID = 7
UPDATE Categories SET CategoryName='Seafood', Description='Seaweed and fish' WHERE CategoryID = 8
UPDATE Categories SET CategoryName='Cookies', Description='Digestive' WHERE CategoryID = 34
*/

}