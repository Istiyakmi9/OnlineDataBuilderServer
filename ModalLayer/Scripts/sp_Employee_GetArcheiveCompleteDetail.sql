DELIMITER $$

drop procedure if exists sp_Employee_GetArcheiveCompleteDetail $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employee_GetArcheiveCompleteDetail`(
	_EmployeeId bigint
/*	

	Call sp_Employee_GetArcheiveCompleteDetail(39);
    
    
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
			Call sp_LogException(@Message, '', 'sp_Employee_GetArcheiveCompleteDetail', 1, 0, @Result);
		End;
        
        select * from employee_archive
        where EmployeeId = _EmployeeId;
        
        -- select * from employeepayroll
        -- select * from employeebilldocuments
	End;
end$$
DELIMITER ;
