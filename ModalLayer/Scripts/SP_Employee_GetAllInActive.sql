DELIMITER $$

drop procedure if exists SP_Employee_GetAllInActive $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employee_GetAllInActive`(
/*	

	Call SP_Employee_GetAllInActive(' 1=1', '', 1, 10);

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
			Call sp_LogException(@Message, '', 'SP_Employee_GetAllInActive', 1, 0, @Result);
		End;

        Begin
        
			if(_SortBy is null Or _SortBy = '') then
				Set _SortBy = ' CreatedOn Desc ';
			end if;
            
            Set @activeQuery = Concat('
				Select 
					emp.EmployeeId,
					emp.EmployeeCompleteJsonData,
                    emp.CreatedBy,
                    emp.CreatedOn,
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
						where EmployeeUid = emp.EmployeeId
					) as ClientJson
				from employee_archive emp
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
