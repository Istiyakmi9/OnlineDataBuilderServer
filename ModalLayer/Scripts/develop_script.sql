drop table if exists bank_accounts;

CREATE TABLE `bank_accounts` (
  `BankAccountId` int NOT NULL AUTO_INCREMENT,
  `OrganizationId` int DEFAULT NULL,
  `CompanyId` int DEFAULT NULL,
  `BankName` varchar(100) DEFAULT NULL,
  `BranchCode` varchar(20) DEFAULT NULL,
  `Branch` varchar(50) DEFAULT NULL,
  `IFSC` varchar(20) DEFAULT NULL,
  `AccountNo` varchar(45) DEFAULT NULL,
  `OpeningDate` datetime DEFAULT NULL,
  `ClosingDate` datetime DEFAULT NULL,
  `PANNo` varchar(20) DEFAULT NULL,
  `GSTNo` varchar(50) DEFAULT NULL,
  `TradeLicenseNo` varchar(50) DEFAULT NULL,
  CreatedBy long not null,
  UpdatedBy long null,
  CreatedOn datetime not null,
  UpdatedOn datetime null,
  PRIMARY KEY (`BankAccountId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


drop table if exists organization_detail;


CREATE TABLE `organization_detail` (
  `OrganizationId` int NOT NULL AUTO_INCREMENT,
  `OrganizationName` varchar(250) DEFAULT NULL,  
  `OrgMobileNo` varchar(20) DEFAULT NULL,
  `OrgEmail` varchar(50) DEFAULT NULL,
  `OrgPrimaryPhoneNo` varchar(20) DEFAULT NULL,
  `OrgSecondaryPhoneNo` varchar(20) DEFAULT NULL,
  `OrgFax` varchar(50) DEFAULT NULL,  
  CreatedBy long not null,
  UpdatedBy long null,
  CreatedOn datetime not null,
  UpdatedOn datetime null,
  PRIMARY KEY (`OrganizationId`)
);


drop table if exists company;
CREATE TABLE `company` (
  `CompanyId` int NOT NULL AUTO_INCREMENT,
  `OrganizationId` int NOT NULL,
  `OrganizationName` varchar(250) DEFAULT NULL,
  `CompanyName` varchar(150) DEFAULT NULL,
  `CompanyDetail` varchar(250) DEFAULT NULL,
  `SectorType` int DEFAULT NULL,
  `Country` varchar(50) DEFAULT NULL,
  `State` varchar(100) DEFAULT NULL,
  `City` varchar(100) DEFAULT NULL,
  `FirstAddress` varchar(100) DEFAULT NULL,
  `SecondAddress` varchar(100) DEFAULT NULL,
  `ThirdAddress` varchar(100) DEFAULT NULL,
  `ForthAddress` varchar(100) DEFAULT NULL,
  `FullAddress` varchar(150) DEFAULT NULL,
  `MobileNo` varchar(20) DEFAULT NULL,
  `Email` varchar(50) DEFAULT NULL,
  `FirstEmail` varchar(100) DEFAULT NULL,
  `SecondEmail` varchar(100) DEFAULT NULL,
  `ThirdEmail` varchar(100) DEFAULT NULL,
  `ForthEmail` varchar(100) DEFAULT NULL,
  `PrimaryPhoneNo` varchar(20) DEFAULT NULL,
  `SecondaryPhoneNo` varchar(20) DEFAULT NULL,
  `Fax` varchar(50) DEFAULT NULL,
  `Pincode` int DEFAULT NULL,
  `FileId` bigint DEFAULT NULL,
  `LegalDocumentPath` varchar(250) DEFAULT NULL,
  `LegalEntity` varchar(50) DEFAULT NULL,
  `LegalNameOfCompany` varchar(100) DEFAULT NULL,
  `TypeOfBusiness` varchar(150) DEFAULT NULL,
  `InCorporationDate` datetime DEFAULT NULL,
  `IsPrimaryCompany` bit(1) DEFAULT b'0',
  `FixedComponentsId` json DEFAULT NULL,
  CreatedBy long not null,
  UpdatedBy long null,
  CreatedOn datetime not null,
  UpdatedOn datetime null,
  PRIMARY KEY (`CompanyId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;




Go

DELIMITER $$

drop procedure if exists sp_organization_intupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_organization_intupd`(
	_CompanyId int,
	_OrganizationId int,
	_OrganizationName varchar(250),
	_CompanyName varchar(150),
	_CompanyDetail varchar(250),
	_SectorType int,
	_Country varchar(50),
	_State varchar(100),
	_City varchar(100),
	_FirstAddress varchar(100),
	_SecondAddress varchar(100),
	_ThirdAddress varchar(100),
	_ForthAddress varchar(100),
	_FullAddress varchar(150),
	_MobileNo varchar(20),
	_Email varchar(50),
	_FirstEmail varchar(100),
	_SecondEmail varchar(100),
	_ThirdEmail varchar(100),
	_ForthEmail varchar(100),
	_PrimaryPhoneNo varchar(20),
	_SecondaryPhoneNo varchar(20),
	_Fax varchar(50),
	_Pincode int,
	_FileId bigint,
	_LegalDocumentPath varchar(250),
	_LegalEntity varchar(50),
	_LegalNameOfCompany varchar(100),
	_TypeOfBusiness varchar(150),
	_InCorporationDate datetime,
	_IsPrimaryCompany bit(1),
	_FixedComponentsId json,    
	_BankAccountId int,
	_BankName varchar(100),
	_BranchCode varchar(20),
	_Branch varchar(50),
	_IFSC varchar(20),
	_AccountNo varchar(45),
	_OpeningDate datetime,
	_ClosingDate datetime,
	_PANNo varchar(20),
	_GSTNo varchar(50),
	_TradeLicenseNo varchar(50),
	_OrgMobileNo varchar(20),
	_OrgEmail varchar(50),
	_OrgPrimaryPhoneNo varchar(20),
	_OrgSecondaryPhoneNo varchar(20),
	_OrgFax varchar(50),
    _AdminId long,    
	out _ProcessingResult varchar(50)
    
/*


	set @outcome = '';
    
	Call sp_organization_intupd(0, 'BottomHlaf', 'BottomHalf Pvt. Ltd.', null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
    null, null, null, null, null, null, @outcome);    
    
    select @outcome;
    

*/
	
)
Begin
    Set @OperationStatus = '';
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Rollback;
			Call sp_LogException (@Message, @OperationStatus, 'sp_organization_intupd', 1, 0, @Result);
		end;  
        
		Start Transaction;
		begin
			if not exists(select 1 from company where CompanyId = _CompanyId) then
			begin
				if exists(select 1 from company where lower(LegalNameOfCompany) = lower(_LegalNameOfCompany)) then
				begin
					select CompanyId from company 
					where lower(LegalNameOfCompany) = lower(_LegalNameOfCompany) into _CompanyId;
				end;
				end if;
			end;
			end if;
			
            
			if not exists(select * from organization_detail where OrganizationId = _OrganizationId) then
			begin               
				Set @@SESSION.information_schema_stats_expiry = 0;

				SELECT AUTO_INCREMENT into _OrganizationId
				FROM information_schema.tables
				WHERE table_name = 'organization_detail'
				AND table_schema = DATABASE();	
				
				insert into organization_detail values(
					_OrganizationId,
					_OrganizationName,
					_OrgMobileNo,
					_OrgEmail,
					_OrgPrimaryPhoneNo,
					_OrgSecondaryPhoneNo,
					_OrgFax,
					_AdminId,
					null,
					utc_date(),
					null
				);
			end;
            else
            begin
				insert into organization_detail values(
					_OrganizationId,
					_OrganizationName,
					_OrgMobileNo,
					_OrgEmail,
					_OrgPrimaryPhoneNo,
					_OrgSecondaryPhoneNo,
					_OrgFax,
					_AdminId,
					null,
					utc_date(),
					null
				);
            end;
			end if;
					
			if not exists(select 1 from company where CompanyId = _CompanyId) then
			begin
				Set @@SESSION.information_schema_stats_expiry = 0;

				SELECT AUTO_INCREMENT into _CompanyId
				FROM information_schema.tables
				WHERE table_name = 'company'
				AND table_schema = DATABASE();	
				
				Insert into company values(
					_CompanyId,
					_OrganizationId,
					_OrganizationName,
					_CompanyName,
					_CompanyDetail,
					_SectorType,
					_Country,
					_State,
					_City,
					_FirstAddress,
					_SecondAddress,
					_ThirdAddress,
					_ForthAddress,
					_FullAddress,
					_MobileNo,
					_Email,
					_FirstEmail,
					_SecondEmail,
					_ThirdEmail,
					_ForthEmail,
					_PrimaryPhoneNo,
					_SecondaryPhoneNo,
					_Fax,
					_Pincode,
					_FileId,
					_LegalDocumentPath,
					_LegalEntity,
					_LegalNameOfCompany,
					_TypeOfBusiness,
					_InCorporationDate,
					_IsPrimaryCompany,
					_FixedComponentsId,
					_AdminId,
					null,
					utc_date(),
					null
				);
			end;
			end if;
			
			if not exists(select 1 from bank_accounts where BankAccountId = _BankAccountId) then
			begin
				insert into bank_accounts value(
					default,
					_OrganizationId,
					_CompanyId,
					_BankName,
					_BranchCode,
					_Branch,
					_IFSC,
					_AccountNo,
					_OpeningDate,
					_ClosingDate,
					_PANNo,
					_GSTNo,
					_TradeLicenseNo,
					_AdminId,
					null,
					utc_date(),
					null
				);
			end;
			end if;
		end;
		Commit;
		Set _ProcessingResult = 'updated';
	End;
End$$
DELIMITER ;


Go

DELIMITER $$

drop procedure if exists sp_organization_detail_get $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_organization_detail_get`(
    
/*

	Call sp_organization_detail_get();
    
*/
	
)
Begin
    Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
    		Call sp_LogException (@Message, '', 'sp_organization_detail_get', 1, 0, @Result);
		end;  
        
		select 
			c.CompanyId,
			o.OrganizationId,
			o.OrganizationName,
			c.CompanyName,
			c.CompanyDetail,
			c.SectorType,
			c.Country,
			c.State,
			c.City,
			c.FirstAddress,
			c.SecondAddress,
			c.ThirdAddress,
			c.ForthAddress,
			c.FullAddress,
			c.MobileNo,
			c.Email,
			c.FirstEmail,
			c.SecondEmail,
			c.ThirdEmail,
			c.ForthEmail,
			c.PrimaryPhoneNo,
			c.SecondaryPhoneNo,
			c.Fax,
			c.Pincode,
			c.FileId,
			c.LegalDocumentPath,
			c.LegalEntity,
			c.LegalNameOfCompany,
			c.TypeOfBusiness,
			c.InCorporationDate,
			c.IsPrimaryCompany,
			c.FixedComponentsId,
			b.BankAccountId,
			b.BankName,
			b.BranchCode,
			b.Branch,
			b.IFSC,
			b.AccountNo,
			b.OpeningDate,
			b.ClosingDate,
			b.PANNo,
			b.GSTNo,
			TradeLicenseNo,
			o.OrgMobileNo,
			o.OrgEmail,
			o.OrgPrimaryPhoneNo,
			o.OrgSecondaryPhoneNo,
			o.OrgFax
        from organization_detail o
        inner join company c on o.OrganizationId = c.OrganizationId
        inner join bank_accounts b on b.OrganizationId = c.OrganizationId;
        
        select * from filedetail
        where FileId = 0;
	End;
End$$
DELIMITER ;
