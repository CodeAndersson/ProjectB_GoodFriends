To create the secure AppMvc and AppRazor using Microsoft Identity
Both applications are using the same software stack

NOTE: the WebApi application is now removed for simplicity. 
      To shift datasource from database access to WebApi access
        - a local public WebWapi can be created in separate project and started
        - alternatively a published public WebApi can be accessed

1. Create the database. With Terminal in folder _scripts 
   E.g. database name: sql-friends, database type: sqlserver, server: docker, default user: dbo, application: ../AppRazor

   macOs
   ./database-rebuild-all.sh sql-friends sqlserver docker dbo ../AppRazor
   ./database-rebuild-all.sh sql-friends mysql docker dbo ../AppRazor
   ./database-rebuild-all.sh sql-friends postgresql docker dbo ../AppRazor
   
   Windows
   ./database-rebuild-all.ps1 sql-friends sqlserver docker dbo ../AppRazor
   ./database-rebuild-all.ps1 sql-friends mysql docker dbo ../AppRazor
   ./database-rebuild-all.ps1 sql-friends postgresql docker dbo ../AppRazor

   Ensure no errors from build, migration or database update
   ../AppRazor above can be exchanged to ../AppMvc

2. From Azure Data Studio you can now connect to the database
   Use connection string from user secrets:
   connection string corresponding to Tag
   "sql-friends.<db_type>.docker.root"

3. Use Azure Data Studio to execute SQL script DbContext/SqlScripts/<db_type>/azure/initDatabase.sql



Running AppRazor using WebApi or Database as source:
----------------------------------------------------

4. Run AppRazor or AppMvc with debugger

5. Register a user and Login

6. Seed the database

7. Login and logout as a user to see how access is restricted for non loggen in users
   
NOTE: From AppRazor and AppMvc perspective, the ONLY change is the DataAccess services injected to the DI 
      This is one of the strength of a well made software stack with loosly couple objects and alyers