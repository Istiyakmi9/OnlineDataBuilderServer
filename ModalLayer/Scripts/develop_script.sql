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
        
        select * from clients;
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
