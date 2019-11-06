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

using Rhetos.Security;
using Rhetos.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Web;
using System.Web.Security;

namespace Rhetos.AspNetFormsAuthImpersonation
{
    [Export(typeof(IUserInfo))]
    public class AspNetImpersonationUserInfo : IUserInfo
    {
        #region IUserInfo implementation

        public bool IsUserRecognized => _isUserRecognized.Value;

        public string UserName
        {
            get
            {
                CheckIfUserRecognized();
                return _impersonatedUser.Value ?? _actualUser.Value;
            }
        }

        public string Workstation => _workstation.Value;

        public string Report()
        {
            return _impersonatedUser.Value != null
                ? (ReportActualUserNameOrAnonymous() + " as " + _impersonatedUser.Value + "," + _workstation.Value)
                : ReportActualUserNameOrAnonymous() + "," + _workstation.Value;
        }

        #endregion

        /// <summary>
        /// Returns null if there is no impersonation.
        /// If the current user is impersonating another, this property returns the actual (not impersonated) user that is logged in.
        /// </summary>
        public string ImpersonatedBy
        {
            get
            {
                CheckIfUserRecognized();
                return _impersonatedUser.Value != null ? _actualUser.Value : null;
            }
        }

        private readonly Lazy<bool> _isUserRecognized;

        private readonly Lazy<string> _workstation;

        /// <summary>
        /// The actual (not impersonated) user that is logged in.
        /// </summary>
        private readonly Lazy<string> _actualUser;

        /// <summary>
        /// The impersonated user whose context (including security permissions) is in effect.
        /// Null if there is no impersonation.
        /// </summary>
        private readonly Lazy<string> _impersonatedUser;

        public AspNetImpersonationUserInfo(IWindowsSecurity windowsSecurity)
        {
            _isUserRecognized = new Lazy<bool>(GetIsUserRecognized);
            _actualUser = new Lazy<string>(() => HttpContext.Current.User.Identity.Name);
            _impersonatedUser = new Lazy<string>(GetImpersonatedUser);
            _workstation = new Lazy<string>(() => windowsSecurity.GetClientWorkstation());
        }

        private static bool GetIsUserRecognized()
        {
            return HttpContext.Current != null
                && HttpContext.Current.User != null
                && HttpContext.Current.User.Identity != null
                && HttpContext.Current.User.Identity.IsAuthenticated;
        }

        private string GetImpersonatedUser()
        {
            // For any changes in this function's implementation, consider updating the "GetImpersonatedUser" code snippet in Readme.md.

            var formsIdentity = (HttpContext.Current.User.Identity as FormsIdentity);
            if (formsIdentity != null)
            {
                string userData = formsIdentity.Ticket.UserData;
                if (!string.IsNullOrEmpty(userData) && userData.StartsWith(ImpersonationService.ImpersonatingUserInfoPrefix))
                    return userData.Substring(ImpersonationService.ImpersonatingUserInfoPrefix.Length);
            }
            return null;
        }

        private void CheckIfUserRecognized()
        {
            if (!IsUserRecognized)
                throw new ClientException("User is not authenticated.");
        }

        private string ReportActualUserNameOrAnonymous() => IsUserRecognized ? _actualUser.Value : "<anonymous>";
    }
}
