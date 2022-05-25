-------------------------------------------------------------------------------
-- Copyright (c) Microsoft Corporation. All rights reserved.
-------------------------------------------------------------------------------  

SELECT obj.name, obj.schema_id, obj.type_desc 
   FROM sys.objects obj, sys.schemas sch
   WHERE obj.schema_id = sch.schema_id AND QUOTENAME(sch.name) = QUOTENAME(@schema);