DELIMITER $$

drop procedure if exists sp_Employees_InsUpdate $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employees_InsUpdate`(
	_EmployeeUid bigint,
	_FirstName varchar(50),
	_LastName varchar(50),
	_Mobile varchar(20),
	_Email varchar(100),
    _LeavePlanId int,
    _PayrollGroupId int,
    _SalaryGroupId int,
    _CompanyId int,
    _NoticePeriodId int,
    _SecondaryMobile varchar(20),
    _FatherName varchar(50),
    _MotherName varchar(50),
    _SpouseName varchar(50),
    _Gender bit(1),
    _State varchar(75),
    _City varchar(75),
    _Pincode int,
    _Address varchar(100),
    _PANNo varchar(20),
    _AadharNo varchar(20),
    _AccountNumber varchar(50),
    _BankName varchar(100),
    _BranchName varchar(100),
    _IFSCCode varchar(20),
    _Domain varchar(250),
    _Specification varchar(250),
    _ExprienceInYear float(5,2),
    _LastCompanyName varchar(100),
    _IsPermanent bit(1),
	_ActualPackage float(10,2),
    _FinalPackage float(10,2),
    _TakeHomeByCandidate float(10,2),
    _ReportingManagerId bigint,
    _DesignationId int,
    _ProfessionalDetail_Json json,
    _Password varchar(150),
    _AccessLevelId int,
    _UserTypeId int,
	_CTC decimal,
	_GrossIncome decimal,
	_NetSalary decimal,
	_CompleteSalaryDetail Json,
	_TaxDetail Json,
	_AdminId bigint,
    out _ProcessingResult varchar(100)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);  
            
            RollBack;
            SET autocommit = 1;
            Set sql_safe_updates = 1;
            Call sp_LogException (@Message, '', 'sp_Employees_InsUpdate', 1, 0, @Result);
		end;
        
        Set @msg = 'starting';
        
        SET autocommit = 0;
        Set @EmpId = 0;
        Start Transaction;
		Begin 
        
			if(_UserTypeId = 0) then
				Set _UserTypeId = 2;
			end if;
        
			If not exists (Select 1 from employees Where EmployeeUid = _EmployeeUid) then
			Begin
				if not exists (select 1 from employee_archive where EmployeeUid = _EmployeeUid) then
                Begin
					Set @EmpId = 0;
					Select EmployeeUid from employees order by EmployeeUid desc limit 1 into @EmpId ;
					Set @EmpId = @EmpId+1;
                 
					Set _ProcessingResult = @EmpId; 
					/*
					SELECT AUTO_INCREMENT
					FROM information_schema.tables
					WHERE table_name = 'employees'
					AND table_schema = DATABASE() INTO @EmpId;
					*/
                
					Insert into employees (EmployeeUid, FirstName, LastName, Mobile, 
						Email, LeavePlanId, PayrollGroupId, IsActive, CreatedBy, UpdatedBy,  CreatedOn, UpdatedOn, 
						ReportingManagerId, DesignationId, UserTypeId, SalaryGroupId, CompanyId, NoticePeriodId
					) Values (
						@EmpId,
						_FirstName,
						_LastName,
						_Mobile,
						_Email,
                        _LeavePlanId,
						_PayrollGroupId,
						1,
						_AdminId,
						null, 
						now(),
						null,
						_ReportingManagerId,
						_DesignationId,
						_UserTypeId,
                        _SalaryGroupId,
						_CompanyId,
						_NoticePeriodId
					);
				
					Insert into employeepersonaldetail (EmployeePersonalDetailId, EmployeeUid, Mobile, SecondaryMobile, Email, Gender, FatherName, SpouseName,
						MotherName, Address,  State,  City, Pincode, IsPermanent, ActualPackage, FinalPackage, TakeHomeByCandidate, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn
					) Values (
						default,
						@EmpId,
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
						now(),
						null
					);
                    
					Insert into employeeprofessiondetail Values (
						default,
						@EmpId, 
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
						now(),
						null,
                        _ProfessionalDetail_Json
					);
                
                set @msg = 'employeelogin';
					Insert into employeelogin
					Values(
						default, 
						@EmpId, 
						2, 
						_AccessLevelId, 
						_Password, 
						_Email, 
						_Mobile,
                        _CompanyId,
						_AdminId, 
						null, 
						utc_date(), 
						null
					);
                end;
                else 
                begin
					Set @EmpId = _EmployeeUid;
					Update employee_archive SET 
						FirstName				=		_FirstName,
						LastName				=		_LastName,
						Mobile					=		_Mobile,
						Email					=		_Email,
						UpdatedBy				=		_AdminId, 
						UpdatedOn				=		now(),
                        ReportingManagerId		=		_ReportingManagerId,
                        DesignationId			=		_DesignationId,
                        UserTypeId				=		_UserTypeId,
                        LeavePlanId				=		_LeavePlanId,
						PayrollGroupId			=		_PayrollGroupId,
                        SalaryGroupId			=		_SalaryGroupId,
						CompanyId				=		_CompanyId,
						NoticePeriodId			=		_NoticePeriodId
					Where 	EmployeeUid 	= _EmployeeUid;
                
					Update employeepersonaldetail_archive Set
						Mobile						=	_Mobile,
						SecondaryMobile				=	_SecondaryMobile,
						Email						=	_Email,
						Gender						=	_Gender,
						FatherName					=	_FatherName,
						SpouseName					=	_SpouseName,
						MotherName					=	_MotherName,
						Address						=	_Address, 
						State						=	_State, 
						City						=	_City,
						Pincode						=	_Pincode,
						IsPermanent					=	_IsPermanent,
						ActualPackage				=	_ActualPackage,
						FinalPackage				=	_FinalPackage,
						TakeHomeByCandidate			=	_TakeHomeByCandidate,
						UpdatedBy					=	_AdminId,
						UpdatedOn					=	now()
					Where	EmployeeUid					=	_EmployeeUid;
                
					Update	employeeprofessiondetail_archive Set
							FirstName		=	_FirstName,
							LastName		=	_LastName,
							Mobile			=	_Mobile,
							SecondaryMobile	=	_SecondaryMobile,
							Email			=	_Email, 
							PANNo			=	_PANNo,
							AadharNo		=	_AadharNo,
							AccountNumber	=	_AccountNumber,
							BankName		=	_BankName,
							BranchName		=	_BranchName, 
							IFSCCode		=	_IFSCCode,
							Domain			=	_Domain,
                            Specification	=	_Specification,
							ExprienceInYear	=	_ExprienceInYear,
							LastCompanyName	=	_LastCompanyName,
							UpdatedBy		=	_AdminId,
							UpdatedOn		=	now(),
                            ProfessionalDetail_Json = _ProfessionalDetail_Json
					 Where	EmployeeUid		=	_EmployeeUid;

					if(_UserTypeId = 1) then
						Set _AccessLevelId = 1;
					end if;
                    
					Set sql_safe_updates = 0;
					Update employeelogin
						Set AccessLevelId = _AccessLevelId,
						Email 		= _Email,
						UserTypeId 	= _UserTypeId,
						Mobile 		= _Mobile,
                        CompanyId	= _CompanyId,
						UpdatedBy 	= _AdminId,
						UpdatedOn   = utc_date()
					Where EmployeeId = _EmployeeUid;
                end;
                end if;				
			End;
			Else
			Begin
				Set _ProcessingResult = '0';
				Set @EmpId = _EmployeeUid;
                Update employees SET 
						FirstName				=		_FirstName,
						LastName				=		_LastName,
						Mobile					=		_Mobile,
						Email					=		_Email,
						UpdatedBy				=		_AdminId, 
						UpdatedOn				=		now(),
                        ReportingManagerId		=		_ReportingManagerId,
                        DesignationId			=		_DesignationId,
                        UserTypeId				=		_UserTypeId,
                        LeavePlanId				=		_LeavePlanId,
						PayrollGroupId			=		_PayrollGroupId,
                        SalaryGroupId			=		_SalaryGroupId,
						CompanyId				=		_CompanyId,
						NoticePeriodId			=		_NoticePeriodId
					Where 	EmployeeUid 	= _EmployeeUid;
                
					Update employeepersonaldetail Set
						Mobile						=	_Mobile,
						SecondaryMobile				=	_SecondaryMobile,
						Email						=	_Email,
						Gender						=	_Gender,
						FatherName					=	_FatherName,
						SpouseName					=	_SpouseName,
						MotherName					=	_MotherName,
						Address						=	_Address, 
						State						=	_State, 
						City						=	_City,
						Pincode						=	_Pincode,
						IsPermanent					=	_IsPermanent,
						ActualPackage				=	_ActualPackage,
						FinalPackage				=	_FinalPackage,
						TakeHomeByCandidate			=	_TakeHomeByCandidate,
						UpdatedBy					=	_AdminId,
						UpdatedOn					=	now()
					Where	EmployeeUid					=	_EmployeeUid;
                
					Update	employeeprofessiondetail Set
							FirstName		=	_FirstName,
							LastName		=	_LastName,
							Mobile			=	_Mobile,
							SecondaryMobile	=	_SecondaryMobile,
							Email			=	_Email, 
							PANNo			=	_PANNo,
							AadharNo		=	_AadharNo,
							AccountNumber	=	_AccountNumber,
							BankName		=	_BankName,
							BranchName		=	_BranchName, 
							IFSCCode		=	_IFSCCode,
							Domain			=	_Domain,
                            Specification	=	_Specification,
							ExprienceInYear	=	_ExprienceInYear,
							LastCompanyName	=	_LastCompanyName,
							UpdatedBy		=	_AdminId,
							UpdatedOn		=	now(),
                            ProfessionalDetail_Json = _ProfessionalDetail_Json
					 Where	EmployeeUid		=	_EmployeeUid;

					if(_UserTypeId = 1) then
						Set _AccessLevelId = 1;
					end if;
                    
                    Set sql_safe_updates = 0;
					Update employeelogin
						Set AccessLevelId = _AccessLevelId,
						Email 		= 	_Email,
                        UserTypeId 	=	_UserTypeId,
						Mobile 		= 	_Mobile,
                        CompanyId	= _CompanyId,
						UpdatedBy 	= 	_AdminId,
						UpdatedOn   = 	utc_date()
					Where EmployeeId = _EmployeeUid;
				End;
				End if;
			
				set @EmpDecId = 0;
				Set @result = '';
                if not exists(Select 1 from employee_declaration where EmployeeId = @EmpId) then
                begin
					Insert into employee_declaration values(
						default,
						@EmpId,
						'',
						'[]',
						'{}',
						0,
						0
					);  
                    
					select EmployeeDeclarationId into @EmpDecId from employee_declaration  where EmployeeId =  @EmpId;
                end;
                else
                begin
					select EmployeeDeclarationId into @EmpDecId from employee_declaration  where EmployeeId =  @EmpId;
                end;
                end if;
				
				Call sp_employee_declaration_ins_json(@EmpDecId);

                Set @groupId = 0;
                select SalaryGroupId into @groupId from salary_group 
                where _CTC >= MinAmount 
				and _CTC < MaxAmount;
                
                Call sp_employee_salary_detail_InsUpd(@EmpId, _CTC, _GrossIncome, _NetSalary, _CompleteSalaryDetail, @groupId, _TaxDetail, _ProcessingResult);
				Set _ProcessingResult =  @EmpId;
            COMMIT;
            Set sql_safe_updates = 1;
		End;
	End;
End$$
DELIMITER ;