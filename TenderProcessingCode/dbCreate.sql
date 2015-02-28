--use master
--go
--create database tenderProcessing
--go

--use tenderProcessing
--go

--create table Currency
--(
--	Id int not null,
--	Value nvarchar(100) not null,
--	primary key(id) 
--)

--create table ClaimStatus
--(
--	Id int not null,
--	Value nvarchar(100) not null,
--	primary key(id) 
--)

--create table ProtectFact
--(
--	Id int not null,
--	Value nvarchar(100) not null,
--	primary key(id) 
--)

--create table DealType
--(
--	Id int not null,
--	Value nvarchar(100) not null,
--	primary key(id)
--)

--create table TenderStatus
--(
--	Id int not null,
--	Value nvarchar(100) not null,
--	primary key(Id)
--)

--create table PositionState
--(
--	Id int not null,
--	Value nvarchar(100) not null,
--	primary key(Id)
--)

--create table TenderClaim
--(
--	Id int identity not null,
--	TenderNumber nvarchar(150),
--	TenderStart datetime not null,
--	ClaimDeadline datetime not null,
--	KPDeadline datetime not null,
--	Comment nvarchar(1000),
--	Customer nvarchar(150) not null,
--	CustomerInn nvarchar(150) not null,
--	TotalSum decimal(18,2),
--	DealType int not null,
--	TenderUrl nvarchar(1500),
--	TenderStatus int not null,
--	Manager nvarchar(500) not null,
--	ManagerSubDivision nvarchar(500) not null,
--	ClaimStatus int not null,
--	RecordDate datetime not null,
--	Deleted bit not null,
--	Author nvarchar(150),
--  DeletedUser nvarchar(150),
--	DeleteDate datetime,
--	primary key(Id),
--	CONSTRAINT FK_TenderClaim_DealType FOREIGN KEY(DealType)
--		REFERENCES DealType(Id)
--    ON DELETE CASCADE
--    ON UPDATE CASCADE,
--	CONSTRAINT FK_TenderClaim_TenderStatus FOREIGN KEY(TenderStatus)
--		REFERENCES TenderStatus(Id)
--    ON DELETE CASCADE
--    ON UPDATE CASCADE,
--	CONSTRAINT FK_TenderClaim_ClaimStatus FOREIGN KEY(ClaimStatus)
--		REFERENCES ClaimStatus(Id)
--    ON DELETE CASCADE
--    ON UPDATE CASCADE
--)

--create table ClaimPosition
--(
--	Id int identity not null,
--	IdClaim int not null,
--	RowNumber int,
--	CatalogNumber nvarchar(500),
--	Name nvarchar(1000) not null,
--	ReplaceValue nvarchar(1000),
--	Unit nvarchar(10) not null,
--	Value int not null,
--	ProductManager nvarchar(500) not null,
--	Comment nvarchar(1500) not null,
--	Price decimal(18,2),
--	SumMax decimal(18,2),
--	PositionState int not null,
--	Author nvarchar(150),
--	Deleted bit not null,
--  DeletedUser nvarchar(150),
--	DeleteDate datetime,
--  Currency int,
--	primary key(Id),
--	CONSTRAINT FK_ClaimPosition_TenderClaim FOREIGN KEY(IdClaim)
--		REFERENCES TenderClaim(Id)
--    ON DELETE CASCADE
--    ON UPDATE CASCADE,
--	CONSTRAINT FK_ClaimPosition_PositionState FOREIGN KEY(PositionState)
--		REFERENCES PositionState(Id)
--    ON DELETE CASCADE
--    ON UPDATE CASCADE,
--	CONSTRAINT FK_ClaimPosition_Currency FOREIGN KEY(Currency)
--		REFERENCES Currency(Id)
--    ON DELETE NO ACTION
--    ON UPDATE NO ACTION
--)

--create table CalculateClaimPosition
--(
--	Id int identity not null,
--	IdPosition int not null,
--	IdClaim int not null,
--	CatalogNumber nvarchar(500) not null,
--	Name nvarchar(1000) not null,
--	ReplaceValue nvarchar(1000),
--	PriceCurrency decimal(18,2),
--	SumCurrency decimal(18,2),
--	PriceRub decimal(18,2),
--	SumRub decimal(18,2) not null,
--	Provider nvarchar(150),
--	ProtectFact int not null,
--	ProtectCondition nvarchar(500),
--	Comment nvarchar(1000),
--	Author nvarchar(150),
--	Deleted bit not null,
--  DeletedUser nvarchar(150),
--	DeleteDate datetime,
--  Currency int,
--	primary key(Id),
--	CONSTRAINT FK_CalculateClaimPosition_ClaimPosition FOREIGN KEY(IdPosition)
--		REFERENCES ClaimPosition(Id)
--    ON DELETE CASCADE
--    ON UPDATE CASCADE,
--	CONSTRAINT FK_CalculateClaimPosition_ProtectFact FOREIGN KEY(ProtectFact)
--		REFERENCES ProtectFact(Id)
--    ON DELETE CASCADE
--    ON UPDATE CASCADE,
--	CONSTRAINT FK_CalculateClaimPosition_TenderClaim FOREIGN KEY(IdClaim)
--		REFERENCES TenderClaim(Id)
--     ON DELETE NO ACTION
--     ON UPDATE NO ACTION,
--	CONSTRAINT FK_CalculateClaimPosition_Currency FOREIGN KEY(Currency)
--		REFERENCES Currency(Id)
--    ON DELETE NO ACTION
--    ON UPDATE NO ACTION
--)

--create table ClaimStatusHistory
--(
--	Id int identity not null,
--	RecordDate datetime not null,
--	IdClaim int not null,
--	IdStatus int not null,
--	Comment nvarchar(1000),
--	IdUser nvarchar(500) not null,
--	primary key(Id),
--	CONSTRAINT FK_ClaimStatusHistory_TenderClaim FOREIGN KEY(IdClaim)
--		REFERENCES TenderClaim(Id)
--		ON DELETE CASCADE
--		ON UPDATE CASCADE,
--	CONSTRAINT FK_ClaimStatusHistory_ClaimStatus FOREIGN KEY(IdStatus)
--		REFERENCES ClaimStatus(Id)
--		ON DELETE No action
--		ON UPDATE no action
--)

--create table Roles
--(
--	Id int not null,
--	GroupId nvarchar(500) not null,
--	GroupName nvarchar(500) not null, 
--	primary key(Id)
--)

--use tenderProcessing
--go

--create index i_idClaim_claimPosition on ClaimPosition(IdClaim);
--create index i_idClaim_calculateClaimPosition on CalculateClaimPosition(IdClaim);
--create index i_idPosition_calculateClaimPosition on CalculateClaimPosition(IdPosition);

--use tenderProcessing
--go

--create procedure SaveTenderClaim
--(
--	@tenderNumber nvarchar(150) = '',
--	@tenderStart datetime,
--	@claimDeadline datetime,
--	@kPDeadline datetime,
--	@comment nvarchar(1000) = '',
--	@customer nvarchar(150),
--	@customerInn nvarchar(150),
--	@totalSum decimal(18,2) = -1,
--	@dealType int,
--	@tenderUrl nvarchar(1500) = '',
--	@tenderStatus int,
--	@manager nvarchar(500),
--	@managerSubDivision nvarchar(500),
--	@claimStatus int,
--	@recordDate datetime,
--	@author nvarchar(150),
--	@deleted bit
--)
--as
--declare @id int;
--insert into TenderClaim values(@tenderNumber, @tenderStart, @claimDeadline, @kPDeadline, @comment, @customer, 
--	@customerInn, @totalSum, @dealType, @tenderUrl, @tenderStatus, @manager, @managerSubDivision, @claimStatus, @recordDate, @deleted, @author, null, null)
--set @id = @@IDENTITY;
--select @id;
--go

--use tenderProcessing
--go

--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--create function [dbo].[Split]
--(
--    @value varchar(max),
--    @delimiter nvarchar(10)
--)
--returns @SplittedValues table
--(
--    value int
--)
--as
--begin
--    declare @SplitLength int
    
--    while len(@value) > 0
--    begin 
--        select @SplitLength = (case charindex(@delimiter,@value) when 0 then
--            len(@value) else charindex(@delimiter,@value) -1 end)
 
--        insert into @SplittedValues
--        select cast(substring(@value,1,@SplitLength) as int)
    
--        select @value = (case (len(@value) - @SplitLength) when 0 then  ''
--            else right(@value, len(@value) - @SplitLength - 1) end)
--    end 
--return  
--end
--go

--use tenderProcessing
--go

--create procedure LoadTenderClaims
--(
--	@pageSize int
--)
--as
--select top (@pageSize) * from TenderClaim where Deleted = 0 order by Id desc
--go

--use tenderProcessing
--go

--create procedure DeleteTenderClaims
--(
--	@id int,
--	@deletedUser nvarchar(150),
--	@date datetime
--)
--as
--update TenderClaim set Deleted = 1, DeletedUser = @deletedUser, DeleteDate = @date where Id = @id
--go

--use tenderProcessing
--go

--create procedure ChangeTenderClaimClaimStatus
--(
--	@id int,
--	@claimStatus int
--)
--as
--update TenderClaim set ClaimStatus = @claimStatus where Id = @id
--go

--use tenderProcessing
--go

--create procedure ChangeTenderClaimTenderStatus
--(
--	@id int,
--	@status int
--)
--as
--update TenderClaim set TenderStatus = @status where Id = @id
--go

--use tenderProcessing
--go

--create procedure LoadTenderClaimById
--(
--	@id int
--)
--as
--select * from TenderClaim where Deleted = 0 and Id = @id
--go

--use tenderProcessing
--go

--create procedure FilterTenderClaims
--(
--  @rowCount int,
--	@idClaim int = null,
--	@tenderNumber nvarchar(150) = null,
--	@claimStatusIds nvarchar(max) = null,
--	@manager nvarchar(500) = null,
--	@managerSubDivision nvarchar(500) = null,
--	@tenderStartFrom datetime = null,
--	@tenderStartTo datetime = null,
--	@overdie bit = null,
--	@idProductManager nvarchar(500) = null,
--	@author nvarchar(150) = null
--)
--as
--select top(@rowCount) * from TenderClaim where Deleted = 0 and ((@idClaim is null) or (@idClaim is not null and Id = @idClaim)) 
--and ((@tenderNumber is null) or (@tenderNumber is not null and TenderNumber = @tenderNumber))
--and ((@claimStatusIds is null) or (@claimStatusIds is not null and ClaimStatus in (select * from dbo.Split(@claimStatusIds,','))))
--and ((@manager is null) or (@manager is not null and Manager = @manager))
--and ((@managerSubDivision is null) or (@managerSubDivision is not null and ManagerSubDivision = @managerSubDivision))
--and ((@author is null) or (@author is not null and Author = @author))
--and ((@idProductManager is null) or (@idProductManager is not null and @idProductManager in (select ProductManager from ClaimPosition where IdClaim = [TenderClaim].Id)))
--and ((@overdie is null) or (@overdie is not null and 
--((@overdie = 1 and GETDATE() > ClaimDeadline) or (@overdie = 0 and GETDATE() < ClaimDeadline))))
--and ((@tenderStartFrom is null and @tenderStartTo is null) or (@tenderStartFrom is not null and @tenderStartTo is not null
--and ClaimDeadline BETWEEN @tenderStartFrom AND @tenderStartTo) or (@tenderStartFrom is null and @tenderStartTo is not null
--and ClaimDeadline <= @tenderStartTo) or (@tenderStartFrom is not null and @tenderStartTo is null
--and ClaimDeadline >= @tenderStartFrom)) order by Id desc
--go

--use tenderProcessing
--go

--create procedure FilterTenderClaimsCount
--(
--	@idClaim int = null,
--	@tenderNumber nvarchar(150) = null,
--	@claimStatusIds nvarchar(max) = null,
--	@manager nvarchar(500) = null,
--	@managerSubDivision nvarchar(500) = null,
--	@tenderStartFrom datetime = null,
--	@tenderStartTo datetime = null,
--	@overdie bit = null,
--	@idProductManager nvarchar(500) = null,
--	@author nvarchar(150) = null
--)
--as
--select count(*) from TenderClaim where Deleted = 0 and ((@idClaim is null) or (@idClaim is not null and Id = @idClaim)) 
--and ((@tenderNumber is null) or (@tenderNumber is not null and TenderNumber = @tenderNumber))
--and ((@claimStatusIds is null) or (@claimStatusIds is not null and ClaimStatus in (select * from dbo.Split(@claimStatusIds,','))))
--and ((@manager is null) or (@manager is not null and Manager = @manager))
--and ((@managerSubDivision is null) or (@managerSubDivision is not null and ManagerSubDivision = @managerSubDivision))
--and ((@author is null) or (@author is not null and Author = @author))
--and ((@idProductManager is null) or (@idProductManager is not null and @idProductManager in (select ProductManager from ClaimPosition where IdClaim = [TenderClaim].Id)))
--and ((@overdie is null) or (@overdie is not null and 
--((@overdie = 1 and GETDATE() > ClaimDeadline) or (@overdie = 0 and GETDATE() < ClaimDeadline))))
--and ((@tenderStartFrom is null and @tenderStartTo is null) or (@tenderStartFrom is not null and @tenderStartTo is not null
--and ClaimDeadline BETWEEN @tenderStartFrom AND @tenderStartTo) or (@tenderStartFrom is null and @tenderStartTo is not null
--and ClaimDeadline <= @tenderStartTo) or (@tenderStartFrom is not null and @tenderStartTo is null
--and ClaimDeadline >= @tenderStartFrom))
--go

--use tenderProcessing
--go

--create procedure LoadApproachingTenderClaim
--as
--select Id from TenderClaim where Deleted = 0 and ClaimStatus not in(4,5,8) and datediff(hour, GETDATE(), ClaimDeadline) <= 24
--go

--use tenderProcessing
--go

--create procedure LoadOverdieTenderClaim
--as
--select Id from TenderClaim where Deleted = 0 and ClaimStatus in(2,3,6,7) and ClaimDeadline > GETDATE()
--go

--use tenderProcessing
--go

--create procedure GetTenderClaimCount
--as
--select count(*) from TenderClaim where Deleted = 0
--go

--use tenderProcessing
--go

--create procedure SaveClaimPosition
--(
--	@idClaim int,
--	@rowNumber int = -1,
--	@catalogNumber nvarchar(500) = '',
--	@name nvarchar(1000),
--	@replaceValue nvarchar(1000) = '',
--	@unit nvarchar(10),
--	@value int,
--	@productManager nvarchar(500),
--	@comment nvarchar(1500) = '',
--	@price decimal(18,2) = -1,
--	@sumMax decimal(18,2) = -1,
--	@positionState int,
--	@author nvarchar(150),
--  @currency int
--)
--as
--declare @id int;
--insert into ClaimPosition values(@idClaim, @rowNumber, @catalogNumber, @name, @replaceValue, @unit,
--	@value, @productManager, @comment, @price, @sumMax, @positionState, @author, 0, null, null, @currency)
--set @id = @@IDENTITY;
--select @id;
--go

--use tenderProcessing
--go

--create procedure UpdateClaimPosition
--(
--	@id int,
--	@rowNumber int = -1,
--	@catalogNumber nvarchar(500) = '',
--	@name nvarchar(1000),
--	@replaceValue nvarchar(1000) = '',
--	@unit nvarchar(10),
--	@value int,
--	@productManager nvarchar(500),
--	@comment nvarchar(1500) = '',
--	@price decimal(18,2) = -1,
--	@sumMax decimal(18,2) = -1,
--	@positionState int,
--	@author nvarchar(150),
--  @currency int
--)
--as
--update ClaimPosition set RowNumber = @rowNumber, CatalogNumber = @catalogNumber, Name = @name, 
--	ReplaceValue = @replaceValue, Unit = @unit, Value = @value, ProductManager = @productManager, 
--	Comment = @comment, Price = @price, SumMax = @sumMax, PositionState = @positionState, Author = @author,
--	Currency = @currency where Id = @id
--go

--use tenderProcessing
--go

--create procedure DeleteClaimPosition
--(
--	@id int,
--	@deletedUser nvarchar(150),
--	@date datetime
--)
--as
--update ClaimPosition set Deleted = 1, DeletedUser = @deletedUser, DeleteDate = @date where Id = @id
--go

--use tenderProcessing
--go

--create procedure GetClaimsPositionsStatistics
--(
--	@ids nvarchar(max)
--)
--as
--select IdClaim, ProductManager, Count(*) from ClaimPosition where Deleted = 0 and IdClaim in(select * from dbo.Split(@ids,',')) group by IdClaim, ProductManager;
--go

--use tenderProcessing
--go

--create procedure GetClaimsCalculatePositionsStatistics
--(
--	@ids nvarchar(max)
--)
--as
--select [CalculateClaimPosition].IdClaim, ProductManager, Count([CalculateClaimPosition].IdClaim) 
--from CalculateClaimPosition, ClaimPosition 
--where [CalculateClaimPosition].Deleted = 0 and [ClaimPosition].Deleted = 0 
--and IdPosition = [ClaimPosition].Id
--and [CalculateClaimPosition].IdClaim in(select * from dbo.Split(@ids,',')) 
--and PositionState in (2,4)
--group by [CalculateClaimPosition].IdClaim, ProductManager;
--go

--use tenderProcessing
--go

--create procedure LoadNoneCalculatePosition
--(
--	@id int
--)
--as
--select * from ClaimPosition where Deleted = 0 and IdClaim = @id and (PositionState = 1 or PositionState = 3)
--go

--use tenderProcessing
--go

--create procedure GetProductsForClaims
--(
--	@ids nvarchar(max)
--)
--as
--select distinct IdClaim, ProductManager from ClaimPosition where Deleted = 0 and IdClaim in (select * from dbo.Split(@ids,','));
--go

--use tenderProcessing
--go

--create procedure HasClaimTranmissedPosition
--(
--	@id int
--)
--as
--select count(*) from ClaimPosition where Deleted = 0 and IdClaim = @id and PositionState > 1 
--go

--use tenderProcessing
--go

--create procedure IsPositionsReadyToConfirm
--(
--	@ids nvarchar(max)
--)
--as
--select IdPosition, count(*) from CalculateClaimPosition where Deleted = 0 and IdPosition in(select * from dbo.Split(@ids,',')) group by IdPosition;
--go

--use tenderProcessing
--go

--create procedure SetPositionsToConfirm
--(
--	@ids nvarchar(max)
--)
--as
--update ClaimPosition set PositionState = 2 where Deleted = 0 and Id in(select * from dbo.Split(@ids,','));
--go

--use tenderProcessing
--go

--create procedure ExistsClaimPosition
--(
--	@idClaim int,
--	@rowNumber int = -1,
--	@catalogNumber nvarchar(500) = '',
--	@name nvarchar(1000),
--	@replaceValue nvarchar(1000) = '',
--	@unit nvarchar(10),
--	@value int,
--	@productManager nvarchar(500),
--	@comment nvarchar(1500),
--	@price decimal(18,2) = -1,
--	@sumMax decimal(18,2) = -1,
--	@positionState int,
--  @currency int
--)
--as
--declare @result int;
--declare @count int;
--set @result = 0;
--set @count = (select count(*) from ClaimPosition where Deleted = 0 and IdClaim = @idClaim and RowNumber = @rowNumber and CatalogNumber = @catalogNumber
--	and Name = @name and ReplaceValue = @replaceValue and Unit = @unit and Value = @value and ProductManager = @productManager and
--	Comment = @comment and Price = @price and SumMax = @sumMax and PositionState = @positionState and Currency = @currency);
--if @count > 0
--begin
--	set @result = 1;
--end
--select @result;
--go

--use tenderProcessing
--go

--create procedure ChangeClaimPositionState
--(
--	@id int,
--	@positionState int
--)
--as
--update ClaimPosition set PositionState = @positionState where Deleted = 0 and Id = @id
--go

--use tenderProcessing
--go

--create procedure ChangePositionsState
--(
--	@ids nvarchar(max),
--	@state int
--)
--as
--update ClaimPosition set PositionState = @state where Deleted = 0 and Id in (select * from dbo.Split(@ids,','));
--go

--use tenderProcessing
--go

--create procedure GetProductsForClaim
--(
--	@id int
--)
--as
--select ProductManager from ClaimPosition where Deleted = 0 and IdClaim = @id and (PositionState = 1 or PositionState = 3)
--go

--use tenderProcessing
--go

--create procedure HasClaimPosition
--(
--	@id int
--)
--as
--select count(*) from ClaimPosition where Deleted = 0 and IdClaim = @id
--go

--use tenderProcessing
--go

--create procedure LoadProductManagersForClaim
--(
--	@idClaim int
--)
--as
--select distinct ProductManager from ClaimPosition where Deleted = 0 and IdClaim = @idClaim
--go

--use tenderProcessing
--go

--create procedure LoadClaimPositionForTenderClaim
--(
--	@id int
--)
--as
--select * from ClaimPosition where Deleted = 0 and IdClaim = @id
--go

--use tenderProcessing
--go

--create procedure LoadClaimPositionForTenderClaimForProduct
--(
--	@id int,
--	@product nvarchar(500)
--)
--as
--select * from ClaimPosition where Deleted = 0 and IdClaim = @id and ProductManager = @product
--go

--use tenderProcessing
--go

--create procedure SaveCalculateClaimPosition
--(
--	@idPosition int,
--	@idClaim int,
--	@catalogNumber nvarchar(500),
--	@name nvarchar(1000),
--	@replaceValue nvarchar(1000) = '',
--	@priceCurrency decimal(18,2) = -1,
--	@sumCurrency decimal(18,2) = -1,
--	@priceRub decimal(18,2) = -1,
--	@sumRub decimal(18,2),
--	@provider nvarchar(150) = '',
--	@protectFact int,
--	@protectCondition nvarchar(500) = '',
--	@comment nvarchar(1500) = '',
--	@author nvarchar(150),
--  @currency int
--)
--as
--declare @id int;
--insert into CalculateClaimPosition values(@idPosition, @idClaim, @catalogNumber, @name, @replaceValue, @priceCurrency, @sumCurrency,
--		@priceRub, @sumRub, @provider, @protectFact, @protectCondition, @comment, @author, 0, null, null, @currency)
--set @id = @@IDENTITY;
--select @id;
--go

--use tenderProcessing
--go

--create procedure UpdateCalculateClaimPosition
--(
--	@id int,
--	@catalogNumber nvarchar(500),
--	@name nvarchar(1000),
--	@replaceValue nvarchar(1000) = '',
--	@priceCurrency decimal(18,2) = -1,
--	@sumCurrency decimal(18,2) = -1,
--	@priceRub decimal(18,2) = -1,
--	@sumRub decimal(18,2),
--	@provider nvarchar(150) = '',
--	@protectFact int,
--	@protectCondition nvarchar(500) = '',
--	@comment nvarchar(1500) = '',
--	@author nvarchar(150),
--    @currency int
--)
--as
--Update CalculateClaimPosition set CatalogNumber = @catalogNumber, Name = @name, ReplaceValue = @replaceValue, 
--	PriceCurrency = @priceCurrency, SumCurrency = @sumCurrency, PriceRub = @priceRub, SumRub = @sumRub, Provider = @provider, 
--	ProtectFact = @protectFact, ProtectCondition = @protectCondition, Comment = @comment, Author = @author, Currency = @currency
--	where Id = @id
--go

--use tenderProcessing
--go

--create procedure DeleteCalculateClaimPosition
--(
--	@id int,
--	@deletedUser nvarchar(150),
--	@date datetime
--)
--as
--update CalculateClaimPosition set Deleted = 1, DeletedUser = @deletedUser, DeleteDate = @date where Id = @id
--go

--use tenderProcessing
--go

--create procedure LoadCalculateClaimPositionForClaim
--(
--	@id int
--)
--as
--select * from CalculateClaimPosition where Deleted = 0 and IdClaim = @id
--go

--use tenderProcessing
--go

--create procedure DeleteCalculatePositionForClaim
--(
--	@id int
--)
--as
--delete from CalculateClaimPosition where IdClaim = @id
--go

--use tenderProcessing
--go

--create procedure SaveClaimStatusHistory
--(
--	@idClaim int,
--	@idStatus int,
--	@comment nvarchar(1000) = '',
--	@idUser nvarchar(500),
--	@recordDate datetime
--)
--as
--insert into ClaimStatusHistory values(@recordDate, @idClaim, @idStatus, @comment, @idUser)
--go

--use tenderProcessing
--go

--create procedure DeleteCalculateForPositions
--(
--    @idClaim int,
--	@ids nvarchar(max)
--)
--as
--delete from CalculateClaimPosition where IdClaim = @idClaim and IdPosition in(select * from dbo.Split(@ids,','));
--go

--use tenderProcessing
--go

--create procedure LoadStatusHistoryForClaim
--(
--	@idClaim int
--)
--as
--select [ClaimStatusHistory].*, Value from ClaimStatusHistory, ClaimStatus where IdClaim = @idClaim and IdStatus = [ClaimStatus].Id order by RecordDate
--go

--use tenderProcessing
--go

--create procedure LoadLastStatusHistoryForClaim
--(
--	@idClaim int
--)
--as
--select top(1) [ClaimStatusHistory].*, Value from ClaimStatusHistory, ClaimStatus where IdClaim = @idClaim and IdStatus = [ClaimStatus].Id order by RecordDate desc
--go

--use tenderProcessing
--go

--create procedure LoadCurrencies
--as
--select * from Currency
--go

--use tenderProcessing
--go

--create procedure LoadDealTypes
--as
--select * from DealType
--go

--use tenderProcessing
--go

--create procedure LoadClaimStatus
--as
--select * from ClaimStatus
--go

--use tenderProcessing
--go

--create procedure LoadTenderStatus
--as
--select * from TenderStatus
--go

--use tenderProcessing
--go

--create procedure LoadPositionStates
--as
--select * from PositionState
--go

--use tenderProcessing
--go

--create procedure LoadProtectFacts
--as
--select * from ProtectFact
--go

--use tenderProcessing
--go

--create procedure LoadRoles
--as
--select * from Roles
--go

--use tenderProcessing
--go

--insert into DealType values(1, N'Аукцион');
--insert into DealType values(2, N'Котировка');
--insert into DealType values(3, N'Открытый запрос цен');
--insert into DealType values(4, N'Открытый запрос предложений');
--insert into DealType values(5, N'Закрытый запрос цен');
--insert into DealType values(6, N'Закрытый запрос предложений');
--insert into DealType values(7, N'Открытый конкурс');
--insert into DealType values(8, N'Закрытый конкурс');

--insert into ClaimStatus values(1, N'Создано');
--insert into ClaimStatus values(2, N'Передано');
--insert into ClaimStatus values(3, N'В работе');
--insert into ClaimStatus values(4, N'Приостановлено');
--insert into ClaimStatus values(5, N'Отменено');
--insert into ClaimStatus values(6, N'Частично расчитано');
--insert into ClaimStatus values(7, N'Рассчитано');
--insert into ClaimStatus values(8, N'Подтверждено');

--insert into PositionState values(1, N'Создана');
--insert into PositionState values(2, N'Отправлена');
--insert into PositionState values(3, N'Отклонена');
--insert into PositionState values(4, N'Подтверждена');

--insert into TenderStatus values(1, N'В процессе');
--insert into TenderStatus values(2, N'Выигран');
--insert into TenderStatus values(3, N'Проигран');
--insert into TenderStatus values(4, N'Отказ');

--insert into ProtectFact values(1, N'Получена нами');
--insert into ProtectFact values(2, N'Получена конкурентом');
--insert into ProtectFact values(3, N'Не предоставляется');

--insert into Roles values(1, 'S-1-5-21-1970802976-3466419101-4042325969-4287', 'SpeCalc-Operator');
--insert into Roles values(2, 'S-1-5-21-1970802976-3466419101-4042325969-4283', 'SpeCalc-Manager');
--insert into Roles values(3, 'S-1-5-21-1970802976-3466419101-4042325969-4284', 'SpeCalc-Product');
--insert into Roles values(4, 'S-1-5-21-1970802976-3466419101-4042325969-4286', 'SpeCalc-Kontroler');
--insert into Roles values(5, 'S-1-5-21-1970802976-3466419101-4042325969-4285', 'SpeCalc-Konkurs');
--insert into Roles values(6, 'S-1-5-21-1970802976-3466419101-4042325969-4282', 'SpeCalc-Enter');
--insert into Roles values(7, 'S-1-5-21-1970802976-3466419101-4042325969-4296', 'SpeClac-ExpiredNote');

--insert into Currency values(1, N'руб');
--insert into Currency values(2, N'USD');
--insert into Currency values(3, N'EUR');

--go