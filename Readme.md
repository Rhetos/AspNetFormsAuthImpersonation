AspNetFormsAuthImpersonation
============================

AspNetFormsAuthImpersonation is a package (a plugin module) for [Rhetos development platform](https://github.com/Rhetos/Rhetos).
It extends AspNetFormsAuth package with **user impersonation**, i.e. logging in as another user.

See [rhetos.org](http://www.rhetos.org/) for more information on Rhetos.


Deployment
----------

### Prerequisites

* *CommonConcepts* and *AspNetFormsAuth* packages must be deployed along with this package.

Building binaries from source
-----------------------------

### Prerequisites

* Before building this project please download [Rhetos source](https://github.com/Rhetos/Rhetos), compile it (using `Build.bat`) and enter the source folder's full path in `RhetosSourcePath.txt`.

### Build

1. Build this package by executing `Build.bat`.

### Create installation package

1. Edit `ChangeVersion.bat` to set the new version number, and execute it.
2. Execute `Build.bat`.
3. Execute `CreatePackage.bat`. It creates an installation package (.zip) in the parent directory.
