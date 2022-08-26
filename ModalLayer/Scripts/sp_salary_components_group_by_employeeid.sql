DELIMITER $$

drop procedure if exists sp_salary_components_group_by_employeeid $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_components_group_by_employeeid`(

/*

	Call sp_salary_components_group_by_employeeid(39, 0);

*/
	 _EmployeeId long,
     _CTC decimal
)
Begin
	Declare groupId int;
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_salary_components_group_by_employeeid', 1, 0, @Result);
		end;  
		
        set @ctc = 0;
        
        if(_CTC = 0) then
        begin
			select CTC into @ctc from employee_salary_detail where EmployeeId = _EmployeeId;
		end;
        else 
        begin
			set @ctc = _CTC;
        end;
        end if;
        
        select SalaryGroupId into groupId from salary_group
        where @ctc >= MinAmount and
        @ctc <= MaxAmount;
                
		select * from salary_group
        where SalaryGroupId = groupId;
        
        select * from employee_salary_detail where EmployeeId = _EmployeeId;
	End;
End$$
DELIMITER ;
