DELIMITER $$

drop procedure if exists SP_Employees_ById $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employees_ById`(
	_EmployeeId int,
    _IsActive int
/*	

	Call SP_Employees_ById(4, -1);

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
			Call sp_LogException(@Message, '', 'SP_Employees_ById', 1, 0, @Result);
		End;

        Begin			
            if(_IsActive = 1) then
            begin				
                Select 
					e.EmployeeUid, 
					e.FirstName,
					e.LastName,
					e.Mobile,
					e.Email,
                    e.LeavePlanId,
                    e.PayrollGroupId,
                    e.SalaryGroupId,
					e.CompanyId,
					e.NoticePeriodId,
					ep.SecondaryMobile,
					ep.Gender,
					ep.Address,
					ep.IsPermanent,
					ep.ActualPackage,
					ep.FinalPackage,
					ep.TakeHomeByCandidate,
					er.DateofSalary,
					er.PerMonthSalary,
					er.PaySlip_PDF_Filepath,
					er.PaySlipNo,
					er.GeneratedOn,
					epro.EmpProfDetailUid,
					epro.ExprienceInYear,
					epro.Specification,
					epro.PANNo,
					epro.AadharNo,
					epro.AccountNumber,
					epro.BankName,
					epro.BranchName,
					epro.Domain,
					epro.IFSCCode,
					epro.LastCompanyName,
                    l.AccessLevelId,
                    l.UserTypeId,
                    e.CreatedOn,
					epro.ProfessionalDetail_Json
				from employees e
                inner join employeelogin l on l.EmployeeId = e.EmployeeUid
				Left Join employeepersonaldetail ep on e.EmployeeUid = ep.EmployeeUid
				Left Join employeepayroll er on e.EmployeeUid = er.EmployeeUid
				left join employeeprofessiondetail epro on e.EmployeeUid = epro.EmployeeUid
				Where e.EmployeeUid = _EmployeeId;
			end;
            elseif(_IsActive = 0) then
            begin
                Select 
					e.EmployeeUid, 
					e.FirstName,
					e.LastName,
					e.Mobile,
					e.Email,
					ep.SecondaryMobile,
					ep.Gender,
					ep.Address,
					ep.IsPermanent,
					ep.ActualPackage,
					ep.FinalPackage,
					ep.TakeHomeByCandidate,
					er.DateofSalary,
					er.PerMonthSalary,
					er.PaySlip_PDF_Filepath,
					er.PaySlipNo,
					er.GeneratedOn,
					epro.ExprienceInYear,
					epro.Specification,
					epro.PANNo,
					epro.AadharNo,
					epro.AccountNumber,
					epro.BankName,
					epro.BranchName,
					epro.Domain,
					epro.IFSCCode,
					epro.LastCompanyName,
                    l.AccessLevelId,
                    l.UserTypeId,
                    e.CreatedOn,
					epro.ProfessionalDetail_Json
				from employee_archive e
                inner join employeelogin l on l.EmployeeId = e.EmployeeUid
				Left Join employeepersonaldetail_archive ep on e.EmployeeUid = ep.EmployeeUid
				Left Join employeepayroll er on e.EmployeeUid = er.EmployeeUid
				left join employeeprofessiondetail_archive epro on e.EmployeeUid = epro.EmployeeUid
				Where e.EmployeeUid = _EmployeeId;
            end;
            else
            begin
				Select 
					e.EmployeeUid, 
					e.FirstName,
					e.LastName,
					e.Mobile,
					e.Email,
					ep.SecondaryMobile,
					ep.Gender,
					ep.Address,
					ep.IsPermanent,
					ep.ActualPackage,
					ep.FinalPackage,
					ep.TakeHomeByCandidate,
					er.DateofSalary,
					er.PerMonthSalary,
					er.PaySlip_PDF_Filepath,
					er.PaySlipNo,
					er.GeneratedOn,
					epro.ExprienceInYear,
					epro.Specification,
					epro.PANNo,
					epro.AadharNo,
					epro.AccountNumber,
					epro.BankName,
					epro.BranchName,
					epro.Domain,
					epro.IFSCCode,
					epro.LastCompanyName,
                    l.AccessLevelId,
                    l.UserTypeId,
                    e.CreatedOn,
					epro.ProfessionalDetail_Json                  
				from (
						Select * from employees 
                        union distinct
                        Select * from employee_archive
				) e
                inner join employeelogin l on l.EmployeeId = e.EmployeeUid
				Left Join employeepersonaldetail_archive ep on e.EmployeeUid = ep.EmployeeUid
				Left Join employeepayroll er on e.EmployeeUid = er.EmployeeUid
				left join employeeprofessiondetail_archive epro on e.EmployeeUid = epro.EmployeeUid
				Where e.EmployeeUid = _EmployeeId;
            end;
			end if;
		End;
	End;
end$$
DELIMITER ;
