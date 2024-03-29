﻿// Copyright © 2004, 2010, Oracle and/or its affiliates. All rights reserved.
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

//  This code was contributed by Sean Wright (srwright@alcor.concordia.ca) on 2007-01-12
//  The copyright was assigned and transferred under the terms of
//  the MySQL Contributor License Agreement (CLA)

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Profile;
using System.Web.Security;

namespace MySql.Web.Tests
{
  public class TestProfile : ProfileBase
  {
    public static TestProfile GetUserProfile(string username, bool auth)
    {
      return Create(username, auth) as TestProfile;
    }

    public static TestProfile GetUserProfile(bool auth)
    {
      return Create(Membership.GetUser().UserName, auth) as TestProfile;
    }

    [SettingsAllowAnonymous(false)]
    public string Description
    {
      get { return base["Description"] as string; }
      set { base["Description"] = value; }
    }

    [SettingsAllowAnonymous(false)]
    public string Location
    {
      get { return base["Location"] as string; }
      set { base["Location"] = value; }
    }

    [SettingsAllowAnonymous(false)]
    public string FavoriteMovie
    {
      get { return base["FavoriteMovie"] as string; }
      set { base["FavoriteMovie"] = value; }
    }
  }
}
