﻿// Copyright © 2012, Oracle and/or its affiliates. All rights reserved.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Antlr.Runtime;
using Antlr.Runtime.Tree;

namespace MySql.Parser.Tests
{
	[TestFixture]
	public class Select
	{
		[Test]
		public void SelectSimple()
		{
			MySQL51Parser.program_return r = Utility.ParseSql("select * from `t`");
            //CommonTree ct = r.Tree as CommonTree;
            //Assert.AreEqual( "select", ct.Text );
            //Assert.AreEqual( 2, ct.ChildCount );
            //Assert.AreEqual("into_from", ct.Children[1].Text.ToLower());

            //Assert.AreEqual( "from", ct.Children[ 1 ].GetChild( 0 ).Text );
            //Assert.AreEqual(1, ct.Children[1].GetChild(0).ChildCount);
            //Assert.AreEqual("TABLE", ct.Children[1].GetChild(0).GetChild(0).Text);
            //Assert.AreEqual(1, ct.Children[1].GetChild(0).GetChild(0).ChildCount);
            //Assert.AreEqual( "`t`", ct.Children[ 1 ].GetChild( 0 ).GetChild( 0 ).GetChild( 0 ).Text );

            //Assert.AreEqual( "COLUMNS", ct.Children[ 0 ].Text );
            //Assert.AreEqual( 1, ct.Children[ 0 ].ChildCount );
            //Assert.AreEqual( "SELECT_EXPR", ct.Children[ 0 ].GetChild( 0 ).Text );
            //Assert.AreEqual( 1, ct.Children[ 0 ].GetChild( 0 ).ChildCount );
            //Assert.AreEqual( "*", ct.Children[ 0 ].GetChild( 0 ).GetChild( 0 ).Text );
		}

		[Test]
		public void UnionTest()
		{
			Utility.ParseSql(@"(SELECT a FROM t1 WHERE a=10 AND B=1)
				UNION
				(SELECT a FROM t2 WHERE a=11 AND B=2)
				union all ( select 1, 2, 3, t.* from t1 straight_join t2 )
				");
		}

		[Test]
		public void CompoundOperatorsTest()
		{
			Utility.ParseSql("select a from t where ( a <= b )");
		}

		[Test]
		public void LimitTest()
		{
			Utility.ParseSql("select a, b, c from Table1 limit 100");
			Utility.ParseSql("select a, b, c from Table1 limit 5000, 6000");
			Utility.ParseSql("select a, b, c from Table1 where ( a = 1 ) limit 200 offset 100");
		}

		[Test]
		public void OrderByTest()
		{
			Utility.ParseSql( "select a, b, c from Table1 order by c asc, b desc, 2 asc" );
			Utility.ParseSql("select a, b, c from Table1 order by null");
		}

		[Test]
		public void WhereTestBroken()
		{
			Utility.ParseSql("select a from", true);
		}

		[Test]
		public void WhereBetweenTest()
		{
			Utility.ParseSql("select a from `table` where `a` between 1 and 2");
		}

		[Test]
		public void WhereBetweenTest2()
		{
			Utility.ParseSql("select `a` from `table` where ( `a` between 1 and 2 )");
		}

		[Test]
		public void JoinTest()
		{
			Utility.ParseSql("select `a`, `b`, `c` from `tabA` inner join `tabB` on `tabA`.`KeyId` = `tabB`.`ForeignKeyId`;");
		}

		[Test]
		public void StarSimpleTest()
		{
			Utility.ParseSql("select * from TabA");
		}

		[Test]
		public void StarComplexTest()
		{
			Utility.ParseSql(@"select t1.*, t2.*, t3.*, * from t1 inner join t2 on t1.Id = t2.ParentId 
				  inner join t3 on t2.Id = t3.ParentId ");
		}

		[Test]
		public void SimpleCrossJoinWithoutOnTest()
		{
			Utility.ParseSql("select * from t1 cross join t2");
		}

		[Test]
		public void SimpleCrossJoinTest()
		{
			Utility.ParseSql("select * from t1 cross join t2 on t1.col1 = t2.col2");
		}

		[Test]
		public void SimpleInnerJoinWithoutOnTest()
		{
			Utility.ParseSql("select * from t1 inner join t2");
		}

		[Test]
		public void SimpleInnerJoinTest()
		{
			Utility.ParseSql("select * from t1 inner join t2 on t1.col1 = t2.col2");
		}

		[Test]
		public void SimpleStraightJoinWithoutOnTest()
		{
			Utility.ParseSql("select * from t1 straight_join t2");
		}

		[Test]
		public void SimpleStraightJoinTest()
		{
			Utility.ParseSql("select * from t1 straight_join t2 on t1.col1 = t2.col2");
		}

		[Test]
		public void SimpleLeftJoinTest()
		{
			Utility.ParseSql("select * from t1 left join t2 on t1.col1 = t2.col2");
		}

		[Test]
		public void SimpleRightJoinTest()
		{
			Utility.ParseSql("select * from t1 right join t2 on t1.col1 = t2.col2");
		}

		[Test]
		public void SimpleLeftOuterJoinTest()
		{
			Utility.ParseSql("select * from t1 left outer join t2 on t1.col1 = t2.col2");
		}

		[Test]
		public void SimpleRightOuterJoinTest()
		{
			Utility.ParseSql("select * from t1 right outer join t2 on t1.col1 = t2.col2");
		}

		[Test]
		public void SimpleNaturalJoinTest()
		{
			Utility.ParseSql("select * from t1 natural join t2");
		}

		[Test]
		public void SimpleNaturalLeftJoinTest()
		{
			Utility.ParseSql("select * from t1 natural left join t2");
		}

		[Test]
		public void SimpleNaturalLeftOuterJoinTest()
		{
			Utility.ParseSql("select * from t1 natural left outer join t2");
		}

		[Test]
		public void SimpleNaturalRightJoinTest()
		{
			Utility.ParseSql("select * from t1 natural right join t2");
		}

		[Test]
		public void SimpleNaturalRightOuterJoinTest()
		{
			Utility.ParseSql("select * from t1 natural right outer join t2");
		}

		[Test]
		public void SimpleInnerJoinSimplestOnConditionalTest()
		{
			Utility.ParseSql("select * from t1 inner join t2 on true");
		}

		[Test]
		public void MissingOnClausuleForJoinsTest()
		{
			Utility.ParseSql("select * from t1 left join t2", true);
			Utility.ParseSql("select * from t1 left join t2 ", true);
			Utility.ParseSql("select * from t1 right join t2 ", true);
			Utility.ParseSql("select * from t1 left outer join t2", true);
			Utility.ParseSql("select * from t1 right outer join t2", true);
		}

		[Test]
		public void WithoutFromTestTest()
		{
			Utility.ParseSql("select 1, 2, 3");
		}

		[Test]
		public void OdbcJoinTest()
		{
			Utility.ParseSql("select * from { oj TabA left outer join TabB on TabA.ID = TabB.ID }");
		}

		[Test]
		public void LikeConditionTest()
		{
			Utility.ParseSql("select * from t where a like 'ab'");
		}

		[Test]
		public void T()
		{
			Utility.ParseSql("select * from mysql.user limit 2;");
			//Utility.ParseSql("select * from t where a between b");
		}

        [Test]
        public void MissingTable()
        {
          StringBuilder sb;
          MySQL51Parser.program_return r =
            Utility.ParseSql("select * from", true, out sb);
        }
      /*
        [Test]
        public void Tx1()
        {
          StringBuilder sb;
          MySQL51Parser.program_return r =
            Utility.ParseSql(
            "select  \nfrom t; \n delete from tbl1", false, out sb);
        }

        [Test]
        public void MissingColumn()
        {
          MySQL51Parser.program_return r =
            Utility.ParseSql("select from t2, t3;", true);
          //Utility.ParseSql("select c1 from t2, t3;");
        }

        [Test]
        public void Tx2()
        {
          StringBuilder sb;
          MySQL51Parser.program_return r =
            Utility.ParseSql("delete from", false, out sb);
        }

        [Test]
        public void Tx3()
        {
          StringBuilder sb;
          MySQL51Parser.program_return r =
            Utility.ParseSql("select from table1 inner join table2 on true", true, out sb);
        }

        [Test]
        public void Tx4()
        {
          StringBuilder sb;
          MySQL51Parser.program_return r =
            Utility.ParseSql("select * from table1 inner join table2 on true", false, out sb);
        }

        [Test]
        public void Tx5()
        {
          // "select c, from table1 "
          // "insert into ta( )"
          // "insert into ta( "
          // "insert into ta"
          // "insert into ta ( c, d )"
          // "insert into ta( a,  "
          // "select always from t as x"
          // "select always from t where "           
          StringBuilder sb;
          MySQL51Parser.program_return r =
            Utility.ParseSql( "update t set c = 5 where  ", true, out sb);
        }

        
  */
        [Test]
        public void Tx6()
        {
          // "select c, from table1 "
          // "insert into ta( )"
          // "insert into ta( "
          // "insert into ta"
          // "insert into ta ( c, d )"           
          StringBuilder sb;
          MySQL51Parser.program_return r =
            //Utility.ParseSql("select * from t; --comment \r\n delete from a; call sp;",
          Utility.ParseSql(
            @"
select *, `fromtable`.`Id` from `fromtable` as f inner join `fromtable` on true where ( 1 = 1 );
delete from `order` where `order`.`Id` = 1;
update `facility` set `facility`.`name` = '' where `facility`.`Id` is null or `facility`.`Id` > 0;
insert into `computer`( `computer`.`Processor`, `computer`.`Model` ) values ( 1, 0 );
select `computer`.`Brand`  from `facility` as a;
",
            false, out sb);
        }

      [Test]
      public void Subquery()
      {
        StringBuilder sb;
        MySQL51Parser.program_return r =
          Utility.ParseSql("SELECT * FROM t1 WHERE column1 = (SELECT column1 FROM t2);", false, out sb);
      }
      
      [Test]
      public void Subquery2()
      {
        StringBuilder sb;
        MySQL51Parser.program_return r =
          Utility.ParseSql("SELECT *,(SELECT COUNT(*) FROM table2 WHERE table2.field1 = table1.id) AS count FROM table1 WHERE table1.field1 = 'value';", false, out sb);
      }
		/*
		 * TODO:
		 * Since it is legal ( select * from t ) limit 10, but illegal
		 * ((select * from t ) limit 20 ) limit 30
		 * This might need a semantic predicate to be resolved.
		 * */
		/*
		[Test]
		public void NestedLimitTest()
		{
			Utility.ParseSql("(SELECT * from T LIMIT 1) LIMIT 2;");
		}
		 * */

      [Test]
      public void WithPartition_55()
      {
        StringBuilder sb;
        MySQL51Parser.program_return r = Utility.ParseSql(
          @"SELECT * FROM employees PARTITION (p1);", true, out sb, new Version(5, 5));
        Assert.IsTrue(sb.ToString().IndexOf("missing EndOfFile at '('", StringComparison.OrdinalIgnoreCase) != -1);
      }

      [Test]
      public void WithPartition_56()
      {
        StringBuilder sb;
        MySQL51Parser.program_return r = Utility.ParseSql(
          @"SELECT * FROM employees PARTITION (p1);", false, out sb, new Version(5, 6));
      }

      [Test]
      public void WithPartition_2_56()
      {
        StringBuilder sb;
        MySQL51Parser.program_return r = Utility.ParseSql(
          @"SELECT * FROM employees PARTITION (p0, p2)
WHERE lname LIKE 'S%';", false, out sb, new Version(5, 6));
      }

      [Test]
      public void WithPartition_3_56()
      {
        StringBuilder sb;
        MySQL51Parser.program_return r = Utility.ParseSql(
          @"SELECT id, CONCAT(fname, ' ', lname) AS name 
    FROM employees PARTITION (p0) ORDER BY lname;", false, out sb, new Version(5, 6));
      }

      [Test]
      public void WithPartition_4_56()
      {
        StringBuilder sb;
        MySQL51Parser.program_return r = Utility.ParseSql(
          @"SELECT store_id, COUNT(department_id) AS c 
    FROM employees PARTITION (p1,p2,p3) 
    GROUP BY store_id HAVING c > 4;", false, out sb, new Version(5, 6));
      }

      [Test]
      public void WithPartition_5_56()
      {
        StringBuilder sb;
        MySQL51Parser.program_return r = Utility.ParseSql(
          @"SELECT id, CONCAT(fname, ' ', lname) AS name
    FROM employees_sub PARTITION (p2sp1);", false, out sb, new Version(5, 6));
      }

      [Test]
      public void WithPartition_6_56()
      {
        StringBuilder sb;
        MySQL51Parser.program_return r = Utility.ParseSql(
          @"SELECT
         e.id AS 'Employee ID', CONCAT(e.fname, ' ', e.lname) AS Name,
         s.city AS City, d.name AS department
     FROM employees AS e
         JOIN stores PARTITION (p1) AS s ON e.store_id=s.id
         JOIN departments PARTITION (p0) AS d ON e.department_id=d.id
     ORDER BY e.lname;", false, out sb, new Version(5, 6));
      }
  }
}
