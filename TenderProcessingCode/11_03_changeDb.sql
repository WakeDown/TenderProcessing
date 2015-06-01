use tenderProcessing
go

alter procedure GetClaimsCalculatePositionsStatistics
(
	@ids nvarchar(max)
)
as
select [CalculateClaimPosition].IdClaim, ProductManager, IdPosition, Count([CalculateClaimPosition].IdClaim) 
from CalculateClaimPosition, ClaimPosition 
where [CalculateClaimPosition].Deleted = 0 and [ClaimPosition].Deleted = 0 
and IdPosition = [ClaimPosition].Id
and [CalculateClaimPosition].IdClaim in(select * from dbo.Split(@ids,',')) 
and PositionState in (2,4)
group by [CalculateClaimPosition].IdClaim, ProductManager, IdPosition;
go