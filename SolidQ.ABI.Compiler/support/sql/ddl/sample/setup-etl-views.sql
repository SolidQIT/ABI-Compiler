/*
#
# Adaptive BI Framework 3.0
#
#
# File Revision: 1
# File Date: 2015-09-17
#
*/

USE [AdaptiveBI30_STG]
GO

IF (OBJECT_ID('etl.vw_dim_Customers') IS NOT NULL)	DROP VIEW etl.vw_dim_Customers
IF (OBJECT_ID('etl.vw_fact_Sales') IS NOT NULL) DROP VIEW etl.vw_fact_Sales
GO

CREATE VIEW etl.vw_dim_Customers
AS
SELECT	
	[bk_customer_key_1]	= CAST(5 AS INT),
	[bk_customer_key_2]	= CAST('DM' AS CHAR(2)),
	[name]				= CAST('Davide' AS NVARCHAR(50)),
	[surname]			= CAST('Mauri' AS NVARCHAR(50)),
	[city]				= CAST('Milano' AS NVARCHAR(50))

UNION ALL

SELECT
	*
FROM ( 
		VALUES
		(6, 'KW', 'Ken', 'White', 'Seattle'),
		(7, 'MG', 'Mike', 'Green', 'Milan')
	) T ([bk_customer_key_1], [bk_customer_key_2], [name], [surname], [city])
GO

CREATE VIEW etl.vw_fact_Sales
AS
SELECT
	id_dim_date					= CAST(20150917 AS INT),
	bk_paying_customer_key_1	= CAST(6 AS INT),
	bk_paying_customer_key_2	= CAST('KW' AS CHAR(2)),
	bk_receiving_customer_key_1 = CAST(7 AS INT),
	bk_receiving_customer_key_2 = CAST('MG' AS CHAR(2)),
	quantity					= CAST(1 AS INT),
	[value]						= CAST(10 AS DECIMAL(12,4))
GO

