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
				update organization_detail set
					OrganizationName					=			_OrganizationName,
					OrgMobileNo							=			_OrgMobileNo,
					OrgEmail							=			_OrgEmail,
					OrgPrimaryPhoneNo					=			_OrgPrimaryPhoneNo,
					OrgSecondaryPhoneNo					=			_OrgSecondaryPhoneNo,
					OrgFax								=			_OrgFax,
					UpdatedBy							=			_AdminId,
                    UpdatedOn							=			utc_date()
				where OrganizationId = _OrganizationId;
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
            else
            begin
				update company set
					OrganizationId					=			_OrganizationId,
					OrganizationName				=			_OrganizationName,
					CompanyName						=			_CompanyName,
					CompanyDetail					=			_CompanyDetail,
					SectorType						=			_SectorType,
					Country							=			_Country,
					State							=			_State,
					City							=			_City,
					FirstAddress					=			_FirstAddress,
					SecondAddress					=			_SecondAddress,
					ThirdAddress					=			_ThirdAddress,
					ForthAddress					=			_ForthAddress,
					FullAddress						=			_FullAddress,
					MobileNo						=			_MobileNo,
					Email							=			_Email,
					FirstEmail						=			_FirstEmail,
					SecondEmail						=			_SecondEmail,
					ThirdEmail						=			_ThirdEmail,
					ForthEmail						=			_ForthEmail,
					PrimaryPhoneNo					=			_PrimaryPhoneNo,
					SecondaryPhoneNo				=			_SecondaryPhoneNo,
					Fax								=			_Fax,
					Pincode							=			_Pincode,
					FileId							=			_FileId,
					LegalDocumentPath				=			_LegalDocumentPath,
					LegalEntity						=			_LegalEntity,
					LegalNameOfCompany				=			_LegalNameOfCompany,
					TypeOfBusiness					=			_TypeOfBusiness,
					InCorporationDate				=			_InCorporationDate,
					IsPrimaryCompany				=			_IsPrimaryCompany,
					FixedComponentsId				=			_FixedComponentsId,
					UpdatedBy						=			_AdminId,
                    UpdatedOn						=			utc_date()
				where CompanyId 	= 	_CompanyId;
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
            else
            begin
				update bank_accounts set
					BankName							=			_BankName,
					BranchCode							=			_BranchCode,
					Branch								=			_Branch,
					IFSC								=			_IFSC,
					AccountNo							=			_AccountNo,
					OpeningDate							=			_OpeningDate,
					ClosingDate							=			_ClosingDate,
					PANNo								=			_PANNo,
					GSTNo								=			_GSTNo,
					TradeLicenseNo						=			_TradeLicenseNo,
					UpdatedBy							=			_AdminId,
                    UpdatedOn							=			utc_date()
				where BankAccountId = _BankAccountId;
            end;
			end if;
		end;
		Commit;
		Set _ProcessingResult = 'updated';
	End;
End$$
DELIMITER ;
