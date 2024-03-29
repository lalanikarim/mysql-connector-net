﻿// Copyright © 2008, 2011, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Data;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Text;

namespace MySql.Data.Entity
{
  class EFMySqlDataReader : DbDataReader
  {
    private EFMySqlCommand command;
    private MySqlDataReader reader;
    private PrimitiveType[] types;

    public EFMySqlDataReader(EFMySqlCommand cmd, MySqlDataReader wrappedReader)
    {
      command = cmd;
      reader = wrappedReader;
      types = command.ColumnTypes;
    }

    #region Properties

    public override int Depth
    {
      get { return reader.Depth; }
    }

    public override int FieldCount
    {
      get { return reader.FieldCount; }
    }

    public override bool HasRows
    {
      get { return reader.HasRows; }
    }

    public override bool IsClosed
    {
      get { return reader.IsClosed; }
    }

    public override int RecordsAffected
    {
      get { return reader.RecordsAffected; }
    }

    public override object this[string name]
    {
      get { return GetValue(GetOrdinal(name)); }
    }

    public override object this[int ordinal]
    {
      get { return GetValue(ordinal); }
    }

    #endregion

    public override void Close()
    {
      GC.SuppressFinalize(this);
      reader.Close();
    }

    public override bool GetBoolean(int ordinal)
    {
      return (bool)GetValue(ordinal);
    }

    public override byte GetByte(int ordinal)
    {
      return reader.GetByte(ordinal);
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
    {
      return reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
    }

    public override char GetChar(int ordinal)
    {
      return reader.GetChar(ordinal);
    }

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
    {
      return reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
    }

    public override string GetDataTypeName(int ordinal)
    {
      if (types != null)
        return types[ordinal].Name;
      return reader.GetDataTypeName(ordinal);
    }

    public override DateTime GetDateTime(int ordinal)
    {
      return reader.GetDateTime(ordinal);
    }

    public override decimal GetDecimal(int ordinal)
    {
      return reader.GetDecimal(ordinal);
    }

    public override double GetDouble(int ordinal)
    {
      return reader.GetDouble(ordinal);
    }

    public override IEnumerator GetEnumerator()
    {
      return reader.GetEnumerator();
    }

    public override Type GetFieldType(int ordinal)
    {
      if (types != null)
        return types[ordinal].ClrEquivalentType;
      return reader.GetFieldType(ordinal);
    }

    public override float GetFloat(int ordinal)
    {
      return reader.GetFloat(ordinal);
    }

    public override Guid GetGuid(int ordinal)
    {
      return reader.GetGuid(ordinal);
    }

    public override short GetInt16(int ordinal)
    {
      return reader.GetInt16(ordinal);
    }

    public override int GetInt32(int ordinal)
    {
      return reader.GetInt32(ordinal);
    }

    public override long GetInt64(int ordinal)
    {
      return reader.GetInt64(ordinal);
    }

    public override string GetName(int ordinal)
    {
      return reader.GetName(ordinal);
    }

    public override int GetOrdinal(string name)
    {
      return reader.GetOrdinal(name);
    }

    public override DataTable GetSchemaTable()
    {
      return reader.GetSchemaTable();
    }

    public override string GetString(int ordinal)
    {
      return reader.GetString(ordinal);
    }

    public override object GetValue(int ordinal)
    {
      object value = reader.GetValue(ordinal);

      if (types != null)
      {
        if (!(value is DBNull) && value.GetType()
          != types[ordinal].ClrEquivalentType)
          value = ChangeType(value, types[ordinal].ClrEquivalentType);
      }
      return value;
    }

    private object ChangeType(object sourceValue, Type targetType)
    {
      if (sourceValue is byte[])
      {
        if (targetType == typeof(Guid))
          return new Guid((byte[])sourceValue);
        else if (targetType == typeof(bool))
        {
          byte[] bytes = (byte[])sourceValue;
          return bytes[0] == '1';
        }
      }

      if (sourceValue is DateTime && targetType == typeof(DateTimeOffset))
      {
        return new DateTimeOffset((DateTime)sourceValue);
      }

      return Convert.ChangeType(sourceValue, targetType, CultureInfo.InvariantCulture);
    }

    public override int GetValues(object[] values)
    {
      for (int i = 0; i < values.Length; ++i)
        values[i] = GetValue(i);
      return values.Length;
    }

    public override bool IsDBNull(int ordinal)
    {
      return reader.IsDBNull(ordinal);
    }

    public override bool NextResult()
    {
      return reader.NextResult();
    }

    public override bool Read()
    {
      return reader.Read();
    }

    protected override void Dispose(bool disposing)
    {
      GC.SuppressFinalize(this);
      if (disposing)
        reader.Dispose();
      base.Dispose(disposing);
    }

  }
}
