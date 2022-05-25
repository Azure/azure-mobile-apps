-------------------------------------------------------------------------------
-- Copyright (c) Microsoft Corporation. All rights reserved.
-------------------------------------------------------------------------------  

DECLARE @sqlCommand nvarchar(max);

SET @sqlCommand = 'CREATE SCHEMA ' + QUOTENAME(@schema) + CHAR(13)
EXEC (@sqlCommand)

SET @sqlCommand = 'CREATE TABLE ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table1) + ' ( T1_Id int NOT NULL, LastName varchar(64) NOT NULL, FirstName varchar(64) NOT NULL, PRIMARY KEY (T1_Id));' + CHAR(13)
SET @sqlCommand = @sqlCommand + 'CREATE TABLE ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table2) + ' ( T2_Id int NOT NULL, OrderNo int NOT NULL, T1_Id int, PRIMARY KEY (T2_Id), FOREIGN KEY (T1_Id) REFERENCES ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table1) + '(T1_Id));' + CHAR(13)
SET @sqlCommand = @sqlCommand + 'CREATE TABLE ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table3) + ' ( T3_Id int NOT NULL, Geo geometry, Doc xml, PRIMARY KEY CLUSTERED (T3_Id) );' + CHAR(13)
EXEC(@sqlCommand)

SET @sqlCommand = 'CREATE SPATIAL INDEX SpatialIndex ON ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table3) + ' (Geo) WITH ( BOUNDING_BOX = ( xmin=0, ymin=0, xmax=500, ymax=200 ), GRIDS = (LOW, LOW, MEDIUM, HIGH), CELLS_PER_OBJECT = 64, PAD_INDEX  = ON );' + CHAR(13)
EXEC(@sqlCommand)

SET @sqlCommand = 'CREATE PRIMARY XML INDEX XmlIndex ON ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table3) + ' (Doc);' + CHAR(13)
EXEC(@sqlCommand)

SET @sqlCommand = 'CREATE TRIGGER ' +  QUOTENAME(@schema) + '.' + QUOTENAME(@trigger1) + ' ON ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table2) + ' AFTER INSERT, UPDATE AS RAISERROR (''Triggered!'', 16, 10);' + CHAR(13)
EXEC(@sqlCommand)

SET @sqlCommand = 'CREATE VIEW ' + QUOTENAME(@schema) + '.' + QUOTENAME(@view1) + ' AS SELECT ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table1) + '.FirstName, ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table1) + '.LastName FROM ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table1) + ';' + CHAR(13)
EXEC(@sqlCommand)

SET @sqlCommand = 'CREATE FUNCTION ' + QUOTENAME(@schema) + '.' + QUOTENAME(@function1) + ' (@str nvarchar(4000)) RETURNS bigint AS BEGIN RETURN(1024); END;' + CHAR(13)
EXEC(@sqlCommand)

SET @sqlCommand = 'CREATE PROCEDURE ' + QUOTENAME(@schema) + '.' + QUOTENAME(@procedure1) + ' AS SELECT TOP(10) LastName FROM ' + QUOTENAME(@schema) + '.' + QUOTENAME(@table1) + ';' + CHAR(13)
EXEC(@sqlCommand)
