// Copyright � 2008, 2011, Oracle and/or its affiliates. All rights reserved.
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
using System.Threading;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using MySql.Data.MySqlClient.Tests;
using System.Data.EntityClient;
using System.Data.Common;
using System.Data.Objects;
using System.Linq;
using MySql.Data.Entity.Tests.Properties;
using System.Collections;

namespace MySql.Data.Entity.Tests
{
  [TestFixture]
  public class JoinTests : BaseEdmTest
  {
    [Test]
    public void SimpleJoin()
    {
      MySqlDataAdapter da = new MySqlDataAdapter(
          @"SELECT b.id,b.name,a.name as author_name from books b JOIN
                    authors a ON b.author_id=a.id", conn);
      DataTable dt = new DataTable();
      da.Fill(dt);

      using (testEntities context = new testEntities())
      {
        var q = from b in context.Books
                join a in context.Authors
                on b.Author.Id equals a.Id
                select new
                {
                  bookId = b.Id,
                  bookName = b.Name,
                  authorName = a.Name
                };

        string sql = q.ToTraceString();
        CheckSql(sql, SQLSyntax.SimpleJoin);

        int i = 0;
        foreach (var o in q)
          Assert.AreEqual(dt.Rows[i++][0], o.bookId);
        Assert.AreEqual(dt.Rows.Count, i);
      }
    }

    [Test]
    public void SimpleJoinWithPredicate()
    {
      MySqlDataAdapter da = new MySqlDataAdapter(
          @"SELECT b.id,b.name,a.name as author_name from books b JOIN
                    authors a ON b.author_id=a.id WHERE b.pages > 300", conn);
      DataTable dt = new DataTable();
      da.Fill(dt);

      using (testEntities context = new testEntities())
      {
        var q = from b in context.Books
                join a in context.Authors
                on b.Author.Id equals a.Id
                where b.Pages > 300
                select new
                {
                  bookId = b.Id,
                  bookName = b.Name,
                  authorName = a.Name
                };

        string sql = q.ToTraceString();
        CheckSql(sql, SQLSyntax.SimpleJoinWithPredicate);

        int i = 0;
        foreach (var o in q)
          Assert.AreEqual(dt.Rows[i++][0], o.bookId);
        Assert.AreEqual(dt.Rows.Count, i);
      }
    }

    [Test]
    public void JoinOnRightSideAsDerivedTable()
    {
      using (testEntities context = new testEntities())
      {
        var q = from child in context.Children
                join emp in context.Employees
                on child.EmployeeID equals emp.Id
                where child.BirthWeight > 7
                select child;
        string sql = q.ToTraceString();
        CheckSql(sql, SQLSyntax.JoinOnRightSideAsDerivedTable);

        foreach (Child c in q)
        {
        }
      }
    }

    [Test]
    public void JoinOfUnionsOnRightSideofJoin()
    {
      using (testEntities context = new testEntities())
      {
        string eSql = @"SELECT c.Id, c.Name, Union1.Id, Union1.Name, 
                                Union2.Id, Union2.Name FROM 
                                testEntities.Companies AS c JOIN (
                                ((SELECT t.Id, t.Name FROM testEntities.Toys as t) 
                                UNION ALL 
                                (SELECT s.Id, s.Name FROM testEntities.Shops as s)) AS Union1
                                JOIN 
                                ((SELECT a.Id, a.Name FROM testEntities.Authors AS a) 
                                UNION ALL 
                                (SELECT b.Id, b.Name FROM testEntities.Books AS b)) AS Union2
                                ON Union1.Id = Union2.Id) ON c.Id = Union1.Id";
        ObjectQuery<DbDataRecord> query = context.CreateQuery<DbDataRecord>(eSql);
        string sql = query.ToTraceString();
        CheckSql(sql, SQLSyntax.JoinOfUnionsOnRightSideOfJoin);
        foreach (DbDataRecord record in query)
        {
          Assert.AreEqual(6, record.FieldCount);
        }
      }
    }

    [Test]
    public void t()
    {
      using (testEntities context = new testEntities())
      {
        var q = from d in context.Computers
                join l in context.Computers on d.Id equals l.Id
                where (d is DesktopComputer)
                select new { d.Id, d.Brand };
        foreach (var o in q)
        {
        }
      }
    }

    /// <summary>
    /// Tests for bug http://bugs.mysql.com/bug.php?id=61729 
    /// (Skip/Take Clauses Causes Null Reference Exception in 6.3.7 and 6.4.1 Only).
    /// </summary>
    [Test]
    public void JoinOfNestedUnionsWithLimit()
    {
      using (testEntities context = new testEntities())
      {
        var q = context.Books.Include("Author");
        q = q.Include("Publisher");
        q = q.Include("Publisher.Books");
        string sql = q.ToTraceString();
        CheckSql(sql, SQLSyntax.JoinOfNestedUnionsWithLimit);
        foreach (var o in q.Where(p => p.Id > 0).OrderBy(p => p.Name).ThenByDescending(p => p.Id).Skip(0).Take(32).ToList())
        {
        }
      }
    }

    [Test]
    public void JoinOnRightSideNameClash()
    {
      using (testEntities context = new testEntities())
      {
        string eSql = @"SELECT c.Id, c.Name, a.Id, a.Name, b.Id, b.Name FROM
                                testEntities.Companies AS c JOIN (testEntities.Authors AS a
                                JOIN testEntities.Books AS b ON a.Id = b.Id) ON c.Id = a.Id";
        ObjectQuery<DbDataRecord> query = context.CreateQuery<DbDataRecord>(eSql);
        string sql = query.ToTraceString();
        CheckSql(sql, SQLSyntax.JoinOnRightSideNameClash);
        foreach (DbDataRecord record in query)
        {
          Assert.AreEqual(6, record.FieldCount);
        }
      }
    }

    /// <summary>
    /// Test for fix of Oracle bug 12807366.
    /// </summary>
    [Test]
    public void JoinsAndConcatsWithComposedKeys()
    {
      using (testEntities1 ctx = new testEntities1())
      {
        IQueryable<gamingplatform> l2 = ctx.gamingplatform.Where(p => string.IsNullOrEmpty(p.Name)).Take(10);
        IQueryable<gamingplatform> l = ctx.gamingplatform.Where(p => string.IsNullOrEmpty(p.Name)).Take(10);
        var l4 = ctx.gamingplatform.Where(p => string.IsNullOrEmpty(p.Name)).Take(10);
        l = l.Concat(l4);
        l = l.Concat(ctx.gamingplatform.Where(p => string.IsNullOrEmpty(p.Name)).Take(10).Distinct());

        IQueryable<gamingplatform> q = (from i in l join i2 in l2 on i.Id equals i2.Id select i);
        IQueryable<videogameplatform> l3 = from t1 in q
                                           join t2 in q.SelectMany(p => p.videogameplatform)
                                               on t1.Id equals t2.IdGamingPlatform
                                           select t2;
        videogameplatform pu = null;

        pu = l3.FirstOrDefault();
      }
    }
    
    /// <summary>
    /// Test to fix Oracle bug 13491698
    /// </summary>
    [Test]
    public void CanIncludeWithEagerLoading()
    {
      var myarray = new ArrayList();
      using (var db = new mybooksEntities())
      {
        var author = db.myauthors.Include("mybooks.myeditions").AsEnumerable().First();
        var strquery = ((ObjectQuery)db.myauthors.Include("mybooks.myeditions").AsEnumerable()).ToTraceString();
        CheckSql(strquery, SQLSyntax.JoinUsingInclude);
        foreach (var book in author.mybooks.ToList())
        {
          foreach (var edition in book.myeditions.ToList())
          {
            myarray.Add(edition.Title);
          }
        }
        myarray.Sort();
        Assert.AreEqual(0, myarray.IndexOf("Another Book First Edition"));
        Assert.AreEqual(1, myarray.IndexOf("Another Book Second Edition"));
        Assert.AreEqual(2, myarray.IndexOf("Another Book Third Edition"));
        Assert.AreEqual(3, myarray.IndexOf("Some Book First Edition"));
        Assert.AreEqual(myarray.Count, 4);
      }
    }
  }
}