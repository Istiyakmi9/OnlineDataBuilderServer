DELIMITER $$

drop procedure if exists sp_EmployeeBillDetail_ById $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_EmployeeBillDetail_ById`(
	_EmployeeId bigint,
    _ClientId bigint,
    _FileId bigint,
    _ForYear int,
    _ForMonth int,
	_UserTypeId int,
    _AdminId bigint
/*	

	Call sp_EmployeeBillDetail_ById(2, 1, 1, 2022, 9, 2, 1);
    
    
*/

)
Begin
    Begin
		Declare exit handler for sqlexception
		Begin
		
			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
										
			Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
            RollBack;
			Call sp_LogException(@Message, '', 'sp_EmployeeBillDetail_ById', 1, 0, @Result);
		End;

		select b.* from billdetail b
        Where b.EmployeeUid = _EmployeeId 
        And b.ClientId = _ClientId
        And b.FileDetailId = _FileId;
        
        Call SP_Employee_GetAll(concat(' 1=1 and emp.EmployeeUid = ', _EmployeeId), null, 1, 10);
        
		Select 
			a.*
		from employee_timesheet a   
		Where a.EmployeeId = _EmployeeId
		And _ForYear = a.ForYear and _ForMonth = a.ForMonth
		And a.UserTypeId = _UserTypeId
        And a.ClientId = _ClientId;
	End;
end$$
DELIMITER ;


DELIMITER $$

drop procedure if exists sp_Employeelogin_Auth $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employeelogin_Auth`(

/*

	Call sp_Employeelogin_Auth(0, NULL, 'istiyaq.mi9@gmail.com', 1, 1000);
    
    Call sp_Employeelogin_Auth(0, null, 'mgb@gmail.com', 1, 1000);

*/
	_UserId bigint,
	_MobileNo varchar(20),
	_EmailId varchar(50),
    _UserTypeId int,
    _PageSize int
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_Employeelogin_Auth', 1, 0, @Result);
			end;  
		
		Set @RoutePrefix = 'admin';
		Set @AccessLevelId = 0;
        Set @CompanyId = 0;
		Select AccessLevelId, CompanyId from employeelogin
		Where (Email = _EmailId Or Mobile = _MobileNo)
        And UserTypeId = _UserTypeId
		into @AccessLevelId, @CompanyId;
	   
		if(_UserTypeId = 2) then
		begin
			Set @RoutePrefix = 'user';
		end;
		end if;

		Select
			e.EmployeeUid UserId,
			e.FirstName,
			e.LastName,
			'NA' Address,
			e.Email,
			e.Mobile,
            e.ReportingManagerId,
            e.DesignationId,
			@AccessLevelId RoleId,
			_UserTypeId UserTypeId,
            l.OrganizationId,
            l.CompanyId
		from employees e
        Inner join employeelogin l on l.EmployeeId = e.EmployeeUid
		Where e.Email = _EmailId Or e.Mobile = _MobileNo;
        
        select * from employees e
        Where e.Email = _EmailId Or e.Mobile = _MobileNo;
	   
		if(@AccessLevelId = 1) then
		begin
			Select RM.Catagory, RM.Childs, concat(@RoutePrefix, '/', RM.Link) Link, RM.Icon, RM.Badge,
			RM.BadgeType, RM.AccessCode,  1 as Permission from rolesandmenu RM
			where Catagory <> 'Home' or Childs <> 'Home';
		end;
		else
		begin
			Select RM.Catagory, RM.Childs, concat(@RoutePrefix, '/', RM.Link) Link, RM.Icon, RM.Badge,
			RM.BadgeType, RM.AccessCode,
			AccessibilityId Permission from rolesandmenu RM
			left Join role_accessibility_mapping r on r.AccessCode = RM.AccessCode
			where r.AccessLevelId = @AccessLevelId
			and r.AccessibilityId > 0;
		end;
		end if;
        
        if (_UserTypeId = 1) then
        begin
			select * from (	
				Select 
					EmployeeUid, 
					Concat(FirstName, ' ', LastName) Name
				from employees
                where CompanyId = @CompanyId And IsActive = true
				Order by  UpdatedOn Desc, CreatedOn Desc
			)T limit _PageSize offset 0;
		end;
        end if;
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_employee_autocomplete_data $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_autocomplete_data`(
/*	

	Call sp_employee_autocomplete_data('', 2, 2, 1);

*/

	_SearchString varchar(500),
    _PageIndex int,
    _PageSize int,
    _CompanyId int
)
Begin
	Declare exit handler for sqlexception
	Begin
	
		GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
									
		Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
		Call sp_LogException(@Message, '', 'SP_Employee_GetAll', 1, 0, @Result);
	End;

	Set _PageIndex = (_PageIndex - 1) * _PageSize;

	Select * from (	
		Select 
			EmployeeUid `value`, 
			Concat(FirstName, ' ', LastName) `text`
		from employees
		where CompanyId = _CompanyId And IsActive = true And
		CASE 
			When _SearchString is null OR _SearchString = ''
			then 1=1
			Else (FirstName like concat('%', _SearchString, '%') OR LastName  like concat('%', _SearchString, '%'))
		END
		Order by  UpdatedOn Desc, CreatedOn Desc
	)T limit _PageSize offset _PageIndex;
end$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_timesheet_insupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_timesheet_insupd`(

/*

	Set @OutParam = '';
	Call sp_timesheet_insupd(0, 4, 1, '2022-02-15', 9.0, 0, 0, 7, 'working', '2022-02-15', 4, @OutParam);
    Select @OutParam;

*/

	_TimesheetId bigint,
    _EmployeeId bigint,
    _ClientId bigint,
    _UserTypeId int,
    _TimesheetMonthJson Json,
    _TotalDays int,
    _DaysAbsent int,
    _ExpectedBurnedMinutes int,
    _ActualBurnedMinutes int,
    _TotalWeekDays int,
    _TotalWorkingDays int,
    _TotalHolidays int,
    _MonthTimesheetApprovalState int,
    _ForYear int,
    _ForMonth int,
    _AdminId bigint,
    out _ProcessingResult varchar(100)
)
Begin
    Set @OperationStatus = '';
		Begin
			Declare Exit handler for sqlexception
			Begin
				Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
											@errorno = MYSQL_ERRNO,
											@errortext = MESSAGE_TEXT;
                             
				Set sql_safe_updates = 1;
				Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
				Call sp_LogException (@Message, @OperationStatus, 'sp_timesheet_insupd', 1, 0, @Result);
			end;
            
            Set _ProcessingResult = '';
			Begin
				If not exists (Select * from employee_timesheet where TimesheetId = _TimesheetId) then
                Begin
					Set @@SESSION.information_schema_stats_expiry = 0;

					SELECT AUTO_INCREMENT into _TimesheetId
					FROM information_schema.tables
					WHERE table_name = 'employee_timesheet'
					AND table_schema = DATABASE();	

					Insert into employee_timesheet values(
						_TimesheetId,
						_EmployeeId,
						_ClientId,
						_UserTypeId,
						_TimesheetMonthJson,
						_TotalDays,
						_DaysAbsent,
						_ExpectedBurnedMinutes,
						_ActualBurnedMinutes,
						_TotalWeekDays,
						_TotalWorkingDays,
						_TotalHolidays,
						_MonthTimesheetApprovalState,
						_ForYear,
						_ForMonth,
                        now(),
                        null,
                        _AdminId,
                        null
					);
                    
                    Set _ProcessingResult = 'inserted';
                End;
                Else
                Begin
					Update employee_timesheet Set
							TimesheetId						=				_TimesheetId,
							EmployeeId						=				_EmployeeId,
							ClientId						=				_ClientId,
							UserTypeId						=				_UserTypeId,
							TimesheetMonthJson				=				_TimesheetMonthJson,
							TotalDays						=				_TotalDays,
							DaysAbsent						=				_DaysAbsent,
							ExpectedBurnedMinutes			=				_ExpectedBurnedMinutes,
							ActualBurnedMinutes				=				_ActualBurnedMinutes,
							TotalWeekDays					=				_TotalWeekDays,
							TotalWorkingDays				=				_TotalWorkingDays,
							TotalHolidays					=				_TotalHolidays,
							MonthTimesheetApprovalState		=				_MonthTimesheetApprovalState,
							ForYear							=				_ForYear,
							ForMonth						=				_ForMonth,
							UpdatedOn						=				NOW(),
							UpdatedBy						= 				_AdminId
					where 	TimesheetId 					= 				_TimesheetId;
                    
                    Set _ProcessingResult = 'updated';
                End;
                End if;	
			End;
		End;
	End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_company_intupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_company_intupd`(
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
	_TypeOfBusiness varchar(150),
	_InCorporationDate datetime,
	_IsPrimaryCompany bit(1),
	_FixedComponentsId json,    
	_BankAccountId int,
	_BankName varchar(100),
	_BranchCode varchar(20),
	_Branch varchar(50),
	_IFSC varchar(20),
    _PANNo varchar(45),
    _GSTNo varchar(45),
    _TradeLicenseNo varchar(45),
    _AdminId long,    
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
            
            set @organizationName = '';
            select OrganizationName into @organizationName 
            from organization_detail
            where OrganizationId = _OrganizationId;
		
			if not exists(select 1 from company where CompanyId = _CompanyId) then
            begin
				Insert into company values(
					_CompanyId,
					_OrganizationId,
					@organizationName,
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
					_TypeOfBusiness,
					_InCorporationDate,
                    _PANNo,
					_GSTNo,
					_TradeLicenseNo,
					_IsPrimaryCompany,
					_FixedComponentsId,
					_AdminId,
					null,
					utc_date(),
					null
				);
                
                Set _ProcessingResult = 'inserted';
			end;
            else 
            begin
				update company set
					OrganizationId					=			_OrganizationId,
					OrganizationName				=			@organizationName,
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
					TypeOfBusiness					=			_TypeOfBusiness,
					InCorporationDate				=			_InCorporationDate,
                    PANNo							=			_PANNo,
					GSTNo							=			_GSTNo,
					TradeLicenseNo					=			_TradeLicenseNo,
					IsPrimaryCompany				=			_IsPrimaryCompany,
					FixedComponentsId				=			_FixedComponentsId,
					UpdatedBy						=			_AdminId,
                    UpdatedOn						=			utc_date()
				where CompanyId 					= 			_CompanyId;
                
                Set _ProcessingResult = 'updated';
            end;
            end if;
	End;
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists sp_bank_accounts_intupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_bank_accounts_intupd`(
	_BankAccountId int,
    _OrganizationId int,
    _CompanyId int,
    _BankName varchar(100),
    _BranchCode varchar(20),    
    _Branch varchar(50),
    _IFSC varchar(20),
    _AccountNo varchar(45),
    _OpeningDate datetime,
    _ClosingDate datetime,
    _IsPrimaryAccount bit,
    _AdminId bigint,
	out _ProcessingResult varchar(50)
    
/*
	set @out = '';
	Call sp_bank_accounts_intupd(0,0,2,'SBI','14544','Asansol','SBIN0014544','20141408787',now(), );
	select @out;
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_bank_accounts_intupd', 1, 0, @Result);
			end;  
			start transaction;
            
            if (_IsPrimaryAccount = 1) then 
            begin
				SET SQL_SAFE_UPDATES = 0;
				Update bank_accounts Set
					IsPrimaryAccount 			=			false
					where CompanyId 			= 			_CompanyId;
				SET SQL_SAFE_UPDATES = 1;
            end;
            end if;
            
			if not exists(select * from bank_accounts where BankAccountId = _BankAccountId) then
            begin
				Insert into bank_accounts values(
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
					_IsPrimaryAccount,
                    _AdminId,
                    null,
                    utc_date(),
                    null
				);
                
                Set _ProcessingResult = 'inserted';
			end;
            else 
            begin
				Update bank_accounts Set
					OrganizationId			=			_OrganizationId,
                    CompanyId				=			_CompanyId,
					BankName				=			_BankName,
					BranchCode				=			_BranchCode,
					Branch					=			_Branch,
					IFSC					=			_IFSC,
					AccountNo				=			_AccountNo,
					OpeningDate				=			_OpeningDate,
                    ClosingDate				=			_ClosingDate,
                    IsPrimaryAccount 		=			_IsPrimaryAccount,
                    UpdatedBy				=			_AdminId,
                    UpdatedOn				=			utc_date()
                where BankAccountId 		= 			_BankAccountId;
                
                Set _ProcessingResult = 'updated';
            end;
            end if;
            commit;
	End;
End$$
DELIMITER ;
