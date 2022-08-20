DELIMITER $$

drop procedure if exists SP_ManageEmployeeDetail_Get $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_ManageEmployeeDetail_Get`(
	_employeeId bigint
/*	

	Call SP_ManageEmployeeDetail_Get(1);

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
            
            if exists (select 1 from employees where EmployeeUid = _employeeId) then
            begin
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
                    Case
						When el.LeavePlanId is null then 0
                        else el.LeavePlanId
					End LeavePlanId
				from employees e 
				Inner Join employeepersonaldetail ep on e.EmployeeUid = ep.EmployeeUid
				Inner Join employeeprofessiondetail ef on e.EmployeeUid = ef.EmployeeUid
                Left join employee_leaveplan_mapping el on el.EmployeeId = e.EmployeeUid
				Where e.EmployeeUid = _employeeId;
                        
				Select * from employeemappedclients 
				where EmployeeUid = _employeeId; #and IsActive = 1;
            
				Select FileId, FilePath, FileName, FileExtension, UserTypeId from userfiledetail 
				where FileOwnerId = _employeeId and FileName = 'profile';
            end;
            else
            begin
				Select UserTypeId from employee_archive
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
					Case
						When el.LeavePlanId is null then 0
                        else el.LeavePlanId
					End LeavePlanId
				from employee_archive e 
				Inner Join employeepersonaldetail_archive ep on e.EmployeeUid = ep.EmployeeUid
				Inner Join employeeprofessiondetail_archive ef on e.EmployeeUid = ef.EmployeeUid
                Left join employee_leaveplan_mapping el on el.EmployeeId = e.EmployeeUid
				Where e.EmployeeUid = _employeeId;
							
				Select * from employeemappedclients 
				where EmployeeUid = _employeeId; #and IsActive = 0;
            
				Select FileId, FilePath, FileName, FileExtension, UserTypeId from userfiledetail 
				where FileOwnerId = _employeeId and FileName = 'profile';
            end;
            end if;
            
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