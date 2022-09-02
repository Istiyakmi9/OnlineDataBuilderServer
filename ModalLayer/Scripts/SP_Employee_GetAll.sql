DELIMITER $$

drop procedure if exists SP_Employee_GetAll $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employee_GetAll`(
/*	

	Call SP_Employee_GetAll(' 1=1', '', 1, 10, -1);

*/

	_SearchString varchar(500),
    _SortBy varchar(100),
    _PageIndex int,
    _PageSize int,
    _IsActive int
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
				Left Join employeepersonaldetail eper on emp.EmployeeUid = eper.EmployeeUid
				left join employeeprofessiondetail eprof on emp.EmployeeUid = eprof.EmployeeUid
				where ', _SearchString, '
			');
                
                
			Set @inActiveQuery = Concat('
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
									''ClientId'', ClientUid,
									''ClientName'', ClientName,
									''FinalPackage'', FinalPackage
								)
                            )
						from employeemappedclients 
						where EmployeeUid = emp.EmployeeUid
					) as ClientJson,
                    emp.UpdatedOn, 
                    emp.CreatedOn
				from employee_archive emp
				Left Join employeepersonaldetail_archive eper on emp.EmployeeUid = eper.EmployeeUid
				left join employeeprofessiondetail_archive eprof on emp.EmployeeUid = eprof.EmployeeUid
				where ', _SearchString, '
			');			
            
            Set @SelectQuery = '';
            if(_IsActive = -1) then
            begin
				Set @SelectQuery = Concat('
					select *, Count(1) Over() as Total from (
						', @activeQuery ,'
						union all
						', @inActiveQuery ,'
					)T Order by ', _SortBy ,' limit ', _PageSize ,' offset ', (_PageIndex - 1) * 10
				);
            end;
            else
            begin
				if(_IsActive = 1) then
					Set @SelectQuery = Concat('
						select *, Count(1) Over() as Total from (
							', @activeQuery ,'
						)T Order by ', _SortBy ,' limit ', _PageSize ,' offset ', (_PageIndex - 1) * 10
					);
				else
					Set @SelectQuery = Concat('
						select *, Count(1) Over() as Total  from (
							', @inActiveQuery ,'
						)T Order by ', _SortBy ,' limit ', _PageSize ,' offset ', (_PageIndex - 1) * 10
					);
				end if;
            end;
            end if;
        

            
		# select @SelectQuery;
		prepare SelectQuery from @SelectQuery;
		execute SelectQuery;
		End;
	End;
end$$
DELIMITER ;
