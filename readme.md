# SQL Bulk Inserter

A generic dotnet core console app that will download a CSV from an FTP server and bulk insert the data into a database. Optimized for large datasets 

## Features

- Currently only supports CSV's
- Currently only extracts & inserts a single column
- Will extract the CSV out of a zip file
- Bulk inserts to a temp table using paging for optimal performance
- No data loss as data goes to a temp table first
- Logs exceptions to Slack

## Configuration & Usage 

- Add your SQL connection string, FTP details & Slack config to `appsettings.json`
- Run locally using dotnet (`dotnet run`) or via Docker (`docker build -t sql-bulk-inserter . && docker run sql-bulk-inserter`)

## Notes

You may want to add this data to a separate database so that the transactional logs can be configured separately to the main data store (e.g. Simple recovery mode) to prevent filling up transaction logs for your primary database

You can also bulk insert directly into SQL server through SQL with the following:

```
BULK INSERT dbo.TPS_Load 
FROM 'C:\Users\me\data.csv'
WITH (TABLOCK)
```

## Resources

https://www.mssqltips.com/sqlservertip/2387/net-bulk-insert-into-sql-server/

