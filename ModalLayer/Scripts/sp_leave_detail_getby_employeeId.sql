DELIMITER $$

drop procedure if exists sp_leave_detail_getby_employeeId $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_detail_getby_employeeId`(
/*
	
    Call sp_leave_detail_getby_employeeId(39);
    
    
*/

	_EmployeeId bigint
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, '', 'sp_leave_page_getby_employeeId', 1, 0, @Result);
		end;

		select * from employees
        where DesignationId = 1 Or DesignationId = 4;		
        
        select l.* from leave_plan l 
        inner join employees e on e.LeavePlanId = l.LeavePlanId or l.IsDefaultPlan = 1
        where EmployeeUid = _EmployeeId;
	End;
End$$
DELIMITER ;
