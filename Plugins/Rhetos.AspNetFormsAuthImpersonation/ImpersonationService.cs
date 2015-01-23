using Rhetos.AspNetFormsAuth;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Logging;
using Rhetos.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Rhetos.AspNetFormsAuthImpersonation
{
    #region Service parameters

    public class ImpersonateParameters
    {
        public string ImpersonatedUser { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ImpersonatedUser))
                throw new UserException("Empty ImpersonatedUser is not allowed.");
        }
    }

    #endregion

    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class ImpersonationService
    {
        private readonly ILogger _logger;
        private readonly Lazy<IAuthorizationManager> _authorizationManager;
        private readonly Lazy<GenericRepository<IPrincipal>> _principals;
        private readonly Lazy<GenericRepository<IPermission>> _permissions;
        private readonly Lazy<GenericRepository<ICommonClaim>> _claims;
        private readonly Lazy<AspNetFormsAuthorizationProvider> _authorizationProvider;

        public ImpersonationService(
            ILogProvider logProvider,
            Lazy<AuthenticationService> authenticationService,
            Lazy<IAuthorizationManager> authorizationManager,
            Lazy<GenericRepository<IPrincipal>> principals,
            Lazy<GenericRepository<IPermission>> permissions,
            Lazy<GenericRepository<ICommonClaim>> claims,
            Lazy<IAuthorizationProvider> authorizationProvider)
        {
            _logger = logProvider.GetLogger(GetType().Name);
            _authorizationManager = authorizationManager;
            _principals = principals;
            _permissions = permissions;
            _claims = claims;
            _authorizationProvider = new Lazy<AspNetFormsAuthorizationProvider>(() => (AspNetFormsAuthorizationProvider)authorizationProvider.Value);
        }

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/Impersonate", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Impersonate(ImpersonateParameters parameters)
        {
            if (parameters == null)
                throw new ClientException("It is not allowed to call this service method with no parameters provided.");
            _logger.Trace(() => "Impersonate: " + HttpContext.Current.User.Identity.Name + " as " + parameters.ImpersonatedUser);
            CheckCurrentUserClaim(ImpersonationServiceClaims.ImpersonateClaim);
            parameters.Validate();

            // Update the existing security cookie (from Request).
            var authenticationCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            ActivateImpersonation(parameters.ImpersonatedUser, authenticationCookie, limitImpersonationToSingleSession: true);
            HttpContext.Current.Response.Cookies.Add(authenticationCookie);
        }

        /// <summary>
        /// Checks the user's permissions, and sets the impersonation info in the authentication cookie.
        /// </summary>
        private void ActivateImpersonation(string impersonatedUser, HttpCookie authenticationCookie, bool limitImpersonationToSingleSession)
        {
            CheckImperionatedUserPermissions(impersonatedUser);
            UpdateAuthTicketToImpersonate(authenticationCookie, impersonatedUser, limitImpersonationToSingleSession);
        }

        private void CheckCurrentUserClaim(Claim claim)
        {
            bool allowedImpersonate = _authorizationManager.Value.GetAuthorizations(new[] { claim }).Single();
            if (!allowedImpersonate)
                throw new UserException(string.Format(
                    "You are not authorized for action '{0}' on resource '{1}'. The required security claim is not set.",
                    claim.Right, claim.Resource));
        }

        private void CheckImperionatedUserPermissions(string impersonatedUser)
        {
            var impersonatedPrincipalId = _principals.Value
                .Query(p => p.Name == impersonatedUser)
                .Select(p => p.ID).SingleOrDefault();

            // This function must be called after the user is authenticated and authorized (see CheckCurrentUserClaim),
            // otherwise the provided error information would be a security issue.
            if (impersonatedPrincipalId == default(Guid))
                throw new UserException("User '" + impersonatedUser + "' is not registered.");

            var allowIncreasePermissions = _authorizationManager.Value.GetAuthorizations(new[] { ImpersonationServiceClaims.IncreasePermissionsClaim }).Single();
            if (!allowIncreasePermissions)
            {
                // The impersonatedUser must have subset of permissions of the impersonating user.
                // It is not allowed to impersonate a user with more permissions then the impersonating user.

                var currentRoles = _authorizationProvider.Value.GetUsersRoles(HttpContext.Current.User.Identity.Name);
                var impersonatedRoles = _authorizationProvider.Value.GetUsersRoles(impersonatedUser);

                var currentClaims = _permissions.Value
                    .Query(p => currentRoles.Contains(p.Role.ID))
                    .Where(p => p.Claim.Active.Value)
                    .Select(p => p.Claim.ID);

                var surplusImpersonatedClaims = _permissions.Value
                    .Query(p => impersonatedRoles.Contains(p.Role.ID) && !currentClaims.Contains(p.Claim.ID))
                    .Where(p => p.Claim.Active.Value)
                    .Select(p => p.Claim.ID)
                    .ToList();

                if (surplusImpersonatedClaims.Count() > 0)
                {
                    string sampleClaim = _claims.Value.Query(new[] { surplusImpersonatedClaims.First() })
                        .Select(c => c.ClaimResource + "." + c.ClaimRight)
                        .SingleOrDefault();

                    _logger.Info(
                        "User '{0}' is not allowed to impersonate '{1}' because the impersonated user has {2} more security claims (for example '{3}'). Increase the user's permissions or add '{4}' security claim.",
                        HttpContext.Current.User.Identity.Name,
                        impersonatedUser,
                        surplusImpersonatedClaims.Count(),
                        sampleClaim,
                        ImpersonationServiceClaims.IncreasePermissionsClaim.FullName);

                    throw new UserException("You are not allowed to impersonate user '" + impersonatedUser + "'.", "See server log for more information.");
                }
            }
        }

        public const string ImpersonatingUserInfoPrefix = "Impersonating:";

        /// <summary>
        /// This function is updating previously generated security, instead of creating a new one,
        /// to make sure that the proper ticket and cookie settings are used (Expiration, HttpOnly, ...).
        /// </summary>
        private void UpdateAuthTicketToImpersonate(HttpCookie authenticationCookie, string impersonatedUser, bool limitImpersonationToSingleSession)
        {
            if (authenticationCookie.Value == null)
                throw new FrameworkException("There is no authentication cookie created.");

            var authenticationTicket = FormsAuthentication.Decrypt(authenticationCookie.Value);

            if (!string.IsNullOrEmpty(authenticationTicket.UserData) && !authenticationTicket.UserData.StartsWith(ImpersonatingUserInfoPrefix))
                throw new FrameworkException("Login impersonation plugin is not supported (" + GetType().FullName + "). The authentication ticket already has the UserData property set.");

            var newTicket = new FormsAuthenticationTicket(
                authenticationTicket.Version,
                authenticationTicket.Name,
                authenticationTicket.IssueDate,
                authenticationTicket.Expiration,
                limitImpersonationToSingleSession ? false : authenticationTicket.IsPersistent,
                ImpersonatingUserInfoPrefix + impersonatedUser,
                authenticationTicket.CookiePath);

            authenticationCookie.Value = FormsAuthentication.Encrypt(newTicket);

            if (limitImpersonationToSingleSession)
                authenticationCookie.Expires = default(DateTime);
        }

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/StopImpersonating", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void StopImpersonating()
        {
            _logger.Trace(() => "StopImpersonating: " + HttpContext.Current.User.Identity.Name);

            var authenticationCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            UpdateAuthTicketToStopImpersonating(authenticationCookie);
            HttpContext.Current.Response.Cookies.Add(authenticationCookie);
        }

        private void UpdateAuthTicketToStopImpersonating(HttpCookie authenticationCookie)
        {
            if (authenticationCookie.Value == null)
                return; // Ignore if not logged-in.

            var authenticationTicket = FormsAuthentication.Decrypt(authenticationCookie.Value);

            if (string.IsNullOrEmpty(authenticationTicket.UserData) || !authenticationTicket.UserData.StartsWith(ImpersonatingUserInfoPrefix))
                return; // Ignore if not impersonating.

            string userData = "";

            var newTicket = new FormsAuthenticationTicket(
                authenticationTicket.Version,
                authenticationTicket.Name,
                authenticationTicket.IssueDate,
                authenticationTicket.Expiration,
                authenticationTicket.IsPersistent,
                userData,
                authenticationTicket.CookiePath);

            authenticationCookie.Value = FormsAuthentication.Encrypt(newTicket);
        }
    }
}
