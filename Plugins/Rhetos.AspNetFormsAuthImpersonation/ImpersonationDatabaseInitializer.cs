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

using Rhetos.AspNetFormsAuth;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Rhetos.AspNetFormsAuthImpersonation
{
    // Executes at deployment-time.
    [Export(typeof(Rhetos.Extensibility.IServerInitializer))]
    public class ImpersonationDatabaseInitializer : Rhetos.Extensibility.IServerInitializer
    {
        private readonly GenericRepositories _repositories;

        public ImpersonationDatabaseInitializer(
            GenericRepositories repositories,
            ILogProvider logProvider)
        {
            _repositories = repositories;
        }

        public void Initialize()
        {
            // Admin role should already be created in AuthenticationDatabaseInitializer, see Dependencies property.
            var adminRole = _repositories.Load<IRole>(role => role.Name == AuthenticationDatabaseInitializer.AdminRoleName).Single();

            foreach (var securityClaim in ImpersonationServiceClaims.GetDefaultAdminClaims())
            {
                var commonClaim = _repositories.CreateInstance<ICommonClaim>();
                commonClaim.ClaimResource = securityClaim.Resource;
                commonClaim.ClaimRight = securityClaim.Right;
                _repositories.InsertOrReadId(commonClaim, item => new { item.ClaimResource, item.ClaimRight });

                var permission = _repositories.CreateInstance<IRolePermission>();
                permission.RoleID = adminRole.ID;
                permission.ClaimID = commonClaim.ID;
                permission.IsAuthorized = true;
                _repositories.InsertOrUpdateReadId(permission, item => new { item.RoleID, item.ClaimID }, item => item.IsAuthorized);
            }
        }

        public IEnumerable<string> Dependencies
        {
            get { return new[] { typeof(AuthenticationDatabaseInitializer).FullName }; }
        }
    }
}
