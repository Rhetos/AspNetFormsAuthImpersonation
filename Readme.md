# AspNetFormsAuthImpersonation

AspNetFormsAuthImpersonation is a package (a plugin module) for [Rhetos development platform](https://github.com/Rhetos/Rhetos).
It extends [AspNetFormsAuth](https://github.com/Rhetos/Rhetos/tree/master/AspNetFormsAuth) package with **user impersonation**,
allowing a user to log in as another user.
The impersonation information is persisted only in the standard authentication cookie (already used by AspNetFormsAuth).

Contents:

* [Installation and configuration](#installation-and-configuration)
    * [Installation](#installation)
    * [Configuring user's permissions](#configuring-users-permissions)
    * [Simple administration GUI](#simple-administration-gui)
* [Impersonation web service API](#impersonation-web-service-api)
    * [Impersonate](#impersonate)
    * [StopImpersonating](#stopimpersonating)
* [Implementing web GUI](#implementing-web-gui)

## Installation and configuration

### Installation

Add "Rhetos.AspNetFormsAuthImpersonation" to the package list in the server's *RhetosPackages.config* file.

### Configuring user's permissions

All claims related to the impersonation service have resource=`AspNetFormsAuth.Impersonation`.
Admin user (see [AspNetFormsAuth](https://github.com/Rhetos/Rhetos/tree/master/AspNetFormsAuth)) has all the necessary permissions (claims) for all authentication service methods by default.

The following security claims are used in the impersonation web service:

* `Impersonate` - A user with this claim is allowed to impersonate another user (execute [`Impersonate`](#impersonate) web method).
* `IncreasePermissions` - A user with this claim is allowed to **impersonate another user that has more permissions** than the original user.
  This claim is **not assigned** by default to the admin user.

### Simple administration GUI

For testing and administration, a simple web GUI is available at the Rhetos server homepage under *AspNetFormsAuthImpersonation* header.

## Impersonation web service API

The JSON service is available at URI `<rhetos server>/Resources/AspNetFormsAuthImperionation/Impersonation`, with the following methods.

### Impersonate

Activates impersonation for the currently logged in user to act as the given `ImpersonatedUser`.

* Interface: `(string ImpersonatedUser) -> void`
* Requires `Impersonate` [security claim](#configuring-user-s-permissions).
* On successful impersonation, the server response will contain the standard authentication cookie,
  containing the impersonation information.
  The client browser will automatically use the cookie for following requests.
* Response data is empty the impersonation is successful,
  or an error message (*string*) with HTTP error code 4* or 5* in case of an error.

### StopImpersonating

The user stays logged in, but the impersonation of deactivated.

As an alternative to calling this method, the impersonation will be automatically deactivated if a user logs out
(see [authentication service](https://github.com/Rhetos/Rhetos/tree/master/AspNetFormsAuth) `Logout` method),
or the login session expires.

* No request data is needed. Response is empty.

## Implementing web GUI

Web application that [shares user authentication](https://github.com/Rhetos/Rhetos/tree/master/AspNetFormsAuth#sharing-the-authentication-across-web-applications)
with Rhetos server may access the impersonation information and show it in the GUI.

To find out if the current user impersonates another, use the following code snippet:

```C#
    // (the project must reference **System.Web.dll**)

    /// <summary>
    /// Returns the impersonated user whose context (including security permissions) is in effect.
    /// Returns null if there is no impersonation.
    /// </summary>
    public static string GetImpersonatedUser()
    {
        var formsIdentity = (System.Web.HttpContext.Current.User.Identity as System.Web.Security.FormsIdentity);
        if (formsIdentity != null && formsIdentity.IsAuthenticated)
        {
            string userData = formsIdentity.Ticket.UserData;
            const string prefix = "Impersonating:";
            if (!string.IsNullOrEmpty(userData) && userData.StartsWith(prefix))
                return userData.Substring(prefix.Length);
        }
        return null;
    }
```