/*
#
# Adaptive BI Framework 3.0
#
#
# File Revision: 5
# File Date: 2015-08-26
#
*/

USE [AdaptiveBI30_MD]
GO


/*

[md].[extract_phase_info]

*/

IF (OBJECT_ID('[md].[extract_phase_info]') IS NOT NULL) DROP TABLE [md].[extract_phase_info]
GO

CREATE TABLE [md].[extract_phase_info]
(
	[source_object_name] [sysname] NOT NULL,
	[destination_table_name] [sysname] NOT NULL,
	[etl_pattern_id] [varchar](3) NOT NULL,	
	[source_type_id] [char](3) NOT NULL,
	[primary_key_columns] NVARCHAR(MAX) NULL,
	[incremental_column_name] SYSNAME NULL,
	[differential_column_name] SYSNAME NULL,
	[create_primary_key_in_destination] BIT NOT NULL,
	[$xcs] [xml] COLUMN_SET FOR ALL_SPARSE_COLUMNS  NULL,
	CONSTRAINT [pk__extract_phase_info] PRIMARY KEY CLUSTERED 
	(
		[source_object_name] ASC
	)
) 
GO

/*
	[md].[extract_phase_info_connections]
*/

IF (OBJECT_ID('[md].[extract_phase_info_connections]') IS NOT NULL) DROP TABLE [md].[extract_phase_info_connections]
GO

CREATE TABLE [md].[extract_phase_info_connections]
(
	[source_object_name] [sysname] NOT NULL,
	[etl_connection_id] [int] NOT NULL,
	[extract_tag] [INT] NOT NULL,
	CONSTRAINT [pk__extract_phase_info_connections] PRIMARY KEY CLUSTERED 
	(
		[source_object_name] ASC,
		[etl_connection_id] ASC
	)
)


/*

[md].[load_source_types]

*/

IF (OBJECT_ID('[md].[extract_source_types]') IS NOT NULL) DROP TABLE [md].[extract_source_types]
GO

CREATE TABLE [md].[extract_source_types]
(
	source_type_id CHAR(3) NOT NULL PRIMARY KEY,
	source_type VARCHAR(100)
)
GO

ALTER TABLE [md].[extract_phase_info] ADD CONSTRAINT [fk__extract_source_type_id] FOREIGN KEY (source_type_id) REFERENCES [md].[extract_source_types](source_type_id)
GO

INSERT INTO md.[extract_source_types] VALUES
('VIW', 'View'),
('STP', 'Stored Procedure'),
('QRY', 'Query'),
('CSV', 'Comma Separated File'),
('TSV', 'Tab Saparated File'),
('XLS', 'Excel')
GO

/*

[md].[etl_patterns]

*/

IF (OBJECT_ID('[md].[etl_patterns]') IS NOT NULL) DROP TABLE [md].[etl_patterns]
GO

CREATE TABLE [md].[etl_patterns]
(
	etl_pattern_id VARCHAR(3) NOT NULL PRIMARY KEY,
	etl_pattern VARCHAR(100)
)
GO

ALTER TABLE [md].[extract_phase_info] ADD CONSTRAINT [fk__etl_pattern_id] FOREIGN KEY (etl_pattern_id) REFERENCES [md].[etl_patterns](etl_pattern_id)
GO

INSERT INTO md.[etl_patterns] VALUES
('FS', 'Full Sigle'),
('FC', 'Full Composite'),
('FL', 'Full Loop'),
('FP', 'Full Parallel'),
('DMH', 'Differential Merge Hash'),
('DLH', 'Differential Lookup Hash'),
('DLT', 'Differential Lookup Timestamp')
GO

/*

[md].[load_phase_info]

*/

IF (OBJECT_ID('[md].[load_phase_fact_info]') IS NOT NULL) DROP TABLE [md].[load_phase_fact_info]
GO

CREATE TABLE [md].[load_phase_fact_info]
(
	[fact_table_name] [sysname] NOT NULL,
	[fact_table_column_name] [sysname] NOT NULL,
	[dimension_table_name] [sysname] NOT NULL,
	[dimension_table_column_name] [sysname] NOT NULL,
	[dimension_role_suffix] [sysname] NOT NULL,
	CONSTRAINT [pk__load_phase_fact_info] PRIMARY KEY CLUSTERED 
	(
		[fact_table_name] ASC,
		[fact_table_column_name] ASC,
		[dimension_table_name] ASC
	)
) 
go

ALTER TABLE [md].[load_phase_fact_info] ADD  CONSTRAINT [df_1]  DEFAULT ('') FOR [dimension_role_suffix]
go

/*

[md].[etl_load_info]

*/

IF (OBJECT_ID('[md].[etl_incremental_info]') IS NOT NULL) DROP TABLE [md].[etl_incremental_info]
GO

CREATE TABLE [md].[etl_incremental_info]
(
	[object_name] [sysname] NOT NULL PRIMARY KEY CLUSTERED,
	[last_loaded_row_value] BIGINT NULL
)
GO



/*

[md].[extract_source_schema]

*/
IF (OBJECT_ID('[md].[extract_source_schema]') IS NOT NULL) DROP TABLE [md].[extract_source_schema]
GO

CREATE TABLE [md].[extract_source_schema]
(
	[etl_connection_id] [int] NOT NULL,
	[source_object_name] [sysname] NOT NULL,
	[source_column_name] [sysname] NOT NULL,
	[target_data_type] [sysname] NOT NULL,
	[source_casting_expression] [NVARCHAR](MAX) NULL,
	CONSTRAINT [pk__extract_source_schema] PRIMARY KEY CLUSTERED 
	(
		[etl_connection_id] ASC,
		[source_object_name] ASC,
		[source_column_name] ASC
	)
)
GO


/*

[md].[load_connections]

*/
IF (OBJECT_ID('[md].[etl_connections]') IS NOT NULL) DROP TABLE [md].[etl_connections]
GO

CREATE TABLE [md].[etl_connections]
(
	[etl_connection_id] [int] IDENTITY NOT NULL,
	[name] sysname NOT NULL UNIQUE,
	[type] [char](3) NOT NULL,
	[connection_string] [nvarchar](max) NULL,
	[provider] [nvarchar](max) NULL,
	[description] [nvarchar](max) NULL,
	CONSTRAINT [pk__load_connections] PRIMARY KEY CLUSTERED 
	(
		[etl_connection_id] ASC
	)	
) 
GO

ALTER TABLE [md].[etl_connections] WITH CHECK ADD CONSTRAINT ck__connection_type CHECK ([type] IN ('HLP', 'STG', 'DWH', 'DM', 'AUX', 'CFG', 'LOG', 'DI', 'EXT'))
GO

ALTER TABLE [md].[extract_source_schema] ADD CONSTRAINT [fk__extract_source_schema__etl_connection_id] FOREIGN KEY ([etl_connection_id]) REFERENCES [md].[etl_connections]([etl_connection_id])
GO

ALTER TABLE [md].[extract_phase_info_connections] ADD CONSTRAINT [fk__extract_phase_info_connections__etl_connection_id] FOREIGN KEY ([etl_connection_id]) REFERENCES [md].[etl_connections]([etl_connection_id])
GO

/*

[md].[fn_get_fact_mapping]

*/

IF (OBJECT_ID('[md].[fn_get_fact_mapping]') IS NOT NULL) DROP FUNCTION [md].[fn_get_fact_mapping]
GO

CREATE FUNCTION [md].[fn_get_fact_mapping](@source_view sysname = NULL)
RETURNS TABLE
AS
RETURN
SELECT 
	stg_fact_name = fact_table_name,
	stg_bk_column = fact_table_column_name,
	dwh_fact_name = SUBSTRING(fact_table_name, 4, 128),
	dwh_dim_name = dimension_table_name,
	dwh_bk_column =  dimension_table_column_name,
	dwh_dim_suffix = dimension_role_suffix
FROM
	[md].[load_phase_fact_info]
WHERE
	fact_table_name = @source_view
GO


/*

[md].[fn_get_extract_table_mapping]

*/

IF (OBJECT_ID('[md].[fn_get_extract_table_mapping]') IS NOT NULL) DROP FUNCTION [md].[fn_get_extract_table_mapping]
GO

CREATE FUNCTION md.fn_get_extract_table_mapping(@source_object_name sysname = NULL)
RETURNS TABLE
AS
RETURN
SELECT
	[a].[source_object_name],
	source_object_schema = PARSENAME([a].[source_object_name], 2),
    [a].[destination_table_name] ,
    [a].[etl_pattern_id] ,
    [a].[source_type_id] ,
    [a].[primary_key_columns] ,
    [a].[incremental_column_name] ,
    [a].[differential_column_name] ,
    [a].[create_primary_key_in_destination] ,
	[b].[etl_connection_id]
FROM
	md.[extract_phase_info] a
INNER JOIN
	md.[extract_phase_info_connections] b ON a.[source_object_name] = b.[source_object_name]
WHERE
	a.source_object_name = @source_object_name
GO

