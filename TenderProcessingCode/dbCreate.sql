--use master
--go
--create database tenderProcessing
--go

--use tenderProcessing
--go

--create table ClaimStatus
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
--	primary key(Id),
--	CONSTRAINT FK_ClaimPosition_TenderClaim FOREIGN KEY(IdClaim)
--		REFERENCES TenderClaim(Id)
--    ON DELETE CASCADE
--    ON UPDATE CASCADE,
--	CONSTRAINT FK_ClaimPosition_PositionState FOREIGN KEY(PositionState)
--		REFERENCES PositionState(Id)
--    ON DELETE CASCADE
--    ON UPDATE CASCADE,
--)

--create table CalculateClaimPosition
--(
--	Id int identity not null,
--	IdPosition int not null,
--	IdClaim int not null,
--	CatalogNumber nvarchar(500) not null,
--	Name nvarchar(1000) not null,
--	ReplaceValue nvarchar(1000),
--	PriceUsd decimal(18,2),
--	SumUsd decimal(18,2),
--	PriceRub decimal(18,2),
--	SumRub decimal(18,2) not null,
--	Provider nvarchar(150),
--	ProtectFact nvarchar(100) not null,
--	ProtectCondition nvarchar(500),
--	Comment nvarchar(1000),
--	primary key(Id),
--	CONSTRAINT FK_CalculateClaimPosition_ClaimPosition FOREIGN KEY(IdPosition)
--		REFERENCES ClaimPosition(Id)
--    ON DELETE CASCADE
--    ON UPDATE CASCADE,
--	CONSTRAINT FK_CalculateClaimPosition_TenderClaim FOREIGN KEY(IdClaim)
--		REFERENCES TenderClaim(Id)
--     ON DELETE NO ACTION
--     ON UPDATE NO ACTION
--)

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
--	@deleted bit
--)
--as
--declare @id int;
--insert into TenderClaim values(@tenderNumber, @tenderStart, @claimDeadline, @kPDeadline, @comment, @customer, 
--	@customerInn, @totalSum, @dealType, @tenderUrl, @tenderStatus, @manager, @managerSubDivision, @claimStatus, @recordDate, @deleted)
--set @id = @@IDENTITY;
--select @id;
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
--	@priceUsd decimal(18,2) = -1,
--	@sumUsd decimal(18,2) = -1,
--	@priceRub decimal(18,2) = -1,
--	@sumRub decimal(18,2),
--	@provider nvarchar(150) = '',
--	@protectFact nvarchar(100),
--	@protectCondition nvarchar(500) = '',
--	@comment nvarchar(1500) = ''
--)
--as
--declare @id int;
--insert into CalculateClaimPosition values(@idPosition, @idClaim, @catalogNumber, @name, @replaceValue, @priceUsd, @sumUsd,
--		@priceRub, @sumRub, @provider, @protectFact, @protectCondition, @comment)
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
--	@priceUsd decimal(18,2) = -1,
--	@sumUsd decimal(18,2) = -1,
--	@priceRub decimal(18,2) = -1,
--	@sumRub decimal(18,2),
--	@provider nvarchar(150) = '',
--	@protectFact nvarchar(100),
--	@protectCondition nvarchar(500) = '',
--	@comment nvarchar(1500) = ''
--)
--as
--Update CalculateClaimPosition set CatalogNumber = @catalogNumber, Name = @name, ReplaceValue = @replaceValue, 
--	PriceUsd = @priceUsd, SumUsd = @sumUsd, PriceRub = @priceRub, SumRub = @sumRub, Provider = @provider, 
--	ProtectFact = @protectFact, ProtectCondition = @protectCondition, Comment = @comment
--	where Id = @id
--go

--use tenderProcessing
--go

--create procedure DeleteCalculateClaimPosition
--(
--	@id int
--)
--as
--delete from CalculateClaimPosition where Id = @id
--go

--use tenderProcessing
--go

--create procedure LoadCalculateClaimPositionForClaim
--(
--	@id int
--)
--as
--select * from CalculateClaimPosition where IdClaim = @id
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

--create procedure LoadTenderClaims
--(
--	@pageSize int
--)
--as
--select top (@pageSize) * from TenderClaim where deleted = 0
--go

--use tenderProcessing
--go

--create procedure LoadTenderClaimById
--(
--	@id int
--)
--as
--select * from TenderClaim where deleted = 0 and Id = @id
--go

--use tenderProcessing
--go

--create procedure LoadClaimPositionForTenderClaim
--(
--	@id int
--)
--as
--select * from ClaimPosition where IdClaim = @id
--go

--use tenderProcessing
--go

--create procedure DeleteTenderClaims
--(
--	@id int
--)
--as
--update TenderClaim set Deleted = 1 where Id = @id
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
--	@positionState int
--)
--as
--declare @id int;
--insert into ClaimPosition values(@idClaim, @rowNumber, @catalogNumber, @name, @replaceValue, @unit,
--	@value, @productManager, @comment, @price, @sumMax, @positionState)
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
--	@positionState int
--)
--as
--update ClaimPosition set RowNumber = @rowNumber, CatalogNumber = @catalogNumber, Name = @name, 
--	ReplaceValue = @replaceValue, Unit = @unit, Value = @value, ProductManager = @productManager, 
--	Comment = @comment, Price = @price, SumMax = @sumMax, PositionState = @positionState where Id = @id
--go

--use tenderProcessing
--go

--create procedure DeleteClaimPosition
--(
--	@id int
--)
--as
--delete from ClaimPosition where Id = @id
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
--	@positionState int
--)
--as
--declare @result int;
--declare @count int;
--set @result = 0;
--set @count = (select count(*) from ClaimPosition where IdClaim = @idClaim and RowNumber = @rowNumber and CatalogNumber = @catalogNumber
--	and Name = @name and ReplaceValue = @replaceValue and Unit = @unit and Value = @value and ProductManager = @productManager and
--	Comment = @comment and Price = @price and SumMax = @sumMax and PositionState = @positionState);
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
--update ClaimPosition set PositionState = @positionState where Id = @id
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

--create procedure HasClaimPosition
--(
--	@id int
--)
--as
--select count(*) from ClaimPosition where IdClaim = @id
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
--insert into ClaimStatus values(7, N'Расчитано');
--insert into ClaimStatus values(8, N'Подтверждено');

--insert into PositionState values(1, N'Создана');
--insert into PositionState values(2, N'Отправлена');
--insert into PositionState values(3, N'Отклонена');
--insert into PositionState values(4, N'Подтверждена');

--insert into TenderStatus values(1, N'В процессе');
--insert into TenderStatus values(2, N'Выигран');
--insert into TenderStatus values(3, N'Проигран');
--insert into TenderStatus values(4, N'Отказ');

--go