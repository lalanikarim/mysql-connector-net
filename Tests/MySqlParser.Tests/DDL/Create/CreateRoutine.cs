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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Antlr.Runtime;
using Antlr.Runtime.Tree;


namespace MySql.Parser.Tests
{
  [TestFixture]
  public class CreateRoutine
  {
    [Test]
    public void SimpleProc()
    {
      string sql = @"
CREATE PROCEDURE simpleproc (OUT param1 INT)
    BEGIN
      SELECT COUNT(*) INTO param1 FROM t;
    END;";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void SimpleProc2()
    {
      string sql = @"
CREATE PROCEDURE dorepeat(p1 INT)
BEGIN
  SET @x = 0;
  REPEAT SET @x = @x + 1; UNTIL @x > p1 END REPEAT;
END;
";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void CreateProcWithSec()
    {
      string sql = @"CREATE DEFINER = 'admin'@'localhost' PROCEDURE account_count()
SQL SECURITY INVOKER
BEGIN
  SELECT 'Number of accounts:', COUNT(*) FROM mysql.user;
END;";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void ComplexProc()
    {
      string sql = @"
CREATE DEFINER=`root`@`localhost` PROCEDURE `add_error_log`(
`error_level` int(11),
`error_level_name` varchar(512),
`error_message` longtext,
`error_file` text,
`error_line` int(11),
`error_context` longtext,
`error_query_string` longtext,
`error_time` text ,
`user_id` int(11),
`post_data` longtext,
`user_msg` text)
BEGIN
INSERT INTO tbl_error_log(error_level, error_level_name, error_message, error_file,error_line, error_context,error_query_string,error_time, user_id, post_data, user_msg)
values(error_level, error_level_name, error_message, error_file,error_line, error_context,error_query_string, error_time, user_id, post_data, user_msg);
END;";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void CreateProcWithSec2()
    {
      string sql = @"create DEFINER=`root`@`localhost` PROCEDURE `spTest2`()
begin
    declare n,x,y,z int;
	declare str varchar(1100);
    set n = 1;
	set str = 'Armando';

    while n < 10 do
    begin
    
        set n = n + 1;
		set x = n * 2;
		set y = n * 5;
		set z = n * 10;
		set str = CONCAT(str, 'o');
    
    end;
    end while;

end;";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }
    

    [Test]
    public void SimpleFunc()
    {
      string sql = @"CREATE FUNCTION hello (s CHAR(20))
    RETURNS CHAR(50) DETERMINISTIC
    RETURN CONCAT('Hello, ',s,'!');";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void ComplexFunc()
    {
      string sql = @"CREATE FUNCTION fnGetXMLinfoVraag4 (xmlTag varchar(30),message text) returns varchar(255)
DETERMINISTIC
READS SQL DATA
begin
declare lenField int;
declare xmlTagBegin varchar(30);
declare xmlTagEnd varchar(30);
declare fieldresult varchar(255);
set xmlTagBegin = concat('<', xmlTag, '>');
set xmlTagEnd = concat('</', xmlTag, '>');
set lenField = length(xmlTag) + 2;
set fieldresult = case when locate(xmlTagBegin,message) = 0 then ''
else substring(message,locate(xmlTagBegin,message) + lenField,locate(xmlTagEnd,message) - (locate(xmlTagBegin,message) + lenField)) end case;
return fieldresult;
end";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void ComplexFunc2()
    {
      string sql = @"
CREATE FUNCTION db.fnfullname ( id smallint(5) unsigned ) RETURNS varchar(160) CHARACTER SET utf8
COMMENT 'Returns the full name of person in db.people table referenced by id where FirstName and FamilyName are not null but MiddleName may be null'
DETERMINISTIC
READS SQL DATA
BEGIN
DECLARE fulname varchar(160) CHARACTER SET utf8;
SELECT CONCAT_WS(' ', db.people.FirstName, db.people.MiddleName, db.people.FamilyName) into fulname from db.people where db.people.id=id;
RETURN fulname;
END;
";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void SimpleCompare()
    {
      string sql = @"CREATE FUNCTION SimpleCompare(n INT, m INT)
  RETURNS VARCHAR(20)

  BEGIN
    DECLARE s VARCHAR(20);

    IF n > m THEN SET s = '>';
    ELSEIF n = m THEN SET s = '=';
    ELSE SET s = '<';
    END IF;

    SET s = CONCAT(n, ' ', s, ' ', m);

    RETURN s;
  END";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void VerboseCompare()
    {
      string sql = @"CREATE FUNCTION VerboseCompare (n INT, m INT)
  RETURNS VARCHAR(50)

  BEGIN
    DECLARE s VARCHAR(50);

    IF n = m THEN SET s = 'equals';
    ELSE
      IF n > m THEN SET s = 'greater';
      ELSE SET s = 'less';
      END IF;

      SET s = CONCAT('is ', s, ' than');
    END IF;

    SET s = CONCAT(n, ' ', s, ' ', m, '.');

    RETURN s;
  END";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void WithoutName()
    {
      string sql = @"create procedure ( id int, name varchar( 10 ))
begin
	create table test3( id2 int );
	insert into test3 (1), (2), (3);
	# insert into test3 values (1), (2), (3);
end";
      StringBuilder sb;
      MySQL51Parser.program_return r =
        Utility.ParseSql(sql, true, out sb);
    }

    [Test]
    public void NameIsKeyword()
    {
      string sql = @"
CREATE DEFINER=`root`@`localhost` PROCEDURE `count`() 
BEGIN 
  DECLARE y varchar(50); 
  INSERT INTO world.d_table (`name`) VALUES (""Armando""); 
  INSERT INTO world.d_table (`name`) VALUES (""Elisa""); 
  select row_count() into y; 
  select found_rows() into y; 
  select last_insert_id() into y; 
END";
      StringBuilder sb;
      MySQL51Parser.program_return r = Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void DifferentDeclareOrders()
    {
      string sql = @"
CREATE DEFINER=`root`@`localhost` PROCEDURE `dohandler`() 
BEGIN 
  DECLARE dup_keys CONDITION FOR SQLSTATE '23000'; 
  DECLARE y varchar(50); 
  DECLARE CONTINUE HANDLER FOR dup_keys SET @GARBAGE = 1; 
  SET @x = 1; 
  INSERT INTO world.d_table (`name`) VALUES (""Armando""); 
  SET @x = 2; 
  INSERT INTO world.d_table (`name`) VALUES (""Elisa""); 
  set @x = 3; 
  select row_count() into y; 
  select found_rows() into y; 
  select last_insert_id() into y; 
END";
      StringBuilder sb;
      MySQL51Parser.program_return r = Utility.ParseSql(sql, false, out sb);
    }

    [Test]
    public void SetSession()
    {
      string sql = @"
create procedure spClientFiboGen( nMax int )
begin

    declare i int;
    declare myresult int;
    
    SET @@GLOBAL.max_sp_recursion_depth = 20;
    SET @@session.max_sp_recursion_depth = 20; 
    set i = 0;
    
    drop table if exists tblFibo;
    create table tblFibo( n int, fibo int );
    
    while i < nMax do    
        call spFiboGen( i, myresult );
        insert into tblFibo( n, fibo ) values ( i, myresult );
        set i = i + 1;
    end while;

end;";
      StringBuilder sb;
      MySQL51Parser.program_return r = Utility.ParseSql(sql, false, out sb);
    }
  }
}
