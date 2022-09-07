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



DELIMITER $$

drop procedure if exists SP_Employee_GetAll $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employee_GetAll`(
/*	

	Call SP_Employee_GetAll(' 1=1  and l.CompanyId = 2', '', 1, 10);

*/

	_SearchString varchar(500),
    _SortBy varchar(100),
    _PageIndex int,
    _PageSize int
)
Begin
    Begin
		Declare exit handler for sqlexception
		Begin
		
			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
										
			Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
			Call sp_LogException(@Message, '', 'SP_Employee_GetAll', 1, 0, @Result);
		End;

        Begin
        
			if(_SortBy is null Or _SortBy = '') then
				Set _SortBy = ' UpdatedOn Desc, CreatedOn Desc ';
			end if;
            
            Set @activeQuery = Concat('
				Select emp.EmployeeUid, 
					emp.FirstName,
					emp.LastName,
					emp.Mobile,
					emp.Email,
                    emp.LeavePlanId,
                    emp.IsActive,
					eprof.AadharNo,
					eprof.PANNo,
					eprof.AccountNumber,
					eprof.BankName,
					eprof.IFSCCode,
					eprof.Domain,
					eprof.Specification,
					eprof.ExprienceInYear,
					eper.ActualPackage,
					eper.FinalPackage,
					eper.TakeHomeByCandidate,
					(
						Select 
							JSON_ARRAYAGG(
								json_object(
									''CompanyId'', ClientUid,
									''CompanyName'', ClientName,
									''ActualPackage'', ActualPackage
								)
                            )
						from employeemappedclients 
						where EmployeeUid = emp.EmployeeUid
					) as ClientJson,
                    emp.UpdatedOn, 
                    emp.CreatedOn
				from employees emp
                inner join employeelogin l on emp.EmployeeUid = l.EmployeeId
				Left Join employeepersonaldetail eper on emp.EmployeeUid = eper.EmployeeUid
				left join employeeprofessiondetail eprof on emp.EmployeeUid = eprof.EmployeeUid
				where ', _SearchString, '
			');	
            
            Set @SelectQuery = '';
			Set @SelectQuery = Concat('
				select *, Count(1) Over() as Total from (
					', @activeQuery ,'
				)T Order by ', _SortBy ,' limit ', _PageSize ,' offset ', (_PageIndex - 1) * 10
			);
            
		# select @SelectQuery;
		prepare SelectQuery from @SelectQuery;
		execute SelectQuery;
		End;
	End;
end$$
DELIMITER ;


drop table if exists pf_esi_setting;

CREATE TABLE `pf_esi_setting` (
	`PfEsi_setting_Id` int primary key auto_increment,
    CompanyId int,
	PFEnable bit,
	IsPfAmountLimitStatutory bit,
	IsPfCalculateInPercentage bit,
	IsAllowOverridingPf bit,
	IsPfEmployerContribution bit,
	EmployerPFLimit decimal,
	IsHidePfEmployer bit,
	IsPayOtherCharges bit,
	IsAllowVPF bit,
	EsiEnable bit,
	MaximumGrossForESI decimal,
	EsiEmployeeContribution decimal,
	EsiEmployerContribution decimal,
	IsAllowOverridingEsi bit,
	IsHideEsiEmployer bit,
	IsEsiExcludeEmployerShare bit,
	IsEsiExcludeEmployeeGratuity bit,
	IsEsiEmployerContributionOutside bit,
	IsRestrictEsi bit,
	IsIncludeBonusEsiEligibility bit,
	IsIncludeBonusEsiContribution bit,
	IsEmployerPFLimitContribution bit,
	`CreatedOn` datetime DEFAULT NULL,
	`UpdatedOn` datetime DEFAULT NULL,
	`CreatedBy` bigint DEFAULT NULL,
	`UpdatedBy` bigint DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


DELIMITER $$

drop procedure if exists sp_pf_esi_setting_insupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_pf_esi_setting_insupd`(

/*

	Call sp_pf_esi_setting_insupd('BS', 'BASIC SALARY', 40, null, 1);

*/
	_PfEsi_setting_Id int,
    _CompanyId int,
	_PFEnable bit,
	_IsPfAmountLimitStatutory bit,
	_IsPfCalculateInPercentage bit,
	_IsAllowOverridingPf bit,
	_IsPfEmployerContribution bit,
	_EmployerPFLimit decimal,
	_IsHidePfEmployer bit,
	_IsPayOtherCharges bit,
	_IsAllowVPF bit,
	_EsiEnable bit,
	_MaximumGrossForESI decimal,
	_EsiEmployeeContribution decimal,
	_EsiEmployerContribution decimal,
	_IsAllowOverridingEsi bit,
	_IsHideEsiEmployer bit,
	_IsEsiExcludeEmployerShare bit,
	_IsEsiExcludeEmployeeGratuity bit,
	_IsEsiEmployerContributionOutside bit,
	_IsRestrictEsi bit,
	_IsIncludeBonusEsiEligibility bit,
	_IsIncludeBonusEsiContribution bit,
	_IsEmployerPFLimitContribution bit,
    _Admin bigint,
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
				Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
				Call sp_LogException (@Message, @OperationStatus, 'sp_pf_esi_setting_insupd', 1, 0, @Result);
			end;  
		
		if not exists(select 1 from pf_esi_setting where PfEsi_setting_Id = _PfEsi_setting_Id) then
        begin
			insert into pf_esi_setting
            values(
				default,
				_CompanyId,
				_PFEnable,
				_IsPfAmountLimitStatutory,
				_IsPfCalculateInPercentage,
				_IsAllowOverridingPf,
				_IsPfEmployerContribution,
				_EmployerPFLimit,
				_IsHidePfEmployer,
				_IsPayOtherCharges,
				_IsAllowVPF,
				_EsiEnable,
				_MaximumGrossForESI,
				_EsiEmployeeContribution,
				_EsiEmployerContribution,
				_IsAllowOverridingEsi,
				_IsHideEsiEmployer,
				_IsEsiExcludeEmployerShare,
				_IsEsiExcludeEmployeeGratuity,
				_IsEsiEmployerContributionOutside,
				_IsRestrictEsi,
				_IsIncludeBonusEsiEligibility,
				_IsIncludeBonusEsiContribution,
				_IsEmployerPFLimitContribution,
				utc_date(),
				null,
				_Admin,
				null
            );
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update pf_esi_setting set				
				PFEnable           						=       _PFEnable,
				IsPfAmountLimitStatutory        	    =       _IsPfAmountLimitStatutory,
				IsPfCalculateInPercentage       	    =       _IsPfCalculateInPercentage,
				IsAllowOverridingPf        				=       _IsAllowOverridingPf,
				IsPfEmployerContribution            	=       _IsPfEmployerContribution,
				IsHidePfEmployer            			=       _IsHidePfEmployer,
				IsPayOtherCharges           			=       _IsPayOtherCharges,
				IsAllowVPF          					=       _IsAllowVPF,
				EsiEnable         					  	=       _EsiEnable,
				IsAllowOverridingEsi            		=       _IsAllowOverridingEsi,
				IsHideEsiEmployer           			=       _IsHideEsiEmployer,
				IsEsiExcludeEmployerShare           	=       _IsEsiExcludeEmployerShare,
				IsEsiExcludeEmployeeGratuity            =       _IsEsiExcludeEmployeeGratuity,
				IsEsiEmployerContributionOutside        =       _IsEsiEmployerContributionOutside,
				IsRestrictEsi           				=       _IsRestrictEsi,
				IsIncludeBonusEsiEligibility            =       _IsIncludeBonusEsiEligibility,
				IsIncludeBonusEsiContribution           =       _IsIncludeBonusEsiContribution,
				IsEmployerPFLimitContribution           =       _IsEmployerPFLimitContribution,
				EmployerPFLimit         				=       _EmployerPFLimit,
				MaximumGrossForESI          			=       _MaximumGrossForESI,
				EsiEmployeeContribution         		=       _EsiEmployeeContribution,
				EsiEmployerContribution             	= 		_EsiEmployerContribution,
				UpdatedBy								=		_Admin,
                UpdatedOn								=		utc_date()
            where PfEsi_setting_Id = _PfEsi_setting_Id;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_pf_esi_setting_get $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_pf_esi_setting_get`(

	_CompanyId int
/*

	Call sp_pf_esi_setting_get(1);

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
				Call sp_LogException (@Message, @OperationStatus, 'sp_pf_esi_setting_get', 1, 0, @Result);
			end;  
		
		select * from pf_esi_setting
        where CompanyId = _CompanyId;
	End;
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists sp_salary_group_getAll $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_getAll`(

/*

	Call sp_salary_group_getAll();

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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_group_getAll', 1, 0, @Result);
			end;  
		
		select * from salary_group;
	End;
End$$
DELIMITER ;

DELIMITER $$

drop procedure if exists sp_salary_group_insupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_insupd`(

/*



	Set @result = '';
	Call sp_salary_group_insupd(0, 1, 'A', 'For people having salary more than 24L', 0, 500000, 1, @result);
    select @result;



*/
	_SalaryGroupId int,
    _CompanyId int,
    _SalaryComponents json,
    _GroupName varchar(45),
    _GroupDescription varchar(250),
    _MinAmount decimal,
    _MaxAmount decimal,
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
			Call sp_LogException (@Message, '', 'sp_salary_group_insupd', 1, 0, @Result);
		end;  
		
		if not exists(select 1 from salary_group where SalaryGroupId = _SalaryGroupId) then
        begin
			insert into salary_group
            values(
				default,
				_SalaryComponents,
				_GroupName,
				_GroupDescription,
				_MinAmount,
				_MaxAmount,
				_AdminId,
				now(),
                _CompanyId
            );
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update salary_group set				
				SalaryComponents		=			_SalaryComponents,
				GroupName				=			_GroupName,
				GroupDescription		=			_GroupDescription,
				MinAmount				=			_MinAmount,
				MaxAMount				=			_MaxAmount,
				CreatedBy				=			_AdminId,
                CreatedOn				=			utc_date(),
                CompanyId				=			_CompanyId
            where SalaryGroupId = _SalaryGroupId;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists sp_salary_group_getbyCompanyId $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_getbyCompanyId`(

/*

	Call sp_salary_group_getbyCompanyId(1);

*/

	_CompanyId int
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_group_getbyCompanyId', 1, 0, @Result);
			end;  
		
		select * from salary_group
        where CompanyId = _CompanyId;
	End;
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists sp_salary_group_insupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_insupd`(

/*



	Set @result = '';
	Call sp_salary_group_insupd(0, 1, 'A', 'For people having salary more than 24L', 0, 500000, 1, @result);
    select @result;



*/
	_SalaryGroupId int,
    _CompanyId int,
    _SalaryComponents json,
    _GroupName varchar(45),
    _GroupDescription varchar(250),
    _MinAmount decimal,
    _MaxAmount decimal,
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
			Call sp_LogException (@Message, '', 'sp_salary_group_insupd', 1, 0, @Result);
		end;  
		
		if not exists(select 1 from salary_group where SalaryGroupId = _SalaryGroupId) then
        begin
			insert into salary_group
            values(
				default,
				_SalaryComponents,
				_GroupName,
				_GroupDescription,
				_MinAmount,
				_MaxAmount,
				_AdminId,
				now(),
                _CompanyId
            );
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update salary_group set				
				SalaryComponents		=			_SalaryComponents,
				GroupName				=			_GroupName,
				GroupDescription		=			_GroupDescription,
				MinAmount				=			_MinAmount,
				MaxAMount				=			_MaxAmount,
				CreatedBy				=			_AdminId,
                CreatedOn				=			utc_date(),
                CompanyId				=			_CompanyId
            where SalaryGroupId 		= 			_SalaryGroupId;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists SP_ManageEmployeeDetail_Get $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_ManageEmployeeDetail_Get`(
	_employeeId bigint
/*	

	Call SP_ManageEmployeeDetail_Get(0);

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
			Call sp_LogException(@Message, '', 'SP_ManageEmployeeDetail_Get', 1, 0, @Result);
		End;

        Begin
			Set @UserTypeId = 0;
            Set @AccessLevelId = 0;		

			Select UserTypeId from employees 
			Where EmployeeUid = _employeeId 
			into @UserTypeId;
		
			Select AccessLevelId from employeelogin
			where EmployeeId = _employeeId into @AccessLevelId;
	
			Select  e.EmployeeUid,
				e.FirstName,
				e.LastName,
				e.Mobile,
				e.Email,
				e.ReportingManagerId,
				e.DesignationId,
				ep.SecondaryMobile,
				e.IsActive,
				ep.Gender,
				ep.FatherName,
				ep.SpouseName,
				ep.MotherName,
				ep.Address,
				ep.State,
				ep.City,
				ep.Pincode,
				ep.IsPermanent,
				ep.ActualPackage,
				ep.FinalPackage,
				ep.TakeHomeByCandidate,
				ef.PANNo,
				ef.AadharNo,
				ef.AccountNumber,
				ef.BankName,
				ef.BranchName,
				ef.IFSCCode,
				ef.Domain,
				ef.Specification,
				ef.ExprienceInYear,
				ef.LastCompanyName,
				@AccessLevelId AccessLevelId,
				@UserTypeId UserTypeId,
				e.LeavePlanId
			from employees e 
			Inner Join employeepersonaldetail ep on e.EmployeeUid = ep.EmployeeUid
			Inner Join employeeprofessiondetail ef on e.EmployeeUid = ef.EmployeeUid
			Where e.EmployeeUid = _employeeId;
					
			Select * from employeemappedclients 
			where EmployeeUid = _employeeId; #and IsActive = 1;
		
			Select FileId, FilePath, FileName, FileExtension, UserTypeId from userfiledetail 
			where FileOwnerId = _employeeId and FileName = 'profile';            
            
            Select * from accesslevel;
            Call sp_employee_salary_detail_get_by_empid(_employeeId);
            
            select c.* from company c
            Inner join employeelogin e
            on c.CompanyId = e.CompanyId
            where e.EmployeeId = _employeeId;
            
			select * from employees
            where DesignationId = 1 Or DesignationId = 4;
            
			select * from leave_plan; 
		End;
	End;
end$$
DELIMITER ;


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
			End;
			Else
			Begin
				Set _ProcessingResult = '0';
				Set @EmpId = _EmployeeUid;
                set sql_safe_updates = 0;
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
