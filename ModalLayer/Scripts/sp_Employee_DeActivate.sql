DELIMITER $$

drop procedure if exists sp_Employee_DeActivate $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employee_DeActivate`(

/*

	Call sp_Employee_DeActivate(13);

*/
	_EmployeeId bigint,
	_FullName varchar(150),
	_Mobile varchar(20),
	_Email varchar(100),
	_Package decimal,
	_DateOfJoining datetime,
	_DateOfLeaving datetime,
	_EmployeeCompleteDetailModal Json
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
            
			Call sp_LogException (@Message, @OperationStatus, 'sp_Employee_DeActivate', 1, 0, @Result);
		end;  
	
		set autocommit = 0;

		start transaction;
        begin					
            insert into employee_archive values(
				_EmployeeId,
				_FullName,
				_Mobile,
				_Email,
				_Package,
				_DateOfJoining,
				_DateOfLeaving,
				_EmployeeCompleteDetailModal
            );
            
            Set sql_safe_updates = 0;
            
			delete from employees e
			where e.EmployeeUid = _EmployeeId;

			delete from employeepersonaldetail
			where EmployeeUid = _EmployeeId;

			delete from employeeprofessiondetail
			where EmployeeUid = _EmployeeId;

			delete from employeelogin
			where EmployeeId = _EmployeeId;
			
			delete from employee_declaration
			where EmployeeId = _EmployeeId;
			
			delete from employee_leave_request
			where EmployeeId = _EmployeeId;
			
			delete from employee_notice_period
			where EmployeeId = _EmployeeId;
            
			delete from employee_salary_detail
			where EmployeeId = _EmployeeId;
			
			delete from employee_timesheet
			where EmployeeId = _EmployeeId;
			
			delete from employeemappedclients
			where EmployeeUid = _EmployeeId;  
        end;        
        commit;
        Set sql_safe_updates = 1;
        Set autocommit = 1;
	End;
End$$
DELIMITER ;
