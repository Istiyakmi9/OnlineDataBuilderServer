DELIMITER $$

drop procedure if exists sp_Billing_detail $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Billing_detail`(

/*


	Call sp_Billing_detail(1, 2, '000292', 2, 1, 4, 2022);


*/

    _sender bigint,
    _receiver bigint,
    _billNo varchar(45),
    _employeeId bigint,
    _userTypeId int,
    _forMonth int,
    _forYear int
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_Billing_detail', 1, 0, @Result);
			end;
            
            # Calculate amount paid between dates
            Select * from company 
            where CompanyId = _sender;
            
            Select * from clients 
            where clients.ClientId = _receiver;
            
            Select * from employeemappedclients
            Where EmployeeUid = _employeeId 
            And ClientUid = _receiver;
            
            select f.* from billdetail b
			inner join filedetail f on b.FileDetailId = f.FileId
			where b.BillNo = _billNo;
            
            select a.* from attendance a
            where a.EmployeeId = _employeeId
            and a.UserTypeId = _userTypeId
            and a.ForMonth = _forMonth
			and a.ForYear = _forYear;
		End;
	End$$
DELIMITER ;
