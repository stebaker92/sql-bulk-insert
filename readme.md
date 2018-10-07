# Ftp Bulk CSV Downloader

A console app that will download a CSV and bulk insert the data into a database.

## Features

- Currently only supports CSV's
- Currently only extracts & inserts a single column
- Will extract the CSV out of a zip file
- Bulk inserts to a temp table using paging for optimal performance
- No data loss as data goes to a temp table first


## Notes

You may want to add this data to a separate database so that the transactional logs can be configured separately to the main data store (Simple recovery mode)

You can also bulk insert directly into SQL server through SQL with the following:

```
BULK INSERT dbo.TPS_Load 
FROM 'C:\Users\stephen.baker\TPS\tps\tps.txt'
WITH (TABLOCK)
```

## Resources

https://www.mssqltips.com/sqlservertip/2387/net-bulk-insert-into-sql-server/

