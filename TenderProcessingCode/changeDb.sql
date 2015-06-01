--use tenderProcessing
--go

--alter table TenderClaim
--add CurrencyUsd decimal(18,2) not null default -1

--alter table TenderClaim
--add CurrencyEur decimal(18,2) not null default -1

--alter table TenderClaim
--add DeliveryDate datetime null 

--alter table TenderClaim
--add DeliveryPlace nvarchar(1000) not null default ''

--alter table TenderClaim
--add AuctionDate datetime null

--alter table ClaimPosition
--add PriceTzr decimal(18,2) not null default -1

--alter table ClaimPosition
--add SumTzr decimal(18,2) not null default -1

--alter table ClaimPosition
--add PriceNds decimal(18,2) not null default -1

--alter table ClaimPosition
--add SumNds decimal(18,2) not null default -1
--go

--use tenderProcessing
--go

--alter procedure SaveTenderClaim
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
--	@currencyUsd decimal(18,2) = -1,
--	@currencyEur decimal(18,2) = -1,
--	@deliveryDate datetime = null,
--	@deliveryPlace nvarchar(1000) = '',
--	@auctionDate datetime = null,
--	@deleted bit
--)
--as
--declare @id int;
--insert into TenderClaim values(@tenderNumber, @tenderStart, @claimDeadline, @kPDeadline, @comment, @customer, 
--	@customerInn, @totalSum, @dealType, @tenderUrl, @tenderStatus, @manager, @managerSubDivision, @claimStatus, @recordDate, 
--	@deleted, @author, null, null, @currencyUsd, @currencyEur, @deliveryDate, @deliveryPlace, @auctionDate)
--set @id = @@IDENTITY;
--select @id;
--go

--use tenderProcessing
--go

--create procedure UpdateTenderClaimCurrency
--(
--	@id int,
--	@currencyUsd decimal(18,2) = -1,
--	@currencyEur decimal(18,2) = -1
--)
--as
--if @currencyUsd != -1
--begin 
--	update TenderClaim set CurrencyUsd = @currencyUsd where Id = @id
--end
--if @currencyEur != -1
--begin 
--	update TenderClaim set CurrencyEur = @currencyEur where Id = @id
--end
--go

--use tenderProcessing
--go

--create procedure ChangePositionsProduct
--(
--	@ids nvarchar(max),
--	@product nvarchar(500)
--)
--as
--update ClaimPosition set ProductManager = @product where Deleted = 0 and Id in (select * from dbo.Split(@ids,','));
--go

--use tenderProcessing
--go

--alter procedure SaveClaimPosition
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
--   @currency int,
--	@priceTzr decimal(18,2) = -1,
--	@sumTzr decimal(18,2) = -1,
--	@priceNds decimal(18,2) = -1,
--	@sumNds decimal(18,2) = -1
--)
--as
--declare @id int;
--insert into ClaimPosition values(@idClaim, @rowNumber, @catalogNumber, @name, @replaceValue, @unit,
--	@value, @productManager, @comment, @price, @sumMax, @positionState, @author, 0, null, null, @currency,
--	@priceTzr, @sumTzr, @priceNds, @sumNds)
--set @id = @@IDENTITY;
--select @id;
--go

--use tenderProcessing
--go

--alter procedure UpdateClaimPosition
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
--  @currency int,
--	@priceTzr decimal(18,2) = -1,
--	@sumTzr decimal(18,2) = -1,
--	@priceNds decimal(18,2) = -1,
--	@sumNds decimal(18,2) = -1
--)
--as
--update ClaimPosition set RowNumber = @rowNumber, CatalogNumber = @catalogNumber, Name = @name, 
--	ReplaceValue = @replaceValue, Unit = @unit, Value = @value, ProductManager = @productManager, 
--	Comment = @comment, Price = @price, SumMax = @sumMax, PositionState = @positionState, Author = @author,
--	Currency = @currency, PriceTzr = @priceTzr, SumTzr = @sumTzr, PriceNds = @priceNds, SumNds = @sumNds where Id = @id
--go

--use tenderProcessing
--go

--alter procedure ExistsClaimPosition
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
--  @currency int,
--	@priceTzr decimal(18,2) = -1,
--	@sumTzr decimal(18,2) = -1,
--	@priceNds decimal(18,2) = -1,
--	@sumNds decimal(18,2) = -1
--)
--as
--declare @result int;
--declare @count int;
--set @result = 0;
--set @count = (select count(*) from ClaimPosition where Deleted = 0 and IdClaim = @idClaim and RowNumber = @rowNumber and CatalogNumber = @catalogNumber
--	and Name = @name and ReplaceValue = @replaceValue and Unit = @unit and Value = @value and ProductManager = @productManager and
--	Comment = @comment and Price = @price and SumMax = @sumMax and PositionState = @positionState and Currency = @currency
--  and PriceTzr = @priceTzr and SumTzr = @sumTzr and PriceNds = @priceNds and SumNds = @sumNds);
--if @count > 0
--begin
--	set @result = 1;
--end
--select @result;
--go