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
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;


namespace MySql.Data.Entity.CodeFirst.Tests
{
  public class VehicleDbContext : DbContext
  {
    public DbSet<Vehicle> Vehicles { get; set; }

    public VehicleDbContext()
    {
      Database.SetInitializer<VehicleDbContext>( new VehicleDBInitializer() );
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      //modelBuilder.Entity<Vehicle>()
      //    .Map<Car>(o => o.ToTable("Cars"))
      //    .Map<Bike>(o => o.ToTable("Bikes"));
      modelBuilder.Entity<Car>().ToTable("Cars");
      modelBuilder.Entity<Bike>().ToTable("Bikes");
    }
  }

  public class VehicleDBInitializer : DropCreateDatabaseReallyAlways<VehicleDbContext>
  { 
  }

  public class VehicleDbContext2 : DbContext
  {
    public DbSet<Vehicle2> Vehicles { get; set; }

    public VehicleDbContext2()
    {
      Database.SetInitializer<VehicleDbContext2>( new VehicleDBInitializer2() );
    }
  }

  public class VehicleDBInitializer2 : DropCreateDatabaseReallyAlways<VehicleDbContext2>
  { 
  }

  /// <summary>
  /// This initializer really drops the database, not just once per AppDomain (like the DropCreateDatabaseAlways).
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  public class DropCreateDatabaseReallyAlways<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
  { 
    public void InitializeDatabase(TContext context)
    {
      context.Database.Delete();
      context.Database.CreateIfNotExists();
      this.Seed(context);
      context.SaveChanges();
    }

    protected virtual void Seed(TContext context)
    {      
    }
  }

  public class Vehicle
  {
    public int Id { get; set; }
    public int Year { get; set; }
    [MaxLength(1024)]
    public string Name { get; set; }
  }

  public class VehicleDbContext3 : DbContext
  {
    public DbSet<Accessory> Accessories { get; set; }

  }

  public class Accessory
  {
    [Key]
    [MaxLength(255)]
    public string Name { get; set; }

    [Required]
    [MaxLength(80000)]
    public string Description { get; set; }

    [Required]
    [MaxLength(16777216)]
    public string LongDescription { get; set; }
  }

  public class Car : Vehicle
  {
    public string CarProperty { get; set; }
  }

  public class Bike : Vehicle
  {
    public string BikeProperty { get; set; }
  }
  public class Vehicle2
  {
    public int Id { get; set; }
    public int Year { get; set; }
    [MaxLength(1024)]
    public string Name { get; set; }
  }

  public class Car2 : Vehicle2
  {
    public string CarProperty { get; set; }
  }

  public class Bike2 : Vehicle2
  {
    public string BikeProperty { get; set; }
  }
}
