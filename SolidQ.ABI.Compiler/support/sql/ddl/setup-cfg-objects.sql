/*
#
# Adaptive BI Framework 3.0
#
#
# File Revision: 3
# File Date: 2015-08-26
#
*/

USE [AdaptiveBI30_CFG]
GO


/*

[cfg].[extract_phase]

*/

IF (OBJECT_ID('[cfg].[extract_phase]') IS NOT NULL) DROP TABLE [cfg].[extract_phase]
GO

create TABLE [cfg].[extract_phase]
(
	source_object_name sysname not null primary key,
	active_for_load char(1)
)

ALTER TABLE [cfg].[extract_phase] WITH CHECK ADD  CONSTRAINT [ck__active_for_load] CHECK (([active_for_load]='N' OR [active_for_load]='Y'))
go

ALTER TABLE [cfg].[extract_phase] CHECK CONSTRAINT [ck__active_for_load]
go
