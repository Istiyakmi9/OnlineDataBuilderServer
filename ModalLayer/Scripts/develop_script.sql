DELIMITER $$

drop procedure if exists sp_salary_group_get_by_ctc $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_get_by_ctc`(

/*

	Call sp_salary_group_get_by_ctc(4, 2194000);

*/

	_CTC decimal,
    _EmployeeId bigint
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_group_getById', 1, 0, @Result);
			end;  
		
		select g.* from salary_group g
		inner join employees e on e.CompanyId = g.CompanyId
		-- where e.EmployeeUid = _EmployeeId
		and _CTC >= MinAmount 
		and _CTC <= MaxAmount;
	End;
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists sp_employee_salary_detail_InsUpd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_salary_detail_InsUpd`(

/*

	set @result = '';
	Call sp_employee_salary_detail_InsUpd(29, 1000000, 0.0, 0.0, '{}', 2, '[]', @result);
    select @result;

*/

	_EmployeeId bigint,
    _CTC decimal,
    _GrossIncome decimal,
    _NetSalary decimal,
	_CompleteSalaryDetail json,
    _GroupId int,
    _TaxDetail json,
    out _ProcessingResult varchar(100)
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set sql_safe_updates = 1;                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_employee_salary_detail_InsUpd', 1, 0, @Result);
		end; 
        
        
		Begin
        
			Set @groupId = 0;
			select g.SalaryGroupId into @groupId from salary_group  g
            inner join employees e on e.CompanyId = g.CompanyId
			where e.EmployeeUid = _EmployeeId and
            _CTC >= MinAmount and _CTC < MaxAmount;
        
			if not exists (select 1 from employee_salary_detail where EmployeeId = _EmployeeId) then
            Begin
				insert into employee_salary_detail
                values 
                (
					_EmployeeId,
                    _CTC,
                    _GrossIncome,
                    _NetSalary,
                    _CompleteSalaryDetail,
                    @groupId,
                    _TaxDetail
                );
                
                Set _ProcessingResult = 'inserted';
            End;
            Else
            Begin
				Set sql_safe_updates = 0;
				update employee_salary_detail 
					set	CTC						=		_CTC,
						GrossIncome				=		_GrossIncome,
						NetSalary				=		_NetSalary,
						CompleteSalaryDetail	=		_CompleteSalaryDetail,
						GroupId					=		@groupId,
						TaxDetail				= 		_TaxDetail
				where EmployeeId = _EmployeeId;
                
                Set sql_safe_updates = 1;
                Set _ProcessingResult = 'updated';
            End;
            End if;
        End;
	End;
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists sp_leave_plans_type_insupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plans_type_insupd`(
	   /*

	set @result = '';
    Call sp_leave_plans_type_insupd(0, 1, 'Casual Leave', 'Casual leave for employees', 20, @result);
	select @result;

*/
	_LeavePlanTypeId int,
    _LeavePlanCode varchar(10),
    _PlanName varchar(50),
    _PlanDescription varchar(250),
    _MaxLeaveLimit int,
    _ShowDescription bit,
    _IsPaidLeave bit,
    _IsSickLeave bit,
    _IsStatutoryLeave bit,
    _IsMale bit,
    _IsMarried bit,
    _IsRestrictOnGender bit,
    _IsRestrictOnMaritalStatus bit,
    _Reasons json,
    _PlanConfigurationDetail json,
	_AdminId bigint,	
    out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, '', 'sp_leave_plans_type_insupd', 1, 0, @Result);
		end;
        
		If not exists (Select 1 from leave_plans_type Where LeavePlanTypeId = _LeavePlanTypeId) then
		Begin
			Insert into leave_plans_type Values (
				default,
                _LeavePlanCode,
				_PlanName,
				_PlanDescription,
				_MaxLeaveLimit,
                _ShowDescription,
				_IsPaidLeave,
				_IsSickLeave ,
				_IsStatutoryLeave,
				_IsMale,
				_IsMarried,
				_IsRestrictOnGender,
				_IsRestrictOnMaritalStatus,
				_Reasons,
                _PlanConfigurationDetail,
                _AdminId,
                _AdminId,
                now(),
                null
			);
            
            Set _ProcessingResult = 'inserted';
		End;
		Else
		Begin
			Update leave_plans_type SET 
				LeavePlanCode				=				_LeavePlanCode,
				PlanName					=				_PlanName,
				PlanDescription				=				_PlanDescription,
				MaxLeaveLimit				=				_MaxLeaveLimit,
                ShowDescription				=				_ShowDescription,
				IsPaidLeave					=				_IsPaidLeave,
				IsSickLeave					=				_IsSickLeave,
				IsStatutoryLeave			=				_IsStatutoryLeave,
				IsMale						=				_IsMale,
				IsMarried					=				_IsMarried,
				IsRestrictOnGender			=				_IsRestrictOnGender,
				IsRestrictOnMaritalStatus	=				_IsRestrictOnMaritalStatus,
				Reasons						=				_Reasons,
                PlanConfigurationDetail		=				_PlanConfigurationDetail,
                UpdatedBy					=				_AdminId,
                UpdatedOn					=				now()
			Where LeavePlanTypeId = _LeavePlanTypeId;
			
			Set _ProcessingResult = 'updated';
		End;
		End if;
	End;
End$$
DELIMITER ;


Set @result = '';
Call sp_menu_insupd('Leave', 'Manage', 'leave', 'fa fa-calendar-check-o', null, null, @result);    
Select @result;


create table project(
	ProjectId bigint primary key auto_increment,
    ProjectName varchar(150),
    ProjectDescription varchar(500),
    ProjectManagerId bigint,
    TeamMemberIds json,
    ProjectStartedOn datetime null,
    ProjectEndedOn datetime null,
    ArchitectId bigint,
    IsClientProject bit,
    ClientId bigint,
    HomePageUrl varchar(150),
    PageIndexDetail json,
    KeywordDetail json,
    DocumentationDetail json
);

DELIMITER $$

drop procedure if exists sp_project_detail_insupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_project_detail_insupd`(
	   /*

    Call sp_project_detail_insupd(1);

*/
	_ProjectId bigint ,
    _ProjectName varchar(150),
    _ProjectDescription varchar(500),
    _ProjectManagerId bigint,
    _TeamMemberIds json,
    _ProjectStartedOn datetime,
    _ProjectEndedOn datetime,
    _ArchitectId bigint,
    _IsClientProject bit,
    _ClientId bigint,
    _HomePageUrl varchar(150),
    _PageIndexDetail json,
    _KeywordDetail json,
    _DocumentationDetail json,
	out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);

            Call sp_LogException (@Message, '', 'sp_project_detail_insupd', 1, 0, @Result);
		end;
        
        if not exists (select 1 from project where ProjectId = _ProjectId) then
        begin
			Insert into project values(
				default,
				_ProjectName,
				_ProjectDescription,
				_ProjectManagerId,
				_TeamMemberIds,
				_ProjectStartedOn,
				_ProjectEndedOn,
				_ArchitectId,
				_IsClientProject,
				_ClientId,
				_HomePageUrl,
				_PageIndexDetail,
				_KeywordDetail,
				_DocumentationDetail
			);
         
             Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update project set 
				ProjectName						=			_ProjectName,
				ProjectDescription				=			_ProjectDescription,
				ProjectManagerId				=			_ProjectManagerId,
				TeamMemberIds					=			_TeamMemberIds,
				ProjectStartedOn				=			_ProjectStartedOn,
				ProjectEndedOn					=			_ProjectEndedOn,
				ArchitectId						=			_ArchitectId,
				IsClientProject					=			_IsClientProject,
				ClientId						=			_ClientId,
				HomePageUrl						=			_HomePageUrl,
				PageIndexDetail					=			_PageIndexDetail,
				KeywordDetail					=			_KeywordDetail,
				DocumentationDetail				=			_DocumentationDetail
			where ProjectId 					= 			_ProjectId;
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_getAll_Project`(
	   /*

    Call sp_getAll_Project();

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

            Call sp_LogException (@Message, '', 'sp_getAll_Project', 1, 0, @Result);
		end;
        
        select * from project;
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_attendance_get_pending_requests_by_role $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_get_pending_requests_by_role`(

/*

    Call sp_attendance_get_pending_requests_by_role(22, 2);

*/

    _ManagerId bigint,
    _StatusId int
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_get_pending_requests_by_role', 1, 0, @Result);
		end;
        
        Set @AdminAccessId = 0;
        if(_StatusId = 0) then 
        begin
			Select * from approval_request
			where AssigneeId = _ManagerId Or AssigneeId = @AdminAccessId
			order by RequestedOn desc;
        end;
        else
        begin
			Select * from approval_request
			where (AssigneeId = _ManagerId Or AssigneeId = @AdminAccessId)
			and RequestStatusId = _StatusId
			order by RequestedOn desc;        
        end;
        end if;
	End;
End$$
DELIMITER ;




DELIMITER $$

drop procedure if exists sp_attendance_get_pending_requests $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_get_pending_requests`(

/*

    Call sp_attendance_get_pending_requests(22, 2);

*/

    _ManagerId bigint,
    _StatusId int
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_get_pending_requests', 1, 0, @Result);
		end;
              
        if(_StatusId = 0) then 
        begin
			Select * from approval_request
			where AssigneeId = _ManagerId
			order by RequestedOn desc;
        end;
        else
        begin
			Select * from approval_request
			where AssigneeId = _ManagerId
			and RequestStatusId = _StatusId
			order by RequestedOn desc;        
        end;
        end if;
	End;
End$$
DELIMITER ;
