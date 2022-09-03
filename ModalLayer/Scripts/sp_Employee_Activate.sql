DELIMITER $$

drop procedure if exists sp_Employee_Activate $$


CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employee_Activate`(

/*

	Call sp_Employee_Activate(11);

*/
	_EmployeeId bigint,
	_FirstName varchar(50),
	_LastName varchar(50),
	_Mobile varchar(20),
	_Email varchar(100),
	_IsActive bit,
	_ReportingManagerId bigint,
	_DesignationId int,
	_UserTypeId int,
	_LeavePlanId int,
	_PayrollGroupId int,
	_SalaryGroupId int,
	_CompanyId int,
	_NoticePeriodId int,
	_SecondaryMobile varchar(20),
	_PANNo varchar(20),
	_AadharNo varchar(20),
	_AccountNumber varchar(50),
	_BankName varchar(100),
	_BranchName varchar(100),
	_IFSCCode varchar(20),
	_Domain varchar(250),
	_Specification varchar(250),
	_ExprienceInYear float,
	_LastCompanyName varchar(100),
	_ProfessionalDetail_Json json,
	_Gender bit,
	_FatherName varchar(50),
	_SpouseName varchar(50),
	_MotherName varchar(50),
	_Address varchar(100),
	_State varchar(75),
	_City varchar(75),
	_Pincode int,
	_IsPermanent bit,
	_ActualPackage float,
	_FinalPackage float,
	_TakeHomeByCandidate float,
	_AccessLevelId bigint,
	_Password varchar(150),
	_EmployeeDeclarationId bigint,
	_DocumentPath varchar(250),
	_DeclarationDetail json,
	_HousingProperty json,
	_TotalDeclaredAmount decimal,
	_TotalApprovedAmount decimal,
	_LeaveRequestId bigint,
	_LeaveDetail json,
	_Year int,
	_EmployeeNoticePeriodId bigint,
	_ApprovedOn datetime,
	_ApplicableFrom datetime,
	_ApproverManagerId int,
	_ManagerDescription varchar(500),
	_AttachmentPath varchar(200),
	_EmailTitle varchar(100),
	_OtherApproverManagerIds json,
	_ITClearanceStatus int,
	_ReportingManagerClearanceStatus int,
	_CanteenClearanceStatus int,
	_ClientClearanceStatus int,
	_HRClearanceStatus int,
	_OfficialLastWorkingDay datetime,
	_PeriodDuration int,
	_EarlyLeaveStatus int,
	_Reason varchar(500), 
	_CTC decimal,
	_GrossIncome decimal,
	_NetSalary decimal,
	_CompleteSalaryDetail json,
	_GroupId int,
	_TaxDetail json,
	_TimesheetId bigint,
	_ClientId bigint,
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
	_EmployeeMappedClientUid bigint,
	_ClientName varchar(250),
	_BillingHours int,
	_DaysPerWeek int,
	_DateOfJoining datetime,
	_DateOfLeaving datetime,
	_AdminId long,
	out _ProcessingResult varchar(50)
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
            
            RollBack;
            Set autocommit = 1;
            Set sql_safe_updates = 1;
            
			Call sp_LogException (@Message, @OperationStatus, 'sp_Employee_Activate', 1, 0, @Result);
		end;  
		set autocommit = 0;
        Set @schemaName = 'onlinedatabuilder';
        Set @employeeId = 0;
		start transaction;
        begin		            select * from employees;
			if not exists (Select 1 from employees where EmployeeUid = _EmployeeId) then
            begin				
				Insert into employees values (
					_EmployeeId,
					_FirstName,
					_LastName,
					_Mobile,
					_Email,
					_IsActive,
                    _AdminId,
                    null,
                    null,
                    now(),
					_ReportingManagerId,
					_DesignationId,
					_UserTypeId,
					_LeavePlanId,
					_PayrollGroupId,
					_SalaryGroupId,
					_CompanyId,
					_NoticePeriodId
                );
            end;
            end if;


			if not exists (Select 1 from employeeprofessiondetail where EmployeeUid = _EmployeeId) then
            begin
				
				Insert into employeeprofessiondetail values (
					default,
					_EmployeeId,
					_FirstName,
					_LastName,
					_Mobile,
                    _SecondaryMobile,
					_Email,
                    _PANNo,
					_AadharNo,
					_AccountNumber,
					_BankName,
					_BranchName,
					_IFSCCode,
					_Domain,
					_Specification,
					_ExprienceInYear,
					_LastCompanyName,
                    _AdminId,
                    null,
                    null,
                    now(),
					_ProfessionalDetail_Json
				 );
			end;
            end if;

			if not exists (Select 1 from employeepersonaldetail where EmployeeUid = _EmployeeId) then
            begin
				Insert into employeepersonaldetail values (
					default,
					_EmployeeId,
					_Mobile,
                    _SecondaryMobile,
					_Email,
                    _Gender,
					_FatherName,
					_SpouseName,
					_MotherName,
					_Address,
					_State,
					_City,
					_Pincode,
					_IsPermanent,
					_ActualPackage,
					_FinalPackage,
					_TakeHomeByCandidate,
                    _AdminId,
                    null,
                    null,
                    now()
				  );
			end;
            end if;
            
			if not exists (Select 1 from employeelogin where EmployeeId = _EmployeeId) then
            begin            
				Insert into employeelogin values (
                default,
                _EmployeeId, 
                _UserTypeId, 
                _AccessLevelId, 
                _Password, 
                _Email, 
                _Mobile,
                _CompanyId,
                _AdminId, 
                null, 
                null, 
                now()
                );
			end;
            end if;
            
            if not exists (Select 1 from employee_declaration where EmployeeId = _EmployeeId) then
            begin            
				Insert into employee_declaration values (
					default,
					_EmployeeId,
					_DocumentPath,
					_DeclarationDetail,
                    _HousingProperty,
                    _TotalDeclaredAmount,
					_TotalApprovedAmount
                );
			end;
            end if;
            
            if not exists (Select 1 from employee_leave_request where EmployeeId = _EmployeeId) then
            begin            
				Insert into employee_leave_request values (
					default,
					_EmployeeId,
					_LeaveDetail,
					_Year
                );
			end;
            end if;
            
            if not exists (Select 1 from employee_notice_period where EmployeeId = _EmployeeId) then
            begin            
				Insert into employee_notice_period values (
					default,
					_EmployeeId,
					_ApprovedOn,
					_ApplicableFrom,
					_ApproverManagerId,
					_ManagerDescription,
					_AttachmentPath,
					_EmailTitle,
					_OtherApproverManagerIds,
					_ITClearanceStatus,
					_ReportingManagerClearanceStatus,
					_CanteenClearanceStatus,
					_ClientClearanceStatus,
					_HRClearanceStatus,
					_OfficialLastWorkingDay,
					_PeriodDuration,
					_EarlyLeaveStatus,
					_Reason,
					_AdminId,
					null,
					null,
					now()
                );
			end;
            end if;
            
            if not exists (Select 1 from employee_salary_detail where EmployeeId = _EmployeeId) then
            begin            
				Insert into employee_salary_detail values (
					_EmployeeId,
					_CTC,
					_GrossIncome,
					_NetSalary,
					_CompleteSalaryDetail,
					_GroupId,
					_TaxDetail
                );
			end;
            end if;
            
            if not exists (Select 1 from employee_timesheet where EmployeeId = _EmployeeId) then
            begin            
				Insert into employee_timesheet values (
						default,
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
                        null,
                        now(),
                        null,
                        null
                );
			end;
            end if;
            
            if not exists (Select 1 from employeemappedclients where EmployeeUid = _EmployeeId) then
            begin            
				Insert into employeemappedclients values (
					_EmployeeMappedClientUid,
					_EmployeeId,
					_ClientId,
					_ClientName,
					_FinalPackage,
					_ActualPackage,
					_TakeHomeByCandidate,
					_IsPermanent,
					_IsActive,
					_BillingHours,
					_DaysPerWeek,
					_DateOfJoining,
					_DateOfLeaving
                );
			end;
            end if;

			set sql_safe_updates = 0;
			delete from employee_archive where EmployeeId = _EmployeeId;
        end;        
        commit;
        set _ProcessingResult = 'updated';
        Set sql_safe_updates = 1;
		 Set autocommit = 1;
	End;
End$$
DELIMITER ;
