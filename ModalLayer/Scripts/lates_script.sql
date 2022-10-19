drop table employee_request;

create table leave_request_notification(
	LeaveRequestNotificationId bigint primary key auto_increment,
    LeaveRequestId bigint,
    UserMessage text,
    EmployeeId bigint,
    AssigneeId bigint,
    ProjectId bigint,
    ProjectName varchar(150),
    FromDate Datetime,
    ToDate Datetime,
    NumOfDays decimal,
    RequestStatusId int,
    LeaveTypeId int,
    FeedBackMessage varchar(500),
    LastReactedOn Datetime null,
    CreatedOn Datetime,
    UpdatedOn Datetime,
    CreatedBy bigint,
    UpdatedBy bigint
)


DELIMITER $$

drop procedure if exists sp_leave_request_notification_get_byId $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_request_notification_get_byId`(
	_LeaveRequestNotificationId bigint
    
#	Call sp_leave_request_notification_get_byId(1);
    
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);

            Call sp_LogException (@Message, '', 'sp_leave_request_notification_get_byId', 1, 0, @Result);
		end;
        
		Select n.*, LeaveDetail from leave_request_notification n
        left join employee_leave_request l on l.LeaveRequestId = n.LeaveRequestId
		where LeaveRequestNotificationId = _LeaveRequestNotificationId;
	End;
End$$
DELIMITER ;
