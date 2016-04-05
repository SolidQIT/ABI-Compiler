USE [AdaptiveBI30_STG]
GO
Create table etl.Nums (num int);
go
with mycte(num) as
(Select  1 as num union all
  select num+1 from mycte where num<10000)
insert into etl.Nums
select num from mycte option(maxrecursion 0);
go
create function etl.fn_get_Max_ExecutionID (@SeparatedBycommaExecutionsId varchar(8000)) returns table as 
return(
select Max(cast(SUBSTRING(source,num+1,
					PATINDEX('%,%',SUBSTRING(source+',',num+1,1000))-1
				) as bigint)) ExecutionId
from 
	(select ','+@SeparatedBycommaExecutionsId  source ) source
	   inner join etl.nums Nums 
	   on substring(source.source,num,1)=','
	   )

