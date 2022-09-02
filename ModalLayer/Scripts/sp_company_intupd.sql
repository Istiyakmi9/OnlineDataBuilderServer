DELIMITER $$

drop procedure if exists sp_company_intupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_company_intupd`(
	_CompanyId int,
	_OrganizationName varchar(250),
    _CompanyName varchar(150),
    _CompanyDetail varchar (250),
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
    _PrimaryPhoneNo varchar(20),
    _SecondaryPhoneNo varchar(20),
    _Fax varchar(50),
    _Pincode int,
    _FileId bigint,
    _PANNumber varchar(20),
    _TradeLicenseNumber varchar(50),
    _GSTNO varchar(50),
    _LegalDocumentPath varchar(250),
    _LegalEntity varchar(50),
    _LegalNameOfCompany varchar(100),
    _TypeOfBusiness varchar(150),    
    _InCorporationDate datetime,
    _AccountNo varchar(25),
    _BankName varchar(100),
    _BranchName varchar(100),
    _IFSC varchar(15),
	out _ProcessingResult varchar(50)
    
/*


	set @outcome = '';
    
	Call sp_company_intupd(0, 'BottomHlaf', 'BottomHalf Pvt. Ltd.', null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_company_intupd', 1, 0, @Result);
			end;  
                        
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
            
		
			if not exists(select 1 from company where CompanyId = _CompanyId) then
            begin
				Insert into company values(
					default,
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
					_PrimaryPhoneNo,
					_SecondaryPhoneNo,
					_Fax,
					_Pincode,
					_FileId,
					_PANNumber,
					_TradeLicenseNumber,
					_GSTNO,
					_LegalDocumentPath,
					_LegalEntity,
					_LegalNameOfCompany,
					_TypeOfBusiness,    
					_InCorporationDate,
                    _AccountNo,
                    _BankName,
                    _BranchName,
                    _IFSC,
					now()
				);
                
                Set _ProcessingResult = 'inserted';
			end;
            else 
            begin
				Update company Set
					OrganizationName			=			_OrganizationName,
                    CompanyName					=			_CompanyName,
                    CompanyDetail				=			_CompanyDetail,
					SectorType                 	=			_SectorType,
					Country                		=			_Country,
					State                  		=			_State,
					City                 		=			_City,
					FirstAddress                =			_FirstAddress,
					SecondAddress               =			_SecondAddress,
					ThirdAddress                =			_ThirdAddress,
					ForthAddress               =			_ForthAddress,
					FullAddress                 =			_FullAddress,
					MobileNo                 	=			_MobileNo,
					Email                  		=			_Email,
					PrimaryPhoneNo              =			_PrimaryPhoneNo,
					SecondaryPhoneNo            =			_SecondaryPhoneNo,
					Fax                  		=			_Fax,
					Pincode                  	=			_Pincode,
					FileId                 		=			_FileId,
					PANNumber                  	=			_PANNumber,
					TradeLicenseNumber          =			_TradeLicenseNumber,
					GSTNO		                =			_GSTNO,
					LegalDocumentPath           =			_LegalDocumentPath,
					LegalEntity                 =			_LegalEntity,
					LegalNameOfCompany          =			_LegalNameOfCompany,
					TypeOfBusiness              =			_TypeOfBusiness,    
					InCorporationDate           =			_InCorporationDate,
                     AccountNo					=			_AccountNo,
                    BankName					=			_BankName,
                    BranchName					=			_BranchName,
                    IFSC						=			_IFSC,
					UpdatedOn					=			now()
                where CompanyId = _CompanyId;
                
                Set _ProcessingResult = 'updated';
            end;
            end if;
	End;
End$$
DELIMITER ;
