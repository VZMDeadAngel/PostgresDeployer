# PostgresDeployer

This solution allows you to create and update the existing database.

# Algorithm
The algorithm of the application of the application is quite simple and consists of three steps.
1. From the Script Catalog, the program reads all files and executes them in the created temp database. The application defines the correct sequence of scripts.
2. The pgdiff application is launched, which compares the time database and the target scheme. As a result of execution, the scenario of database changes is generated.
3. The resulting script is optionally executed on the target database.

# Project components
1. Database. Directory with database project files.
2. pgdiff. The application written on "go". Generates a change script by comparing two databases. [Source project](https://github.com/joncrlsn/pgdiff).
3. PostgresDeployer. Basic Net Core app.

# Creating a configuration file
Before using the application, you need to create a configuration .xml file.
The file is created according to the "SettingsSchema.xsd" scheme.
You must enter the following parameters:
1. Path to the database project directory.
2. The path to the catalog containing the scripts of the deployment post. This group of scripts is executed with each deployment, so it is indicated by a separate setting.
3. Connection parameters for a temp database.
4. Enviroments. Connecting parameters to a database for each environment.

# Build
Before build, you need to install [GoLang](https://golang.org/).

run command `dotnet build` inside PostgresDeployer catalog.

# Run
Running with configuration file:

`PostgresDeployer.exe -c MyConfig.xml`

`PostgresDeployer.exe --config MyConfig.xml`

If the configuration file does not specify, the program is trying to open the file **[Machine name].xml**.

If this file is not, the program will try to open the **Default.xml** file.