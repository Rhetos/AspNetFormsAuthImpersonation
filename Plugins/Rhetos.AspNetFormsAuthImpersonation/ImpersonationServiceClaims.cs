/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Rhetos.Dsl;
using Rhetos.Extensibility;
using Rhetos.Processing;
using Rhetos.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Rhetos.AspNetFormsAuthImpersonation
{
    /// <summary>
    /// List of admin claims is provided by a IClaimProvider plugin, in order to automatically create the claims on Rhetos deployment.
    /// </summary>
    [Export(typeof(IClaimProvider))]
    [ExportMetadata(MefProvider.Implements, typeof(DummyCommandInfo))]
    public class ImpersonationServiceClaims : IClaimProvider
    {
        #region IClaimProvider implementation.

        public IList<Claim> GetRequiredClaims(ICommandInfo info)
        {
            return null;
        }

        public IList<Claim> GetAllClaims(IDslModel dslModel)
        {
            return GetDefaultAdminClaims().Concat(new[] { IncreasePermissionsClaim }).ToList();
        }

        #endregion

        public static IList<Claim> GetDefaultAdminClaims()
        {
            return new[] { ImpersonateClaim };
        }

        /// <summary>
        /// A user with this claim is allowed to impersonate another user (execute the web service method Impersonate).
        /// </summary>
        public static readonly Claim ImpersonateClaim = new Claim("AspNetFormsAuth.Impersonation", "Impersonate");

        /// <summary>
        /// A user with this claim is allowed to impersonate another user that has more permissions.
        /// </summary>
        public static readonly Claim IncreasePermissionsClaim = new Claim("AspNetFormsAuth.Impersonation", "IncreasePermissions");
    }

    public class DummyCommandInfo : ICommandInfo
    {
    }
}
