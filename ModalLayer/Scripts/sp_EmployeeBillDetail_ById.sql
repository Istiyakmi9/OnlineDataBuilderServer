DELIMITER $$

drop procedure if exists sp_EmployeeBillDetail_ById $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_EmployeeBillDetail_ById`(
	_EmployeeId bigint,
    _ClientId bigint,
    _FileId bigint,
    _AdminId bigint
/*	

	Call sp_EmployeeBillDetail_ById(4, 5, 13, 1);
    
    
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
        
        Call SP_Employee_GetAll(' 1=1 ', null, 1, 10, 1);
        
        select * from company;
	End;
end$$
DELIMITER ;
