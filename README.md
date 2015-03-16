# Octopus_TFS
Octopus deploy integration for TFS

This project is a custom activity for TFS Server 2012 that calls the Octo.exe with a list of parameters from the build template. This gives you greater control over the version of the packages used in a deployment aswell as allowing you to override Octopus variables controlled by the build. We use the activity in conjunction with the CodePlex activity <a href="https://tfsversioning.codeplex.com/">"TFS Versioning"</a> with a slight modification to allow the generated version number to be exposed back to the build template for later used by Octopus.

Although you can use the "out of the box" integration for TFS, this doesn't provide the level of control you may need in a production environment where you need to maintain multiple versions and branches of your code. This solution used a combination of the standard integration with the additional control of what version of the packages to include, which steps to skip and what values to use for your deployment parameters.

Another advantage is when using gated check-ins with TFS. You can fail a build if the deployment step fails, thus providing you the ability to reject bad checkins not simply based on "Did it compile" or "Did the unit tests run" but also "Did the system deploy correctly?".

The ability to override Octopus variables from the Build template in TFS is very useful when you need to maintain multiple branches of code and each branch uses a different variable value (e.g. Say a database backup file).

#NOTE: the variables you override must be PROMPT variables in Octopus.