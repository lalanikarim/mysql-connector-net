// Copyright � 2004, 2010, Oracle and/or its affiliates. All rights reserved.
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
using System.Data;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using System.Data.Common;

namespace MySql.Data.MySqlClient.Tests
{
  [TestFixture]
  public class InterfaceTests : BaseTest
  {
#if !CF
    [Test]
    public void ClientFactory()
    {
      DbProviderFactory f = new MySqlClientFactory();
      using (DbConnection c = f.CreateConnection())
      {
        DbConnectionStringBuilder cb = f.CreateConnectionStringBuilder();
        cb.ConnectionString = GetConnectionString(true);
        c.ConnectionString = cb.ConnectionString;
        c.Open();

        DbCommand cmd = f.CreateCommand();
        cmd.Connection = c;
        cmd.CommandText = "SELECT 1";
        cmd.CommandType = CommandType.Text;
        using (DbDataReader reader = cmd.ExecuteReader())
        {
          reader.Read();
        }
      }
    }
#endif
  }
}
