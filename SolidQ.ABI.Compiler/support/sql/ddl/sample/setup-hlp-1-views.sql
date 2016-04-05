/*
#
# Adaptive BI Framework 3.0
#
#
# File Revision: 3
# File Date: 2015-08-27
#
*/

USE [AdaptiveBI30_HLP_1]
GO

IF (OBJECT_ID('[bi].[vw_Person__Person]') IS NOT NULL) DROP VIEW [bi].[vw_Person__Person]
IF (OBJECT_ID('[bi].[vw_Production__Product]') IS NOT NULL) DROP VIEW [bi].[vw_Production__Product]
IF (OBJECT_ID('[bi].[vw_Production__ProductCategory]') IS NOT NULL) DROP VIEW [bi].[vw_Production__ProductCategory]
IF (OBJECT_ID('[bi].[vw_Production__ProductSubCategory]') IS NOT NULL) DROP VIEW [bi].[vw_Production__ProductSubCategory]
IF (OBJECT_ID('[bi].[vw_Sales__Customer]') IS NOT NULL) DROP VIEW [bi].[vw_Sales__Customer]
IF (OBJECT_ID('[bi].[vw_Sales__Store]') IS NOT NULL) DROP VIEW [bi].[vw_Sales__Store]
IF (OBJECT_ID('[bi].[vw_Sales__SalesOrderDetail]') IS NOT NULL) DROP VIEW [bi].[vw_Sales__SalesOrderDetail]
IF (OBJECT_ID('[bi].[vw_Sales__SalesOrderDetailTS]') IS NOT NULL) DROP VIEW [bi].[vw_Sales__SalesOrderDetailTS]
IF (OBJECT_ID('[bi].[vw_Sales__SalesOrderHeader]') IS NOT NULL) DROP VIEW [bi].[vw_Sales__SalesOrderHeader]
IF (OBJECT_ID('[bi].[vw_Sales__SalesOrderHeaderSalesReason]') IS NOT NULL) DROP VIEW [bi].[vw_Sales__SalesOrderHeaderSalesReason]
GO

CREATE VIEW [bi].[vw_Person__Person]
AS
SELECT
	[BusinessEntityID],
	[Title],
	[FirstName],
	[MiddleName],
	[LastName],
	[PersonType]
FROM
	AdventureWorks2012.Person.Person
GO

CREATE VIEW [bi].[vw_Production__Product]
AS
SELECT
	[ProductID],
	[Name],
	[ProductNumber],
	[ProductSubcategoryID]
FROM
	[AdventureWorks2012].[Production].[Product]
GO

CREATE VIEW [bi].[vw_Production__ProductCategory]
AS
SELECT
	[ProductCategoryID],
	[Name]
FROM
	[AdventureWorks2012].[Production].[ProductCategory]
GO

CREATE VIEW [bi].[vw_Production__ProductSubCategory]
AS
SELECT
	[ProductSubcategoryID],
	[ProductCategoryID],
	[Name]
FROM
	[AdventureWorks2012].[Production].[ProductSubCategory]
GO

CREATE VIEW [bi].[vw_Sales__Customer]
AS
SELECT 
	[CustomerID],
	[PersonID],
	[StoreID],
	[TerritoryID]
FROM 
	AdventureWorks2012.Sales.[Customer]
GO

CREATE VIEW [bi].[vw_Sales__SalesOrderDetail]
as
SELECT 
	[SalesOrderID],
	[SalesOrderDetailID] = CAST([SalesOrderDetailID] AS INT), -- Remove "IDENTITY" Metadata
	[OrderQty],
	[ProductID],
	[LineTotal],
	[ModifiedDate]
FROM 
	[AdventureWorks2012].[Sales].[SalesOrderDetail]
GO

CREATE VIEW [bi].[vw_Sales__SalesOrderHeader]
AS
SELECT
	[SalesOrderID] = CAST([SalesOrderID] AS INT), -- Remove "IDENTITY" Metadata
	[OrderDate],
	[ShipDate],
	[CustomerID],
	[SalesPersonID],
	[TerritoryID],
	[ShipMethodID],
	[SubTotal],
	[TaxAmt],
	[Freight],
	[OnlineOrderFlag],
	[PurchaseOrderNumber]
FROM
	[AdventureWorks2012].[Sales].[SalesOrderHeader]
WHERE
	[ShipDate] IS NOT NULL
GO

CREATE VIEW [bi].[vw_Sales__SalesOrderHeaderSalesReason]
AS
SELECT 
	[SalesOrderID], 
	[SalesReasonID]	
FROM 
	[AdventureWorks2012].[Sales].[SalesOrderHeaderSalesReason]
GO

CREATE VIEW [bi].[vw_Sales__Store]
AS
WITH XMLNAMESPACES (DEFAULT 'http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey')
SELECT 
	BusinessEntityId,
	Name,
	SquareFeet = Demographics.value('(//StoreSurvey/SquareFeet)[1]', 'int')
FROM 
	AdventureWorks2012.Sales.Store	

GO

/*
CREATE VIEW [bi].[vw_Sales__SalesOrderDetailTS]
AS
SELECT 
	[SalesOrderID],
	[SalesOrderDetailID] = CAST([SalesOrderDetailID] AS INT),
	[OrderQty],
	[ProductID],
	[LineTotal],
	[TS] = CAST(CAST([RowVersion] AS VARBINARY(128)) AS BIGINT)
FROM 
	[AdventureWorks2012].[Sales].[SalesOrderDetailTS]
GO
*/