DELIMITER $$

drop procedure if exists sp_approval_request_attendace_InsUpdate $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_approval_request_attendace_InsUpdate`(

	/*
    set @result = '';
    call sp_approval_request_attendace_InsUpdate(0, 'Marghub', 'Message', 4, 2, '2022-05-22', 'marghub12@mail.com', '9333', '2022-05-22', '2022-05-23', 5, 0, '', 2, 0, 0, 2, '{}', @result);
	select @result;
    */

	_ApprovalRequestId bigint,
	_UserName varchar(100),
	_Message varchar(500),
	_UserId bigint,
	_UserTypeId int,
	_RequestedOn DateTime,
	_Email varchar(100),
	_Mobile varchar(14),
	_FromDate DateTime,
	_ToDate DateTime,
	_AssigneeId bigint,
	_ProjectId bigint,
	_ProjectName varchar(100),
    _RequestStatusId int,
    _AttendanceId bigint,
    _AttendanceDetail Json,
    _LeaveType int,
    _RequestType int,
    _LeaveRequestId bigint,
    out _ProcessingResult varchar(100)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Set _ProcessingResult = @Message;    
            
            RollBack;
            SET autocommit = 1;
            Call sp_LogException (@Message, @OperationStatus, 'sp_approval_request_attendace_InsUpdate', 1, 0, @Result);
		end;
        
        SET autocommit = 0;
        Start Transaction;
		Begin 
			If not exists (Select * from approval_request Where ApprovalRequestId = _ApprovalRequestId) then
			Begin
				Insert into approval_request Values (
					default,
					_UserName,
					_Message,
					_UserId,
					_UserTypeId,
					utc_date(),
					_Email,
					_Mobile,
					_FromDate,
					_ToDate,
					_AssigneeId,
					_ProjectId,
					_ProjectName,
                    _RequestStatusId,
					_AttendanceId,
                    _RequestType,
                    _LeaveType,
                    _LeaveRequestId
				);
                
                Set _ProcessingResult = 'inserted';
			End;
			Else
			Begin
				Update approval_request SET 
					UserName						=			_UserName,
					Message							=			_Message,
					UserId							=			_UserId,
					UserTypeId						=			_UserTypeId,
					RequestedOn						=			utc_date(),
					Email							=			_Email,
					Mobile							=			_Mobile,
					FromDate						=			_FromDate,
					ToDate							=			_ToDate,
					AssigneeId						=			_AssigneeId,
					ProjectId						=			_ProjectId,
					ProjectName						=			_ProjectName,
                    RequestStatusId					=			_RequestStatusId,
					AttendanceId					=			_AttendanceId,
                    LeaveType						=			LeaveType,
                    RequestType						=			_RequestType
				Where 	ApprovalRequestId = _ApprovalRequestId;
                
                if(_AttendanceDetail is not null && _AttendanceDetail <> '') then
                begin
					Update attendance
						Set AttendanceDetail = _AttendanceDetail
					where AttendanceId = _AttendanceId;                
                end;
                end if;
                
                Set _ProcessingResult = 'updated';
			End;
			End if;
            COMMIT;
		End;
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_approval_request_Get_to_act $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_approval_request_Get_to_act`(
	_UserId bigint
    
	/*    
		
		Call sp_approval_request_Get_to_act(1);
		
	*/
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
										
			Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
            Call sp_LogException (@Message, '', 'sp_approval_request_Get_to_act', 1, 0, @Result);
		end;
        
        Select * from approval_request;
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_approval_request_GetById $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_approval_request_GetById`(
	_ApprovalRequestId bigint,
    _LeaveRequestId int,
    _RequestType int
    
#	Call sp_approval_request_GetById(1, 1, 1);    
    
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);

            Call sp_LogException (@Message, '', 'sp_approval_request_GetById', 1, 0, @Result);
		end;
        
        if(_RequestType = 1) then
        begin
			set @leaveDetail = '';
			select LeaveDetail into @leaveDetail from employee_leave_request
            where LeaveRequestId = _LeaveRequestId;

			Select *, @leaveDetail LeaveDetail from approval_request
            where ApprovalRequestId = _ApprovalRequestId;
        end;
        elseif (_RequestType = 2) then
        begin
			Select r.*, AttendanceDetail from approval_request r
			inner join attendance a on r.AttendanceId = a.AttendanceId
			where ApprovalRequestId = _ApprovalRequestId;			
        end;
        end if;
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_approval_request_InsUpdate $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_approval_request_InsUpdate`(

	/*
    set @result = '';
    call sp_approval_request_InsUpdate(0, 'Marghub', 'Message', 4, 2, '2022-05-22', 'marghub12@mail.com', '9333', '2022-05-22', '2022-05-23', 5, 0, '', 2, 0, 0, 2, '{}', @result);
	select @result;
    */

	_ApprovalRequestId bigint,
	_UserName varchar(100),
	_Message varchar(500),
	_UserId bigint,
	_UserTypeId int,
	_RequestedOn DateTime,
	_Email varchar(100),
	_Mobile varchar(14),
	_FromDate DateTime,
	_ToDate DateTime,
	_AssigneeId bigint,
	_ProjectId bigint,
	_ProjectName varchar(100),
    _RequestStatusId int,
    _AttendanceId bigint,
    _AttendanceDetail Json,
    _LeaveType int,
    _RequestType int,
    _LeaveRequestId bigint,
    out _ProcessingResult varchar(100)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Set _ProcessingResult = @Message;    
            
            RollBack;
            SET autocommit = 1;
            Call sp_LogException (@Message, @OperationStatus, 'sp_approval_request_InsUpdate', 1, 0, @Result);
		end;
        
        SET autocommit = 0;
        Start Transaction;
		Begin 
			If not exists (Select * from approval_request Where ApprovalRequestId = _ApprovalRequestId) then
			Begin
				Insert into approval_request Values (
					default,
					_UserName,
					_Message,
					_UserId,
					_UserTypeId,
					utc_date(),
					_Email,
					_Mobile,
					_FromDate,
					_ToDate,
					_AssigneeId,
					_ProjectId,
					_ProjectName,
                    _RequestStatusId,
					_AttendanceId,
                    _RequestType,
                    _LeaveType,
                    _LeaveRequestId
				);
                
                Set _ProcessingResult = 'inserted';
			End;
			Else
			Begin
				Update approval_request SET 
					UserName						=			_UserName,
					Message							=			_Message,
					UserId							=			_UserId,
					UserTypeId						=			_UserTypeId,
					RequestedOn						=			utc_date(),
					Email							=			_Email,
					Mobile							=			_Mobile,
					FromDate						=			_FromDate,
					ToDate							=			_ToDate,
					AssigneeId						=			_AssigneeId,
					ProjectId						=			_ProjectId,
					ProjectName						=			_ProjectName,
                    RequestStatusId					=			_RequestStatusId,
					AttendanceId					=			_AttendanceId,
                    LeaveType						=			LeaveType,
                    RequestType						=			_RequestType,
                    LeaveRequestId					=			_LeaveRequestId
				Where 	ApprovalRequestId = _ApprovalRequestId;
                
                if(_AttendanceDetail is not null And _AttendanceDetail <> '') then
                begin
					Update attendance
						Set AttendanceDetail = _AttendanceDetail
					where AttendanceId = _AttendanceId;                
                end;
                end if;
                
                Set _ProcessingResult = 'updated';
			End;
			End if;
            COMMIT;
		End;
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_approval_request_leave_InsUpdate $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_approval_request_leave_InsUpdate`(

	/*
    set @result = '';
    call sp_approval_request_leave_InsUpdate(0, 'Marghub', 'Message', 4, 2, '2022-05-22', 'marghub12@mail.com', '9333', '2022-05-22', '2022-05-23', 5, 0, '', 2, 0, 0, 2, '{}', @result);
	select @result;
    */

	_ApprovalRequestId bigint,
	_UserName varchar(100),
	_Message varchar(500),
	_UserId bigint,
	_UserTypeId int,
	_RequestedOn DateTime,
	_Email varchar(100),
	_Mobile varchar(14),
	_FromDate DateTime,
	_ToDate DateTime,
	_AssigneeId bigint,
	_ProjectId bigint,
	_ProjectName varchar(100),
    _RequestStatusId int,
    _LeaveRequestId bigint,
    _LeaveDetail Json,
    _LeaveType int,
    _RequestType int,
    _AttendanceId bigint,
    out _ProcessingResult varchar(100)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Set _ProcessingResult = @Message;    
            
            RollBack;
            SET autocommit = 1;
            Call sp_LogException (@Message, @OperationStatus, 'sp_approval_request_leave_InsUpdate', 1, 0, @Result);
		end;
        
        SET autocommit = 0;
        Start Transaction;
		Begin 
			If not exists (Select * from approval_request Where ApprovalRequestId = _ApprovalRequestId) then
			Begin select * from approval_request;
				Insert into approval_request Values (
					default,
					_UserName,
					_Message,
					_UserId,
					_UserTypeId,
					utc_date(),
					_Email,
					_Mobile,
					_FromDate,
					_ToDate,
					_AssigneeId,
					_ProjectId,
					_ProjectName,
                    _RequestStatusId,
					_AttendanceId,
                    _RequestType,
                    _LeaveType,
                    _LeaveRequestId
				);
                
                Set _ProcessingResult = 'inserted';
			End;
			Else
			Begin
				Update approval_request SET 
					UserName						=			_UserName,
					Message							=			_Message,
					UserId							=			_UserId,
					UserTypeId						=			_UserTypeId,
					RequestedOn						=			utc_date(),
					Email							=			_Email,
					Mobile							=			_Mobile,
					FromDate						=			_FromDate,
					ToDate							=			_ToDate,
					AssigneeId						=			_AssigneeId,
					ProjectId						=			_ProjectId,
					ProjectName						=			_ProjectName,
                    RequestStatusId					=			_RequestStatusId,
					AttendanceId					=			_AttendanceId,
                    LeaveType						=			LeaveType,
                    RequestType						=			_RequestType,
                    LeaveRequestId					=			_LeaveRequestId
				Where 	ApprovalRequestId = _ApprovalRequestId;
                
                if(_LeaveDetail is not null && _LeaveDetail <> '') then
                begin
					Update employee_leave_request
						Set LeaveDetail 		= 	_LeaveDetail
					where LeaveRequestId = _LeaveRequestId;                
                end;
                end if;
                
                Set _ProcessingResult = 'updated';
			End;
			End if;
            
			Select * from approval_request
			where AssigneeId = _AssigneeId
			and RequestStatusId = 2
			order by RequestedOn desc;        
            
            COMMIT;
		End;
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_approval_requests_get $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_approval_requests_get`(

/*

    Call sp_approval_requests_get(3, 2, 2022, 10);

*/

    _ManagerId bigint,
    _StatusId int,
    _ForYear int,
	_ForMonth int
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_approval_requests_get', 1, 0, @Result);
		end;
              
        if(_StatusId = 0) then 
        begin
			Select * from approval_request
			where AssigneeId = _ManagerId
			order by RequestedOn desc;
        end;
        else
        begin
			Select * from approval_request
			where AssigneeId = _ManagerId
			and RequestStatusId = _StatusId
			order by RequestedOn desc;        
        end;
        end if;
        
        select a.* from attendance a
        inner join employees e on a.EmployeeId = e.EmployeeUid
        where e.ReportingManagerId = _ManagerId
        and ForYear = _ForYear 
        and ForMonth = _ForMonth;
        
	End;
End$$
DELIMITER ;
