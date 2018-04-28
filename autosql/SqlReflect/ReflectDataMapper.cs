using SqlReflect.Attributes;
using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace SqlReflect
{
    public class ReflectDataMapper : AbstractDataMapper
    {
        Type klass;

        readonly string TABLE_NAME;
        readonly string PRIMARY_KEY;
        readonly string SQL_GET_ALL;
        readonly string SQL_GET_BY_ID;
        readonly string SQL_INSERT;
        readonly string SQL_DELETE;
        readonly string SQL_UPDATE;

        string connStr;

        public ReflectDataMapper(Type klass, string connStr, bool withCache) : base(connStr, withCache)
            //: this(klass,connStr)     //for now
        {
        }

        public ReflectDataMapper(Type klass, string connStr) : base(connStr,false)
        {
            this.klass = klass;
            this.connStr = connStr;
            
            PropertyInfo[] pi = klass.GetProperties();

            Attribute tableAtt = klass.GetCustomAttribute(typeof(TableAttribute));
            TABLE_NAME = tableAtt.GetType().GetProperty("Name").GetValue(tableAtt).ToString();
            
            string COLUMNS = "";

            foreach (PropertyInfo p in pi)
            {
                string col = p.Name + 
                    ( (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string)) ? "" : "ID" );

                Attribute pkAtt = p.GetCustomAttribute(typeof(PKAttribute));

                if (pkAtt != null)
                    PRIMARY_KEY = p.Name;
                else
                    COLUMNS += col + ", ";
            }
            
            COLUMNS = COLUMNS.Remove(COLUMNS.Length - 2);
            
            SQL_GET_ALL = "SELECT " + PRIMARY_KEY + ", " + COLUMNS + " FROM " + TABLE_NAME;
            SQL_GET_BY_ID = SQL_GET_ALL + " WHERE " + PRIMARY_KEY + " = ";
            SQL_INSERT = "INSERT INTO " + TABLE_NAME + " (" + COLUMNS + ") OUTPUT INSERTED." + PRIMARY_KEY + " VALUES ";
            SQL_DELETE = "DELETE FROM " + TABLE_NAME + " WHERE " + PRIMARY_KEY + " = ";

            SQL_UPDATE = "UPDATE " + TABLE_NAME + " SET ";
            
        }

        protected override object Load(IDataReader dr)
        {
            Object obj = Activator.CreateInstance(klass);

            PropertyInfo[] PIarr = klass.GetProperties();

            foreach(PropertyInfo pi in PIarr)
            {
                if (pi.PropertyType.IsPrimitive || pi.PropertyType == typeof(string))
                    pi.SetValue(obj, dr[pi.Name] is DBNull ? "NULL" : dr[pi.Name]);
                else
                {
                    IDataMapper dm = new ReflectDataMapper(pi.PropertyType,connStr);
                    PropertyInfo[] objPI = pi.PropertyType.GetProperties();

                    foreach(PropertyInfo p in objPI){
                        if (p.GetCustomAttribute(typeof(PKAttribute)) != null)
                            pi.SetValue(obj, dm.GetById(dr[p.Name]));
                    }
                    
                }
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
            for (int i = 0; i < pi.Length; i++)
            {
                object value = null;

                if (pi[i].PropertyType.IsPrimitive || pi[i].PropertyType == typeof(string))
                    value = pi[i].GetValue(target);
                else
                {
                    object obj = pi[i].GetValue(target);

                    PropertyInfo[] objPI = pi[i].PropertyType.GetProperties();
                    foreach (PropertyInfo p in objPI)
                    {
                        if (p.GetCustomAttribute(typeof(PKAttribute)) != null)
                        {
                            value = p.GetValue(obj);
                            break;
                        }
                    }
                }

                if (!pi[i].Name.Equals(PRIMARY_KEY))
                    values += "'" + value + "', ";
            }
            return SQL_INSERT + "(" + values.Remove(values.Length -2) + ")";
        }

        protected override string SqlDelete(object target)
        {
            PropertyInfo[] pi = target.GetType().GetProperties();

            string PKvalue = "";
            foreach (PropertyInfo p in pi)
            {
                if (p.Name.Equals(PRIMARY_KEY))
                {
                    PKvalue = p.GetValue(target).ToString();
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
                    SETvalues += colName + "= '" + colValue + "', ";
                }
            }
            return SQL_UPDATE + SETvalues.Remove(SETvalues.Length -2) + " WHERE " + PRIMARY_KEY + " = " + PKvalue;
        }
    }

}