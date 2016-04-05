/*
#
# Adaptive BI Framework 3.0
#
#
# File Revision: 1
# File Date: 2015-08-10
#
*/

USE [AdaptiveBI30_LOG]
GO

/*

	[log].[etl_table_load_info]

*/

IF (OBJECT_ID('[log].[etl_table_load_info]') IS NOT NULL) DROP TABLE [log].[etl_table_load_info]
GO

CREATE TABLE [log].[etl_table_load_info]
(
	[id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
	[database_name] [sysname] NOT NULL,
	[schema_name] [sysname] NOT NULL,
	[table_name] [sysname] NOT NULL,
	[type] [char](1) NOT NULL,
	[ssis_server_execution_id] BIGINT NOT NULL,
	[start_date] [datetime2](7) NOT NULL,
	[end_date] [datetime2](7) NULL,
	[status] [char](1) NULL,
	[loaded_by] [sysname] NULL CONSTRAINT [DF__loaded_by]  DEFAULT (suser_sname()),
	[partition_number] [int] SPARSE  NULL,
	[rows] [bigint] SPARSE  NULL,
	inserted_rows INT SPARSE NULL,
	updated_rows INT SPARSE NULL,
	deleted_rows INT SPARSE NULL,
	[$xcs] [xml] COLUMN_SET FOR ALL_SPARSE_COLUMNS  NULL
)
GO

ALTER TABLE [log].[etl_table_load_info]  WITH CHECK ADD  CONSTRAINT [CK__table_load_info__status] CHECK  (([status]='F' OR [status]='S' OR [status]='C'))
GO

ALTER TABLE [log].[etl_table_load_info]  WITH CHECK ADD  CONSTRAINT [CK__table_load_info__type] CHECK  (([type]='F' OR [type]='I' OR [type]='D' OR [type]='M'))
GO

/*

	[log].[stp_etl_table_load_info_set_start]

*/

IF (OBJECT_ID('[log].[stp_etl_table_load_info_set_start]') IS NOT NULL) DROP PROCEDURE [log].[stp_etl_table_load_info_set_start]
GO

CREATE PROCEDURE [log].[stp_etl_table_load_info_set_start]
    @databaseName AS sysname,
    @schemaName AS sysname,
    @tableName AS sysname,
    @type AS char(1),
    @serverExecutionId bigint,
	@partitionNumber int = NULL
AS

insert into [log].[etl_table_load_info] 
([database_name], [schema_name], [table_name], [type], [start_date], [ssis_server_execution_id], [partition_number])
values
(@databaseName, @schemaName, @tableName, @type, SYSDATETIME(), @serverExecutionId, @partitionNumber)

return scope_identity()
GO

/*

	[log].[stp_etl_table_load_info_set_end_cs]

*/

IF (OBJECT_ID('[log].[stp_etl_table_load_info_set_end]') IS NOT NULL) DROP PROCEDURE [log].[stp_etl_table_load_info_set_end]
GO

CREATE PROCEDURE [log].[stp_etl_table_load_info_set_end]
	@rowId int,
	@rows bigint = null,
	@status char(1)
as

update [log].[etl_table_load_info]
set [rows] = @rows, [end_date] = SYSDATETIME(), [status] = @status 
where id = @rowId
GO

/*

	[log].[stp_etl_table_load_info_set_end_cs]

*/

IF (OBJECT_ID('[log].[stp_etl_table_load_info_set_end_cs]') IS NOT NULL) DROP PROCEDURE [log].[stp_etl_table_load_info_set_end_cs]
GO

CREATE PROCEDURE [log].[stp_etl_table_load_info_set_end_cs]
@rowId INT,
@xcs XML,
@status CHAR(1)
AS

UPDATE [log].etl_table_load_info 
SET [$xcs] = @xcs, [end_date] = SYSDATETIME(), [status] = @status
WHERE id = @rowId
GO

