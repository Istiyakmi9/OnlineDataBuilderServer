--
-- Dumping routines for database 'onlinedatabuilder'
--
/*!50003 DROP FUNCTION IF EXISTS `Fn_GenerateRandom_nextBill` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` FUNCTION `Fn_GenerateRandom_nextBill`(
	_BillTypeId int
) RETURNS varchar(25) CHARSET utf8mb4
    DETERMINISTIC
BEGIN
	Set @LoopCount = 0;
	Set @NextBillNo = '';
    Set @LastBillNo = 0;
    Set @Length = 0;
	
	SELECT LastBillNo, BillNoLength
	FROM bills WHERE BillTypeUid = _BillTypeId into @LastBillNo, @Length;
    Set @LastBillNo = @LastBillNo + 1;
	if(@LastBillNo > 0) then
		Begin
			Set @Length = @Length - LENGTH(CAST(@LastBillNo as char(50)));
			if(@Length > 0) then
				Begin
					SET @LoopCount = 0;
					SET @NextBillNo = @LastBillNo;
					WHILE (@LoopCount < @Length) do
						BEGIN
							SET @NextBillNo = CONCAT('0', @NextBillNo);
							SET @LoopCount = @LoopCount + 1;
						End;
					End while;
				END;
			End if;
		End;
	End if;
	SET @NextBillNo = CONCAT('#', @NextBillNo);
	RETURN @NextBillNo;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP FUNCTION IF EXISTS `Fn_Generate_newPassword` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` FUNCTION `Fn_Generate_newPassword`() RETURNS varchar(50) CHARSET utf8mb4
    DETERMINISTIC
BEGIN
	Set @NewPassword = null;
	Set @RowIndex = 0;
	Set @TotalRecords = 0;
	Select Count(1) from sportsname into @TotalRecords;
	SELECT FLOOR((RAND() * (@TotalRecords - 1 + 1)) + 1) into @RowIndex;
	
	Select SportName from sportsname where RowIndex = @RowIndex 
	into @NewPassword;
	Set @NewPassword = Concat("Bottomhalf_09@", Replace(@NewPassword, ' ', ''));
	RETURN @NewPassword;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_AccessLevel_InsUpd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_AccessLevel_InsUpd`(
	
/*	

	Call sp_AccessLevel_InsUpd(2, 'User', 'View or edit only personal detail.', @Result);
	Select @Result;

*/

	_AccessLevelId varchar(50),
    _RoleName varchar(100),
    _AccessCodeDefination varchar(100),
    out _ProcessingResult varchar(100)
)
Begin
    Begin
		Declare exit handler for sqlexception
		Begin
		
			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
										
			Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
			Call sp_LogException(@Message, @OperationStatus, 'sp_AccessLevel_InsUpd', 1, 0, @Result);
			Set _ProcessingResult = Concat(@Message, @Result);
		end;

		Set @AccessLevelId = 0;        
        if not exists(Select 1 from accesslevel where AccessLevelId = _AccessLevelId)then
        Begin
			if exists(Select 1 from accesslevel where lower(RoleName) = lower(_RoleName)) then
			begin
				Select AccessLevelId from accesslevel  
				where lower(RoleName) = lower(_RoleName) into @AccessLevelId;
				
				Update accesslevel Set AccessCodeDefination = _AccessCodeDefination, 
				UpdatedOn = now() where lower(RoleName) = lower(_RoleName);
			end;
			else
			begin
				Insert into accesslevel values(default, _RoleName, _AccessCodeDefination, now(), NULL);
			end;
			end if;    
            
            Set _ProcessingResult = LAST_INSERT_ID();
		end;
        else
        begin
			Update accesslevel Set RoleName = _RoleName, 
				AccessCodeDefination = _AccessCodeDefination, 
				UpdatedOn = now() 
            where AccessLevelId = _AccessLevelId;
                    
			Set _ProcessingResult = _AccessLevelId;
        end;
        end if;
        
        Select AccessLevelId, RoleName, AccessCodeDefination from accesslevel;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_AccessLevel_Sel` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_AccessLevel_Sel`(
	
/*	

	Call sp_AccessLevel_Sel();

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
			Call sp_LogException(@Message, '', 'sp_AccessLevel_Sel', 1, 0, @Result);
		End;

        Begin
            Select AccessLevelId, RoleName, AccessCodeDefination from accesslevel;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_adhoc_detail_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_adhoc_detail_get`(

/*

	Call sp_adhoc_detail_get();

*/
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_adhoc_detail_get', 1, 0, @Result);
			end;  
		
		select * from adhoc_detail;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_adhoc_detail_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_adhoc_detail_insupd`(

/*



	Set @result = '';
	Call sp_adhoc_detail_insupd(0, 'TEST', 'TESTing', 1, @result);
    select @result;



*/
	_AdHocId int,
    _Name varchar(100),
    _Description varchar(250),
    _AdminId bigint,
    out _ProcessingResult varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_adhoc_detail_insupd', 1, 0, @Result);
			end;  
		
		if not exists(select 1 from adhoc_detail where AdHocId = _AdHocId) then
        begin
			insert into adhoc_detail
            values(
				default,
				_Name,
				_Description,
				now(),
                null,
                _AdminId,
                null
            );
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update adhoc_detail set				
				Name					=			_Name,
				Description				=			_Description,
				UpdatedBy				=			_AdminId,
                UpdatedOn				=			utc_date()
            where AdHocId = _AdHocId;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_admin_dashboard_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_admin_dashboard_get`(

/*


	Call sp_admin_dashboard_get(6, 4, '2021-03-07 20:49:08.000000', '2022-07-13 20:49:08.000000');


*/

    _userId bigint,
    _employeeUid bigint,
    _fromDate datetime,
    _toDate datetime
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_admin_dashboard_get', 1, 0, @Result);
			end;
            
            # Calculate amount paid between dates
            Select 
				e.FirstName,
                e.LastName,
                e.Email,
                e.Mobile,
                c.ClientName,
                c.ClientId,
                b.PaidAmount,
                b.BillDetailUid,
                b.BillForMonth,
                b.BillYear,
                b.PaidOn,
                b.BillNo,
                b.BillUpdatedOn
            from billdetail b 
            Inner join employees e on b.EmployeeUid = e.EmployeeUid
            Left join clients c on b.ClientId = c.ClientId
            where b.BillStatusId = 1 and 
            b.EmployeeUid = _employeeUid and
            b.BillUpdatedOn between _fromDate and _toDate;
                     
			select 
				b.BillDetailUid, 
				b.EmployeeUid, 
				b.ClientId, 
                b.PaidAmount,
				b.BillYear,
                b.PaidOn,
                b.BillNo,
                g.amount,
                b.BillUpdatedOn ,
				case
					when g.gststatus is null then 4
					else g.gststatus
				end gststatus
			from billdetail b
			left join gstdetail g on b.BillNo = g.billno;


			Select EmployeeUid, FirstName, LastName, Mobile, Email, IsActive from employees;

			select 
				ClientId, 
				ClientName, 
				MobileNo, 
				PrimaryPhoneNo, 
				SecondaryPhoneNo, 
				Email, 
				FirstAddress, 
				SecondAddress, 
				ThirdAddress, 
				ForthAddress, 
				Pincode, 
				City, 
				State, 
				Country 
			from clients;

		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_ApplicationData_Get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_ApplicationData_Get`(
/*	

	Call SP_ApplicationData_Get();

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
			Call sp_LogException(@Message, '', 'SP_ApplicationData_Get', 1, 0, @Result);
		End;

        Begin            
            select * from accesslevel;
            
            select * from leave_plan;
            
            select 
				CompanyId,
				OrganizationId,
				OrganizationName,
				CompanyName,
				Country,
				State,
				City,
				FirstAddress,
				SecondAddress,
				ThirdAddress,
				ForthAddress,
				FullAddress,
				MobileNo,
				Email,
				FirstEmail,
				SecondEmail,
				ThirdEmail,
				ForthEmail,
				PrimaryPhoneNo,
				SecondaryPhoneNo,
				Fax,
				Pincode,
				IsPrimaryCompany,
				FixedComponentsId
            from company;            
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_approval_request_attendace_InsUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
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
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_approval_request_GetById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
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
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_approval_request_Get_to_act` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
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
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_approval_request_InsUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
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
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_approval_request_leave_InsUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
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
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_attandence_detail_by_employeeId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attandence_detail_by_employeeId`(

/*

    Call sp_attandence_detail_by_employeeId(0);

*/

    _EmployeeId bigint
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_attandence_detail_by_employeeId', 1, 0, @Result);
		end;
		
		select c.* from clients c
        inner join employeemappedclients ec on c.ClientId = ec.ClientUid
        where ec.EmployeeUid = _EmployeeId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_attendance_detall_pending` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_detall_pending`(

/*


    Call sp_attendance_detall_pending(4, 1, 3, 2022);


*/

    _EmployeeId bigint,
    _UserTypeId int,
    _ForMonth int,
    _ForYear int
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_detall_pending', 1, 0, @Result);
		end;
        
		Select 
			a.*
		from attendance a   
		Where a.EmployeeId = _EmployeeId
        And a.UserTypeId = _UserTypeId
        And ForYear = _ForYear and ForMonth = _ForMonth
        And TotalDays <> DaysPending;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_attendance_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'NO_AUTO_VALUE_ON_ZERO' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_get`(

/*


    Call sp_attendance_get(2, 5, 1, 2022, 4);


*/

    _EmployeeId bigint,
    _ClientId bigint,
    _UserTypeId int,
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_get', 1, 0, @Result);
		end;
		
		if (_ForYear > 0 && _ForMonth > 0) then
		begin
			Select 
				a.*
			from attendance a   
			Where a.EmployeeId = _EmployeeId
			And _ForYear = a.ForYear and _ForMonth = a.ForMonth
			And a.UserTypeId = _UserTypeId;
		end;
		else 
		begin
			Select 
				a.*
			from attendance a                 
			Where a.EmployeeId = _EmployeeId
			And a.UserTypeId = _UserTypeId;
		end;
		end if;
        
        Select distinct e.* from employees e
        -- left join employeemappedclients m on e.EmployeeUid = m.EmployeeUid
        Where e.EmployeeUid = _EmployeeId;
        -- And m.ClientUid = _ClientId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_attendance_getAll` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_getAll`(

/*


    Call sp_attendance_getAll(4, 5, 1, '2022-02-14 16:44:36');


*/

    _EmployeeId bigint,
    _ClientId bigint,
    _UserTypeId int,
    _Doj datetime
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_getAll', 1, 0, @Result);
		end;
        
		Select 
			a.*,
            e.DOJ
		from attendance a   
        Inner join employeemappedclients e on e.EmployeeUid = a.EmployeeId
		Where a.EmployeeId = _EmployeeId
        And e.ClientUid = _ClientId
        And a.UserTypeId = _UserTypeId
		And e.DOJ between CONVERT_TZ(_Doj, @@session.time_zone, '+00:00' ) and utc_timestamp()
        And ( FirstWeek = 0 or SecondWeek = 0  or ThirdWeek = 0 or ForthWeek = 0 or FifthWeek = 0 or SixthWeek = 0);
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_attendance_getall_pending` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_getall_pending`(

/*


    Call sp_attendance_getall_pending(4, 5, 1, 3, 2022);


*/

    _EmployeeId bigint,
    _ClientId bigint,
    _UserTypeId int,
    _ForMonth int,
    _ForYear int
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_getall_pending', 1, 0, @Result);
		end;
        
		Select 
			a.*
		from attendance a   
		Where a.EmployeeId = _EmployeeId
        And a.UserTypeId = _UserTypeId
        And ForYear = _ForYear and ForMonth = _ForMonth
        And TotalDays <> DaysPending;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `Sp_Attendance_GetById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `Sp_Attendance_GetById`(
	_EmployeeId long,
    _UserTypeId int,
    _ForMonth int,
    _ForYear int
    
/*


	call Sp_Attendance_GetById (4, 1, 4, 2022)


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
			Call sp_LogException(@Message, '', 'Sp_Attendance_GetById', 1, 0, @Result);
		End;
		
        select 
			a.*
		from attendance a
        where EmployeeId = _EmployeeId
        and UserTypeId = _UserTypeId
        And ForYear = _ForYear
        and ForMonth = _ForMonth;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_attendance_get_byid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_get_byid`(

/*


    Call sp_attendance_get_byid(2);


*/

    _AttendanceId bigint
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_get_byid', 1, 0, @Result);
		end;
		
		Select 
			a.*,
            e.CreatedOn DOJ
		from attendance a   
        Inner Join employees e on e.EmployeeUid = a.EmployeeId
		Where a.AttendanceId = _AttendanceId;
	
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_approval_requests_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
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
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_approval_requests_get_by_role` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_approval_requests_get_by_role`(

/*

    Call sp_approval_requests_get_by_role(22, 2);

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
			Call sp_LogException (@Message, @OperationStatus, 'sp_approval_requests_get_by_role', 1, 0, @Result);
		end;
        
        Set @AdminAccessId = 0;
        if(_StatusId = 0) then 
        begin
			Select * from approval_request
			where AssigneeId = _ManagerId Or AssigneeId = @AdminAccessId
			order by RequestedOn desc;
        end;
        else
        begin
			Select * from approval_request
			where (AssigneeId = _ManagerId Or AssigneeId = @AdminAccessId)
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
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_attendance_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_insupd`(

/*

	Set @OutParam = '';
	Call sp_attendance_insupd(0, 4, 1, '2022-02-15', 9.0, 0, 0, 7, 'working', '2022-02-15', 4, @OutParam);
    Select @OutParam;

*/

	_AttendanceId bigint,
	_EmployeeId bigint,
	_UserTypeId int,
	_AttendanceDetail json,
	_TotalDays int,
    _TotalWeekDays int,
	_DaysPending int,
	_TotalBurnedMinutes int,
    _ForYear int,
	_ForMonth int,
    _UserId long,
    out _ProcessingResult varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_insupd', 1, 0, @Result);
			end;
            
            Set _ProcessingResult = '';
			Begin
				If not exists (Select 1 from attendance where AttendanceId = _AttendanceId) then
                Begin
					Insert into attendance values(
						default,
						_EmployeeId,
						_UserTypeId,
						_AttendanceDetail,
						_TotalDays,
                        _TotalWeekDays,
						_DaysPending,
						_TotalBurnedMinutes,
                        _ForYear,
						_ForMonth,
                        now(),
                        null,
                        _UserId,
                        null
					);
                End;
                Else
                Begin
					Update attendance Set
						EmployeeId				=		_EmployeeId,
						UserTypeId				=		_UserTypeId,
						AttendanceDetail		=		_AttendanceDetail,
						TotalDays				=		_TotalDays,
                        TotalWeekDays			=		_TotalWeekDays,
						DaysPending				=		_DaysPending,
						TotalBurnedMinutes		=		_TotalBurnedMinutes,
						ForMonth				=		_ForMonth,
                        ForYear					=		_ForYear,
                        UpdatedOn				=		NOW(),
                        UpdatedBy				= 		_UserId
					where AttendanceId 			= 		_AttendanceId;
                End;
                End if;	
                
                Set _ProcessingResult = 'Inserted/Updated successfully';
			End;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_attendance_insupd_by_monthandyear` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_insupd_by_monthandyear`(

/*

	Set @OutParam = '';
	Call sp_attendance_insupd_by_monthandyear(0, 4, 1, '2022-02-15', 9.0, 0, 0, 7, 'working', '2022-02-15', 4, @OutParam);
    Select @OutParam;

*/

	_AttendanceId bigint,
	_EmployeeId bigint,
	_UserTypeId int,
	_AttendanceDetail json,
	_TotalDays int,
    _TotalWeekDays int,
	_DaysPending int,
	_TotalBurnedMinutes int,
    _ForYear int,
	_ForMonth int,
    _UserId long,
    out _ProcessingResult varchar(100)
)
Begin
    Set @OperationStatus = '';
		Begin
			Declare Exit handler for sqlexception
			Begin
				Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
											@errorno = MYSQL_ERRNO,
											@errortext = MESSAGE_TEXT;
                             
				Set sql_safe_updates = 1;
				Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
				Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_insupd_by_monthandyear', 1, 0, @Result);
			end;
            
            Set _ProcessingResult = '';
			Begin
				If not exists (Select 1 from attendance where EmployeeId = _EmployeeId and UserTypeId = _UserTypeId 
								and ForMonth = _ForMonth and ForYear = _ForYear) then
                Begin
					Insert into attendance values(
						default,
						_EmployeeId,
						_UserTypeId,
						_AttendanceDetail,
						_TotalDays,
                        _TotalWeekDays,
						_DaysPending,
						_TotalBurnedMinutes,
                        _ForYear,
						_ForMonth,
                        now(),
                        null,
                        _UserId,
                        null
					);
                End;
                Else
                Begin
                
					Set sql_safe_updates = 0;
					Update attendance Set
						EmployeeId				=		_EmployeeId,
						UserTypeId				=		_UserTypeId,
						AttendanceDetail		=		_AttendanceDetail,
						TotalDays				=		_TotalDays,
                        TotalWeekDays			=		_TotalWeekDays,
						DaysPending				=		_DaysPending,
						TotalBurnedMinutes		=		_TotalBurnedMinutes,
						ForMonth				=		_ForMonth,
                        ForYear					=		_ForYear,
                        UpdatedOn				=		NOW(),
                        UpdatedBy				= 		_UserId
					where EmployeeId 			= 		_EmployeeId and 
						  UserTypeId 			= 		_UserTypeId and 
                          ForMonth 				= 		_ForMonth and 
                          ForYear 				= 		_ForYear;
                End;
                End if;	
                
                Set sql_safe_updates = 1;
                Set _ProcessingResult = 'Inserted/Updated successfully';
			End;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_attendance_update_timesheet` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_update_timesheet`(

/*


	set @result = '';
    Call sp_attendance_update_timesheet(0, '[]', '2022-05-22', '2022-05-23', 1, '', 1, 1, 1, 1, 1, 2022, 8, 1, @result);
    select @result;


*/

    _AttendanceId bigint,
    _AttendanceDetail Json,
	_FromDate DateTime,
	_ToDate DateTime,
    _UserTypeId int,
    _Message varchar(500),
    _EmployeeId bigint,    
	_TotalDays int,
    _TotalWeekDays int,
	_DaysPending int,
	_TotalBurnedMinutes int,
    _ForYear int,
	_ForMonth int,
    _UserId long,
    out _ProcessingResult varchar(100)
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
            Rollback;
            Set _ProcessingResult = '';
            SET autocommit = 1;
			Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_update_timesheet', 1, 0, @Result);
		end;
		Set _ProcessingResult = '';
        
        SET autocommit = 0;
        Start Transaction;
        Begin        
			if exists (select 1 from attendance where AttendanceId = _AttendanceId) then
			Begin
				update attendance Set
					AttendanceDetail		=		_AttendanceDetail
				where AttendanceId 			= 		_AttendanceId;
				
				set _ProcessingResult = 'updated';
			End;
			else
			Begin
				Set @@SESSION.information_schema_stats_expiry = 0;
            
				SELECT AUTO_INCREMENT into _AttendanceId
				FROM information_schema.tables
				WHERE table_name = 'attendance'
				AND table_schema = DATABASE();			
                
				Insert into attendance values(
					default,
					_EmployeeId,
					_UserTypeId,
					_AttendanceDetail,
					_TotalDays,
					_TotalWeekDays,
					_DaysPending,
					_TotalBurnedMinutes,
					_ForYear,
					_ForMonth,
					now(),
					null,
					_UserId,
					null
				);
                
				set _ProcessingResult = 'inserted';
			End;
			End if;
            
			Set @EmployeeName = '';
            Set @Email = '';
            Set @Mobile = '';
            Set @ManagerId = 0;
            Set @ProjectId = 0;
            Set @ProjectName = null;
			Set @outCome = '';
            
            Select 
				Concat(FirstName, ' ', LastName) FullName, Mobile, Email, ReportingManagerId 
            from employees 
            where EmployeeUid = _EmployeeId
            into @EmployeeName, @Mobile, @Email, @ManagerId;
            
            Set @approvalRequestId = 0;
            
            Select ApprovalRequestId from approval_request 
            where AttendanceId = _AttendanceId
            and datediff(_FromDate, FromDate) = 0
            and datediff(_ToDate, ToDate) = 0
            into @approvalRequestId;
            
            if(@approvalRequestId is null) then
				Set @approvalRequestId = 0;
			end if;
            
            Call sp_approval_request_attendace_InsUpdate(
				@approvalRequestId,
				@EmployeeName,
				_Message,
				_EmployeeId,
				_UserTypeId,
				utc_date(),
				@Email,
				@Mobile,
				_FromDate,
				_ToDate,
				@ManagerId,
				@ProjectId,
				@ProjectName,
                2,
                _AttendanceId,
                null,
                0,
                2,
                0,
                @outCome
			);
		End;
        Commit;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_AuthenticationToken_VerifyAndGet` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_AuthenticationToken_VerifyAndGet`(
	_UserId bigint,
    _Mobile varchar(20),
    _Email varchar(50)
    
/*

    Call SP_AuthenticationToken_VerifyAndGet(0, null, 'istiyaq.mi9@gmail.com');

*/    
    
    
)
Begin
	Declare Exit handler for sqlexception
	Begin
		Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
		Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
		Call sp_LogException (@Message, @OperationStatus, 'SP_AuthenticationToken_VerifyAndGet', 1, 0, @Result);
	end;    

	If exists(Select 1 from UserDetail Where (Mobile = _Mobile Or EmailId = _Email Or UserId  = _UserId)) then
	Begin
        If (_UserId <= 0) then
		Begin	
            Select UserId from userdetail 
			where (Mobile = _Mobile Or EmailId = _Email Or UserId  = _UserId)
			Into _UserId;
		End;
        End if;
    
		If Exists(SELECT 1 FROM  RefreshToken r WHERE
					UserId = _UserId AND DATEDIFF(r.ExpiryTime, NOW()) >= 0) then
        Begin
			SELECT * FROM  RefreshToken r WHERE
					UserId = _UserId AND DATEDIFF(r.ExpiryTime, NOW()) >= 0;
        End;
        Else
        Begin
			Delete from RefreshToken 
            where UserId = _UserId;
        End;
        End if;
	End;
    End if;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_bank_accounts_getById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_bank_accounts_getById`(
	_BankAccountId int
    
/*

	Call sp_bank_accounts_getById(1);

*/
	
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_bank_accounts_getById', 1, 0, @Result);
			end;  
            
			Select * from bank_accounts
			where BankAccountId = _BankAccountId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_bank_accounts_getby_cmpId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_bank_accounts_getby_cmpId`(
/*	

	Call sp_bank_accounts_getby_cmpId(' 1=1  and CompanyId = 1 and OrganizationId=1', '', 1, 10);

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
			Call sp_LogException(@Message, '', 'sp_bank_accounts_getby_cmpId', 1, 0, @Result);
		End;

        Begin

			if(_SortBy is null Or _SortBy = '') then
				Set _SortBy = ' UpdatedOn Desc, CreatedOn Desc ';
			end if;

           Set @activeQuery = Concat('
				Select *
				from bank_accounts 
				where ', _SearchString, '
			');	

            Set @SelectQuery = '';
			Set @SelectQuery = concat('
				select *, Count(1) Over() as Total from (
					', @activeQuery ,'
				)T Order by ', _SortBy ,' limit ', _PageSize ,' offset ', (_PageIndex - 1) * 10
			);
            
		# select @SelectQuery;
		prepare SelectQuery from @SelectQuery;
		execute SelectQuery;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_bank_accounts_get_by_orgId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_bank_accounts_get_by_orgId`(
	_OrganizationId int
    
/*

	Call sp_bank_accounts_get_by_orgId(1);

*/
	
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_bank_accounts_get_by_orgId', 1, 0, @Result);
			end;  
            
			Select * from bank_accounts
			where OrganizationId = _OrganizationId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_bank_accounts_intupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_bank_accounts_intupd`(
	_BankAccountId int,
    _OrganizationId int,
    _CompanyId int,
    _BankName varchar(100),
    _BranchCode varchar(20),    
    _Branch varchar(50),
    _IFSC varchar(20),
    _AccountNo varchar(45),
    _OpeningDate datetime,
    _ClosingDate datetime,
    _IsPrimaryAccount bit,
    _AdminId bigint,
	out _ProcessingResult varchar(50)
    
/*
	set @out = '';
	Call sp_bank_accounts_intupd(0,0,2,'SBI','14544','Asansol','SBIN0014544','20141408787',now(), );
	select @out;
*/
	
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_bank_accounts_intupd', 1, 0, @Result);
			end;  
			start transaction;
            
            if (_IsPrimaryAccount = 1) then 
            begin
				SET SQL_SAFE_UPDATES = 0;
				Update bank_accounts Set
					IsPrimaryAccount 			=			false
					where CompanyId 			= 			_CompanyId;
				SET SQL_SAFE_UPDATES = 1;
            end;
            end if;
            
			if not exists(select * from bank_accounts where BankAccountId = _BankAccountId) then
            begin
				Insert into bank_accounts values(
					default,
					_OrganizationId,
                    _CompanyId,
					_BankName,
                    _BranchCode,
					_Branch,    
					_IFSC,
					_AccountNo,
					_OpeningDate,
                    _ClosingDate,
					_IsPrimaryAccount,
                    _AdminId,
                    null,
                    utc_date(),
                    null
				);
                
                Set _ProcessingResult = 'inserted';
			end;
            else 
            begin
				Update bank_accounts Set
					OrganizationId			=			_OrganizationId,
                    CompanyId				=			_CompanyId,
					BankName				=			_BankName,
					BranchCode				=			_BranchCode,
					Branch					=			_Branch,
					IFSC					=			_IFSC,
					AccountNo				=			_AccountNo,
					OpeningDate				=			_OpeningDate,
                    ClosingDate				=			_ClosingDate,
                    IsPrimaryAccount 		=			_IsPrimaryAccount,
                    UpdatedBy				=			_AdminId,
                    UpdatedOn				=			utc_date()
                where BankAccountId 		= 			_BankAccountId;
                
                Set _ProcessingResult = 'updated';
            end;
            end if;
            commit;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_billdata_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_billdata_get`(
	_BillTypeUid bigint
/*	

	Call sp_billdata_get(1);

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
			Call sp_LogException(@Message, '', 'sp_billdata_get', 1, 0, @Result);
		End;

        Begin
			Set @NewBillNo = '';
			select Fn_GenerateRandom_nextBill(_BillTypeUid) Into @NewBillNo;
			select BillUid, 
				LastBillNo, 
				LastBillNo + 1 as NextBillNo, 
				@NewBillNo GeneratedBillNo,
                BillNoLength
            from bills where BillTypeUId = _BillTypeUid;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_billdetail_filter` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_billdetail_filter`(
	_Type varchar(20),
	_Uid bigint,
	_searchString varchar(250),
	_sortBy varchar(50),
	_pageIndex int,
	_pageSize int
/*	

	Call sp_billdetail_filter('employee', 0, '1=1 ', '', 1, 10);

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
			Call sp_LogException(@Message, '', 'sp_billdetail_filter', 1, 0, @Result);
		End;

        Begin
			If(_sortBy is NULL OR _sortBy = '') then
            begin
				Set _sortBy = ' b.BillUpdatedOn DESC ';
			end;
			End if;
            
            Set _SearchString = Concat(_SearchString, ' AND f.UserTypeId != 6 ');
            If (_Uid > 0) then
            begin
				Set _SearchString = Concat(_SearchString, ' AND f.FileOwnerId = ', _Uid, ' ');
            end;
            end if;
			
				Set @SelectQuery = CONCAT('Select * from (
				Select 
					Row_Number() over(Order by ', _sortBy, ') as `Index`,
					f.FilePath,
					f.FileName,
					f.FileExtension,
					s.Status,
                    coalesce(g.gststatus, 2) GstStatus,
					DATE_FORMAT(b.BillUpdatedOn, ''%d %M %Y'') GeneratedOn,
					DATE_FORMAT(b.PaidOn, ''%d %M %Y'') PaidOn,
					Convert(f.FileId, char) as FileUid,
					f.FileOwnerId,
					b.BillForMonth `Month`,
                    b.BillYear `Year`,
                    b.NoOfDaysAbsent Absents,
					b.TDS,
                    b.NoOfDays,
					b.PaidAmount SalaryAmount,
                    (Select TakeHomeByCandidate from employeemappedclients
						where EmployeeUid = e.EmployeeUid and ClientUid = c.ClientId) TakeHome,
					b.IGST,
					b.SGST,
					b.CGST,
					c.ClientName,
					c.ClientId,
					b.BillNo,
                    b.BillStatusId,
                    Concat(e.FirstName, '' '', e.LastName) Name,
                    Count(1) Over() as Total 
                    from employees e
					Inner Join filedetail f on f.FileOwnerId = e.EmployeeUid
                    inner join billdetail b on b.FileDetailId = f.FileId
					left join itemstatus s on s.ItemStatusId = f.ItemStatusId
					left Join clients c on c.ClientId = b.ClientId
                    left Join gstdetail g on g.billno = b.BillNo
				where ', _SearchString, '
			)T where `Index` between ', (_pageIndex - 1) * _pageSize + 1 ,' and ', (_pageIndex * _pageSize)) ;
            
			# Select @SelectQuery;
			prepare SelectQuery from @SelectQuery;
			execute SelectQuery;	
            
            Select 
				concat(e.FirstName, ' ', e.LastName) `Name`,
				e.Mobile,
                e.Email
			from employees e 
            Where EmployeeUid = _Uid;
            
            Select 
				EmployeeUId,
				concat(e.FirstName, ' ', e.LastName) `Name`,
				e.Mobile,
                e.Email
			from employees e;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_billingdetail_byempid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_billingdetail_byempid`(
	_EmployeeUid bigint
    )
Begin      
        Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, @OperationStatus, 'sp_billingdetail_byempid', 1, 0, @Result);
		end;
		Begin
            Select FileOwnerId, FilePath, FileName, FileExtension, PaidOn, concat (E.FirstName, '  ', E.LastName) as FileOwnerName from filedetail as FD
			inner join employees as E
			On FD.FileOwnerId = E.EmployeeUid
			where EmployeeUid = _EmployeeUid;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Billing_detail` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
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
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_candidatefiledetail_InsUpd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_candidatefiledetail_InsUpd`(
	_FileId long,
	_FilePath varchar(500), 
	_FileName varchar(100),
	_FileExtension varchar(10),
    _UserTypeId int
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_candidatefiledetail_InsUpd', 1, 0, @Result);
		end;
        Begin
			if not exists (select 1 from candidatefiledetail where FileId 	= _FileId) then
			Begin
				insert into candidatefiledetail values (
					default,
					_FilePath,
					_FileName,
					_FileExtension, 
					_UserTypeId,
					utc_timestamp(), 
					null
				);
			End;
			Else
			Begin
				update candidatefiledetail set
						FilePath 		= 	_FilePath,
						FileName		=	_FileName,
						FileExtension	=	_FileExtension, 
						UserTypeId		=	_UserTypeId,
						UpdatedOn		=	utc_timestamp()
				where	FileId 			= 	_FileId;
			End;
			End if;
		End;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Candidatelogin_Auth` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Candidatelogin_Auth`(

/*

    Call sp_Candidatelogin_Auth(-1, '9100544384', null, 3, '12345');

*/
	_UserId bigint,
	_MobileNo varchar(20),
	_EmailId varchar(50),
    _UserTypeId int,
    _Password varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_Candidatelogin_Auth', 1, 0, @Result);
			end;  
            
            Set @UserExistsFlag = 0;
			Begin				
				Set @pass = '';
				Select Password from candidatelogin 
				where Email = _EmailId Or Mobile = _MobileNo
				into @pass;
				
				If (@pass = _Password) then
				Begin
					Set @UserExistsFlag = 1;
				End;
				End if;
                
                If (@UserExistsFlag = 1)then
				Begin
					Set @AccessLevelId = 0;
                    Select AccessLevelId from candidatelogin 
                    Where Email = _EmailId Or Mobile = _MobileNo
                    into @AccessLevelId;
                    
					Select 
						UserId,
                        FirstName,
                        LastName,
                        Email,
                        Mobile,
                        AccessLevelId,
                        _UserTypeId UserTypeId
                    from candidatelogin 
                    Where Email = _EmailId Or Mobile = _MobileNo;
								 
					Select RM.Catagory, RM.Childs, RM.Link, RM.Icon, RM.Badge, 
					RM.BadgeType, RM.AccessCode, 
					AccessibilityId Permission from rolesandmenu RM
					left Join role_accessibility_mapping r on r.AccessCode = RM.AccessCode
					where r.AccessLevelId = @AccessLevelId 
					and r.AccessibilityId > 0;
						
                    Call sp_ColumnMapping_GetByPageName('all');
				End;
                End if;
			End;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_ClientsAndSender_Emails_By_Id` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_ClientsAndSender_Emails_By_Id`(
	_SenderId bigint,
	_ReceiverId bigint,
    _FileId bigint
/*	

	Call SP_ClientsAndSender_Emails_By_Id(1, 2);

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
			Call sp_LogException(@Message, '', 'SP_ClientsAndSender_Emails_By_Id', 1, 0, @Result);
		End;

		Select * from clients
        where ClientId = _SenderId
        
        Union
        
        Select * from clients
        where ClientId = _ReceiverId;
        
        select * from filedetail
        where FileId = _FileId;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_Clients_Get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Clients_Get`(
	_searchString varchar(250),
	_sortBy varchar(50),
	_pageIndex int,
	_pageSize int
/*	

	Call SP_Clients_Get('1', '', 1, 5);

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
			Call sp_LogException(@Message, '', 'SP_Clients_Get', 1, 0, @Result);
		End;

        Begin
			If(_sortBy is NULL OR _sortBy = '') then
				Set _sortBy = 'CreatedOn, UpdatedOn Desc';
			End if;
            Set @SelectQuery = CONCAT('Select * from (
				Select 
					Row_Number() over(Order by ', _sortBy, ') as `Index`,
					ClientId, 
					ClientName,
					PrimaryPhoneNo,
					Email,
					FirstAddress,
                    City,
                    OtherEmail_1,
                    OtherEmail_2,
                    OtherEmail_3,
                    OtherEmail_4,
					Count(1) Over() as Total from clients
				Where ', _SearchString, '
			)T where `Index` between ', (_pageIndex - 1) * _pageSize + 1 ,' and ', (_pageIndex * _pageSize));
            
		# Select @SelectQuery;
		prepare SelectQuery from @SelectQuery;
		execute SelectQuery;	
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_Client_ById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Client_ById`(
	_ClientId bigint,
	_IsActive bit,
    _UserTypeId int
/*	

	Call SP_Client_ById(1, 1, 2);

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
			Call sp_LogException(@Message, '', 'SP_Client_ById', 1, 0, @Result);
		End;

        Begin
			Select * from clients Where ClientId = _ClientId;
            Select FileId, FilePath, FileName, FileExtension, UserTypeId from userfiledetail 
            where FileOwnerId = _ClientId and FileName = 'profile' and UserTypeId = _UserTypeId;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_Client_IntUpd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Client_IntUpd`(
	_ClientId bigint,
	_ClientName varchar(100),
	_PrimaryPhoneNo varchar(20),
	_SecondaryPhoneNo varchar(20),
	_MobileNo varchar(20),
	_Email varchar(50),
    _OtherEmail_1 varchar(100),
    _OtherEmail_2 varchar(100),
    _OtherEmail_3 varchar(100),
    _OtherEmail_4 varchar(100),
	_Fax varchar(20),
	_GSTNO varchar(20),
	_PanNo varchar(20),
	_Pincode int,
	_Country varchar(50),
	_State varchar(50),
	_City varchar(50),
	_FirstAddress varchar(100),
	_SecondAddress varchar(100),
	_ThirdAddress varchar(100),
	_ForthAddress varchar(100),
	_IFSC varchar(15),
	_AccountNo varchar(25),
	_BankName varchar(100),
	_BranchName varchar(100),
	_AdminId bigint,
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
            
            RollBack;
            SET autocommit = 1;
            Call sp_LogException (@Message, @OperationStatus, 'SP_Client_IntUpd', 1, 0, @Result);
		end;
        
        SET autocommit = 0;
        Start Transaction;
		Begin 
			If not exists (Select 1 from clients Where ClientId = _ClientId) then
			Begin
				Set @@SESSION.information_schema_stats_expiry = 0;
            
				SELECT AUTO_INCREMENT into _ClientId
				FROM information_schema.tables
				WHERE table_name = 'Clients'
				AND table_schema = DATABASE();
                
                Insert into Clients Values(
					_ClientId,
					_ClientName,
					_MobileNo,
					_PrimaryPhoneNo,
					_SecondaryPhoneNo,						
					_Email,
					_Fax,
					_FirstAddress,
					_SecondAddress,
					_ThirdAddress,
					_ForthAddress,
					_Pincode,
					_City, 
					_State, 
					_Country,
					_GSTNO,
					_AccountNo,
					_BankName,
					_BranchName,
					_IFSC,
					_PanNo,
					_AdminId,
					null,
					now(),
					null,
					_OtherEmail_1,
					_OtherEmail_2,
					_OtherEmail_3,
					_OtherEmail_4
				);
                    
                    Set _ProcessingResult = 'Created successfully';
			End;
			Else
			Begin
				Update clients SET 
					ClientName			=	_ClientName,
					MobileNo			=	_MobileNo,
					PrimaryPhoneNo		=	_PrimaryPhoneNo,
					SecondaryPhoneNo	=	_SecondaryPhoneNo,						
					Email				=	_Email,
					Fax					=	_Fax,
					FirstAddress		=	_FirstAddress,
					SecondAddress		=	_SecondAddress,
					ThirdAddress		=	_ThirdAddress,
					ForthAddress		=	_ForthAddress,
					Pincode				=	_Pincode,
					City				=	_City, 
					State				=	_State, 
					Country				=	_Country,
					GSTNO				=	_GSTNO,
					AccountNo			=	_AccountNo,
					BankName			=	_BankName,
					BranchName			=	_BranchName,
					IFSC				=	_IFSC,
					PanNo				=	_PanNo,
                    OtherEmail_1		=	_OtherEmail_1,
					OtherEmail_2		=	_OtherEmail_2,
					OtherEmail_3		=	_OtherEmail_3,
					OtherEmail_4		=	_OtherEmail_4,
					UpdatedBy			= 	_AdminId,
					UpdatedOn			= 	now()
				Where 	ClientId 		= 	_ClientId;
                
                Set _ProcessingResult = 'Updated successfully';
			End;
			End if;
            
            COMMIT;
            
            Select * from clients
            where ClientId = _ClientId;
		End;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_ColumnMapping_GetByPageName` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ColumnMapping_GetByPageName`(
	
/*	

	Call sp_ColumnMapping_GetByPageName('admin/employees')

*/

	_PageName varchar(100)
)
Begin
	Declare exit handler for sqlexception
	Begin
	
		GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
									
		Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
		Call sp_LogException(@Message, @OperationStatus, 'sp_ColumnMapping_GetByPageName', 1, 0, @Result);
	end;

	If (_PageName = 'all') then
    Begin
		Select PageName, 
			ColumnName, 
			DisplayName, 
			Style, 
			ClassName,
			IsHidden
		from tablecolumnmapping;
	End;
    Else
    Begin
		Select PageName, 
			ColumnName, 
			DisplayName, 
			Style, 
			ClassName,
			IsHidden
		from tablecolumnmapping
        Where PageName = _PageName;
    End;
    End if;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_ColumnMapping_InsUpd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ColumnMapping_InsUpd`(
	
/*	

	Call sp_ColumnMapping_InsUpd(-1, 'admin/employees', 'FirstName', 'First Name', null, null, false)
    Call sp_ColumnMapping_InsUpd(-1, 'admin/employees', 'LastName', 'Last Name', null, null, false)
    Call sp_ColumnMapping_InsUpd(-1, 'admin/employees', 'Mobile', 'Mobile No.#', null, null, false)
    Call sp_ColumnMapping_InsUpd(-1, 'admin/employees', 'Email', 'Email Id', null, null, false)

*/

	_MappingId bigint,
	_PageName varchar(100),
    _ColumnName varchar(50),    
    _DisplayName varchar(50),
    _Style varchar(250),
    _ClassName varchar(250),
    _IsHidden bit
)
Begin
	Declare exit handler for sqlexception
	Begin
	
		GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
									
		Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
		Call sp_LogException(@Message, @OperationStatus, 'sp_ColumnMapping_InsUpd', 1, 0, @Result);
	end;

	if not exists(Select 1 from TableColumnMapping where MappingId = _MappingId)then
	Begin 
		Insert into TableColumnMapping values(default, 
			_PageName, 
            _ColumnName, 
            _DisplayName, 
            _Style, 
            _ClassName,
			_IsHidden
		);
	end;
	else
	begin
		Update TableColumnMapping Set 
			PageName = _PageName, 
            ColumnName = _ColumnName, 
            Style = _Style, 
			DisplayName = _DisplayName, 
            ClassName = _ClassName
		Where MappingId = _MappingId;
	End;
	End if;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_company_calendar_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_company_calendar_insupd`(
	   /*

	set @result = '';
    Call sp_company_calendar_insupd(0, 1, '2022-09-10 16:08:05', 'Casual leave for employees', 1, 1, 'Description', 4, 1, @result);
	select @result;

*/

	_CompanyCalendarId bigint,
    _CompanyId int,
    _EventDate datetime,
    _EventName varchar(250),
    _IsHoliday bit,
    _IsHalfDay bit,
    _DescriptionNote text,
    _ApplicableFor int,
    _AdminId bigint,
    out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            RollBack;
            Call sp_LogException (@Message, '', 'sp_company_calendar_insupd', 1, 0, @Result);
		end;

		Start Transaction;
        begin
			If not exists (Select 1 from company_calendar Where CompanyCalendarId = _CompanyCalendarId) then
			Begin
				Insert into company_calendar Values (
					default,
					_CompanyId ,
					_EventDate,
					_EventName,
					_IsHoliday,
					_IsHalfDay,
					_DescriptionNote,
					_ApplicableFor,
					_AdminId,
                    null,
                    utc_date(),
                    null
				);
				Set _ProcessingResult = "inserted";
			End;
			Else
			Begin
				Update company_calendar SET 
					CompanyId				=			_CompanyId,
					EventDate				=			_EventDate,
					EventName				=			_EventName,
					IsHoliday				=			_IsHoliday,
					IsHalfDay				=			_IsHalfDay,
					DescriptionNote			=			_DescriptionNote,
					ApplicableFor			=			_ApplicableFor,
					UpdatedBy				=			_AdminId,
                    UpdatedOn				=			utc_date()
				Where CompanyCalendarId = _CompanyCalendarId;
				
				Set _ProcessingResult = "updated";
			End;
			End if;
		end;
        Commit;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_company_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_company_get`(

/*

	Call sp_company_get();

*/
	
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_company_get', 1, 0, @Result);
		end;  
		
		Select * from company;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_company_getById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_company_getById`(
	_CompanyId int

/*

	Call sp_company_getById(1);

*/
	
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_company_getById', 1, 0, @Result);
		end;  
		
		Select c.* from company c
        left join bank_accounts b on c.CompanyId = b.CompanyId
		where c.CompanyId = _CompanyId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_company_intupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_company_intupd`(
	_CompanyId int,
	_OrganizationId int,
	_OrganizationName varchar(250),
	_CompanyName varchar(150),
	_CompanyDetail varchar(250),
	_SectorType int,
	_Country varchar(50),
	_State varchar(100),
	_City varchar(100),
	_FirstAddress varchar(100),
	_SecondAddress varchar(100),
	_ThirdAddress varchar(100),
	_ForthAddress varchar(100),
	_FullAddress varchar(150),
	_MobileNo varchar(20),
	_Email varchar(50),
	_FirstEmail varchar(100),
	_SecondEmail varchar(100),
	_ThirdEmail varchar(100),
	_ForthEmail varchar(100),
	_PrimaryPhoneNo varchar(20),
	_SecondaryPhoneNo varchar(20),
	_Fax varchar(50),
	_Pincode int,
	_FileId bigint,
	_LegalDocumentPath varchar(250),
	_LegalEntity varchar(50),
	_TypeOfBusiness varchar(150),
	_InCorporationDate datetime,
	_IsPrimaryCompany bit(1),
	_FixedComponentsId json,    
	_BankAccountId int,
	_BankName varchar(100),
	_BranchCode varchar(20),
	_Branch varchar(50),
	_IFSC varchar(20),
    _PANNo varchar(45),
    _GSTNo varchar(45),
    _TradeLicenseNo varchar(45),
    _AdminId long,    
	out _ProcessingResult varchar(50)
    
/*


	set @outcome = '';
    
	Call sp_company_intupd(0, 'BottomHlaf', 'BottomHalf Pvt. Ltd.', null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
    null, null, null, null, null, null, @outcome);    
    
    select @outcome;
    

*/
	
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_company_intupd', 1, 0, @Result);
			end;  
                        
            if not exists(select 1 from company where CompanyId = _CompanyId) then
            begin
				if exists(select 1 from company where lower(LegalNameOfCompany) = lower(_LegalNameOfCompany)) then
                begin
					select CompanyId from company 
                    where lower(LegalNameOfCompany) = lower(_LegalNameOfCompany) into _CompanyId;
                end;
                end if;
            end;
            end if;
            
            set @organizationName = '';
            select OrganizationName into @organizationName 
            from organization_detail
            where OrganizationId = _OrganizationId;
		
			if not exists(select 1 from company where CompanyId = _CompanyId) then
            begin
				Insert into company values(
					_CompanyId,
					_OrganizationId,
					@organizationName,
					_CompanyName,
					_CompanyDetail,
					_SectorType,
					_Country,
					_State,
					_City,
					_FirstAddress,
					_SecondAddress,
					_ThirdAddress,
					_ForthAddress,
					_FullAddress,
					_MobileNo,
					_Email,
					_FirstEmail,
					_SecondEmail,
					_ThirdEmail,
					_ForthEmail,
					_PrimaryPhoneNo,
					_SecondaryPhoneNo,
					_Fax,
					_Pincode,
					_FileId,
					_LegalDocumentPath,
					_LegalEntity,
					_TypeOfBusiness,
					_InCorporationDate,
                    _PANNo,
					_GSTNo,
					_TradeLicenseNo,
					_IsPrimaryCompany,
					_FixedComponentsId,
					_AdminId,
					null,
					utc_date(),
					null
				);
                
                Set _ProcessingResult = 'inserted';
			end;
            else 
            begin
				update company set
					OrganizationId					=			_OrganizationId,
					OrganizationName				=			@organizationName,
					CompanyName						=			_CompanyName,
					CompanyDetail					=			_CompanyDetail,
					SectorType						=			_SectorType,
					Country							=			_Country,
					State							=			_State,
					City							=			_City,
					FirstAddress					=			_FirstAddress,
					SecondAddress					=			_SecondAddress,
					ThirdAddress					=			_ThirdAddress,
					ForthAddress					=			_ForthAddress,
					FullAddress						=			_FullAddress,
					MobileNo						=			_MobileNo,
					Email							=			_Email,
					FirstEmail						=			_FirstEmail,
					SecondEmail						=			_SecondEmail,
					ThirdEmail						=			_ThirdEmail,
					ForthEmail						=			_ForthEmail,
					PrimaryPhoneNo					=			_PrimaryPhoneNo,
					SecondaryPhoneNo				=			_SecondaryPhoneNo,
					Fax								=			_Fax,
					Pincode							=			_Pincode,
					FileId							=			_FileId,
					LegalDocumentPath				=			_LegalDocumentPath,
					LegalEntity						=			_LegalEntity,
					TypeOfBusiness					=			_TypeOfBusiness,
					InCorporationDate				=			_InCorporationDate,
                    PANNo							=			_PANNo,
					GSTNo							=			_GSTNo,
					TradeLicenseNo					=			_TradeLicenseNo,
					IsPrimaryCompany				=			_IsPrimaryCompany,
					FixedComponentsId				=			_FixedComponentsId,
					UpdatedBy						=			_AdminId,
                    UpdatedOn						=			utc_date()
				where CompanyId 					= 			_CompanyId;
                
                Set _ProcessingResult = 'updated';
            end;
            end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_company_setting_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_company_setting_insupd`(	
    _SettingId bigint,
    _CompanyId int,
    _ProbationPeriodInDays int,
    _NoticePeriodInDays int,
    _AdminId bigint,
    out _ProcessingResult varchar(50)
)
Begin
	Declare Exit handler for sqlexception
	Begin
		Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
                                    
		Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
		Call sp_LogException (@Message, '', 'sp_company_setting_insupd', 1, 0, @Result);
	end;
    begin
		if not exists (select 1 from company_setting where CompanyId = _CompanyId) then
		begin
			insert into company_setting values (
				default,
                _CompanyId,
				_ProbationPeriodInDays,
				_NoticePeriodInDays,
                _AdminId,
                null,
                now(),
                null
            );
            set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update company_setting set
					ProbationPeriodInDays				=		_ProbationPeriodInDays,
					NoticePeriodInDays					=		_NoticePeriodInDays,
					UpdatedBy							=		_AdminId,
					UpdatedOn							= 		now()
			where 	CompanyId 							= 		_CompanyId;
			
			set _ProcessingResult = 'updated';
        end;
        end if;
	end;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_dashboard_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_dashboard_get`(

/*


	Call sp_dashboard_get(0, 0, null, null);


*/

    _userId bigint,
    _employeeUid bigint,
    _fromDate datetime,
    _toDate datetime
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_dashboard_get', 1, 0, @Result);
			end;
            
            Set @workingMonth = Month(utc_date());
            Set @workingYear  = year(utc_date());
            
            if (@workingMonth = 1) then
			Set @workingYear = @workingYear - 1;
            end if;
            
            Set @workingMonth = @workingMonth - 1;
            
            
            # Bill raised but pending payment from client side
            Select 
				e.FirstName,
                e.LastName,
                e.Email,
                e.Mobile,
                c.ClientName,
                c.ClientId,
                b.PaidAmount,
                b.BillDetailUid,
                b.BillForMonth,
                b.BillYear,
                b.PaidOn,
                b.BillNo,
                b.BillUpdatedOn,
                b.BillStatusId
            from billdetail b 
            Inner join employees e on b.EmployeeUid = e.EmployeeUid
            Left join clients c on b.ClientId = c.ClientId
            where b.BillStatusId = 2 and 
            b.BillForMonth = @workingMonth and 
            b.BillYear = @workingYear;
            
            # GST status
            select 
				g.gstId, 
                g.billno, 
                g.amount, 
                c.ClientName, 
                c.ClientId, 
                b.EmployeeUid,
                b.BillStatusId
			from gstdetail g
			inner join billdetail b on g.billno = b.BillNo
			inner join clients c on c.ClientId = b.ClientId
			where g.gststatus = 2 and 
			b.BillYear = year(utc_date());
                     
			# pending attendace for employees
            Select * from attendance a
            where a.ForMonth = Month(utc_date())
            and a.ForYear = Year(utc_date()) and
            a.DaysPending != 0;
            
            select b.PaidAmount, b.BillForMonth, count(b.PaidAmount) TotalBills from billdetail b
			where b.BillYear = Year(utc_date())
			Group by b.BillForMonth;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_DeActivatedEmployee_Get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_DeActivatedEmployee_Get`(
/*	

	Call SP_DeActivatedEmployee_Get();

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
			Call sp_LogException(@Message, '', 'SP_DeActivatedEmployee_Get', 1, 0, @Result);
		End;

        Begin
			#Select * from clients;            
            
            Select e.EmployeeUid,
            e.FirstName,
            e.LastName,
            e.Mobile,
            e.Email,
            ep.AadharNo,
            ep.PANNo,
            ep.AccountNumber,
            ep.BankName,
            ep.IFSCCode,
            ep.Domain,
            ep.Specification,
            ep.ExprienceInYear,
            false isActive,
            0 ActualPackage,
            0 FinalPackage,
            0 TakeHomeByCandidate,
			(
				Select 
					JSON_ARRAYAGG(ClientUid)
				from employeemappedclients 
				where EmployeeUid = e.EmployeeUid
			) as ClientJson
            from employee_archive e 
            Inner Join employeeprofessiondetail_archive ep on ep.EmployeeUid = e.EmployeeUid;
            
            #select * from employeemappedclients;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_deactivateOrganization_delandgetall` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_deactivateOrganization_delandgetall`(

/*

	Call sp_deactivateOrganization_delandgetall(89, null, null);

*/
	_ClientMappedId bigint,
    _UserId bigint
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_deactivateOrganization_delandgetall', 1, 0, @Result);
			end;  
            
            update employeemappedclients
            set IsActive = 0 
            where EmployeeMappedClientsUid = _ClientMappedId;
            
            Select * from employeemappedclients 
            where EmployeeUid = _UserId and IsActive = 1;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_deductions_detail_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_deductions_detail_insupd`(

/*

	Set @outcome = '';
	Call sp_deductions_detail_insupd(0, 'ESI (Employee State Insurance)', 1, 0, 0, 1, 1, @outcome);
    Select @outcome;

*/
	_DeductionId int,
    _DeductionDescription varchar(100),
    _IsPaidByEmployee bit,
    _IsPaidByEmployeer bit,
    _IsMandatory bit,
    _IsFixedAmount bit,
    _Admin bigint,
    out _ProcessingResult varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_deductions_detail_insupd', 1, 0, @Result);
			end;  
		
		if not exists(select 1 from deductions_detail where DeductionId = _DeductionId) then
        begin
			insert into deductions_detail
            values(
				default,
				_DeductionDescription,
                _IsPaidByEmployee,
				_IsPaidByEmployeer,
				_IsMandatory,
                _IsFixedAmount,
				utc_date(),
                null,
                _Admin,
                null
            );
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update deductions_detail set				
				DeductionDescription			=			_DeductionDescription,
				IsPaidByEmployee				=			_IsPaidByEmployee,
				IsPaidByEmployeer				=			_IsPaidByEmployeer,
				IsMandatory						=			_IsMandatory,
                IsFixedAmount					= 			_IsFixedAmount,
				UpdatedBy						=			_Admin,
                UpdatedOn						=			utc_date()
            where DeductionId = _DeductionId;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_document_filedetail_delete` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_document_filedetail_delete`(
	_FileIds varchar(20),
    out _ProcessingResult varchar(100)
/*	

	Set @OutParam = '';
	Call sp_document_filedetail_delete('1,2', @OutParam);
    Select @OutParam;

*/

)
Begin
    Begin
		Declare exit handler for sqlexception
		Begin
		
			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
				
			ROLLBACK;
            Set _ProcessingResult = "Error";
			Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
			Call sp_LogException(@Message, '', 'sp_document_filedetail_delete', 1, 0, @Result);
		End;

		Set _ProcessingResult = "";
		Start Transaction;
        Begin			
			Set @DeleteQuery = Concat('
				Delete from userfiledetail f where f.FileId in (', _FileIds ,')
            ');
            
			# Select @SelectQuery;
			prepare DeleteQuery from @DeleteQuery;
			execute DeleteQuery;
            
            Set _ProcessingResult = "Deleted successfully";
		End;
        
        COMMIT;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_document_filedetail_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_document_filedetail_get`(

/*

	Call sp_document_filedetail_get(1, 3);

*/

	_OwnerId bigint,
    _UserTypeId bigint
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_document_filedetail_get', 1, 0, @Result);
			end;
            
			Begin
				Select 
					u.FileId,
                    u.FileOwnerId,
                    u.FilePath,
                    u.ParentFolder,
                    u.FileName,
                    u.FileExtension,
                    u.UserTypeId,
                    u.CreatedBy,
                    u.UpdatedOn
				from userfiledetail u
                Where u.FileOwnerId = _OwnerId And u.UserTypeId = _UserTypeId;
			End;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_document_filedetail_getById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_document_filedetail_getById`(
	_FileIds varchar(20)
/*	

	Call sp_document_filedetail_getById('1,2,3,4');

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
			Call sp_LogException(@Message, '', 'sp_document_filedetail_getById', 1, 0, @Result);
		End;

        Begin
			Set @SelectQuery = Concat('
				Select 
					f.FileId FileId,
					f.FileOwnerId FileOwnerId,
					f.FileName FileName,
					f.FilePath FilePath,
					f.FileExtension FileExtension,
					f.ItemStatusId StatusId,
					f.UserTypeId UserTypeId,
					f.CreatedBy AdminId  
				from userfiledetail f where f.FileId in (', _FileIds ,')
            ');
            
			# Select @SelectQuery;
			prepare SelectQuery from @SelectQuery;
			execute SelectQuery;	
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_document_filedetail_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_document_filedetail_insupd`(

/*
	
    Call sp_document_filedetail_insupd('[{
		"FileId":-1,
        "FileOwnerId":1,
        "FileName":"testfile",
        "FilePath":"c drive",
        "ParentFolder":"",
        "FileExtension":"pdf",
        "StatusId":1,
        "UserTypeId":1,
        "AdminId":1
	}]');

*/
    
    _InsertFileJsonData json
)
Begin
	DECLARE i      INT DEFAULT  0;
	DECLARE count INT DEFAULT -1;
    DECLARE _FileId bigint default -1;
	DECLARE _FileOwnerId bigint default -1;
	DECLARE _FileName varchar(100);
	DECLARE _FilePath varchar(500);
	DECLARE _ParentFolder varchar(500);
	DECLARE _FileExtension varchar(100);
	DECLARE _StatusId bigint default -1;
	DECLARE _UserTypeId int default -1;
	DECLARE _AdminId bigint default -1;
    Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
										
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_document_filedetail_insupd', 1, 0, @Result);
		end;

		if (_InsertFileJsonData is not null) then
        begin
			Set _FileId = -1;
			Set count = count + JSON_LENGTH(_InsertFileJsonData, '$');
            
            while(i <= count) do
            begin
				Set _FileId = JSON_EXTRACT(_InsertFileJsonData, CONCAT( '$[', i, '].FileId'));
				Set _FileOwnerId = JSON_EXTRACT(_InsertFileJsonData, CONCAT( '$[', i, '].FileOwnerId'));
				Set _FileName = JSON_UNQUOTE(JSON_EXTRACT(_InsertFileJsonData, CONCAT( '$[', i, '].FileName')));
				Set _FilePath = JSON_UNQUOTE(JSON_EXTRACT(_InsertFileJsonData, CONCAT( '$[', i, '].FilePath')));
				Set _ParentFolder = JSON_UNQUOTE(JSON_EXTRACT(_InsertFileJsonData, CONCAT( '$[', i, '].ParentFolder')));
				Set _FileExtension = JSON_UNQUOTE(JSON_EXTRACT(_InsertFileJsonData, CONCAT( '$[', i, '].FileExtension')));
				Set _StatusId = JSON_EXTRACT(_InsertFileJsonData, CONCAT( '$[', i, '].StatusId'));
				Set _UserTypeId = JSON_EXTRACT(_InsertFileJsonData, CONCAT( '$[', i, '].UserTypeId'));
				Set _AdminId = JSON_EXTRACT(_InsertFileJsonData, CONCAT( '$[', i, '].AdminId'));
                
				If not exists (Select 1 from userfiledetail where FileId = _FileId) then
                begin
					Insert into userfiledetail values(
						default, 
						_FileOwnerId,
						_FilePath,
                        _ParentFolder,
						_FileName,
						_FileExtension,
						_StatusId,
						_UserTypeId,
						_AdminId,
						null,
						NOW(),
						null
					);
                end;
                else 
                begin
					Update userfiledetail Set
						FilePath			=		_FilePath,
                        ParentFolder		=		_ParentFolder,
						FileName			=		_FileName,
						FileExtension		=		_FileExtension,
						ItemStatusId		=		_StatusId,
						UserTypeId			=		_UserTypeId,
						UpdatedBy			=		_AdminId,
						UpdatedOn			=		NOW()
					Where FileId = _FileId;
                end;
                end if;
				set i = i + 1;
            end;
            end while;
        end; 
        end if;
        
        Select * from userfiledetail where FileOwnerId = _FileOwnerId;
	End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_email_setting_detail_by_companyId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_email_setting_detail_by_companyId`(
	   /*

    Call sp_email_setting_detail_by_companyId(1);

*/
	_CompanyId int
    
 
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);

            Call sp_LogException (@Message, '', 'sp_email_setting_detail_by_companyId', 1, 0, @Result);
		end;
        
        select * from email_setting_detail
        where CompanyId = _CompanyId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_email_setting_detail_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_email_setting_detail_get`(
	   /*

    Call sp_email_setting_detail_get(3);

*/
	_EmailSettingDetailId int
    
 
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);

            Call sp_LogException (@Message, '', 'sp_email_setting_detail_get', 1, 0, @Result);
		end;
        if(_EmailSettingDetailId = 0) then
        begin
			select * from email_setting_detail
			where IsPrimary = true;
        end;
        else
        begin
			select * from email_setting_detail
			where EmailSettingDetailId = _EmailSettingDetailId;
        end;
        end if;
        
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_email_setting_detail_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_email_setting_detail_insupd`(
	   /*

    Call sp_email_setting_detail_insupd(1);

*/
	_EmailSettingDetailId int,
	_CompanyId int,
	_EmailAddress varchar(200),
	_EmailHost varchar(100),
	_PortNo int,
	_EnableSsl bit,
	_DeliveryMethod varchar(50),
	_UserDefaultCredentials bit,
	_Credentials varchar(100),
	_EmailName varchar(100),
	_POP3EmailHost varchar(100),
	_POP3PortNo int,
	_POP3EnableSsl bit,
    _IsPrimary bit,
    _UpdatedBy bigint,
	out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);

            Call sp_LogException (@Message, '', 'sp_email_setting_detail_insupd', 1, 0, @Result);
		end;
        
        if not exists (select * from email_setting_detail where EmailSettingDetailId = _EmailSettingDetailId) then
        begin
			Insert into email_setting_detail values(
				default,
				_CompanyId,
				_EmailAddress,
                _EmailName,
				_EmailHost,
                _POP3EmailHost,
				_PortNo,
                _POP3PortNo,
				_EnableSsl,
                _POP3EnableSsl,
				_DeliveryMethod,
				_UserDefaultCredentials,
				_Credentials,
				_IsPrimary,
                null,
                utc_date()
			);
         
             Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update email_setting_detail set 
				CompanyId					=			_CompanyId,
				EmailAddress				=			_EmailAddress,
                EmailName					=			_EmailName,
				EmailHost					=			_EmailHost,
                POP3EmailHost				=			_POP3EmailHost,
				PortNo						=			_PortNo,
                POP3PortNo					=			_POP3PortNo,
				EnableSsl					=			_EnableSsl,
                POP3EnableSsl				=			_POP3EnableSsl,
				DeliveryMethod				=			_DeliveryMethod,
				UserDefaultCredentials		=			_UserDefaultCredentials,
				Credentials					=			_Credentials,
				IsPrimary					=			_IsPrimary,
                UpdatedBy					=			_UpdatedBy,
                UpdatedOn					=			utc_date()
			where EmailSettingDetailId 		= 			_EmailSettingDetailId;
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_email_template_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_email_template_get`(

/*

	Call sp_email_template_get(1);

*/
	_EmailTemplateId int    
)
Begin
	Declare Exit handler for sqlexception
	Begin
		Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
		@errorno = MYSQL_ERRNO,
		@errortext = MESSAGE_TEXT;
		Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
		Call sp_LogException (@Message, @OperationStatus, 'sp_email_template_get', 1, 0, @Result);
	end;  

	select * from email_templates
    where EmailTemplateId = _EmailTemplateId;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_EmployeeBillDetail_ById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_EmployeeBillDetail_ById`(
	_EmployeeId bigint,
    _ClientId bigint,
    _FileId bigint,
    _ForYear int,
    _ForMonth int,
	_UserTypeId int,
    _AdminId bigint
/*	

	Call sp_EmployeeBillDetail_ById(2, 1, 1, 2022, 9, 2, 1);
    
    
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
        
        Call SP_Employee_GetAll(concat(' 1=1 and emp.EmployeeUid = ', _EmployeeId), null, 1, 10);
        
		Select 
			a.*
		from employee_timesheet a   
		Where a.EmployeeId = _EmployeeId
		And _ForYear = a.ForYear and _ForMonth = a.ForMonth
		And a.UserTypeId = _UserTypeId
        And a.ClientId = _ClientId;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Employeelogin_Auth` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employeelogin_Auth`(

/*

	Call sp_Employeelogin_Auth(0, NULL, 'istiyaq.mi9@gmail.com', 1, 1000);
    
    Call sp_Employeelogin_Auth(0, null, 'mgb@gmail.com', 1, 1000);

*/
	_UserId bigint,
	_MobileNo varchar(20),
	_EmailId varchar(50),
    _UserTypeId int,
    _PageSize int
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_Employeelogin_Auth', 1, 0, @Result);
			end;  
		
		Set @RoutePrefix = 'admin';
		Set @AccessLevelId = 0;
        Set @CompanyId = 0;
		Select AccessLevelId, CompanyId from employeelogin
		Where (Email = _EmailId Or Mobile = _MobileNo)
        And UserTypeId = _UserTypeId
		into @AccessLevelId, @CompanyId;
	   
		if(_UserTypeId = 2) then
		begin
			Set @RoutePrefix = 'user';
		end;
		end if;

		Select
			e.EmployeeUid UserId,
			e.FirstName,
			e.LastName,
			'NA' Address,
			e.Email,
			e.Mobile,
            e.ReportingManagerId,
            e.DesignationId,
			@AccessLevelId RoleId,
			_UserTypeId UserTypeId,
            l.OrganizationId,
            l.CompanyId
		from employees e
        Inner join employeelogin l on l.EmployeeId = e.EmployeeUid
		Where e.Email = _EmailId Or e.Mobile = _MobileNo;
        
        select * from employees e
        Where e.Email = _EmailId Or e.Mobile = _MobileNo;
	   
		if(@AccessLevelId = 1) then
		begin
			Select RM.Catagory, RM.Childs, concat(@RoutePrefix, '/', RM.Link) Link, RM.Icon, RM.Badge,
			RM.BadgeType, RM.AccessCode,  1 as Permission from rolesandmenu RM
			where Catagory <> 'Home' or Childs <> 'Home';
		end;
		else
		begin
			Select RM.Catagory, RM.Childs, concat(@RoutePrefix, '/', RM.Link) Link, RM.Icon, RM.Badge,
			RM.BadgeType, RM.AccessCode,
			AccessibilityId Permission from rolesandmenu RM
			left Join role_accessibility_mapping r on r.AccessCode = RM.AccessCode
			where r.AccessLevelId = @AccessLevelId
			and r.AccessibilityId > 0;
		end;
		end if;
        
        if (_UserTypeId = 1) then
        begin
			select * from (	
				Select 
					EmployeeUid, 
					Concat(FirstName, ' ', LastName) Name
				from employees
                where CompanyId = @CompanyId And IsActive = true
				Order by  UpdatedOn Desc, CreatedOn Desc
			)T limit _PageSize offset 0;
		end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_Employees_AddUpdateRemoteClient` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employees_AddUpdateRemoteClient`(
	_employeeMappedClientsUid bigint,
	_employeeUid bigint,
	_clientUid bigint,
    _finalPackage float(9, 2),
    _actualPackage float(9, 2),
    _takeHome float(9, 2),
    _isPermanent bit,
    _BillingHours int,
	_DaysPerWeek int,
    _DateOfLeaving datetime
/*	

	Call SP_Employees_AddUpdateRemoteClient(25, 4, 4, 15000.00, 15000.00, 15000.00, 0, 0, 0, null);

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
			Call sp_LogException(@Message, '', 'SP_Employees_AddUpdateRemoteClient', 1, 0, @Result);
		End;

		SET autocommit=1;
		Set @ClientName = '';
        Select ClientName from clients where ClientId = _clientUid into @ClientName;
        Begin
			If exists(Select 1 from employeemappedclients  where EmployeeMappedClientsUid = _employeeMappedClientsUid) then 
            Begin
				if (_DateOfLeaving is null) then
                begin
					Update employeemappedclients Set 					
						ClientUid 				=		_clientUid, 
						ClientName				=		 @ClientName, 
						FinalPackage			=		_finalPackage, 
						ActualPackage			=		_actualPackage, 
						TakeHomeByCandidate		=		_takeHome, 
						IsPermanent				=		_isPermanent,
						BillingHours			=		_BillingHours,
						DaysPerWeek				=		_DaysPerWeek						
					Where EmployeeMappedClientsUid = _employeeMappedClientsUid;
				end;
                else
                begin
					Update employeemappedclients Set 					
						ClientUid 				=		_clientUid, 
						ClientName				=		 @ClientName, 
						FinalPackage			=		_finalPackage, 
						ActualPackage			=		_actualPackage, 
						TakeHomeByCandidate		=		_takeHome, 
						IsPermanent				=		_isPermanent,
						BillingHours			=		_BillingHours,
						DaysPerWeek				=		_DaysPerWeek,
                        DateOfLeaving			=		_DateOfLeaving
					Where EmployeeMappedClientsUid = _employeeMappedClientsUid;
                end;
                end if;
            end;
            else
            Begin
				Insert into employeemappedclients values(
					default, 
                    _employeeUid, 
                    _clientUid, 
                    @ClientName, 
                    _finalPackage, 
                    _actualPackage, 
                    _takeHome, 
                    _isPermanent,
                    1,
                    _BillingHours,
                    _DaysPerWeek,
                    utc_timestamp(),
                    null
				);
            end;
            end if;
		End;
        
		Select * from employeemappedclients 
		where EmployeeUid = _employeeUid and IsActive = 1;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_Employees_ById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employees_ById`(
	_EmployeeId int,
    _IsActive int
/*	

	Call SP_Employees_ById(22, 1);

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
                    l.OrganizationId,
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
					e.EmployeeId, 
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
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_Employees_Get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employees_Get`(
	_searchString varchar(250),
	_sortBy varchar(50),
	_pageIndex int,
	_pageSize int
/*	

	Call SP_Employees_Get('1=1', '', 1, 10);
    
    Call SP_Employees_Get('1=1 And IsActive = 0', '', 1, 10);

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
			Call sp_LogException(@Message, '', 'SP_Employees_Get', 1, 0, @Result);
		End;

        Begin
			If(_sortBy is NULL OR _sortBy = '') then
				Set _sortBy = 'UpdatedOn, CreatedOn Desc';
			End if;
            Set @SelectQuery = CONCAT('Select * from (
				Select 
					Row_Number() over(Order by ', _sortBy, ') as `Index`,
					EmployeeUid, 
					FirstName,
                    0 ClientUid,
					LastName,
					Mobile,
					Email,
                    IsActive,
                    ReportingManagerId,
					Count(1) Over() as Total from employees
				Where ', _SearchString, '
			)T where `Index` between ', ((_pageIndex - 1 ) * _pageSize + 1), ' and ', (_pageIndex * _pageSize));
            
		# Select @SelectQuery;
		prepare SelectQuery from @SelectQuery;
		execute SelectQuery;	
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Employees_InsUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employees_InsUpdate`(
	_EmployeeUid bigint,
    _OrganizationId int,
	_FirstName varchar(50),
	_LastName varchar(50),
	_Mobile varchar(20),
	_Email varchar(100),
    _LeavePlanId int,
    _PayrollGroupId int,
    _SalaryGroupId int,
    _CompanyId int,
    _NoticePeriodId int,
    _SecondaryMobile varchar(20),
    _FatherName varchar(50),
    _MotherName varchar(50),
    _SpouseName varchar(50),
    _Gender bit(1),
    _State varchar(75),
    _City varchar(75),
    _Pincode int,
    _Address varchar(100),
    _PANNo varchar(20),
    _AadharNo varchar(20),
    _AccountNumber varchar(50),
    _BankName varchar(100),
    _BranchName varchar(100),
    _IFSCCode varchar(20),
    _Domain varchar(250),
    _Specification varchar(250),
    _ExprienceInYear float(5,2),
    _LastCompanyName varchar(100),
    _IsPermanent bit(1),
	_ActualPackage float(10,2),
    _FinalPackage float(10,2),
    _TakeHomeByCandidate float(10,2),
    _ReportingManagerId bigint,
    _DesignationId int,
    _ProfessionalDetail_Json json,
    _Password varchar(150),
    _AccessLevelId int,
    _UserTypeId int,
	_CTC decimal,
	_GrossIncome decimal,
	_NetSalary decimal,
	_CompleteSalaryDetail Json,
	_TaxDetail Json,
	_AdminId bigint,
    _DOB datetime,
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
            
            RollBack;
            SET autocommit = 1;
            Set sql_safe_updates = 1;
            Call sp_LogException (@Message, '', 'sp_Employees_InsUpdate', 1, 0, @Result);
		end;
        
        Set @msg = 'starting';
        
        SET autocommit = 0;
        Set @EmpId = 0;
        Start Transaction;
		Begin 
        
			if(_UserTypeId = 0) then
				Set _UserTypeId = 2;
			end if;
        
			If not exists (Select 1 from employees Where EmployeeUid = _EmployeeUid) then
			Begin
				Set @EmpId = 0;
				Select EmployeeUid from employees order by EmployeeUid desc limit 1 into @EmpId ;
				Set @EmpId = @EmpId+1;
			 
				Set _ProcessingResult = @EmpId; 
				/*
				SELECT AUTO_INCREMENT
				FROM information_schema.tables
				WHERE table_name = 'employees'
				AND table_schema = DATABASE() INTO @EmpId;
				*/
			
				Insert into employees (EmployeeUid, FirstName, LastName, Mobile, 
					Email, LeavePlanId, PayrollGroupId, IsActive, CreatedBy, UpdatedBy,  CreatedOn, UpdatedOn, 
					ReportingManagerId, DesignationId, UserTypeId, SalaryGroupId, CompanyId, NoticePeriodId
				) Values (
					@EmpId,
					_FirstName,
					_LastName,
					_Mobile,
					_Email,
					_LeavePlanId,
					_PayrollGroupId,
					1,
					_AdminId,
					null, 
					now(),
					null,
					_ReportingManagerId,
					_DesignationId,
					_UserTypeId,
					_SalaryGroupId,
					_CompanyId,
					_NoticePeriodId
				);
			
				Insert into employeepersonaldetail (EmployeePersonalDetailId, EmployeeUid, Mobile, SecondaryMobile, Email, Gender, FatherName, SpouseName,
					MotherName, Address,  State,  City, Pincode, IsPermanent, ActualPackage, FinalPackage, TakeHomeByCandidate, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, DOB
				) Values (
					default,
					@EmpId,
					_Mobile,
					_SecondaryMobile,
					_Email,
					_Gender,
					_FatherName,
					_SpouseName,
					_MotherName,
					_Address,
					_State,
					_City,
					_Pincode,
					_IsPermanent,
					_ActualPackage,
					_FinalPackage,
					_TakeHomeByCandidate,
					_AdminId,
					null,
					now(),
					null,
                    _DOB
				);
				
				Insert into employeeprofessiondetail Values (
					default,
					@EmpId, 
					_FirstName,
					_LastName,
					_Mobile,
					_SecondaryMobile,
					_Email, 
					_PANNo,
					_AadharNo,
					_AccountNumber,
					_BankName,
					_BranchName,
					_IFSCCode,
					_Domain,
					_Specification,
					_ExprienceInYear,
					_LastCompanyName,
					_AdminId,
					null,
					now(),
					null,
					_ProfessionalDetail_Json
				);
			
			set @msg = 'employeelogin';
				Insert into employeelogin
				Values(
					default, 
					@EmpId, 
					2, 
					_AccessLevelId, 
					_Password, 
					_Email, 
					_Mobile,
                    _OrganizationId,
					_CompanyId,
					_AdminId, 
					null, 
					utc_date(), 
					null
				);			
			End;
			Else
			Begin
				Set _ProcessingResult = '0';
				Set @EmpId = _EmployeeUid;
                set sql_safe_updates = 0;
                Update employees SET 
						FirstName				=		_FirstName,
						LastName				=		_LastName,
						Mobile					=		_Mobile,
						Email					=		_Email,
						UpdatedBy				=		_AdminId, 
						UpdatedOn				=		now(),
                        ReportingManagerId		=		_ReportingManagerId,
                        DesignationId			=		_DesignationId,
                        UserTypeId				=		_UserTypeId,
                        LeavePlanId				=		_LeavePlanId,
						PayrollGroupId			=		_PayrollGroupId,
                        SalaryGroupId			=		_SalaryGroupId,
						CompanyId				=		_CompanyId,
						NoticePeriodId			=		_NoticePeriodId
					Where 	EmployeeUid 	= _EmployeeUid;
                
					Update employeepersonaldetail Set
						Mobile						=	_Mobile,
						SecondaryMobile				=	_SecondaryMobile,
						Email						=	_Email,
						Gender						=	_Gender,
						FatherName					=	_FatherName,
						SpouseName					=	_SpouseName,
						MotherName					=	_MotherName,
						Address						=	_Address,
                        DOB							=	_DOB,
						State						=	_State, 
						City						=	_City,
						Pincode						=	_Pincode,
						IsPermanent					=	_IsPermanent,
						ActualPackage				=	_ActualPackage,
						FinalPackage				=	_FinalPackage,
						TakeHomeByCandidate			=	_TakeHomeByCandidate,
						UpdatedBy					=	_AdminId,
						UpdatedOn					=	now()
					Where	EmployeeUid					=	_EmployeeUid;
                
					Update	employeeprofessiondetail Set
							FirstName		=	_FirstName,
							LastName		=	_LastName,
							Mobile			=	_Mobile,
							SecondaryMobile	=	_SecondaryMobile,
							Email			=	_Email, 
							PANNo			=	_PANNo,
							AadharNo		=	_AadharNo,
							AccountNumber	=	_AccountNumber,
							BankName		=	_BankName,
							BranchName		=	_BranchName, 
							IFSCCode		=	_IFSCCode,
							Domain			=	_Domain,
                            Specification	=	_Specification,
							ExprienceInYear	=	_ExprienceInYear,
							LastCompanyName	=	_LastCompanyName,
							UpdatedBy		=	_AdminId,
							UpdatedOn		=	now(),
                            ProfessionalDetail_Json = _ProfessionalDetail_Json
					 Where	EmployeeUid		=	_EmployeeUid;

					if(_UserTypeId = 1) then
						Set _AccessLevelId = 1;
					end if;
                    
                    Set sql_safe_updates = 0;
					Update employeelogin
						Set AccessLevelId = _AccessLevelId,
						Email 				= 			_Email,
                        UserTypeId 			=			_UserTypeId,
						Mobile 				= 			_Mobile,
                        OrganizationId		= 			_OrganizationId,
                        CompanyId			= 			_CompanyId,
						UpdatedBy 			= 			_AdminId,
						UpdatedOn   		= 			utc_date()
					Where EmployeeId 		= 			_EmployeeUid;
				End;
				End if;
			
				set @EmpDecId = 0;
				Set @result = '';
                if not exists(Select 1 from employee_declaration where EmployeeId = @EmpId) then
                begin
					Insert into employee_declaration values(
						default,
						@EmpId,
						'',
						'[]',
						'{}',
						0,
						0
					);  
                    
					select EmployeeDeclarationId into @EmpDecId from employee_declaration  where EmployeeId =  @EmpId;
                end;
                else
                begin
					select EmployeeDeclarationId into @EmpDecId from employee_declaration  where EmployeeId =  @EmpId;
                end;
                end if;
				
				Call sp_employee_declaration_ins_json(@EmpDecId);

                Set @groupId = 0;
                select SalaryGroupId into @groupId from salary_group 
                where _CTC >= MinAmount 
				and _CTC < MaxAmount;
                
                Call sp_employee_salary_detail_InsUpd(@EmpId, _CTC, _GrossIncome, _NetSalary, _CompleteSalaryDetail, @groupId, _TaxDetail, _ProcessingResult);
				Set _ProcessingResult =  @EmpId;
            COMMIT;
            Set sql_safe_updates = 1;
		End;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Employee_Activate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employee_Activate`(

/*

	Call sp_Employee_Activate(11);

*/
	_EmployeeId bigint,
	_FirstName varchar(50),
	_LastName varchar(50),
	_Mobile varchar(20),
	_Email varchar(100),
	_IsActive bit,
	_ReportingManagerId bigint,
	_DesignationId int,
	_UserTypeId int,
	_LeavePlanId int,
	_PayrollGroupId int,
	_SalaryGroupId int,
	_CompanyId int,
	_NoticePeriodId int,
	_SecondaryMobile varchar(20),
	_PANNo varchar(20),
	_AadharNo varchar(20),
	_AccountNumber varchar(50),
	_BankName varchar(100),
	_BranchName varchar(100),
	_IFSCCode varchar(20),
	_Domain varchar(250),
	_Specification varchar(250),
	_ExprienceInYear float,
	_LastCompanyName varchar(100),
	_ProfessionalDetail_Json json,
	_Gender bit,
	_FatherName varchar(50),
	_SpouseName varchar(50),
	_MotherName varchar(50),
	_Address varchar(100),
	_State varchar(75),
	_City varchar(75),
	_Pincode int,
	_IsPermanent bit,
	_ActualPackage float,
	_FinalPackage float,
	_TakeHomeByCandidate float,
	_AccessLevelId bigint,
	_Password varchar(150),
	_EmployeeDeclarationId bigint,
	_DocumentPath varchar(250),
	_DeclarationDetail json,
	_HousingProperty json,
	_TotalDeclaredAmount decimal,
	_TotalApprovedAmount decimal,
	_LeaveRequestId bigint,
	_LeaveDetail json,
	_Year int,
	_EmployeeNoticePeriodId bigint,
	_ApprovedOn datetime,
	_ApplicableFrom datetime,
	_ApproverManagerId int,
	_ManagerDescription varchar(500),
	_AttachmentPath varchar(200),
	_EmailTitle varchar(100),
	_OtherApproverManagerIds json,
	_ITClearanceStatus int,
	_ReportingManagerClearanceStatus int,
	_CanteenClearanceStatus int,
	_ClientClearanceStatus int,
	_HRClearanceStatus int,
	_OfficialLastWorkingDay datetime,
	_PeriodDuration int,
	_EarlyLeaveStatus int,
	_Reason varchar(500), 
	_CTC decimal,
	_GrossIncome decimal,
	_NetSalary decimal,
	_CompleteSalaryDetail json,
	_GroupId int,
	_TaxDetail json,
	_TimesheetId bigint,
	_ClientId bigint,
	_TimesheetMonthJson Json,
	_TotalDays int,
	_DaysAbsent int,
	_ExpectedBurnedMinutes int,
	_ActualBurnedMinutes int,
	_TotalWeekDays int,
	_TotalWorkingDays int,
	_TotalHolidays int,
	_MonthTimesheetApprovalState int,
	_ForYear int,
	_ForMonth int,
	_EmployeeMappedClientUid bigint,
	_ClientName varchar(250),
	_BillingHours int,
	_DaysPerWeek int,
	_DateOfJoining datetime,
	_DateOfLeaving datetime,
	_AdminId long,
	out _ProcessingResult varchar(50)
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
            
            RollBack;
            Set autocommit = 1;
            Set sql_safe_updates = 1;
            
			Call sp_LogException (@Message, @OperationStatus, 'sp_Employee_Activate', 1, 0, @Result);
		end;  
		set autocommit = 0;
        Set @schemaName = 'onlinedatabuilder';
        Set @employeeId = 0;
		start transaction;
        begin		            select * from employees;
			if not exists (Select 1 from employees where EmployeeUid = _EmployeeId) then
            begin				
				Insert into employees values (
					_EmployeeId,
					_FirstName,
					_LastName,
					_Mobile,
					_Email,
					_IsActive,
                    _AdminId,
                    null,
                    null,
                    now(),
					_ReportingManagerId,
					_DesignationId,
					_UserTypeId,
					_LeavePlanId,
					_PayrollGroupId,
					_SalaryGroupId,
					_CompanyId,
					_NoticePeriodId
                );
            end;
            end if;


			if not exists (Select 1 from employeeprofessiondetail where EmployeeUid = _EmployeeId) then
            begin
				
				Insert into employeeprofessiondetail values (
					default,
					_EmployeeId,
					_FirstName,
					_LastName,
					_Mobile,
                    _SecondaryMobile,
					_Email,
                    _PANNo,
					_AadharNo,
					_AccountNumber,
					_BankName,
					_BranchName,
					_IFSCCode,
					_Domain,
					_Specification,
					_ExprienceInYear,
					_LastCompanyName,
                    _AdminId,
                    null,
                    null,
                    now(),
					_ProfessionalDetail_Json
				 );
			end;
            end if;

			if not exists (Select 1 from employeepersonaldetail where EmployeeUid = _EmployeeId) then
            begin
				Insert into employeepersonaldetail values (
					default,
					_EmployeeId,
					_Mobile,
                    _SecondaryMobile,
					_Email,
                    _Gender,
					_FatherName,
					_SpouseName,
					_MotherName,
					_Address,
					_State,
					_City,
					_Pincode,
					_IsPermanent,
					_ActualPackage,
					_FinalPackage,
					_TakeHomeByCandidate,
                    _AdminId,
                    null,
                    null,
                    now()
				  );
			end;
            end if;
            
			if not exists (Select 1 from employeelogin where EmployeeId = _EmployeeId) then
            begin            
				Insert into employeelogin values (
                default,
                _EmployeeId, 
                _UserTypeId, 
                _AccessLevelId, 
                _Password, 
                _Email, 
                _Mobile,
                _CompanyId,
                _AdminId, 
                null, 
                null, 
                now()
                );
			end;
            end if;
            
            if not exists (Select 1 from employee_declaration where EmployeeId = _EmployeeId) then
            begin            
				Insert into employee_declaration values (
					default,
					_EmployeeId,
					_DocumentPath,
					_DeclarationDetail,
                    _HousingProperty,
                    _TotalDeclaredAmount,
					_TotalApprovedAmount
                );
			end;
            end if;
            
            if not exists (Select 1 from employee_leave_request where EmployeeId = _EmployeeId) then
            begin            
				Insert into employee_leave_request values (
					default,
					_EmployeeId,
					_LeaveDetail,
					_Year
                );
			end;
            end if;
            
            if not exists (Select 1 from employee_notice_period where EmployeeId = _EmployeeId) then
            begin            
				Insert into employee_notice_period values (
					default,
					_EmployeeId,
					_ApprovedOn,
					_ApplicableFrom,
					_ApproverManagerId,
					_ManagerDescription,
					_AttachmentPath,
					_EmailTitle,
					_OtherApproverManagerIds,
					_ITClearanceStatus,
					_ReportingManagerClearanceStatus,
					_CanteenClearanceStatus,
					_ClientClearanceStatus,
					_HRClearanceStatus,
					_OfficialLastWorkingDay,
					_PeriodDuration,
					_EarlyLeaveStatus,
					_Reason,
					_AdminId,
					null,
					null,
					now()
                );
			end;
            end if;
            
            if not exists (Select 1 from employee_salary_detail where EmployeeId = _EmployeeId) then
            begin            
				Insert into employee_salary_detail values (
					_EmployeeId,
					_CTC,
					_GrossIncome,
					_NetSalary,
					_CompleteSalaryDetail,
					_GroupId,
					_TaxDetail
                );
			end;
            end if;
            
            if not exists (Select 1 from employee_timesheet where EmployeeId = _EmployeeId) then
            begin            
				Insert into employee_timesheet values (
						default,
						_EmployeeId,
						_ClientId,
						_UserTypeId,
						_TimesheetMonthJson,
						_TotalDays,
						_DaysAbsent,
						_ExpectedBurnedMinutes,
						_ActualBurnedMinutes,
						_TotalWeekDays,
						_TotalWorkingDays,
						_TotalHolidays,
						_MonthTimesheetApprovalState,
						_ForYear,
						_ForMonth,
                        null,
                        now(),
                        null,
                        null
                );
			end;
            end if;
            
            if not exists (Select 1 from employeemappedclients where EmployeeUid = _EmployeeId) then
            begin            
				Insert into employeemappedclients values (
					_EmployeeMappedClientUid,
					_EmployeeId,
					_ClientId,
					_ClientName,
					_FinalPackage,
					_ActualPackage,
					_TakeHomeByCandidate,
					_IsPermanent,
					_IsActive,
					_BillingHours,
					_DaysPerWeek,
					_DateOfJoining,
					_DateOfLeaving
                );
			end;
            end if;

			set sql_safe_updates = 0;
			delete from employee_archive where EmployeeId = _EmployeeId;
        end;        
        commit;
        set _ProcessingResult = 'updated';
        Set sql_safe_updates = 1;
		 Set autocommit = 1;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_and_all_clients_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_and_all_clients_get`(
	_searchString varchar(250),
	_sortBy varchar(50),
	_pageIndex int,
	_pageSize int
/*	

	Call sp_employee_and_all_clients_get('1=1', '', 1, 10);
    
    Call SP_Employees_Get('1=1 And IsActive = 0', '', 1, 10);

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
			Call sp_LogException(@Message, '', 'SP_Employees_Get', 1, 0, @Result);
		End;

        Begin
			If(_sortBy is NULL OR _sortBy = '') then
				Set _sortBy = 'UpdatedOn, CreatedOn Desc';
			End if;
            Set @SelectQuery = CONCAT('Select * from (
				Select 
					Row_Number() over(Order by ', _sortBy, ') as `Index`,
					EmployeeUid, 
					FirstName,
                    0 ClientUid,
					LastName,
					Mobile,
					Email,
                    IsActive,
                    ReportingManagerId,
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
					Count(1) Over() as Total from employees emp
				Where ', _SearchString, '
			)T where `Index` between ', ((_pageIndex - 1 ) * _pageSize + 1), ' and ', (_pageIndex * _pageSize));
            
			# Select @SelectQuery;
			prepare SelectQuery from @SelectQuery;
			execute SelectQuery;	
            
            select * from clients;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_autocomplete_data` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_autocomplete_data`(
/*	

	Call sp_employee_autocomplete_data('', 2, 2, 1);

*/

	_SearchString varchar(500),
    _PageIndex int,
    _PageSize int,
    _CompanyId int
)
Begin
	Declare exit handler for sqlexception
	Begin
	
		GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
									
		Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
		Call sp_LogException(@Message, '', 'SP_Employee_GetAll', 1, 0, @Result);
	End;

	Set _PageIndex = (_PageIndex - 1) * _PageSize;

	Select * from (	
		Select 
			EmployeeUid `value`, 
			Concat(FirstName, ' ', LastName) `text`
		from employees
		where CompanyId = _CompanyId And IsActive = true And
		CASE 
			When _SearchString is null OR _SearchString = ''
			then 1=1
			Else (FirstName like concat('%', _SearchString, '%') OR LastName  like concat('%', _SearchString, '%'))
		END
		Order by  UpdatedOn Desc, CreatedOn Desc
	)T limit _PageSize offset _PageIndex;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_brakup_detail_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_brakup_detail_insupd`(

/*

	Set @outcome = '';
	Call sp_employee_brakup_detail_insupd(4, null, 0, null, 0, @outcome);
    Select @outcome;

*/
	_EmployeeId bigint,
    _BreakUpDetail Json,
    _BreakUpHeaderCount int,
    _DeductionDetail Json,
    _DeductionHeaderCount int,
    out _ProcessingResult varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_employee_brakup_detail_insupd', 1, 0, @Result);
			end;  
		
		if not exists(select 1 from employee_brakup_detail where EmployeeId = _EmployeeId) then
        begin
			insert into employee_brakup_detail
            values(
				_EmployeeId,
				_BreakUpDetail,
                _BreakUpHeaderCount,
				_DeductionDetail,
				_DeductionHeaderCount,
				utc_date()
            );
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update employee_brakup_detail set				
				BreakUpDetail					=			_BreakUpDetail,
				BreakUpHeaderCount				=			_BreakUpHeaderCount,
				DeductionDetail					=			_DeductionDetail,
				DeductionHeaderCount			=			_DeductionHeaderCount,
                UpdatedOn						=			utc_date()
            where EmployeeId = _EmployeeId;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Employee_DeActivate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employee_DeActivate`(

/*

	Call sp_Employee_DeActivate(13);

*/
	_EmployeeId bigint,
	_FullName varchar(150),
	_Mobile varchar(20),
	_Email varchar(100),
	_Package decimal,
	_DateOfJoining datetime,
	_DateOfLeaving datetime,
	_EmployeeCompleteDetailModal Json,
    _AdminId long,
    out _ProcessingResult varchar(50)
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
            
            RollBack;
            Set autocommit = 1;
            Set sql_safe_updates = 1;
            
			Call sp_LogException (@Message, @OperationStatus, 'sp_Employee_DeActivate', 1, 0, @Result);
		end;  
	
		set autocommit = 0;

		start transaction;
        begin					
            insert into employee_archive values(
				_EmployeeId,
				_FullName,
				_Mobile,
				_Email,
				_Package,
				_DateOfJoining,
				_DateOfLeaving,
				_EmployeeCompleteDetailModal,
                _AdminId,
                now()
            );
            
            Set sql_safe_updates = 0;

			delete from employeepersonaldetail
			where EmployeeUid = _EmployeeId;

			delete from employeeprofessiondetail
			where EmployeeUid = _EmployeeId;

			delete from employeelogin
			where EmployeeId = _EmployeeId;
			
			delete from employee_declaration
			where EmployeeId = _EmployeeId;
			
			delete from employee_leave_request
			where EmployeeId = _EmployeeId;
			
			delete from employee_notice_period
			where EmployeeId = _EmployeeId;
            
			delete from employee_salary_detail
			where EmployeeId = _EmployeeId;
			
			delete from employee_timesheet
			where EmployeeId = _EmployeeId;
			
			delete from employeemappedclients
			where EmployeeUid = _EmployeeId;
            
			delete from employees e
			where e.EmployeeUid = _EmployeeId;
        end;        
        commit;
        
        set _ProcessingResult = 'updated';
        Set sql_safe_updates = 1;
        Set autocommit = 1;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_declaration_get_byEmployeeId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_declaration_get_byEmployeeId`(

/*

	Call sp_employee_declaration_get_byEmployeeId(8, 6);

*/
	 _EmployeeId bigint,
     _UserTypeId int
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_employee_declaration_get_byEmployeeId', 1, 0, @Result);
		end;  
		
		select 
			d.*, e.Email
		from employee_declaration d
        inner join employees e on e.EmployeeUid = d.EmployeeId
		where EmployeeId = _EmployeeId;
        
        Select * from userfiledetail
        where FileOwnerId = _EmployeeId and 
        UserTypeId = _UserTypeId;
        
        Select * from employee_salary_detail
        where EmployeeId = _EmployeeId;
        
        Select * from salary_components;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_declaration_get_byId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_declaration_get_byId`(

/*

	Call sp_employee_declaration_get_byId(1);

*/
	 _EmployeeDeclarationId bigint
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_employee_declaration_get_byId', 1, 0, @Result);
		end;  
		
		select 
			*
		from employee_declaration
		where EmployeeDeclarationId = _EmployeeDeclarationId;
        
        Select * from salary_components;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_declaration_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_declaration_insupd`(

/*

	Set @result = '';
	Call sp_employee_declaration_insupd(2, 0, '', '[]', @result);
    Select @result;

*/

	_EmployeeDeclarationId bigint,
	_EmployeeId bigint,
    _DocumentPath varchar(250),
	_DeclarationDetail Json,
    _HousingProperty Json,
    _TotalDeclaredAmount decimal,
    _TotalApprovedAmount decimal,
    out _ProcessingResult varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_employee_declaration_insupd', 1, 0, @Result);
			end;  
		
        if not exists(Select 1 from employee_declaration where EmployeeDeclarationId = _EmployeeDeclarationId) then
        begin
			if not exists(select 1 from employee_declaration where EmployeeId = _EmployeeId) then
            begin
				Insert into employee_declaration values(
					default,
					_EmployeeId,
					_DocumentPath,
					_DeclarationDetail,
                    _HousingProperty,
                    _TotalDeclaredAmount,
					_TotalApprovedAmount
				);            
            end;
            end if;
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			Set sql_safe_updates = 0;
            if (_HousingProperty is not null and _TotalDeclaredAmount > 0 and _TotalApprovedAmount > 0) then
            begin
				Update employee_declaration Set
					DeclarationDetail			=			_DeclarationDetail,
					DocumentPath				=			_DocumentPath,
					HousingProperty				=			_HousingProperty,
                    TotalDeclaredAmount			=			_TotalDeclaredAmount,
					TotalApprovedAmount			=			_TotalApprovedAmount
				where EmployeeDeclarationId = _EmployeeDeclarationId;
            end;
            elseif (_HousingProperty is not null) then
            begin
				Update employee_declaration Set
					DeclarationDetail			=			_DeclarationDetail,
					DocumentPath				=			_DocumentPath,
					HousingProperty				=			_HousingProperty
				where EmployeeDeclarationId = _EmployeeDeclarationId;
            end;
            elseif (_TotalDeclaredAmount > 0 and _TotalApprovedAmount > 0) then
            begin
				Update employee_declaration Set
					DeclarationDetail			=			_DeclarationDetail,
					DocumentPath				=			_DocumentPath,
					TotalDeclaredAmount			=			_TotalDeclaredAmount,
					TotalApprovedAmount			=			_TotalApprovedAmount
				where EmployeeDeclarationId = _EmployeeDeclarationId;
            end;
			else
            begin
				Update employee_declaration Set
					DeclarationDetail			=			_DeclarationDetail
				where EmployeeDeclarationId = _EmployeeDeclarationId;
            end;
			end if;
            
            Set sql_safe_updates = 0;
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_declaration_ins_json` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_declaration_ins_json`(

/*

	Call sp_employee_declaration_ins_json(19);

*/
     _EmployeeDeclarationId bigint
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_employee_declaration_ins_json', 1, 0, @Result);
		end;  
		        
        Set @i = 0;
        Set @total = 0;
        Set @jsonArray = '';
        Set @seperater = '';
        
        Select Count(1) from salary_components into @total;
        
        while @i < @total do
        begin
			Set @jsonArray = Concat(@jsonArray, @seperater,
				(Select jsonData from(
					Select Row_number() over(order by ComponentId) as rowIndex, JSON_OBJECT(
						'ComponentId', ComponentId,
						'ComponentFullName', ComponentFullName,
						'ComponentDescription', ComponentDescription,
						'CalculateInPercentage', if(CalculateInPercentage = 1, cast(TRUE as json), cast(FALSE as json)),
						'TaxExempt', if(TaxExempt = 1, cast(TRUE as json), cast(FALSE as json)),
						'ComponentTypeId', ComponentTypeId,
						'PercentageValue', PercentageValue,
						'MaxLimit', MaxLimit,
						'DeclaredValue', DeclaredValue,
						'Formula', Formula,
						'EmployeeContribution', EmployeeContribution,
						'EmployerContribution', EmployerContribution,
						'IncludeInPayslip', if(IncludeInPayslip = 1, cast(TRUE as json), cast(FALSE as json)),
						'Section', Section,
						'SectionMaxLimit', SectionMaxLimit,
						'IsAdHoc', if(IsAdHoc = 1, cast(TRUE as json), cast(FALSE as json)),
						'AdHocId', AdHocId,
						'IsOpted', if(IsOpted = 1, cast(TRUE as json), cast(FALSE as json)),
						'IsActive', if(IsActive = 1, cast(TRUE as json), cast(FALSE as json))
					) as jsonData from salary_components
				)T where rowIndex = @i + 1)
			);
            
			Set @seperater = ','; 
            Set @i = @i + 1;
        end;
        end while;
        
        -- Select Concat('[', @jsonArray, ']') as JsonData;
		Set @result = '';
		Call sp_employee_declaration_insupd(_EmployeeDeclarationId, 0, '', CONVERT(Concat('[', @jsonArray, ']'),  JSON), '{}', 0, 0, @result);
		Select @result;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_Employee_GetAll` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'NO_AUTO_VALUE_ON_ZERO' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employee_GetAll`(
/*	

	Call SP_Employee_GetAll(' 1=1 ', '', 1, 20);

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
                    emp.CompanyId,
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
                inner join employeelogin l on emp.EmployeeUid = l.EmployeeId
				Left Join employeepersonaldetail eper on emp.EmployeeUid = eper.EmployeeUid
				left join employeeprofessiondetail eprof on emp.EmployeeUid = eprof.EmployeeUid
				where ', _SearchString, '
			');	
            
            Set @SelectQuery = '';
			Set @SelectQuery = Concat('
				select *, Row_Number() over() as RowIndex, Count(1) Over() as Total from (
					', @activeQuery ,'
				)T Order by ', _SortBy ,' limit ', _PageSize ,' offset ', (_PageIndex - 1) * 10
			);
            
		# select @SelectQuery;
		prepare SelectQuery from @SelectQuery;
		execute SelectQuery;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_Employee_GetAllInActive` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
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
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Employee_GetArcheiveCompleteDetail` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employee_GetArcheiveCompleteDetail`(
	_EmployeeId bigint
/*	

	Call sp_Employee_GetArcheiveCompleteDetail(4);
    
    
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
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Employee_GetCompleteDetail` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employee_GetCompleteDetail`(
	_EmployeeId bigint
/*	

	Call sp_Employee_GetCompleteDetail(4);
    
    
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
			Call sp_LogException(@Message, '', 'sp_Employee_GetCompleteDetail', 1, 0, @Result);
		End;
        
		select e.* from employees e
		where e.EmployeeUid = _EmployeeId;

		select * from employeepersonaldetail
        where EmployeeUid = _EmployeeId;

		select * from employeeprofessiondetail
		where EmployeeUid = _EmployeeId;

		select * from employeelogin
		where EmployeeId = _EmployeeId;
        
        select * from employee_declaration
        where EmployeeId = _EmployeeId;
        
        select * from employee_leave_request
        where EmployeeId = _EmployeeId;
        
        select * from employee_notice_period
        where EmployeeId = _EmployeeId;
        
        -- select * from employee_request;
        
        select * from employee_salary_detail
        where EmployeeId = _EmployeeId;
        
        select * from employee_timesheet
        where EmployeeId = _EmployeeId;
        
        select * from employeemappedclients
        where EmployeeUid = _EmployeeId;
        
        -- select * from employeepayroll
        -- select * from employeebilldocuments
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_leaveplan_mapping_GetByPlanId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_leaveplan_mapping_GetByPlanId`(
	_LeavePlanId int
    
#	Call sp_employee_leaveplan_mapping_GetByPlanId(1);    
    
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);

            Call sp_LogException (@Message, '', 'sp_employee_leaveplan_mapping_GetByPlanId', 1, 0, @Result);
		end;
        
        select * from employee_leaveplan_mapping
        where 	LeavePlanId 			= 		_LeavePlanId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_leaveplan_mapping_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_leaveplan_mapping_insupd`(	
	_EmployeeLeaveplanMappingId bigint,
    _EmployeeId bigint,
    _LeavePlanId int,
    _IsAdded bit,
    out _ProcessingResult varchar(50)
)
Begin
	Declare Exit handler for sqlexception
	Begin
		Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
                                    
		Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
		Call sp_LogException (@Message, '', 'sp_employee_leaveplan_mapping_insupd', 1, 0, @Result);
	end;
   
   if (_IsAdded) then
   begin
	   if not exists(select 1 from employee_leaveplan_mapping where EmployeeLeaveplanMappingId = _EmployeeLeaveplanMappingId) then
	   begin
			insert into employee_leaveplan_mapping values(
				default,
				_EmployeeId,
				_LeavePlanId
			);
			
			set _ProcessingResult = 'inserted';
	   end;
	   else
	   begin
			update employee_leaveplan_mapping set
				EmployeeId		=		_EmployeeId,
				LeavePlanId		=		_LeavePlanId
			where EmployeeLeaveplanMappingId = _EmployeeLeaveplanMappingId;
			
			set _ProcessingResult = 'updated';
	   end;
	   end if;
	end;
    else
    begin
		if exists(select 1 from employee_leaveplan_mapping where EmployeeLeaveplanMappingId = _EmployeeLeaveplanMappingId) then
		begin
			delete from employee_leaveplan_mapping
			where EmployeeLeaveplanMappingId = _EmployeeLeaveplanMappingId;
			
			set _ProcessingResult = 'deleted';
	   end;
       end if;
    end;
    end if;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_leaveplan_upd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_leaveplan_upd`(	
    _EmployeeId bigint,
    _LeavePlanId int,
    _AdminId bigint,
    out _ProcessingResult varchar(50)
)
Begin
	Declare Exit handler for sqlexception
	Begin
		Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
                                    
		Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
		Call sp_LogException (@Message, '', 'sp_employee_leaveplan_upd', 1, 0, @Result);
	end;
    begin
		update employees set
				LeavePlanId		=		_LeavePlanId,
                UpdatedBy		=		_AdminId,
                UpdatedOn		= 		now()
		where EmployeeUid		=		_EmployeeId;
        
		set _ProcessingResult = 'updated';
	end;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_leave_request_filter` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_leave_request_filter`(
	_EmployeeId bigint,
	_searchString varchar(250),
	_sortBy varchar(50),
	_pageIndex int,
	_pageSize int
/*	

	Call sp_employee_leave_request_filter(10, '1=1 ', '', 1, 10);

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
			Call sp_LogException(@Message, '', 'sp_employee_leave_request_filter', 1, 0, @Result);
		End;

        Begin
			If(_sortBy is NULL OR _sortBy = '') then
            begin
				Set _sortBy = ' e.FromDate DESC';
			end;
			End if;
            
            If (_EmployeeId > 0) then
            begin
				Set _SearchString = Concat(_SearchString, ' AND e.EmployeeId = ', _EmployeeId, ' ');
            end;
            end if;
			
				Set @SelectQuery = CONCAT('Select * from (
				Select 
					Row_Number() over(Order by ', _sortBy, ') as `Index`,
                    e.LeaveRequestId,
                    e.EmployeeId,
                    e.LeaveDetail,
                    e.FromDate,
                    e.ToDate,
                    Count(1) Over() as Total 
					from employee_leave_request e
				where ', _SearchString, ' 
			)T where `Index` between ', (_pageIndex - 1) * _pageSize + 1 ,' and ', (_pageIndex * _pageSize)) ;
			# select * from employee_leave_request;
			# Select @SelectQuery;
			prepare SelectQuery from @SelectQuery;
			execute SelectQuery;	
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_leave_request_GetById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_leave_request_GetById`(
	_EmployeeId bigint,
    _Year int
    
#	Call sp_employee_leave_request_GetById(4, 2022);    
    
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);

            Call sp_LogException (@Message, '', 'sp_employee_leave_request_GetById', 1, 0, @Result);
		end;
        
        select * from employee_leave_request
        where 	EmployeeId 			= 		_EmployeeId and 
				Year 				= 		_Year;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_leave_request_InsUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_leave_request_InsUpdate`(
	   /*

	set @result = '';
    Call sp_employee_leave_request_InsUpdate(10, '{}', 6, 2022, 'covid', 2, 1, '2022-05-22', '2022-05-24', 2, @result);
	select @result;

*/
	_LeaveRequestId bigint,
	_EmployeeId bigint,
	_LeaveDetail Json,
    _Reason varchar(500),
    _UserTypeId int,
    _AssignTo bigint,
    _Year int,
    _LeaveFromDay datetime,
	_LeaveToDay datetime,
    _LeaveType int,
    _RequestStatusId int,
    _RequestType int,
    _AvailableLeaves decimal,
	_TotalLeaveApplied decimal,
	_TotalApprovedLeave decimal,
	_TotalLeaveQuota decimal,
    _LeaveQuotaDetail json,
    out _ProcessingResult varchar(1000)
    
 
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
             
			Set _ProcessingResult = null;
            RollBack;
            SET autocommit = 1;
            Call sp_LogException (@Message, @OperationStatus, 'sp_employee_leave_request_InsUpdate', 1, 0, @Result);
		end;
        
        Set @LeaveRequestId = 0;
        SET autocommit = 0;
        Start Transaction;
		Begin 			
			If not exists (Select 1 from employee_leave_request Where LeaveRequestId = _LeaveRequestId Or (EmployeeId = _EmployeeId and `Year` = _Year)) then
			Begin
                Select LeaveRequestId into @LeaveRequestId from employee_leave_request order by LeaveRequestId desc limit 1;
                Set @LeaveRequestId = @LeaveRequestId + 1;
                
				Insert into employee_leave_request Values (
					@LeaveRequestId,
					_EmployeeId,
					_LeaveDetail,
					_Year,
                    _AvailableLeaves,
                    _TotalLeaveApplied,
                    _TotalApprovedLeave,
                    _TotalLeaveQuota,
					_LeaveQuotaDetail
				);
			End;
			Else
			Begin
				Select LeaveRequestId into @LeaveRequestId from employee_leave_request 
                Where LeaveRequestId = _LeaveRequestId Or (EmployeeId = _EmployeeId and `Year` = _Year);
				
                Update employee_leave_request SET 
					LeaveDetail				=		_LeaveDetail,
                    Year					=		_Year,
                    AvailableLeaves			=		_AvailableLeaves,
                    TotalLeaveApplied		=		_TotalLeaveApplied,
                    TotalApprovedLeave		=		_TotalApprovedLeave,
                    TotalLeaveQuota			=		_TotalLeaveQuota,
                    LeaveQuotaDetail		=		_LeaveQuotaDetail
				Where LeaveRequestId = @LeaveRequestId;
			End;
			End if;
		
			Set @EmployeeName = '';
			Set @Email = '';
			Set @Mobile = '';
			Set @ProjectId = 0;
			Set @ProjectName = null;
			Set @outCome = '';
			
			Select 
				Concat(FirstName, ' ', LastName) FullName, Mobile, Email 
			from employees 
			where EmployeeUid = _EmployeeId
			into @EmployeeName, @Mobile, @Email;
			
			Set @approvalRequestId = 0;
			
			if(@approvalRequestId is null) then
				Set @approvalRequestId = 0;
			end if;

			If not exists (Select 1 from approval_request Where ApprovalRequestId = @approvalRequestId) then
			Begin
				Insert into approval_request Values (
					default,
					@EmployeeName,
					_Reason,
					_EmployeeId,
					_UserTypeId,
					utc_date(),
					@Email,
					@Mobile,
					_LeaveFromDay,
					_LeaveToDay,
					_AssignTo,
					@ProjectId,
					@ProjectName,
                    _RequestStatusId,
					0, -- _AttendanceId,
                    _RequestType,
                    _LeaveType,
                    @LeaveRequestId
				);                
			End;
			Else
			Begin
				Update approval_request SET 
					UserName						=			@EmployeeName,
					Message							=			_Reason,
					UserId							=			_EmployeeId,
					UserTypeId						=			_UserTypeId,
					RequestedOn						=			utc_date(),
					Email							=			@Email,
					Mobile							=			@Mobile,
					FromDate						=			_LeaveFromDay,
					ToDate							=			_LeaveToDay,
					AssigneeId						=			_AssignTo,
					ProjectId						=			@ProjectId,
					ProjectName						=			@ProjectName,
                    RequestStatusId					=			_RequestStatusId,
                    LeaveType						=			_LeaveType,
                    LeaveRequestId					=			_LeaveRequestId,
                    RequestType						=			_RequestType
				Where 	ApprovalRequestId = @approvalRequestId;
			End;
			End if;
        
			Set _ProcessingResult = 'updated';
        End;
        COMMIT;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_notice_period_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_notice_period_insupd`(	
    _EmployeeNoticePeriodId bigint,
    _EmployeeId bigint,
    _ApprovedOn datetime,
    _ApplicableFrom datetime,
    _ApproverManagerId int,
    _ManagerDescription varchar(500),
    _AttachmentPath varchar(200),
    _EmailTitle varchar(100),
    _OtherApproverManagerIds json,
    _ITClearanceStatus int,
    _ReportingManagerClearanceStatus int,
    _CanteenClearanceStatus int,
    _ClientClearanceStatus int,
    _HRClearanceStatus int,
    _OfficialLastWorkingDay datetime,
    _PeriodDuration int,
    _EarlyLeaveStatus int,
    _Reason varchar(500),
    _AdminId bigint,
    out _ProcessingResult varchar(50)
)
Begin
	Declare Exit handler for sqlexception
	Begin
		Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
                                    
		Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
		Call sp_LogException (@Message, '', 'sp_employee_notice_period_insupd', 1, 0, @Result);
	end;
    begin
		if not exists (select 1 from employee_notice_period where EmployeeId = _EmployeeId) then
		begin
			insert into employee_notice_period values (
				default,
                _EmployeeId,
				_ApprovedOn,
				_ApplicableFrom,
				_ApproverManagerId,
				_ManagerDescription,
				_AttachmentPath,
				_EmailTitle,
				_OtherApproverManagerIds,
				_ITClearanceStatus,
				_ReportingManagerClearanceStatus,
				_CanteenClearanceStatus,
				_ClientClearanceStatus,
				_HRClearanceStatus,
				_OfficialLastWorkingDay,
				_PeriodDuration,
				_EarlyLeaveStatus,
				_Reason,
                _AdminId,
                null,
                now(),
                null
            );
            set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update employee_notice_period set
					ApprovedOn							=		_ApprovedOn,
					ApplicableFrom						=		_ApplicableFrom,
					ApproverManagerId					=		_ApproverManagerId,
					ManagerDescription					=		_ManagerDescription,
					AttachmentPath						=		_AttachmentPath,
					EmailTitle							=		_EmailTitle,
					OtherApproverManagerIds				=		_OtherApproverManagerIds,
					ITClearanceStatus					=		_ITClearanceStatus,
					ReportingManagerClearanceStatus		=		_ReportingManagerClearanceStatus,
					CanteenClearanceStatus				=		_CanteenClearanceStatus,
					ClientClearanceStatus				=		_ClientClearanceStatus,
					HRClearanceStatus					=		_HRClearanceStatus,
					OfficialLastWorkingDay				=		_OfficialLastWorkingDay,
					PeriodDuration						=		_PeriodDuration,
					EarlyLeaveStatus					=		_EarlyLeaveStatus,
					Reason								=		_Reason,
					UpdatedBy							=		_AdminId,
					UpdatedOn							= 		now()
			where EmployeeId		=		_EmployeeId;
			
			set _ProcessingResult = 'updated';
        end;
        end if;
	end;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_notification_InsUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_notification_InsUpdate`(
	_NotificationId bigint,
	_Message varchar(500),
	_UserId bigint,
	_UserTypeId int,
	_RequestedOn bigint,
	_UserName varchar(100),
	_Email varchar(100),
	_Mobile varchar(14),
	_AssigneeId long,
	_Status	int,
	ActionTakenOn DateTime,
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
            Call sp_LogException (@Message, @OperationStatus, 'sp_employee_notification_InsUpdate', 1, 0, @Result);
		end;
        
        Set @ClientName = '';
        Select ClientName from clients where ClientId = _AllocatedClientId into @ClientName;
        
        SET autocommit = 0;
        Start Transaction;
		Begin 
			If not exists (Select 1 from employee_notification Where NotificationId = _NotificationId) then
			Begin
				Insert into employees Values (
					default,
					_Message,
					_UserId,
					_UserTypeId,
					_RequestedOn,
					_UserName,
					_Email,
					_Mobile,
					_AssigneeId,
					_Status,
					_ActionTakenOn
				);
                
                Set _ProcessingResult = 'inserted';
			End;
			Else
			Begin
				Update employee_notification SET 
					Message 				= 		_Message,
					UserId 					= 		_UserId,
					UserTypeId 				= 		_UserTypeId,
					RequestedOn 			= 		_RequestedOn,
					UserName 				= 		_UserName,
					Email 					= 		_Email,
					Mobile 					= 		_Mobile,
					AssigneeId 				= 		_AssigneeId,
					Status 					= 		_Status,
					ActionTakenOn 			= 		_ActionTakenOn
				Where 	NotificationId = _NotificationId;
                
                Set _ProcessingResult = 'updated';
			End;
			End if;
            COMMIT;
		End;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_profile` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_profile`(

/*

	Call sp_employee_profile(10, null, null, 2);

*/
	_UserId bigint,
	_Mobile varchar(20),
	_Email varchar(50),
    _UserTypeId int
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_professionaldetail_filter', 1, 0, @Result);
			end;  
            if exists (select 1 from employeeprofessiondetail where EmployeeUid = _UserId) then
            begin
				Select * from employeeprofessiondetail p
				where p.EmployeeUid = _UserId;
            end;
            else
            begin
				Select * from employeeprofessiondetail_archive p
				where p.EmployeeUid = _UserId;
            end;
            end if;
            
            Select * from candidatefiledetail c
            where c.FileOwnerId = _UserId and c.UserTypeId = _UserTypeId;
            
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_salary_detail_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_salary_detail_get`(

/*

	Call sp_employee_salary_detail_get();

*/
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_employee_salary_detail_get', 1, 0, @Result);
		end;  
		        
        Select * from employee_salary_detail;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_salary_detail_get_by_empid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_salary_detail_get_by_empid`(

/*

	Call sp_employee_salary_detail_get_by_empid(4);

*/

	_EmployeeId bigint
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_employee_salary_detail_get', 1, 0, @Result);
		end;  
		      
        Select 
			d.EmployeeId, 
            d.CTC,
            d.GrossIncome,
            d.NetSalary, 
            d.CompleteSalaryDetail,
            case
				when exists (select 1 from salary_group where d.CTC between MinAmount and MaxAmount)
					then (select SalaryGroupId from salary_group where d.CTC between MinAmount and MaxAmount)
				else 0
			end GroupId, 
            d.TaxDetail 
		from employee_salary_detail d
        where EmployeeId = _EmployeeId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_salary_detail_InsUpd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_salary_detail_InsUpd`(

/*

	set @result = '';
	Call sp_employee_salary_detail_InsUpd(29, 1000000, 0.0, 0.0, '{}', 2, '[]', @result);
    select @result;

*/

	_EmployeeId bigint,
    _CTC decimal,
    _GrossIncome decimal,
    _NetSalary decimal,
	_CompleteSalaryDetail json,
    _GroupId int,
    _TaxDetail json,
    out _ProcessingResult varchar(100)
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set sql_safe_updates = 1;                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_employee_salary_detail_InsUpd', 1, 0, @Result);
		end; 
        
        
		Begin
        
			Set @groupId = 0;
			select SalaryGroupId into @groupId from salary_group 
			where _CTC >= MinAmount 
			and _CTC < MaxAmount;
        
			if not exists (select 1 from employee_salary_detail where EmployeeId = _EmployeeId) then
            Begin
				insert into employee_salary_detail
                values 
                (
					_EmployeeId,
                    _CTC,
                    _GrossIncome,
                    _NetSalary,
                    _CompleteSalaryDetail,
                    @groupId,
                    _TaxDetail
                );
                
                Set _ProcessingResult = 'inserted';
            End;
            Else
            Begin
				Set sql_safe_updates = 0;
				update employee_salary_detail 
					set	CTC						=		_CTC,
						GrossIncome				=		_GrossIncome,
						NetSalary				=		_NetSalary,
						CompleteSalaryDetail	=		_CompleteSalaryDetail,
						GroupId					=		@groupId,
						TaxDetail				= 		_TaxDetail
				where EmployeeId = _EmployeeId;
                
                Set sql_safe_updates = 1;
                Set _ProcessingResult = 'updated';
            End;
            End if;
        End;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_timesheet_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_timesheet_get`(

/*


    Call sp_employee_timesheet_get(4, 5, 1, 2022, 4);


*/

    _EmployeeId bigint,
    _ClientId bigint,
    _UserTypeId int,
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_employee_timesheet_get', 1, 0, @Result);
		end;
		
		if (_ForYear > 0 && _ForMonth > 0) then
		begin
			Select 
				a.*
			from employee_timesheet a   
			Where a.EmployeeId = _EmployeeId
			And _ForYear = a.ForYear and _ForMonth = a.ForMonth
			And a.UserTypeId = _UserTypeId;
		end;
		else 
		begin
			Select 
				a.*
			from employee_timesheet a                 
			Where a.EmployeeId = _EmployeeId
			And a.UserTypeId = _UserTypeId;
		end;
		end if;
        
        Select distinct e.*, m.BillingHours from employees e
        left join employeemappedclients m on e.EmployeeUid = m.EmployeeUid
        Where e.EmployeeUid = _EmployeeId;
        -- And m.ClientUid = _ClientId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_employee_timesheet_getby_empid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_timesheet_getby_empid`(
	_EmployeeId long,
    _UserTypeId int,
    _ForMonth int,
    _ForYear int,
    _ClientId bigint
    
/*


	call sp_employee_timesheet_getby_empid(45, 2, 9, 2022, 2)


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
			Call sp_LogException(@Message, '', 'sp_employee_timesheet_getby_empid', 1, 0, @Result);
		End;
		
		Select 
			a.*
		from employee_timesheet a   
		Where a.EmployeeId = _EmployeeId
		And _ForYear = a.ForYear and _ForMonth = a.ForMonth
		And a.UserTypeId = _UserTypeId
        And a.ClientId = _ClientId;
        
        select * from employees
        Where EmployeeUid = _EmployeeId;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_Employee_ToggleDelete` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employee_ToggleDelete`(
	_employeeId bigint,
    _active bit,
	_adminId bigint
/*	

	Call SP_Employee_ToggleDelete(1, 0, 1);

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
			Call sp_LogException(@Message, '', 'SP_Employee_ToggleDelete', 1, 0, @Result);
		End;

        Begin
            Update employees set 
            IsActive = _active 
            where EmployeeUid = _employeeId;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_ExistingBill_GetById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ExistingBill_GetById`(
	_EmployeeId bigint,
    _UserTypeId int,
    _ClientId bigint,
    _FileId bigint,
    _AdminId bigint
/*	

	Call sp_ExistingBill_GetById(4, 1, 5, 93, 6);
    
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
			Call sp_LogException(@Message, '', 'sp_ExistingBill_GetById', 1, 0, @Result);
		End;
        
        Drop table if exists employeebill;
        
        CREATE temporary table employeebill (
		  `BillDetailUid` bigint,
		  `PaidAmount` double DEFAULT NULL,
		  `BillForMonth` int DEFAULT NULL,
		  `BillYear` int DEFAULT NULL,
		  `NoOfDays` int DEFAULT NULL,
		  `NoOfDaysAbsent` double DEFAULT NULL,
		  `IGST` int DEFAULT NULL,
		  `SGST` int DEFAULT NULL,
		  `CGST` int DEFAULT NULL,
		  `TDS` int DEFAULT NULL,
		  `BillStatusId` bigint NOT NULL,
		  `PaidOn` datetime(6) DEFAULT NULL,
		  `FileDetailId` bigint NOT NULL,
		  `EmployeeUid` bigint DEFAULT NULL,
		  `ClientId` bigint NOT NULL,
		  `BillNo` varchar(45) NOT NULL,
		  `UpdateSeqNo` int NOT NULL DEFAULT '0',
		  `CreatedBy` bigint NOT NULL,
		  `UpdatedBy` bigint DEFAULT NULL,
		  `CreatedOn` datetime(6) NOT NULL,
		  `UpdatedOn` datetime DEFAULT NULL,
		  `BillUpdatedOn` datetime NOT NULL,
		  `IsCustomBill` bit(1) NOT NULL DEFAULT b'0',
          DeveloperName varchar(100)
		);
        
        Insert into employeebill
		select b.*, Concat(e.FirstName, ' ', e.LastName) DeveloperName from billdetail b
        Inner join (select * from employees) e on e.EmployeeUid = b.EmployeeUid
        Where b.EmployeeUid = _EmployeeId 
        And b.ClientId = _ClientId
        And b.FileDetailId = _FileId;
        
        Set @BillDetailId = 0;
        Set @ForMonth = 0;
        Set @ForYear = 0;
        Set @DeveloperName = '';
        Select BillDetailUid, BillForMonth, BillYear, DeveloperName from employeebill
        Into @BillDetailId, @ForMonth, @ForYear, @DeveloperName;
        
        Select b.*, @DeveloperName DeveloperName from billdetail b
        where BillDetailUid = @BillDetailId;
        
        Select f.* from filedetail f
        where f.FileId = _FileId;
        
        Select c.* from clients c 
        where c.ClientId = _ClientId;
        
        Select c.* from clients c 
        where c.ClientId = 1;
        
		select a.* from attendance a
		where a.EmployeeId = _EmployeeId
		and a.UserTypeId = _UserTypeId
		and a.ForMonth = @ForMonth
		and a.ForYear = @ForYear;
        
        Drop table employeebill;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_filedetail_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_filedetail_insupd`(
	_FileId bigint,
    _ClientId bigint,
    _FileName varchar(100),
    _FilePath varchar(500),
    _FileExtension varchar(10),
    _StatusId bigint,
    _GeneratedBillNo int,
    _BillUid bigint,
    _BillDetailId bigint,
    _BillNo varchar(20),
    _PaidAmount double,
	_BillForMonth int,
	_BillYear int,
	_NoOfDays int,
	_NoOfDaysAbsent double,
	_IGST float,
	_SGST float,
	_CGST float,
	_TDS float,
	_BillStatusId bigint,
	_PaidOn Datetime,
	_FileDetailId bigint,
	_EmployeeUid bigint,    
    _UpdateSeqNo int,
    _BillUpdatedOn Datetime,
    _IsCustomBill bit,
    _UserTypeId int,
    _AdminId bigint,
    out _ProcessingResult varchar(100)
/*	

	Call sp_filedetail_insupd(1);

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
			Call sp_LogException(@Message, '', 'sp_filedetail_insupd', 1, 0, @Result);
		End;

		Start Transaction;
        Begin
			Set _ProcessingResult = '';
			if not exists(Select 1 from filedetail where FileId = _FileId) then
            Begin
				Set @fileDetailId = 0;
                Select FileId from filedetail order by FileId DESC limit 1 into @fileDetailId;
                if(@fileDetailId is null OR @fileDetailId = '')then
					Set _FileId = 1;
				else	
					Set _FileId = @fileDetailId + 1;
                end if;
                
				Insert into filedetail values(
					_FileId, 
					_EmployeeUid,
					_FilePath,
                    _FileName,
					_FileExtension,
					_StatusId,					
                    _PaidOn,
                    _UserTypeId,
                    _AdminId,
                    null,
                    NOW(),
                    null
                );
                
                Insert into billdetail values(
					default,
                    _PaidAmount,
                    _BillForMonth,
                    _BillYear,
                    _NoOfDays,
                    _NoOfDaysAbsent,
                    _IGST,
                    _SGST,
                    _CGST,
                    _TDS,
                    _BillStatusId,
                    _PaidOn,
                    _FileId,
                    _EmployeeUid,
                    _ClientId,
                    _BillNo,
                    _UpdateSeqNo,
                    _AdminId,
                    null,
                    NOW(),
                    null,
                    NOW(),
                    _IsCustomBill
                );
                
                
				# select * from billdetail;
                
                if(_GeneratedBillNo is not null And _GeneratedBillNo <> '') then
					Update bills b Set
						b.LastBillNo = _GeneratedBillNo
					Where BillUid = _BillUid;
                End if;
                
                Set _ProcessingResult = _FileId;
            End;
            Else
            Begin
				Update filedetail Set
					FilePath		=	_FilePath,
                    FileName		=	_FileName,
					FileExtension	=	_FileExtension,
					ItemStatusId	=	_StatusId,				
                    PaidOn			=	_PaidOn,
                    UpdatedBy		=	_AdminId,
                    UpdatedOn 		=	NOW()
                Where FileId = _FileId;
                
                If exists(Select 1 from billdetail Where BillDetailUid = _BillDetailId) then
                Begin
					Update billdetail Set
						PaidAmount			=		_PaidAmount,
                        BillNo				=		_BillNo,
						BillForMonth		=		_BillForMonth,
						BillYear			=		_BillYear,
						NoOfDays			=		_NoOfDays,
						NoOfDaysAbsent		=		_NoOfDaysAbsent,
						IGST				=		_IGST,
						SGST				=		_SGST,
						CGST				=		_CGST,
						TDS					=		_TDS,
						PaidOn				=		_PaidOn,
                        BillStatusId		=		_StatusId,
                        UpdateSeqNo			=		_UpdateSeqNo,
						UpdatedBy			=		_AdminId,
						UpdatedOn			=		now(),
                        BillUpdatedOn		=		_BillUpdatedOn,
                        IsCustomBill		=		_IsCustomBill
					Where BillDetailUid 	=		_BillDetailId;
				End;
                End if;
                Set _ProcessingResult = _FileId;
            End;
            End if;
		End;
        
        Commit;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_FileDetail_PatchRecord` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_FileDetail_PatchRecord`(
	_FileId bigint,
	_StatusId long,
	_UpdatedOn datetime,
	_Notes varchar(250),
    _AdminId bigint,
    out _ProcessingResult varchar(250)
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
            Call sp_LogException (@Message, @OperationStatus, 'sp_FileDetail_PatchRecord', 1, 0, @Result);
		end;
        
        Start Transaction;
        Begin
			Set _ProcessingResult = 'Fail to update. Please contact to admin.';
			if exists(Select * from filedetail where FileId = _FileId) then
			begin
				Update filedetail Set
					ItemStatusId	=		_StatusId,
					PaidOn 			=		_UpdatedOn,
					UpdatedBy		=		_AdminId,
					UpdatedOn 		=		NOW()
				where FileId = _FileId;
			end;
            else
            begin
				SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Record not found in FileDetail table';
            end;
			end if;
            
            if exists(Select * from billdetail where FileDetailId  = _FileId) then
			begin
				Update billdetail Set
					PaidOn			=		_UpdatedOn,
					BillStatusId	=		_StatusId,
					UpdatedBy		=		_AdminId,
					UpdatedOn 		=		NOW()
				where FileDetailId  = _FileId;
			end;
            else
            begin
				SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Record not found in FileDetail table';
            end;
			end if;
            
            Set _ProcessingResult = 'Record updated successfully.';
		End;
        Commit;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Files_GetById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Files_GetById`(
	_Type varchar(20),
	_Uid bigint,
	_searchString varchar(250),
	_sortBy varchar(50),
	_pageIndex int,
	_pageSize int
/*	

	Call sp_Files_GetById('employee', 2, '1=1 ', '', 1, 10);

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
			Call sp_LogException(@Message, '', 'sp_Files_GetById', 1, 0, @Result);
		End;

        Begin
			If(_sortBy is NULL OR _sortBy = '') then
				Set _sortBy = 'f.CreatedOn, f.UpdatedOn Desc';
			End if;
            
            If (_Uid > 0) then
            begin
				Set _SearchString = Concat(_SearchString, ' AND f.FileOwnerId = ', _Uid, ' ');
            end;
            end if;
			
				Set @SelectQuery = CONCAT('Select * from (
				Select 
					Row_Number() over(Order by ', _sortBy, ') as `Index`,
					f.FilePath,
					f.FileName,
					f.FileExtension,
					s.Status,
                    coalesce(g.gststatus, 2) GstStatus,
					DATE_FORMAT(b.BillUpdatedOn, ''%d %M %Y'') GeneratedOn,
					DATE_FORMAT(b.PaidOn, ''%d %M %Y'') PaidOn,
					Convert(f.FileId, char) as FileUid,
					f.FileOwnerId,
					b.BillForMonth `Month`,
                    b.NoOfDaysAbsent Absents,
					b.TDS,
                    b.NoOfDays,
					b.PaidAmount SalaryAmount,
                    (Select TakeHomeByCandidate from employeemappedclients
						where EmployeeUid = e.EmployeeUid and ClientUid = c.ClientId) TakeHome,
					b.IGST,
					b.SGST,
					b.CGST,
					c.ClientName,
					c.ClientId,
					b.BillNo,
                    b.BillStatusId,
                    Concat(e.FirstName, '' '', e.LastName) Name,
                    Count(1) Over() as Total 
                    from (select * from employees
						 union distinct
                         select * from employee_archive) e
					Inner Join filedetail f on f.FileOwnerId = e.EmployeeUid
					left join itemstatus s on s.ItemStatusId = f.ItemStatusId
					left join billdetail b on b.FileDetailId = f.FileId
					left Join clients c on c.ClientId = b.ClientId
                    left Join gstdetail g on g.billno = b.BillNo
				where ', _SearchString, '
			)T where `Index` between ', (_pageIndex - 1) * _pageSize + 1 ,' and ', (_pageIndex * _pageSize)) ;
            
			# Select @SelectQuery;
			prepare SelectQuery from @SelectQuery;
			execute SelectQuery;	
            
            Select 
				concat(e.FirstName, ' ', e.LastName) `Name`,
				e.Mobile,
                e.Email
			from employees e 
            Where EmployeeUid = _Uid
            union all
            Select
				concat(e.FirstName, ' ', e.LastName) `Name`,
				e.Mobile,
                e.Email
			from employee_archive e 
            Where EmployeeUid = _Uid;
            
            Select 
				EmployeeUId,
				concat(e.FirstName, ' ', e.LastName) `Name`,
				e.Mobile,
                e.Email
			from employees e
            Union all
            Select 
				EmployeeUId,
				concat(e.FirstName, ' ', e.LastName) `Name`,
				e.Mobile,
                e.Email
			from employee_archive e;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Files_GetBy_OwnerId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Files_GetBy_OwnerId`(
	_FileOwnerId int,
	_UserTypeId int
/*	

	Call sp_Files_GetBy_OwnerId(0, 6);

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
			Call sp_LogException(@Message, '', 'sp_Files_GetBy_OwnerId', 1, 0, @Result);
		End;

		Select * from filedetail f
        where f.FileOwnerId = _FileOwnerId and
		f.UserTypeId = _UserTypeId;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Files_InsUpd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Files_InsUpd`(
	_FileId bigint,
	_FileOwnerId bigint,
	_FilePath varchar(500), 
	_FileName varchar(100),
	_FileExtension varchar(30),
	_ItemStatusId bigint,
    _UserTypeId int,
	_PaidOn datetime(6),
	_CreatedBy bigint,
	_UpdatedBy bigint,
	_CreatedOn datetime(6), 
	_UpdatedOn datetime(6)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, '', 'sp_Files_InsUpd', 1, 0, @Result);
		end;
        Begin
			if not exists (select 1 from filedetail where FileId = _FileId) then
			Begin
				insert into filedetail 
							values
							(
								default,
								_FileOwnerId,
								_FilePath,
								_FileName,
								_FileExtension, 
								_ItemStatusId,
								_PaidOn,
                                _UserTypeId,
								_CreatedBy,
								null,
								now(), 
								null
							);
				End;
                Else
				Begin
					update filedetail set 	FileOwnerId 	= 	_FileOwnerId,
											FilePath 		= 	_FilePath,
											FileName		=	_FileName,
											FileExtension	=	_FileExtension, 
											UpdatedBy		=	_UpdatedBy,
											UpdatedOn		=	now
									where	FileId 			= 	_FileId;
				End;
                End if;
			End;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_fixed_salary_component_percent_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_fixed_salary_component_percent_get`(

/*

	Call sp_fixed_salary_component_percent_get();

*/
	
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_fixed_salary_component_percent_insupd', 1, 0, @Result);
			end;  
		
		select * from fixed_salary_component_percent;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_fixed_salary_component_percent_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_fixed_salary_component_percent_insupd`(

/*

	Call sp_fixed_salary_component_percent_insupd('BS', 'BASIC SALARY', 40, null, 1);

*/
	_ComponentId varchar(10),
    _ComponentDescription varchar(250),
    _CalculateInPercentage bit,
    _PercentageValue decimal,
    _Amount decimal,
    _EmployeeContribution decimal,
    _EmployerContribution decimal,
    _IncludeInPayslip bit,
    _IsDeductions bit,
    _IsOpted bit,
    _IsActive bit,
    _Admin bigint,
    out _ProcessingResult varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_fixed_salary_component_percent_insupd', 1, 0, @Result);
			end;  
		
		if not exists(select 1 from fixed_salary_component_percent where ComponentId = _ComponentId) then
        begin
			insert into fixed_salary_component_percent
            values(
				_ComponentId,
				_ComponentDescription,
                _CalculateInPercentage,
				_PercentageValue,
				_Amount,
				_EmployeeContribution,
				_EmployerContribution,
				_IncludeInPayslip,
                _IsDeductions,
				_IsOpted,
                _IsActive,
				utc_date(),
                null,
                _Admin,
                null
            );
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update fixed_salary_component_percent set				
				ComponentDescription			=			_ComponentDescription,
				CalculateInPercentage			=			_CalculateInPercentage,
				PercentageValue					=			_PercentageValue,
				Amount							=			_Amount,
                EmployeeContribution			= 			_EmployeeContribution,
                EmployerContribution			=			_EmployerContribution,
                IncludeInPayslip				=			_IncludeInPayslip,
                IsDeductions					=			_IsDeductions,
                IsOpted							=			_IsOpted,
                IsActive						=			_IsActive,
				UpdatedBy						=			_Admin,
                UpdatedOn						=			utc_date()
            where ComponentId = _ComponentId;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_GenerateNewPassword` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GenerateNewPassword`(

/*

	Set @outparam = '';
	Call sp_GenerateNewPassword(@outparam);
    Select @outparam;

*/


	out _ProcessingResult varchar(100)
)
Begin
	Declare Exit handler for sqlexception
	Begin
		Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
		Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
		Call sp_LogException (@Message, @OperationStatus, 'sp_GenerateNewPassword', 1, 0, @Result);
	end;
    
    Set _ProcessingResult = Fn_Generate_newPassword();
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_GenerateProcedures` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GenerateProcedures`(

#	Call sp_GenerateProcedures('Periods')

	_TableName varchar(50)
)
Begin
	Declare n int default 0;
    Declare i int default 0;
    Declare QueryTextOrg text default '';
    Declare QueryText text default '';
    Declare exit handler for sqlexception
    Begin
		Drop table TblDetails;
        Drop table FinalTable;
        Get Diagnostics condition 1 @sqlstate = returned_sqlstate,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
                                    
		Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
        Select @Message;
    End;
    
    Set QueryTextOrg = '
    
Delimiter $$

Drop procedure if exists sp_{{@TableName}}_InsUpd $$
Create procedure sp_{{@TableName}}_InsUpd(
{{@ProcedureParameterWithDataType}}
)

Begin
	Set @UUid:= (CAST(UNIX_TIMESTAMP(CURTIME(3)) * 1000 AS unsigned));
	Set @OperationStatus = \'\';
	Begin
		Declare exit handler for sqlexception
		Begin
		
			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
										
			Set @Message = CONCAT(\'ERROR \', @errorno ,  \' (\', @sqlstate, \'): \', @errortext);
			Call sp_LogException(@Message, @OperationStatus, \'sp_{{@TableName}}_InsUpd\', 1, 0, @Result);
			Set _ProcessingResult = Concat(@Message, @Result);
		end;

		Set _ProcessingResult = \'No processing\';
		if not exists(Select 1 from {{@TableName}} where {{@FindCondition}})then
		Begin
			Set @OperationStatus = \'[{{@TableName}} insert] \';
			Insert into {{@TableName}} values(@UUid, {{@ProcedureParameterForInsert}});
							
			Set _ProcessingResult = @UUid;
		end;
		else
		begin
			Set @OperationStatus = \'[{{@TableName}} Update] \';
			Update {{@TableName}} Set {{@ProcedureParameterForUpdate}}
				where {{@FindCondition}};
					
			Set _ProcessingResult = \'Updated\';
		end;
		end if;
	End;
end;
$$
	';    
    
    Set @Indexer:=0;
    Create temporary table TblDetails(Indexer int, TableName varchar(100));
    Create temporary table FinalTable(RowIndex int, TableQualifiedName varchar(100), ColumnsNameString text null);
    Insert into TblDetails Select Indexer, table_name as TableName from (
			Select (@Indexer:= @Indexer + 1) As Indexer, table_name from information_schema.tables where table_schema = 'eds')T;
            
    SELECT Count(table_name) Tablecount FROM information_schema.tables where table_schema = 'eds' into n;
    #Select n;
    
	Set i = 1;
    while i <= n do
    Begin

		Set QueryText = QueryTextOrg;
		Set @ProcedureParameterWithDataType = '';
        Set @ProcedureParameterForInsert = '';
        Set @ProcedureParameterForUpdate = '';
        Set @FindCondition = '';
        Set @TblName:= '';
        Select TD.TableName from TblDetails TD where TD.Indexer = i into @TblName;
        
        #Select @TblName;
        
        if(Lower(_TableName) = 'all')then
        Begin
			Begin # Procedure Paramter
			
				Select Lower(Group_Concat(Concat('\n\t_', column_name, ' ', column_type))) as ColumnsName from 
                information_schema.columns where 
				table_name = @TblName and Lower(column_name) <> 'createdon' and Lower(column_name) <> 'updatedon'
                and Lower(column_name) <> 'createdby' and Lower(column_name) <> 'updatedby'
				into @ProcedureParameterWithDataType;
				
				Select Concat(@ProcedureParameterWithDataType, ',\n\t_adminid varchar(50), \n\tout _ProcessingResult varchar(100)') 
				into @ProcedureParameterWithDataType;
                
                #Select @ProcedureParameterWithDataType;
			End;
		
			Begin # Insert query paramter

				Select Lower(Group_Concat(' _', column_name)) as ColumnsName from information_schema.columns where 
				table_name = @TblName and Column_key <> 'PRI' into @ProcedureParameterForInsert;
				
				Select Replace(@ProcedureParameterForInsert, '_createdon', 'now()') into @ProcedureParameterForInsert;
				Select Replace(@ProcedureParameterForInsert, '_updatedon', 'NULL') into @ProcedureParameterForInsert;
				Select Replace(@ProcedureParameterForInsert, '_createdby', '_adminid') into @ProcedureParameterForInsert;
				Select Replace(@ProcedureParameterForInsert, '_updatedby', 'NULL') into @ProcedureParameterForInsert;
			
			End;
			
			Begin # Find Condition
			
				Select Concat(Lower(column_name), ' = ', '_', Lower(column_name)) as ConditionColumn from 
                information_schema.columns where 
				table_name = @TblName and Column_key = 'PRI' into @FindCondition;

			End;
			
			Begin # Update query parameter
				
				Select Lower(Group_Concat(' ', Concat(column_name, ' = ', '_', column_name))) as ColumnsName 
				from information_schema.columns where table_name = @TblName and Column_key <> 'PRI' and 
				Lower(column_name) <> 'createdon' and Lower(column_name) <> 'createdby'
                and Lower(column_name) <> 'schooltenentid' into @ProcedureParameterForUpdate;
				
				Select Replace(@ProcedureParameterForUpdate, '_updatedon', 'now()') into @ProcedureParameterForUpdate;
				Select Replace(@ProcedureParameterForUpdate, '_updatedby', '_adminid') into @ProcedureParameterForUpdate;

			End;
			
			Select Replace(QueryText, '{{@ProcedureParameterWithDataType}}', @ProcedureParameterWithDataType) into QueryText;
			Select Replace(QueryText, '{{@ProcedureParameterForInsert}}', @ProcedureParameterForInsert) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@FindCondition}}', @FindCondition) into QueryText;
			Select Replace(QueryText, '{{@ProcedureParameterForUpdate}}', @ProcedureParameterForUpdate) into QueryText;
			
			Insert into FinalTable values(i, @TblName, QueryText);
		End;
        Else if(Lower(_TableName) = Lower(@TblName))then
        Begin
			Begin # Procedure Paramter
			
				Select Lower(Group_Concat(Concat('\n\t_', column_name, ' ', column_type))) as ColumnsName from information_schema.columns where 
				table_name = @TblName and Lower(column_name) <> 'createdon' and Lower(column_name) <> 'updatedon' 
                and Lower(column_name) <> 'createdby' and Lower(column_name) <> 'updatedby'
				into @ProcedureParameterWithDataType;
				
				Select Concat(@ProcedureParameterWithDataType, ',\n\t_adminid varchar(50), \n\tout _ProcessingResult varchar(100)') 
				into @ProcedureParameterWithDataType;
                
                #Select @ProcedureParameterWithDataType;
			End;
		
			Begin # Insert query paramter

				Select Lower(Group_Concat(' _', column_name)) as ColumnsName from information_schema.columns where 
				table_name = @TblName and Column_key <> 'PRI' into @ProcedureParameterForInsert;
				
				Select Replace(@ProcedureParameterForInsert, '_createdon', 'now()') into @ProcedureParameterForInsert;
				Select Replace(@ProcedureParameterForInsert, '_updatedon', 'NULL') into @ProcedureParameterForInsert;
				Select Replace(@ProcedureParameterForInsert, '_createdby', '_adminid') into @ProcedureParameterForInsert;
				Select Replace(@ProcedureParameterForInsert, '_updatedby', 'NULL') into @ProcedureParameterForInsert;
                
                #Select @ProcedureParameterForInsert;
			
			End;
			
			Begin # Find Condition
			
				Select Concat(Lower(column_name), ' = ', '_', Lower(column_name)) as ConditionColumn from 
                information_schema.columns where 
				table_name = @TblName and Column_key = 'PRI' limit 1 into @FindCondition;
                
                #Select @FindCondition;

			End;
			
			Begin # Update query parameter
				
				Select Lower(Group_Concat(' ', Concat(column_name, ' = ', '_', column_name))) as ColumnsName 
				from information_schema.columns where table_name = @TblName and Column_key <> 'PRI' and 
				Lower(column_name) <> 'createdon' and Lower(column_name) <> 'createdby' 
                and Lower(column_name) <> 'schooltenentid' into @ProcedureParameterForUpdate;
				
				Select Replace(@ProcedureParameterForUpdate, '_updatedon', 'now()') into @ProcedureParameterForUpdate;
				Select Replace(@ProcedureParameterForUpdate, '_updatedby', '_adminid') into @ProcedureParameterForUpdate;
                
                #Select @ProcedureParameterForUpdate;

			End;			
            
            Select QueryText;
            
			Select Replace(QueryText, '{{@ProcedureParameterWithDataType}}', @ProcedureParameterWithDataType) into QueryText;
            Select Replace(QueryText, '{{@ProcedureParameterForInsert}}', @ProcedureParameterForInsert) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;
			Select Replace(QueryText, '{{@TableName}}', @TblName) into QueryText;            
			Select Replace(QueryText, '{{@FindCondition}}', @FindCondition) into QueryText;
			Select Replace(QueryText, '{{@ProcedureParameterForUpdate}}', @ProcedureParameterForUpdate) into QueryText;
			
            Select QueryText;
            
			Insert into FinalTable values(i, @TblName, QueryText);
			Set i = n;       
        End;
        End if;
        End if;
        
		Set i = i + 1;
		Set QueryText = '';
	end;
	end while;
    
    Select * from FinalTable;
    Drop table TblDetails;
    Drop table FinalTable;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_generate_current_month_attandance` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_generate_current_month_attandance`(

	_AttendacenMonth Datetime

/*


    Call sp_generate_current_month_attandance('2022-04-01');


*/

)
Begin
	Declare AttendaceData Json; 
    Set @OperationStatus = '';
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
										
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_get', 1, 0, @Result);
		end;
        
        Set AttendaceData = JSON_ARRAY();
        Set @TotalDays = DAY(LAST_DAY(_AttendacenMonth));
        Set @Index = 1;
        Set @StartDate = LAST_DAY(_AttendacenMonth - interval 1 month);
        Set @InputData = '';
        
        While @Index <= @TotalDays Do
        Begin
			Set @StartDate = @StartDate + interval 1 day;
			Set AttendaceData = JSON_ARRAY_APPEND(
				AttendaceData, 
                '$', 
                JSON_OBJECT(
					'AttendanceId', 0,
                    'ClientId', 0,
                    'IsHoliday', 0,
                    'IsOnLeave', 0,
                    'LeaveId', 0,
					'UserTypeId', 1,
					'AttendanceDay', @StartDate,
					'Hours', Case When DAYOFWEEK(@StartDate) = 1 Or DAYOFWEEK(@StartDate) = 7 then 0 Else 480 end,
					'AttendenceStatus', 4,
					'UserComments', '',
					'EmployeeUid', 0
				)
			);
            
            Set @Index = @Index + 1;
        end;
        End while;
        
        Insert into attendance(EmployeeId, UserTypeId, AttendanceDetail, TotalDays, TotalWeekDays, DaysPending, 
        TotalBurnedMinutes, ForYear, ForMonth, SubmittedOn, UpdatedOn, SubmittedBy, UpdatedBy)
		Select
			e.EmployeeUid,
            1,
			AttendaceData,
            @TotalDays,            
            (Select WeekDaysCount(DATE_SUB(_AttendacenMonth, INTERVAL DAYOFMONTH(_AttendacenMonth) - 1 DAY), Last_Day(_AttendacenMonth))),
            0,
            0,
            Year(_AttendacenMonth),
            Month(_AttendacenMonth),
            utc_timestamp(),
            null,
            1,
            null
		from employees e where IsActive = 1;        
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_getAll_Project` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_getAll_Project`(
	   /*

    Call sp_getAll_Project();

*/

    
 
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);

            Call sp_LogException (@Message, '', 'sp_getAll_Project', 1, 0, @Result);
		end;
        
        select * from project;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_GetInfo_ByMobileOrEmail` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetInfo_ByMobileOrEmail`(

/*

	Call sp_GetInfo_ByMobileOrEmail(NULL, 'istiyaq.mi9@gmail.com', 1);
    
    Call sp_GetInfo_ByMobileOrEmail('9100544384', 'istiyaq.mi9@gmail.com', 1);

*/
	_Mobile varchar(20),
	_Email varchar(50),
    _UserTypeId int
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_GetInfo_ByMobileOrEmail', 1, 0, @Result);
			end;
            
			Begin				
				CASE 
					WHEN _UserTypeId = 1
                    Then 
						Begin
							Select e.* from employees e where e.Mobile = _Mobile OR e.Email = _Email;
                        End;
					WHEN _UserTypeId = 2
					Then
						Begin
							Select c.* from clients c where c.MobileNo = _Mobile OR e.Email = _Email;
                        End;
                    WHEN _UserTypeId = 3
					Then
						Begin
							Select c.Name, c.Email_ID, c.Phone_Number from professionalcandidates c where c.Mobile = _Mobile OR c.Email = _Email;
                        End;
					End Case;
			End;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_gstdetail_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_gstdetail_insupd`(
	_gstId bigint,
    _billno varchar(20),
    _gststatus int,
    _paidon datetime,
    _paidby bigint,
    _amount double,
    _fileId bigint,
    out _ProcessingResult varchar(100)
/*	

	Call sp_gstdetail_insupd('employee', 4, '1=1 And  BillForMonth = 1', '', 1, 10);

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
			Call sp_LogException(@Message, '', 'sp_gstdetail_insupd', 1, 0, @Result);
		End;

		Set _ProcessingResult = "";
        Begin
			If not exists(Select 1 from gstdetail where billno = _billno)then
            begin
				Insert into gstdetail 
                Values(
					default,
                    _billno,
					_gststatus,
					_paidon,
					_paidby,
					_amount,
					_fileId
                );
                
                Set _ProcessingResult = "GST status record inserted successfully";
            end;
            else
            begin
				Update gstdetail Set
                    billno			=		_billno,
					gststatus		=		_gststatus,
					paidon			=		_paidon,
					paidby			=		_paidby,
					amount			=		_amount
                Where billno = _billno;
                
                Set _ProcessingResult = "GST status record updated successfully";
            end;
            end if;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_incometax_slab_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_incometax_slab_get`(

/*

	Call sp_incometax_slab_get();

*/
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_incometax_slab_get', 1, 0, @Result);
		end;  
		        
        Select * from incometax_slab;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_insert_byjson` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_insert_byjson`(
 
 /*

	Call sp_insert_byjson(null);    
 
 */
 
    _catagory varchar(100)
 )
Begin
	DECLARE i      INT DEFAULT  0;
	DECLARE jCount INT DEFAULT -1;
 	Set @OperationStatus = '';
 	Begin
 		Declare exit handler for sqlexception
 		Begin
			
 			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
 										@errorno = MYSQL_ERRNO,
 										@errortext = MESSAGE_TEXT;
 										
 			Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
 			Call sp_LogException(@Message, @OperationStatus, 'sp_menu_insupd', 1, 0, @Result);
 		end;

		Drop table if exists tmp;
		CREATE TEMPORARY TABLE tmp( ou_from INT, out_to INT );

		SET @mapJSON = '[{"from":12,"to":0},{"from":11,"to":-1},{"from":1,"to":1},{"a":"teste"}]' ;
		SET jCount = jCount + JSON_LENGTH( @mapJSON, '$');

		WHILE ( i <= jCount ) DO
			INSERT INTO tmp(ou_from , out_to) 
			VALUES( 
				JSON_EXTRACT(@mapJSON, CONCAT( '$[', i, '].from') ), 
                JSON_EXTRACT(@mapJSON, CONCAT( '$[', i, '].to'  ) )
			);
			SET i = i + 1;
		END WHILE;

		SELECT ou_from AS 'from', out_to AS 'to' FROM tmp;
 	End;
 end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_accrual_InsUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_accrual_InsUpdate`(	
	_LeaveAccrualId int,
    _LeavePlanTypeId int,
    _CanApplyEntireLeave bit,
    _IsLeaveAccruedPatternAvail bit,
	_JoiningMonthLeaveDistribution json,
    _ExitMonthLeaveDistribution json,
    _IsLeaveAccruedProrateDefined bit,
	_LeaveDistributionRateOnStartOfPeriod json,
    _LeaveDistributionSequence varchar(45),
    _LeaveDistributionAppliedFrom decimal,
    _IsLeavesProratedForJoinigMonth bit,
    _IsLeavesProratedOnNotice bit,
    _IsNotAllowProratedOnNotice bit,
    _IsNoLeaveOnNoticePeriod bit,
    _IsVaryOnProbationOrExprience bit,
	_IsAccrualStartsAfterJoining bit,
	_IsAccrualStartsAfterProbationEnds bit,
	_AccrualDaysAfterJoining decimal,
	_AccrualDaysAfterProbationEnds decimal,
    _AccrualProrateDetail json,
    _IsImpactedOnWorkDaysEveryMonth bit,
    _WeekOffAsAbsentIfAttendaceLessThen decimal,
    _HolidayAsAbsentIfAttendaceLessThen decimal,    
    _CanApplyForFutureDate bit,
	_IsExtraLeaveBeyondAccruedBalance bit,
    _IsNoExtraLeaveBeyondAccruedBalance bit,
    _NoOfDaysForExtraLeave decimal,
	_IsAccrueIfHavingLeaveBalance bit,
    _AllowOnlyIfAccrueBalanceIsAlleast decimal,
    _IsAccrueIfOnOtherLeave bit,
    _NotAllowIfAlreadyOnLeaveMoreThan decimal,    
    _RoundOffLeaveBalance bit,
    _ToNearestHalfDay bit,
    _ToNearestFullDay bit,
    _ToNextAvailableHalfDay bit,
    _ToNextAvailableFullDay bit,
    _ToPreviousHalfDay bit,    
    _DoesLeaveExpireAfterSomeTime bit,
    _AfterHowManyDays decimal,
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
            Call sp_LogException (@Message, '', 'sp_leave_accrual_InsUpdate', 1, 0, @Result);
		end;


		if not exists (select 1 from leave_accrual where LeaveAccrualId = _LeaveAccrualId ) then
		Begin
			set @leaveTypeId = 0;
			select LeaveAccrualId from leave_accrual order by LeaveAccrualId Desc limit 1 into @leaveTypeId;
			set @leaveTypeId = @leaveTypeId + 1;
			
			Insert into leave_accrual values(
					@leaveTypeId,
					_LeavePlanTypeId,
					_CanApplyEntireLeave,
					_IsLeaveAccruedPatternAvail,
					_JoiningMonthLeaveDistribution,
                    _IsLeaveAccruedProrateDefined,
					_LeaveDistributionRateOnStartOfPeriod,
					_ExitMonthLeaveDistribution,
					_LeaveDistributionSequence,
					_LeaveDistributionAppliedFrom,
					_IsLeavesProratedForJoinigMonth,
					_IsLeavesProratedOnNotice,
					_IsNotAllowProratedOnNotice,
					_IsNoLeaveOnNoticePeriod,
					_IsVaryOnProbationOrExprience,
                    _IsAccrualStartsAfterJoining,
					_IsAccrualStartsAfterProbationEnds,
					_AccrualDaysAfterJoining,
					_AccrualDaysAfterProbationEnds,
					_AccrualProrateDetail,
					_IsImpactedOnWorkDaysEveryMonth,
					_WeekOffAsAbsentIfAttendaceLessThen,
					_HolidayAsAbsentIfAttendaceLessThen,    
					_CanApplyForFutureDate,
					_IsExtraLeaveBeyondAccruedBalance,
					_IsNoExtraLeaveBeyondAccruedBalance,
					_NoOfDaysForExtraLeave,
					_IsAccrueIfHavingLeaveBalance,
					_AllowOnlyIfAccrueBalanceIsAlleast,
					_IsAccrueIfOnOtherLeave,
					_NotAllowIfAlreadyOnLeaveMoreThan,    
					_RoundOffLeaveBalance,
					_ToNearestHalfDay,
					_ToNearestFullDay,
					_ToNextAvailableHalfDay,
					_ToNextAvailableFullDay,
					_ToPreviousHalfDay,    
					_DoesLeaveExpireAfterSomeTime,
					_AfterHowManyDays
				);
				
				set _ProcessingResult = @leaveTypeId;
		end;
		else
		Begin
			update leave_accrual set
							LeavePlanTypeId							=			_LeavePlanTypeId,
							CanApplyEntireLeave						=			_CanApplyEntireLeave,
							IsLeaveAccruedPatternAvail				=			_IsLeaveAccruedPatternAvail,
							JoiningMonthLeaveDistribution			=			_JoiningMonthLeaveDistribution,
							ExitMonthLeaveDistribution				=			_ExitMonthLeaveDistribution,
							IsLeaveAccruedProrateDefined			=			_IsLeaveAccruedProrateDefined,
							LeaveDistributionRateOnStartOfPeriod	=			_LeaveDistributionRateOnStartOfPeriod,
							LeaveDistributionSequence				=			_LeaveDistributionSequence,
							LeaveDistributionAppliedFrom			=			_LeaveDistributionAppliedFrom,
							IsLeavesProratedForJoinigMonth			=			_IsLeavesProratedForJoinigMonth,
							IsLeavesProratedOnNotice				=			_IsLeavesProratedOnNotice,
							IsNotAllowProratedOnNotice				=			_IsNotAllowProratedOnNotice,
							IsNoLeaveOnNoticePeriod					=			_IsNoLeaveOnNoticePeriod,
							IsVaryOnProbationOrExprience			=			_IsVaryOnProbationOrExprience,
							IsAccrualStartsAfterJoining				=			_IsAccrualStartsAfterJoining,
							IsAccrualStartsAfterProbationEnds		=			_IsAccrualStartsAfterProbationEnds,
							AccrualDaysAfterJoining					=			_AccrualDaysAfterJoining,
							AccrualDaysAfterProbationEnds			=			_AccrualDaysAfterProbationEnds,
							AccrualProrateDetail					=			_AccrualProrateDetail,
							IsImpactedOnWorkDaysEveryMonth			=			_IsImpactedOnWorkDaysEveryMonth,
							WeekOffAsAbsentIfAttendaceLessThen		=			_WeekOffAsAbsentIfAttendaceLessThen,
							HolidayAsAbsentIfAttendaceLessThen		=			_HolidayAsAbsentIfAttendaceLessThen,    
							CanApplyForFutureDate					=			_CanApplyForFutureDate,
							IsExtraLeaveBeyondAccruedBalance		=			_IsExtraLeaveBeyondAccruedBalance,
							IsNoExtraLeaveBeyondAccruedBalance		=			_IsNoExtraLeaveBeyondAccruedBalance,
							NoOfDaysForExtraLeave					=			_NoOfDaysForExtraLeave,
							IsAccrueIfHavingLeaveBalance			=			_IsAccrueIfHavingLeaveBalance,
							AllowOnlyIfAccrueBalanceIsAlleast		=			_AllowOnlyIfAccrueBalanceIsAlleast,
							IsAccrueIfOnOtherLeave					=			_IsAccrueIfOnOtherLeave,
							NotAllowIfAlreadyOnLeaveMoreThan		=			_NotAllowIfAlreadyOnLeaveMoreThan,    
							RoundOffLeaveBalance					=			_RoundOffLeaveBalance,
							ToNearestHalfDay						=			_ToNearestHalfDay,
							ToNearestFullDay						=			_ToNearestFullDay,
							ToNextAvailableHalfDay					=			_ToNextAvailableHalfDay,
							ToNextAvailableFullDay					=			_ToNextAvailableFullDay,
							ToPreviousHalfDay						=			_ToPreviousHalfDay,    
							DoesLeaveExpireAfterSomeTime			=			_DoesLeaveExpireAfterSomeTime,
							AfterHowManyDays						=			_AfterHowManyDays
					where 	LeaveAccrualId							=			_LeaveAccrualId;
								
				set _ProcessingResult = _LeaveAccrualId;	
		end;
		end if;
    end;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_apply_detail_InsUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_apply_detail_InsUpdate`(	
	_LeaveApplyDetailId int,
    _LeavePlanTypeId int,
    _IsAllowForHalfDay bit,
    _EmployeeCanSeeAndApplyCurrentPlanLeave bit,
    _RuleForLeaveInNotice json,
    _ApplyPriorBeforeLeaveDate int,
    _BackDateLeaveApplyNotBeyondDays int,
    _RestrictBackDateLeaveApplyAfter int,
    _CurrentLeaveRequiredComments bit,
    _ProofRequiredIfDaysExceeds bit,
    _NoOfDaysExceeded int,
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
            Call sp_LogException (@Message, '', 'sp_leave_apply_detail_InsUpdate', 1, 0, @Result);
		end;


		if not exists (select 1 from leave_apply_detail where LeaveApplyDetailId = _LeaveApplyDetailId ) then
		Begin
			set @leaveApplyDetailId = 0;
			select LeaveApplyDetailId from leave_apply_detail order by LeaveApplyDetailId Desc limit 1 into @leaveApplyDetailId;
			set @leaveApplyDetailId = @leaveApplyDetailId + 1;
            
			Insert into leave_apply_detail values (
				@leaveApplyDetailId,
				_LeavePlanTypeId,
				_IsAllowForHalfDay,
				_EmployeeCanSeeAndApplyCurrentPlanLeave,
				_RuleForLeaveInNotice,
				_ApplyPriorBeforeLeaveDate,
				_BackDateLeaveApplyNotBeyondDays,
				_RestrictBackDateLeaveApplyAfter,
				_CurrentLeaveRequiredComments,
				_ProofRequiredIfDaysExceeds,
				_NoOfDaysExceeded
			);
            
			set _ProcessingResult = @leaveApplyDetailId;
		end;
		else
		Begin
			update leave_apply_detail set
					LeavePlanTypeId							=	_LeavePlanTypeId,
					IsAllowForHalfDay						=	_IsAllowForHalfDay,
					EmployeeCanSeeAndApplyCurrentPlanLeave 	= 	_EmployeeCanSeeAndApplyCurrentPlanLeave,
					RuleForLeaveInNotice					=	_RuleForLeaveInNotice,
					ApplyPriorBeforeLeaveDate				=	_ApplyPriorBeforeLeaveDate,
					BackDateLeaveApplyNotBeyondDays			=	_BackDateLeaveApplyNotBeyondDays,
					RestrictBackDateLeaveApplyAfter			=	_RestrictBackDateLeaveApplyAfter,
					CurrentLeaveRequiredComments			=	_CurrentLeaveRequiredComments,
					ProofRequiredIfDaysExceeds				=	_ProofRequiredIfDaysExceeds,
					NoOfDaysExceeded						=	_NoOfDaysExceeded
			where 	LeaveApplyDetailId						=	_LeaveApplyDetailId;
								
				set _ProcessingResult = _LeaveApplyDetailId;	
		end;
		end if;
    end;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_approval_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_approval_insupd`(
	   /*

	set @result = '';
    Call sp_leave_approval_insupd(0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 ,1, 1, 1, @result);
	select @result;

*/
    _LeaveApprovalId int,
	_LeavePlanTypeId int,
    _IsLeaveRequiredApproval bit,
    _ApprovalLevels int,
    _ApprovalChain json,
    _IsRequiredAllLevelApproval bit,
    _CanHigherRankPersonsIsAvailForAction bit,
    _IsPauseForApprovalNotification bit,
    _IsReportingManageIsDefaultForAction bit,
    out _ProcessingResult varchar(50)
    
 
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, '', 'sp_leave_approval_insupd', 1, 0, @Result);
		end;
        
        set @OperationStatus = '';
        If not exists (Select 1 from leave_approval Where LeaveApprovalId = _LeaveApprovalId) then
		Begin
			set @leaveApprovalId = 0;
			select LeaveApprovalId into @leaveApprovalId from leave_approval order by LeaveApprovalId desc limit 1;
            set @leaveApprovalId = @leaveApprovalId + 1;
            
			Insert into leave_approval Values (
				@leaveApprovalId,
				_LeavePlanTypeId,
				_IsLeaveRequiredApproval,
				_ApprovalLevels,
				_ApprovalChain,
				_IsRequiredAllLevelApproval,
				_CanHigherRankPersonsIsAvailForAction,
				_IsPauseForApprovalNotification,
				_IsReportingManageIsDefaultForAction
			);
			
			Set _ProcessingResult = @leaveApprovalId;
		End;
		Else
		Begin
			Update leave_approval SET 
				LeavePlanTypeId								=				_LeavePlanTypeId,
				IsLeaveRequiredApproval						=				_IsLeaveRequiredApproval,
				ApprovalLevels								=				_ApprovalLevels,
				ApprovalChain								=				_ApprovalChain,
				IsRequiredAllLevelApproval					=				_IsRequiredAllLevelApproval,
				CanHigherRankPersonsIsAvailForAction		=				_CanHigherRankPersonsIsAvailForAction,
				IsPauseForApprovalNotification				=				_IsPauseForApprovalNotification,
				IsReportingManageIsDefaultForAction			=				_IsReportingManageIsDefaultForAction
			Where LeaveApprovalId 							= 				_LeaveApprovalId;
			
			Set _ProcessingResult = _LeaveApprovalId;
		End;
		End if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_detail_getby_employeeId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_detail_getby_employeeId`(
/*
	
    Call sp_leave_detail_getby_employeeId(39);
    
    
*/

	_EmployeeId bigint
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, '', 'sp_leave_page_getby_employeeId', 1, 0, @Result);
		end;

		select * from employees
        where DesignationId = 1 Or DesignationId = 4;		
        
        select l.* from leave_plan l 
        inner join employees e on e.LeavePlanId = l.LeavePlanId or l.IsDefaultPlan = 1
        where EmployeeUid = _EmployeeId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_detail_get_by_id` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_detail_get_by_id`(	
	_LeaveDetailId int
)
Begin
	Declare Exit handler for sqlexception
	Begin
		Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
                                    
		Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
		Call sp_LogException (@Message, '', 'sp_leave_detail_get_by_id', 1, 0, @Result);
	end;
   
   Select * from leave_detail
   where LeaveDetailId = _LeaveDetailId;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_detail_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_detail_insupd`(
	   /*

	set @result = '';
    Call sp_leave_detail_insupd(0, 1, 'Casual Leave', 'Casual leave for employees', 20, @result);
	select @result;

*/

	_LeaveDetailId int,
    _LeavePlanTypeId int,
    _IsLeaveDaysLimit bit,
    _LeaveLimit int,
    _CanApplyExtraLeave bit,
    _ExtraLeaveLimit int,
    _IsNoLeaveAfterDate bit,
    _LeaveNotAllocatedIfJoinAfter int,
	_CanCompoffAllocatedAutomatically bit,
    _CanCompoffCreditedByManager bit,
    out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            RollBack;
            Call sp_LogException (@Message, '', 'sp_leave_detail_insupd', 1, 0, @Result);
		end;

		Start Transaction;
        begin
			If not exists (Select 1 from leave_detail Where LeaveDetailId = _LeaveDetailId) then
			Begin
				set @leaveDetailId = 0;
				select LeaveDetailId from leave_detail order by LeaveDetailId Desc limit 1 into @leaveDetailId;
				set @leaveDetailId = @leaveDetailId + 1;
				
				Insert into leave_detail Values (
					@leaveDetailId,
					_LeavePlanTypeId,
					_IsLeaveDaysLimit,
					_LeaveLimit,
					_CanApplyExtraLeave,
					_ExtraLeaveLimit,
					_IsNoLeaveAfterDate,
					_LeaveNotAllocatedIfJoinAfter,
					_CanCompoffAllocatedAutomatically,
					_CanCompoffCreditedByManager
				);
				
                Set _LeaveDetailId = @leaveDetailId;
				Set _ProcessingResult = @leaveDetailId;
			End;
			Else
			Begin
				Update leave_detail SET 
					LeavePlanTypeId							=				_LeavePlanTypeId,
					IsLeaveDaysLimit						=				_IsLeaveDaysLimit,
					LeaveLimit								=				_LeaveLimit,
					CanApplyExtraLeave						=				_CanApplyExtraLeave,
					ExtraLeaveLimit							=				_ExtraLeaveLimit,
					IsNoLeaveAfterDate						=				_IsNoLeaveAfterDate,
					LeaveNotAllocatedIfJoinAfter			=				_LeaveNotAllocatedIfJoinAfter,
					CanCompoffAllocatedAutomatically		=				_CanCompoffAllocatedAutomatically,
					CanCompoffCreditedByManager				=				_CanCompoffCreditedByManager
				Where LeaveDetailId = _LeaveDetailId;
				
				Set _ProcessingResult = _LeaveDetailId;
			End;
			End if;
			
			Update leave_plans_type set
				MaxLeaveLimit = _LeaveLimit
			where LeavePlanTypeId = _LeavePlanTypeId;
		end;
        Commit;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_detail_InsUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_detail_InsUpdate`(	
	_LeaveDetailId int,
    _LeavePlanId int,
    _LeaveLimit int,
    _CanApplyExtraLeave bit,
    _ExtraLeaveLimit int,
    _LeaveNotAllocatedIfJoinAfter int,
	_CanManagerAwardCausalLeave bit,
	_CanCompoffAllocatedAutomatically bit,
    _CanCompoffCreditedByManager bit,
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
            Call sp_LogException (@Message, '', 'sp_leave_detail_InsUpdate', 1, 0, @Result);
		end;
        Begin
			if not exists (select 1 from leave_detail where LeaveDetailId = _LeaveDetailId ) then
            Begin
				Insert into leave_detail values (
					default,
					_LeavePlanId,
					_LeaveLimit,
					_CanApplyExtraLeave,
					_ExtraLeaveLimit,
					_LeaveNotAllocatedIfJoinAfter,
					_CanManagerAwardCausalLeave,
					_CanCompoffAllocatedAutomatically,
					_CanCompoffCreditedByManager
				);
					set _ProcessingResult = 'inserted';
			end;
            else
            Begin
				update leave_detail set
							LeavePlanId						=	_LeavePlanId,
							LeaveLimit						=	_LeaveLimit,
							CanApplyExtraLeave				=	_CanApplyExtraLeave,
							ExtraLeaveLimit					=	_ExtraLeaveLimit,
							LeaveNotAllocatedIfJoinAfter	=	_LeaveNotAllocatedIfJoinAfter,
							CanManagerAwardCausalLeave		=	_CanManagerAwardCausalLeave,
							CanCompoffAllocatedAutomatically =	_CanCompoffAllocatedAutomatically,
							CanCompoffCreditedByManager		=	_CanCompoffCreditedByManager
				where 		LeaveDetailId					=	_LeaveDetailId;
                                    
					set _ProcessingResult = 'updated';	
			end;
            end if;
		end;
    end;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_endyear_processing_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_endyear_processing_insupd`(
	   /*

	set @result = '';
    Call sp_leave_endyear_processing_insupd(0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 ,1, 1, 1, @result);
	select @result;

*/
    _LeaveEndYearProcessingId int,
	_LeavePlanTypeId int,
	_IsLeaveBalanceExpiredOnEndOfYear bit,
	_AllConvertedToPaid bit,
	_AllLeavesCarryForwardToNextYear bit,
	_PayFirstNCarryForwordRemaning bit,
	_CarryForwordFirstNPayRemaning bit,
	_PayNCarryForwardForPercent bit,
	_PayNCarryForwardDefineType varchar(50),
    _FixedPayNCarryForward json,
	_PercentagePayNCarryForward json,
	_DoestCarryForwardExpired bit,
	_ExpiredAfter decimal,
	_DoesExpiryLeaveRemainUnchange bit,
	_DeductFromSalaryOnYearChange bit,
	_ResetBalanceToZero bit,
	_CarryForwardToNextYear bit,
    out _ProcessingResult varchar(50)
    
 
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, '', 'sp_leave_endyear_processing_insupd', 1, 0, @Result);
		end;
        
        If not exists (Select 1 from leave_endyear_processing Where LeaveEndYearProcessingId = _LeaveEndYearProcessingId) then
		Begin
			set @leaveEndYearProcessingId = 0;
			select LeaveEndYearProcessingId into @leaveEndYearProcessingId from leave_endyear_processing order by LeaveEndYearProcessingId desc limit 1;
            set @leaveEndYearProcessingId = @leaveEndYearProcessingId + 1;
            
			Insert into leave_endyear_processing Values (
				@leaveEndYearProcessingId,
				_LeavePlanTypeId,
				_IsLeaveBalanceExpiredOnEndOfYear,
				_AllConvertedToPaid,
				_AllLeavesCarryForwardToNextYear,
				_PayFirstNCarryForwordRemaning,
				_CarryForwordFirstNPayRemaning,
				_PayNCarryForwardForPercent,
				_PayNCarryForwardDefineType,
				_FixedPayNCarryForward,
				_PercentagePayNCarryForward,
				_DoestCarryForwardExpired,
				_ExpiredAfter,
				_DoesExpiryLeaveRemainUnchange,
				_DeductFromSalaryOnYearChange,
				_ResetBalanceToZero,
				_CarryForwardToNextYear
			);
			
			Set _ProcessingResult = @leaveEndYearProcessingId;
		End;
		Else
		Begin
			Update leave_endyear_processing SET 
				LeavePlanTypeId								=				_LeavePlanTypeId,
				IsLeaveBalanceExpiredOnEndOfYear			=				_IsLeaveBalanceExpiredOnEndOfYear,
				AllConvertedToPaid							=				_AllConvertedToPaid,
				AllLeavesCarryForwardToNextYear				=				_AllLeavesCarryForwardToNextYear,
				PayFirstNCarryForwordRemaning				=				_PayFirstNCarryForwordRemaning,
				CarryForwordFirstNPayRemaning				=				_CarryForwordFirstNPayRemaning,
				PayNCarryForwardForPercent					=				_PayNCarryForwardForPercent,
				PayNCarryForwardDefineType					=				_PayNCarryForwardDefineType,
				FixedPayNCarryForward						=				_FixedPayNCarryForward,
                PercentagePayNCarryForward					=				_PercentagePayNCarryForward,
				DoestCarryForwardExpired					=				_DoestCarryForwardExpired,
				ExpiredAfter								=				_ExpiredAfter,
				DoesExpiryLeaveRemainUnchange				=				_DoesExpiryLeaveRemainUnchange,
				DeductFromSalaryOnYearChange				=				_DeductFromSalaryOnYearChange,
				ResetBalanceToZero							=				_ResetBalanceToZero,
				CarryForwardToNextYear						=				_CarryForwardToNextYear
			Where LeaveEndYearProcessingId 					= 				_LeaveEndYearProcessingId;
			
			Set _ProcessingResult = _LeaveEndYearProcessingId;
		End;
		End if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_from_management_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_from_management_insupd`(
	   /*

	set @result = '';
    Call sp_leave_from_management_insupd(0, 1, 'Casual Leave', 'Casual leave for employees', 20, @result);
	select @result;

*/

	_LeaveManagementId int,
    _LeavePlanTypeId int,
	_CanManagerAwardCausalLeave bit,
    out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            RollBack;
            Call sp_LogException (@Message, '', 'sp_leave_from_management_insupd', 1, 0, @Result);
		end;

		Start Transaction;
        begin
			If not exists (Select 1 from leave_from_management Where LeaveManagementId = _LeaveManagementId) then
			Begin
				set @leaveManagementId = 0;
				select LeaveManagementId from leave_from_management order by LeaveManagementId Desc limit 1 into @leaveManagementId;
				set @leaveManagementId = @leaveManagementId + 1;
				
				Insert into leave_from_management Values (
					@leaveManagementId,
					_LeavePlanTypeId,
					_CanManagerAwardCausalLeave
				);
				
                Set _leaveManagementId = @leaveManagementId;
				Set _ProcessingResult = @leaveManagementId;
			End;
			Else
			Begin
				Update leave_from_management SET 
					LeavePlanTypeId							=				_LeavePlanTypeId,
					CanManagerAwardCausalLeave				=				_CanManagerAwardCausalLeave
				Where LeaveManagementId = _LeaveManagementId;
				
				Set _ProcessingResult = _LeaveManagementId;
			End;
			End if;
			
		end;
        Commit;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_group_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_group_get`(
	   /*

    Call sp_leave_group_get();
    

*/
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_leave_group_get', 1, 0, @Result);
		end;
        
        select * from leave_plan;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_group_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_group_insupd`(
	   /*

	set @result = '';
    Call sp_leave_group_insupd(0, 'Test', 'Test group', @result);
	select @result;

*/
	_LeaveGroupId int,
    _GroupName varchar(50),
    _GroupDescription varchar(250),
    out _ProcessingResult varchar(1000)
    
 
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_leave_group_insupd', 1, 0, @Result);
		end;
        
        If not exists (Select 1 from leave_group Where LeaveGroupId = _LeaveGroupId) then
		Begin
			Insert into leave_group Values (
				default,
				_GroupName,
				_GroupDescription
			);
			
			Set _ProcessingResult = 'inserted';
		End;
		Else
		Begin
			Update leave_group SET 
				GroupName			=	_GroupName,
				GroupDescription	=	_GroupDescription
			Where LeaveGroupId = _LeaveGroupId;
			
			Set _ProcessingResult = 'updated';
		End;
		End if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_holidays_and_weekoff_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_holidays_and_weekoff_insupd`(
	   /*

	set @result = '';
    Call sp_leave_holidays_and_weekoff_insupd(0, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, @result);
	select @result;

*/
	_LeaveHolidaysAndWeekOffId int,
    _LeavePlanTypeId int,
    _AdJoiningHolidayIsConsiderAsLeave bit,
    _ConsiderLeaveIfNumOfDays decimal,
    _IfLeaveLieBetweenTwoHolidays bit,
    _IfHolidayIsRightBeforLeave bit,
    _IfHolidayIsRightAfterLeave bit,
    _IfHolidayIsBetweenLeave bit,
    _IfHolidayIsRightBeforeAfterOrInBetween bit,
    _AdjoiningHolidayRulesIsValidForHalfDay bit,
    _AdjoiningWeekOffIsConsiderAsLeave bit,
    _ConsiderLeaveIfIncludeDays decimal,
    _IfLeaveLieBetweenWeekOff bit,
    _IfWeekOffIsRightBeforLeave bit,
    _IfWeekOffIsRightAfterLeave bit,
    _IfWeekOffIsBetweenLeave bit,
    _IfWeekOffIsRightBeforeAfterOrInBetween bit,
    _AdjoiningWeekOffRulesIsValidForHalfDay bit,
    _ClubSandwichPolicy bit,
    out _ProcessingResult varchar(50)
    
 
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, '', 'sp_leave_holidays_and_weekoff_insupd', 1, 0, @Result);
		end;
        
        If not exists (Select 1 from leave_holidays_and_weekoff Where LeaveHolidaysAndWeekOffId = _LeaveHolidaysAndWeekOffId) then
		Begin
			set @leaveHolidaysAndWeekOffId = 0;
			select LeaveHolidaysAndWeekOffId from leave_holidays_and_weekoff order by LeaveHolidaysAndWeekOffId Desc limit 1 into @leaveHolidaysAndWeekOffId;
			set @leaveHolidaysAndWeekOffId = @leaveHolidaysAndWeekOffId + 1;
            
			Insert into leave_holidays_and_weekoff Values (
					@leaveHolidaysAndWeekOffId,
					_LeavePlanTypeId,
					_AdJoiningHolidayIsConsiderAsLeave,
					_ConsiderLeaveIfNumOfDays,
					_IfLeaveLieBetweenTwoHolidays,
					_IfHolidayIsRightBeforLeave,
					_IfHolidayIsRightAfterLeave,
					_IfHolidayIsBetweenLeave,
					_IfHolidayIsRightBeforeAfterOrInBetween,
					_AdjoiningHolidayRulesIsValidForHalfDay,
					_AdjoiningWeekOffIsConsiderAsLeave,
					_ConsiderLeaveIfIncludeDays,
					_IfLeaveLieBetweenWeekOff,
					_IfWeekOffIsRightBeforLeave,
					_IfWeekOffIsRightAfterLeave,
					_IfWeekOffIsBetweenLeave,
					_IfWeekOffIsRightBeforeAfterOrInBetween,
					_AdjoiningWeekOffRulesIsValidForHalfDay,
					_ClubSandwichPolicy
			);
			
			Set _ProcessingResult = @leaveHolidaysAndWeekOffId;
		End;
		Else
		Begin
			Update leave_holidays_and_weekoff SET 
					AdJoiningHolidayIsConsiderAsLeave				=			_AdJoiningHolidayIsConsiderAsLeave,
                    LeavePlanTypeId									=			_LeavePlanTypeId,
					ConsiderLeaveIfIncludeDays						=			_ConsiderLeaveIfIncludeDays,
                    ConsiderLeaveIfNumOfDays						=			_ConsiderLeaveIfNumOfDays,
					IfLeaveLieBetweenTwoHolidays					=			_IfLeaveLieBetweenTwoHolidays,
					IfHolidayIsRightBeforLeave						=			_IfHolidayIsRightBeforLeave,
					IfHolidayIsRightAfterLeave						=			_IfHolidayIsRightAfterLeave,
					IfHolidayIsBetweenLeave							=			_IfHolidayIsBetweenLeave,
					IfHolidayIsRightBeforeAfterOrInBetween			=			_IfHolidayIsRightBeforeAfterOrInBetween,
					AdjoiningHolidayRulesIsValidForHalfDay			=			_AdjoiningHolidayRulesIsValidForHalfDay,
					AdjoiningWeekOffIsConsiderAsLeave				=			_AdjoiningWeekOffIsConsiderAsLeave,
					IfLeaveLieBetweenWeekOff						=			_IfLeaveLieBetweenWeekOff,
					IfWeekOffIsRightBeforLeave						=			_IfWeekOffIsRightBeforLeave,
					IfWeekOffIsRightAfterLeave						=			_IfWeekOffIsRightAfterLeave,
					IfWeekOffIsBetweenLeave							=			_IfWeekOffIsBetweenLeave,
					IfWeekOffIsRightBeforeAfterOrInBetween			=			_IfWeekOffIsRightBeforeAfterOrInBetween,
					AdjoiningWeekOffRulesIsValidForHalfDay			=			_AdjoiningWeekOffRulesIsValidForHalfDay,
					ClubSandwichPolicy								=			_ClubSandwichPolicy
			Where 	LeaveHolidaysAndWeekOffId 						= 			_LeaveHolidaysAndWeekOffId;
			
			Set _ProcessingResult = _LeaveHolidaysAndWeekOffId;
		End;
		End if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plans_and_type_get_by_id` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plans_and_type_get_by_id`(
/*
	
    Call sp_leave_plans_get();
    
*/

)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_leave_plans_and_type_get_by_id', 1, 0, @Result);
		end;
        
		select * from leave_plan;
        select * from leave_plans_type;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plans_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plans_get`(
/*
	
    Call sp_leave_plans_get();
    
*/

)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_leave_plans_get', 1, 0, @Result);
		end;
        
		select * from leave_plan;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plans_getbyId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plans_getbyId`(
/*
	
    Call sp_leave_plans_getbyId(1);
    
*/

	_LeavePlanId int
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_leave_plans_getbyId', 1, 0, @Result);
		end;
        
		select * from leave_plan
        where LeavePlanId = _LeavePlanId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plans_type_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plans_type_get`(
/*
	
    Call sp_leave_plans_type_get();
    

*/
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_leave_plans_type_get', 1, 0, @Result);
		end;
        
		select * from leave_plans_type;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plans_type_getbyId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plans_type_getbyId`(
/*
	
    Call sp_leave_plans_type_getbyId(1);
    
*/

	_LeavePlanTypeId int
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_leave_plans_type_getbyId', 1, 0, @Result);
		end;
        
		select * from leave_plans_type
        where LeavePlanTypeId = _LeavePlanTypeId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plans_type_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plans_type_insupd`(
	   /*

	set @result = '';
    Call sp_leave_plans_type_insupd(0, 1, 'Casual Leave', 'Casual leave for employees', 20, @result);
	select @result;

*/
	_LeavePlanTypeId int,
    _LeavePlanCode varchar(10),
    _PlanName varchar(50),
    _PlanDescription varchar(250),
    _MaxLeaveLimit int,
    _ShowDescription bit,
    _IsPaidLeave bit,
    _IsSickLeave bit,
    _IsStatutoryLeave bit,
    _IsMale bit,
    _IsMarried bit,
    _IsRestrictOnGender bit,
    _IsRestrictOnMaritalStatus bit,
    _Reasons json,
    _PlanConfigurationDetail json,
	_AdminId bigint,	
    out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, '', 'sp_leave_plans_type_insupd', 1, 0, @Result);
		end;
        
		If not exists (Select 1 from leave_plans_type Where LeavePlanTypeId = _LeavePlanTypeId) then
		Begin
			Insert into leave_plans_type Values (
				default,
                _LeavePlanCode,
				_PlanName,
				_PlanDescription,
				_MaxLeaveLimit,
                _ShowDescription,
				_IsPaidLeave,
				_IsSickLeave ,
				_IsStatutoryLeave,
				_IsMale,
				_IsMarried,
				_IsRestrictOnGender,
				_IsRestrictOnMaritalStatus,
				_Reasons,
                _PlanConfigurationDetail,
                _AdminId,
                null,
                now(),
                null
			);
            
            Set _ProcessingResult = 'inserted';
		End;
		Else
		Begin
			Update leave_plans_type SET 
				LeavePlanCode				=				_LeavePlanCode,
				PlanName					=				_PlanName,
				PlanDescription				=				_PlanDescription,
				MaxLeaveLimit				=				_MaxLeaveLimit,
                ShowDescription				=				_ShowDescription,
				IsPaidLeave					=				_IsPaidLeave,
				IsSickLeave					=				_IsSickLeave,
				IsStatutoryLeave			=				_IsStatutoryLeave,
				IsMale						=				_IsMale,
				IsMarried					=				_IsMarried,
				IsRestrictOnGender			=				_IsRestrictOnGender,
				IsRestrictOnMaritalStatus	=				_IsRestrictOnMaritalStatus,
				Reasons						=				_Reasons,
                PlanConfigurationDetail		=				_PlanConfigurationDetail,
                UpdatedBy					=				_AdminId,
                UpdatedOn					=				now()
			Where LeavePlanTypeId = _LeavePlanTypeId;
			
			Set _ProcessingResult = 'updated';
		End;
		End if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plan_calculation_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plan_calculation_get`(
	_EmployeeId int,
    _IsActive int,
    _Year int
/*	

	Call sp_leave_plan_calculation_get(4, 1, 2022);

*/

)
Begin
    Begin
		Declare exit handler for sqlexception
		Begin
		
			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
										
			Drop table if exists employeeDetail;
            
			Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
			Call sp_LogException(@Message, '', 'sp_leave_plan_calculation_get', 1, 0, @Result);
		End;
        
        Create TEMPORARY table employeeDetail(
			EmployeeUid bigint, 
			FirstName varchar(50),
			LastName varchar(50),
			Mobile varchar(20),
			Email varchar(100),
			LeavePlanId int,
			PayrollGroupId int,
			SalaryGroupId int,
			CompanyId int,
			NoticePeriodId int,
			SecondaryMobile varchar(20),
			Gender bit,
			Address varchar(100),
			IsPermanent bit,
			ActualPackage float(10,2),
			FinalPackage float(10, 2),
			TakeHomeByCandidate float(10, 2),
			DateofSalary datetime,
			PerMonthSalary decimal(10,2),
			PaySlip_PDF_Filepath varchar(250),
			PaySlipNo int,
			GeneratedOn datetime,
			EmpProfDetailUid bigint,
			ExprienceInYear float(5,2),
			Specification varchar(250),
			PANNo varchar(20),
			AadharNo varchar(20),
			AccountNumber varchar(50),
			BankName varchar(100),
			BranchName varchar(100),
			Domain varchar(250),
			IFSCCode varchar(20),
			LastCompanyName varchar(100),
			AccessLevelId bigint,
			UserTypeId int,
			CreatedOn datetime,
			ProfessionalDetail_Json json     
        );

        Begin			
            if(_IsActive = 1) then
            begin	
				Insert into employeeDetail
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
				Insert into employeeDetail
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
				Insert into employeeDetail
				Select 
					e.EmployeeUid, 
					e.FirstName,
					e.LastName,
					e.Mobile,
					e.Email,
                    e.LeavePlanId,
                    e.PayrollGroupId,
                    e.NoticePeriodAppliedOn,
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
        
        Select * from employeeDetail;

        Set @leavetypeJson = '';
		Select 
			json_extract(p.AssociatedPlanTypes, '$[*].LeavePlanTypeId') into @leavetypeJson
		from leave_plan p
        where LeavePlanId = (Select LeavePlanId from employeeDetail)
		Or IsDefaultPlan = 1;
        
        Select p.* from
			json_table(
				@leavetypeJson, 
				'$[*]' columns(
					LeavePlanTypeId int PATH '$[0]'
				)
			) as T
            
		inner join leave_plans_type p on p.LeavePlanTypeId = T.LeavePlanTypeId
		where T.LeavePlanTypeId > 0;
        
		select * from employee_leave_request
		where EmployeeId = _EmployeeId and 
		`Year` = _Year;


        select * from company_setting
        where CompanyId = (Select CompanyId from employeeDetail);      
        
		Select 
			p.LeavePlanId,
            p.CompanyId,
            p.PlanName,
            p.PlanDescription,
            p.PlanStartCalendarDate,
            p.IsDefaultPlan,
            p.IsShowLeavePolicy,
            p.IsUploadedCustomLeavePolicy,
            p.CanApplyEntireLeave
        from leave_plan p
        where LeavePlanId = (Select LeavePlanId from employeeDetail)
		Or IsDefaultPlan = 1;
        
        Drop table if exists employeeDetail;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plan_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plan_insupd`(
	   /*

	set @result = '';
    Call sp_leave_plan_insupd(0, 'Test', 'Test group', @result);
	select @result;

*/
	_LeavePlanId int,
    _CompanyId int,
    _PlanName varchar(50),
    _PlanDescription varchar(250),
    _AssociatedPlanTypes Json,
    _PlanStartCalendarDate datetime,
    _IsShowLeavePolicy bit,
    _IsUploadedCustomLeavePolicy bit,
    _IsDefaultPlan bit,
    _CanApplyEntireLeave bit,
    out _ProcessingResult varchar(50)
    
 
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_leave_plan_insupd', 1, 0, @Result);
		end;
        
        If not exists (Select 1 from leave_plan Where LeavePlanId = _LeavePlanId) then
		Begin         
			Insert into leave_plan Values (
				default,
                _CompanyId,
				_PlanName,
				_PlanDescription,
                _AssociatedPlanTypes,
				_PlanStartCalendarDate,
				_IsShowLeavePolicy,
				_IsUploadedCustomLeavePolicy,
                _IsDefaultPlan,
                _CanApplyEntireLeave
			);
			
			Set _ProcessingResult = 'inserted';
		End;
		Else
		Begin
			Update leave_plan SET 
				PlanName						=				_PlanName,
				PlanDescription					=				_PlanDescription,
				PlanStartCalendarDate			=				_PlanStartCalendarDate,
				IsShowLeavePolicy				=				_IsShowLeavePolicy,
				IsUploadedCustomLeavePolicy		=				_IsUploadedCustomLeavePolicy,
                AssociatedPlanTypes				=				_AssociatedPlanTypes,
                IsDefaultPlan					= 				_IsDefaultPlan,
                CanApplyEntireLeave				=				_CanApplyEntireLeave
			Where LeavePlanId = _LeavePlanId;
			
			Set _ProcessingResult = 'updated';
		End;
		End if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plan_restriction_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plan_restriction_insupd`(
	   /*

	set @result = '';
    Call sp_leave_plan_restriction_insupd(0, 2, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, @result);
	select @result;

*/
	_LeavePlanRestrictionId int,
    _LeavePlanId int,
    _CanApplyAfterProbation bit,
    _CanApplyAfterJoining bit,
    _DaysAfterProbation int,
    _DaysAfterJoining int,
    _IsAvailRestrictedLeavesInProbation bit,
    _LeaveLimitInProbation decimal,    
    _IsLeaveInNoticeExtendsNoticePeriod bit,
    _NoOfTimesNoticePeriodExtended decimal,    
    _CanManageOverrideLeaveRestriction bit,    
    _GapBetweenTwoConsicutiveLeaveDates decimal,
    _LimitOfMaximumLeavesInCalendarYear decimal,
    _LimitOfMaximumLeavesInCalendarMonth decimal,    
    _LimitOfMaximumLeavesInEntireTenure decimal,
    _MinLeaveToApplyDependsOnAvailable decimal,
    _AvailableLeaves decimal,    
    _RestrictFromDayOfEveryMonth int,    
    _IsCurrentPlanDepnedsOnOtherPlan bit,
    _AssociatedPlanTypeId int,
    _IsCheckOtherPlanTypeBalance bit,
    _DependentPlanTypeId int,
    out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, '', 'sp_leave_plan_restriction_insupd', 1, 0, @Result);
		end;
        
        If not exists (Select 1 from leave_plan_restriction Where LeavePlanRestrictionId = _LeavePlanRestrictionId) then
		Begin
			set @leavePlanRestrictionId = 0;
			select LeavePlanRestrictionId from leave_plan_restriction order by LeavePlanRestrictionId Desc limit 1 into @leavePlanRestrictionId;
			set @leavePlanRestrictionId = @leavePlanRestrictionId + 1;
            
			Insert into leave_plan_restriction Values (
				@leavePlanRestrictionId,
				_LeavePlanId,
				_CanApplyAfterProbation,
				_CanApplyAfterJoining,
				_DaysAfterProbation,
				_DaysAfterJoining,
				_IsAvailRestrictedLeavesInProbation,
				_LeaveLimitInProbation,
				_IsLeaveInNoticeExtendsNoticePeriod,
				_NoOfTimesNoticePeriodExtended,    
				_CanManageOverrideLeaveRestriction,    
				_GapBetweenTwoConsicutiveLeaveDates,
				_LimitOfMaximumLeavesInCalendarYear,
				_LimitOfMaximumLeavesInCalendarMonth,    
				_LimitOfMaximumLeavesInEntireTenure,
				_MinLeaveToApplyDependsOnAvailable,
				_AvailableLeaves,
				_RestrictFromDayOfEveryMonth,    
				_IsCurrentPlanDepnedsOnOtherPlan,
				_AssociatedPlanTypeId,
				_IsCheckOtherPlanTypeBalance,
				_DependentPlanTypeId
			);
			
			Set _ProcessingResult = @leavePlanRestrictionId;
		End;
		Else
		Begin
			Update leave_plan_restriction SET 
				LeavePlanId										=						_LeavePlanId,
				CanApplyAfterProbation							=						_CanApplyAfterProbation,
				CanApplyAfterJoining							=						_CanApplyAfterJoining,
				DaysAfterProbation								=						_DaysAfterProbation,
				DaysAfterJoining								=						_DaysAfterJoining,
				IsAvailRestrictedLeavesInProbation				=						_IsAvailRestrictedLeavesInProbation,
				LeaveLimitInProbation							=						_LeaveLimitInProbation,
				IsLeaveInNoticeExtendsNoticePeriod				=						_IsLeaveInNoticeExtendsNoticePeriod,
				NoOfTimesNoticePeriodExtended					=						_NoOfTimesNoticePeriodExtended,
				CanManageOverrideLeaveRestriction				=						_CanManageOverrideLeaveRestriction,
				GapBetweenTwoConsicutiveLeaveDates				=						_GapBetweenTwoConsicutiveLeaveDates,
				LimitOfMaximumLeavesInCalendarYear				=						_LimitOfMaximumLeavesInCalendarYear,
				LimitOfMaximumLeavesInCalendarMonth				=						_LimitOfMaximumLeavesInCalendarMonth,    
				LimitOfMaximumLeavesInEntireTenure				=						_LimitOfMaximumLeavesInEntireTenure,
				MinLeaveToApplyDependsOnAvailable				=						_MinLeaveToApplyDependsOnAvailable,
				AvailableLeaves									=						_AvailableLeaves,
				RestrictFromDayOfEveryMonth						=						_RestrictFromDayOfEveryMonth,    
				IsCurrentPlanDepnedsOnOtherPlan					=						_IsCurrentPlanDepnedsOnOtherPlan,
				AssociatedPlanTypeId							=						_AssociatedPlanTypeId,
				IsCheckOtherPlanTypeBalance						=						_IsCheckOtherPlanTypeBalance,
				DependentPlanTypeId								=						_DependentPlanTypeId
			Where 	LeavePlanRestrictionId 						= 						_LeavePlanRestrictionId;
			
			Set _ProcessingResult = _LeavePlanRestrictionId;
		End;
		End if;
        commit;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plan_set_default` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plan_set_default`(
/*
	set @result = '';
    Call sp_leave_plan_set_default(1, false, @result);
    select @result;
    
*/

	_LeavePlanId int,
    _IsDefaultPlan bit,
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
            Call sp_LogException (@Message, '', 'sp_leave_plan_set_default', 1, 0, @Result);
		end;
        start transaction;
			SET SQL_SAFE_UPDATES = 0;
			Update leave_plan Set
				IsDefaultPlan 		=			false;
			SET SQL_SAFE_UPDATES = 1;
			
            Update leave_plan Set
				IsDefaultPlan 		=			_IsDefaultPlan
			where LeavePlanId 		= 			_LeavePlanId;
			
			set _ProcessingResult = 'updated';
        commit;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_leave_plan_upd_configuration` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plan_upd_configuration`(
/*
	
    Call sp_leave_plan_upd_configuration(1, '[]');
    
*/

	_LeavePlanTypeId int,
    _LeavePlanId int,
    _LeavePlanConfiguration json,
    _AssociatedPlanTypes json,
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
            Call sp_LogException (@Message, @OperationStatus, 'sp_leave_plan_upd_configuration', 1, 0, @Result);
		end;
        
		Update leave_plans_type Set
			PlanConfigurationDetail 		=			_LeavePlanConfiguration
		where LeavePlanTypeId 				= 			_LeavePlanTypeId;
        
        Update leave_plan set
			AssociatedPlanTypes = _AssociatedPlanTypes
		where LeavePlanId = _LeavePlanId;
        
        set _ProcessingResult = 'updated';
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_LogException` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_LogException`(

/*

	Set @Result = '';
    Call sp_LogException('ERROR 1054 (42S22): Unknown column updateby in field list', 'Error', 'procedure', 1, 0, @Result);
    Select @Result;


*/

    _StackTrace text,
    _OperationStatus text,
    _MethodFullyQualifiedName varchar(100),
    _IsProcedureException bit,
    _IsCodeException bit,
    out _ProcessingResult varchar(100)
)
Begin

	Set @Message = _StackTrace;
	if(@Message is null or @Message = '')then
	Begin
		Set @Message = Concat(_MethodFullyQualifiedName, ' insert update error');
        Select @Message;
	End;
	end if;

	Set @UUid:= (CAST(UNIX_TIMESTAMP(CURTIME(3)) * 1000 AS unsigned)); -- generate epoc time in ms
	Insert into iexception values(@UUid, Concat(_OperationStatus, @Message), _MethodFullyQualifiedName, now(), 
								  _IsProcedureException, _IsCodeException);
	
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_ManageEmployeeDetail_Get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_ManageEmployeeDetail_Get`(
	_employeeId bigint
/*	

	Call SP_ManageEmployeeDetail_Get(6);

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
                e.CompanyId,
                e.CreatedOn,
                ep.DOB,
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
				e.LeavePlanId
			from employees e 
			Inner Join employeepersonaldetail ep on e.EmployeeUid = ep.EmployeeUid
			Inner Join employeeprofessiondetail ef on e.EmployeeUid = ef.EmployeeUid
			Where e.EmployeeUid = _employeeId;
					
			Select * from employeemappedclients 
			where EmployeeUid = _employeeId; #and IsActive = 1;
		
			Select FileId, FilePath, FileName, FileExtension, UserTypeId from userfiledetail 
			where FileOwnerId = _employeeId and FileName = 'profile';            
            
            Call sp_employee_salary_detail_get_by_empid(_employeeId);
            
            /*select c. from company c
            Inner join employeelogin e
            on c.CompanyId = e.CompanyId
            where e.EmployeeId = _employeeId;*/
            
            select * from clients;
            
			select * from employees
            where DesignationId = 1 Or DesignationId = 4;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_MappedClients_Get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_MappedClients_Get`(
	_EmployeeId bigint
/*	

	Call SP_MappedClients_Get(4);

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
			Call sp_LogException(@Message, '', 'SP_MappedClients_Get', 1, 0, @Result);
		End;

        Begin
			Select * from employeemappedclients
            where EmployeeUid = _EmployeeId;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_menu_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_menu_insupd`(
 
 /*

	Set @result = '';
	Call sp_menu_insupd('Profile', 'Manage', 'profile', 'fa fa-user', null, null, @result);    
    Select @result;
    
	Set @result = '';
	Call sp_menu_insupd('Accounts', null, null, null, null, null, @result);    
    Select @result;
 
 */
 
    _catagory varchar(100),
    _childs varchar(100),
    _link varchar(250),
    _icon varchar(100),
    _badge varchar(100),
    _badgeType varchar(100),
    out _ProcessingResult varchar(100)
 )
Begin
 	Set @OperationStatus = '';
 	Begin
 		Declare exit handler for sqlexception
 		Begin
			
 			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
 										@errorno = MYSQL_ERRNO,
 										@errortext = MESSAGE_TEXT;
 										
 			Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
            RollBack;
            SET autocommit = 1;
 			Call sp_LogException(@Message, @OperationStatus, 'sp_menu_insupd', 1, 0, @Result);
 		end;

		SET autocommit = 0;
		Start Transaction;
        Begin
			Set @AccessCode = 0;
            Select count(1) from rolesandmenu into @AccessCode;    
			           
			-- check if current catagory is not child of any other catagory
			if (_childs is null) then
			Begin
				-- check catagory name not exists in database.
				if not exists(Select 1 from rolesandmenu where Lower(Catagory) = Lower(_catagory)) then
				begin
					Set @AccessCode = @AccessCode + 1;
					Insert into rolesandmenu values(_catagory, null, null, null, null, null, @AccessCode);
					
					Set _ProcessingResult = 'Inserted successfully';
				end;
				end if;					
			End;
			Else
			Begin
				if not exists(Select 1 from rolesandmenu where Lower(Catagory) = Lower(_catagory) and lower(Childs) = lower(_childs)) then
				Begin
					Set @AccessCode = @AccessCode + 1;
					Insert into rolesandmenu values(_catagory, _childs, _link, _icon, _badge, _badgeType, @AccessCode);				
					-- INSERT INTO role_accessibility_mapping values(default, 1, @AccessCode);
					
					Set _ProcessingResult = 'Inserted successfully';
				End;
				Else
				Begin
					SET sql_safe_updates = 0;
					Update rolesandmenu Set Childs = _childs, Link = _link, Icon = _icon, 
					Badge = _badge, BadgeType = _badgeType 
					where Lower(Catagory) = Lower(_catagory) && Childs is not null;
					
					Set _ProcessingResult = 'Updated successfully';
				End;
				End if;
			End;
			End if;
		End;
        
        Commit;
		SET autocommit = 1;
        SET sql_safe_updates = 1;
 	End;
 end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_new_registration` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_new_registration`(

/*
	set @value = '';
	Call sp_new_registration('BOTTOMHALF PVT.LTD', 'BOTTOMHALF', '9000000000','istiyaq.mi9@gmail.com', 'ADMIN', 'USER', "EiOcNOYYuHiQrEc0z16rEjUlp71vYq73fNDXL1PbZZ4=", @value);
    select @value;

*/
	_OrganizationName varchar(250),
	_CompanyName varchar(250),
	_Mobile varchar(20),
	_EmailId varchar(100),
    _FirstName varchar(100),
    _LastName varchar(100),
	_Password varchar(250),
    out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
			@errorno = MYSQL_ERRNO,
			@errortext = MESSAGE_TEXT;
            
            RollBack;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_new_registration', 1, 0, @Result);
		end;  
        
        Start Transaction;
        begin
			if not exists(select 1 from organization_detail where OrganizationName = _OrganizationName) then            
			begin
				set @userTypeId = 0;
				set @accessLevelId = 1;
				select UserTypeId into @userTypeId from usertypedetail
				where RoleName = 'Admin';
				
				set @designationId = 0;
				select DesignationId into @designationId from designation_detail
				where DesignationName = 'Admin';
			
				set @organizationId = 0;
				select OrganizationId into @organizationId from organization_detail
                order by OrganizationId desc;
                set @organizationId = @organizationId + 1;                
              
				Insert into organization_detail values(
					@organizationId,
					_OrganizationName,
					_Mobile,
					_EmailId,
					_Mobile,
					null,
					null,
					0,
                    null,
					utc_date(),
                    null
				);            
				
				set @companyId = 0;
				select CompanyId into @companyId from company
                order by CompanyId desc;
                set @companyId = @companyId + 1;   

				Insert into company values(            
					@companyId,
					@organizationId,
					_OrganizationName,
					_CompanyName,
					null,
					0,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					_Mobile,
					_EmailId,
					null,
					null,
					null,
					null,
					_Mobile,
					null,
					null,
					0,
					0,
					null,
					null,
					null,
					null,
					null,
                    null,
                    null,
					true,
					null,
					0,
					null,
					utc_date(),
					null
				);
				
				set @employeeId = 0;
				select EmployeeUid into @employeeId from employees
                order by EmployeeUid desc;
                set @employeeId = @employeeId + 1;

				Insert into employees values(
					@employeeId,
					_FirstName,
					_LastName,
					_Mobile,
					_EmailId,
					true,
					0,
					null,
					utc_date(),
					null,
					0,
					@designationId,
					@userTypeId,
					0,
					0,
                    0,
					@companyId,
					0
				);

                Insert into employeeprofessiondetail values (
					default,
                    @employeeId,
                    _FirstName,
					_LastName,
					_Mobile,
                    null,
					_EmailId,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    0,
                    null,
                    utc_date(),
                    null,
                    null
                );

                Insert into employeepersonaldetail values (
					default,
					@employeeId,
                    _Mobile,
                    null,
                    _EmailId,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    true,
                    null,
                    null,
                    null,
                    0,
                    null,
                    utc_date(),
                    null,
                    null
                );

				Insert into employeelogin values(
					default,
					@employeeId,
					@userTypeId,
					@accessLevelId,
					_Password,
					_EmailId,
					_Mobile,
					@organizationId,
					@companyId,
					0,
					null,
					utc_date(),
					null
				);
                
				insert into bank_accounts values (
					default,
                    @organizationId,
                    @companyId,
					null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    true,
                    0,
                    null,
                    utc_date(),
                    null
                );
				set _ProcessingResult = 'updated';
			end;
			end if;
        end;
        Commit;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_OnlineDocument_Get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_OnlineDocument_Get`(
	_searchString varchar(250),
	_sortBy varchar(50),
	_pageIndex int,
	_pageSize int
/*	

	CAll SP_OnlineDocument_Get('1=1 AND UD.MOBILENO = '''' OR UD.EMAILID = ''zaid2292@gmail.com''', null, 1, 10)

	CAll SP_OnlineDocument_Get('1=1', null, 1, 10)

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
			Call sp_LogException(@Message, '', 'SP_OnlineDocument_Get', 1, 0, @Result);
		End;

        Begin
			If(_sortBy is NULL OR _sortBy = '') then
				Set _sortBy = 'CreatedOn, UpdatedOn Desc';
			End if;
            Set @SelectQuery = Concat('Select DocumentId, Title, Description, UserId, FilePath, CreatedOn, Total from (
					Select ROW_NUMBER() Over(Order by ', _sortBy, ') RowIndex,
                    d.DocumentId,
                    d.UserId,
                    d.Title,
                    d.Description,
                    d.FilePath,
                    d.CreatedOn,
                    Count(1) Over() as Total 
                    from documents d
					Inner Join UserDetail ud on ud.UserId = d.UserId
					Where ', _SearchString, '
				)T Where RowIndex between ', ((_PageIndex -1 ) * _PageSize) + 1 ,' And ', (_PageIndex * _PageSize), ''
			);
            
		Select @SelectQuery;
		prepare SelectQuery from @SelectQuery;
		execute SelectQuery;	
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_organization_detail_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_organization_detail_get`(
    
/*

	Call sp_organization_detail_get();
    
*/
	
)
Begin
    Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
    		Call sp_LogException (@Message, '', 'sp_organization_detail_get', 1, 0, @Result);
		end;  
        
		select 
			c.CompanyId,
			o.OrganizationId,
			o.OrganizationName,
			c.CompanyName,
			c.CompanyDetail,
			c.SectorType,
			c.Country,
			c.State,
			c.City,
			c.FirstAddress,
			c.SecondAddress,
			c.ThirdAddress,
			c.ForthAddress,
			c.FullAddress,
			c.MobileNo,
			c.Email,
			c.FirstEmail,
			c.SecondEmail,
			c.ThirdEmail,
			c.ForthEmail,
			c.PrimaryPhoneNo,
			c.SecondaryPhoneNo,
			c.Fax,
			c.Pincode,
			c.FileId,
			c.LegalDocumentPath,
			c.LegalEntity,
			c.TypeOfBusiness,
			c.InCorporationDate,
			c.IsPrimaryCompany,
			c.FixedComponentsId,
			b.BankAccountId,
			b.BankName,
			b.BranchCode,
			b.Branch,
			b.IFSC,
			b.AccountNo,
			b.OpeningDate,
			b.ClosingDate,
			c.PANNo,
			c.GSTNo,
			c.TradeLicenseNo,
			o.OrgMobileNo,
			o.OrgEmail,
			o.OrgPrimaryPhoneNo,
			o.OrgSecondaryPhoneNo,
			o.OrgFax
        from organization_detail o
        inner join company c on o.OrganizationId = c.OrganizationId
        inner join bank_accounts b on b.OrganizationId = c.OrganizationId;

        select * from filedetail
        where UserTypeId = 6;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_organization_detail_intupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_organization_detail_intupd`(
	_OrganizationId int,
	_OrganizationName varchar(250),
    _SectorType int,
    _Country varchar(50),
    _State varchar(100),
    _City varchar(100),
    _FirstAddress varchar(100),
    _SecondAddress varchar(100),
    _ThirdAddress varchar(100),
    _FourthAddress varchar(100),
    _FullAddress varchar(150),
    _MobileNo varchar(20),
    _Email varchar(50),
    _PrimaryPhoneNo varchar(20),
    _SecondaryPhoneNo varchar(20),
    _Fax varchar(50),
    _Pincode int,
    _FileId bigint,
    _PANNumber varchar(20),
    _TradeLicenseNumber varchar(50),
    _GSTINNumber varchar(50),
    _LegalDocumentPath varchar(250),
    _LegalEntity varchar(50),
    _LegalNameOfCompany varchar(100),
    _TypeOfBusiness varchar(150),    
    _InCorporationDate datetime,
	out _ProcessingResult varchar(50)
    
/*


	set @outcome = '';
    
	Call sp_organization_detail_intupd(0, 'BottomHlaf', null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
    null, null, null, null, null, null, @outcome);    
    
    select @outcome;
    

*/
	
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_organization_detail_intupd', 1, 0, @Result);
			end;  
                        
            if not exists(select 1 from organization_detail where OrganizationId = _OrganizationId) then
            begin
				if exists(select 1 from organization_detail where lower(LegalNameOfCompany) = lower(_LegalNameOfCompany)) then
                begin
					select OrganizationId from organization_detail 
                    where lower(LegalNameOfCompany) = lower(_LegalNameOfCompany) into _OrganizationId;
                end;
                end if;
            end;
            end if;
            
		
			if not exists(select 1 from organization_detail where OrganizationId = _OrganizationId) then
            begin
				Insert into organization_detail values(
					default,
					_OrganizationName,
					_SectorType,
					_Country,
					_State,
					_City,
					_FirstAddress,
					_SecondAddress,
					_ThirdAddress,
					_FourthAddress,
					_FullAddress,
					_MobileNo,
					_Email,
					_PrimaryPhoneNo,
					_SecondaryPhoneNo,
					_Fax,
					_Pincode,
					_FileId,
					_PANNumber,
					_TradeLicenseNumber,
					_GSTINNumber,
					_LegalDocumentPath,
					_LegalEntity,
					_LegalNameOfCompany,
					_TypeOfBusiness,    
					_InCorporationDate,
					now()
				);
                
                Set _ProcessingResult = 'inserted';
			end;
            else 
            begin
				Update organization_detail Set
					OrganizationName			=			_OrganizationName,
					SectorType                 	=			_SectorType,
					Country                		=			_Country,
					State                  		=			_State,
					City                 		=			_City,
					FirstAddress                =			_FirstAddress,
					SecondAddress               =			_SecondAddress,
					ThirdAddress                =			_ThirdAddress,
					FourthAddress               =			_FourthAddress,
					FullAddress                 =			_FullAddress,
					MobileNo                 	=			_MobileNo,
					Email                  		=			_Email,
					PrimaryPhoneNo              =			_PrimaryPhoneNo,
					SecondaryPhoneNo            =			_SecondaryPhoneNo,
					Fax                  		=			_Fax,
					Pincode                  	=			_Pincode,
					FileId                 		=			_FileId,
					PANNumber                  	=			_PANNumber,
					TradeLicenseNumber          =			_TradeLicenseNumber,
					GSTINNumber                 =			_GSTINNumber,
					LegalDocumentPath           =			_LegalDocumentPath,
					LegalEntity                 =			_LegalEntity,
					LegalNameOfCompany          =			_LegalNameOfCompany,
					TypeOfBusiness              =			_TypeOfBusiness,    
					InCorporationDate           =			_InCorporationDate,
					UpdatedOn					=			now()
                where OrganizationId = _OrganizationId;
                
                Set _ProcessingResult = 'updated';
            end;
            end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_organization_intupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_organization_intupd`(
	_CompanyId int,
	_OrganizationId int,
	_OrganizationName varchar(250),
	_CompanyName varchar(150),
	_CompanyDetail varchar(250),
	_SectorType int,
	_Country varchar(50),
	_State varchar(100),
	_City varchar(100),
	_FirstAddress varchar(100),
	_SecondAddress varchar(100),
	_ThirdAddress varchar(100),
	_ForthAddress varchar(100),
	_FullAddress varchar(150),
	_MobileNo varchar(20),
	_Email varchar(50),
	_FirstEmail varchar(100),
	_SecondEmail varchar(100),
	_ThirdEmail varchar(100),
	_ForthEmail varchar(100),
	_PrimaryPhoneNo varchar(20),
	_SecondaryPhoneNo varchar(20),
	_Fax varchar(50),
	_Pincode int,
	_FileId bigint,
	_LegalDocumentPath varchar(250),
	_LegalEntity varchar(50),
	_TypeOfBusiness varchar(150),
	_InCorporationDate datetime,
	_IsPrimaryCompany bit(1),
	_FixedComponentsId json,    
	_BankAccountId int,
	_BankName varchar(100),
	_BranchCode varchar(20),
	_Branch varchar(50),
	_IFSC varchar(20),
	_AccountNo varchar(45),
	_OpeningDate datetime,
	_ClosingDate datetime,
	_PANNo varchar(20),
	_GSTNo varchar(50),
	_TradeLicenseNo varchar(50),
	_OrgMobileNo varchar(20),
	_OrgEmail varchar(50),
	_OrgPrimaryPhoneNo varchar(20),
	_OrgSecondaryPhoneNo varchar(20),
	_OrgFax varchar(50),
    _IsPrimaryAccount bit,
    _AdminId long,    
	out _ProcessingResult varchar(50)
    
/*


	set @outcome = '';
    
	Call sp_organization_intupd(0, 'BottomHlaf', 'BottomHalf Pvt. Ltd.', null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
    null, null, null, null, null, null, @outcome);    
    
    select @outcome;
    

*/
	
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
            Rollback;
			Call sp_LogException (@Message, @OperationStatus, 'sp_organization_intupd', 1, 0, @Result);
		end;  
        
		Start Transaction;
		begin
        /*
			if not exists(select 1 from company where CompanyId = _CompanyId) then
			begin
				if exists(select 1 from company where lower(LegalNameOfCompany) = lower(_LegalNameOfCompany)) then
				begin
					select CompanyId from company 
					where lower(LegalNameOfCompany) = lower(_LegalNameOfCompany) into _CompanyId;
				end;
				end if;
			end;
			end if;
		*/
            
			if not exists(select 1 from organization_detail where OrganizationId = _OrganizationId) then
			begin               
				Set @@SESSION.information_schema_stats_expiry = 0;

				SELECT AUTO_INCREMENT into _OrganizationId
				FROM information_schema.tables
				WHERE table_name = 'organization_detail'
				AND table_schema = DATABASE();	
				
				insert into organization_detail values(
					_OrganizationId,
					_OrganizationName,
					_OrgMobileNo,
					_OrgEmail,
					_OrgPrimaryPhoneNo,
					_OrgSecondaryPhoneNo,
					_OrgFax,
					_AdminId,
					null,
					utc_date(),
					null
				);
			end;
            else
            begin
				update organization_detail set
					OrganizationName					=			_OrganizationName,
					OrgMobileNo							=			_OrgMobileNo,
					OrgEmail							=			_OrgEmail,
					OrgPrimaryPhoneNo					=			_OrgPrimaryPhoneNo,
					OrgSecondaryPhoneNo					=			_OrgSecondaryPhoneNo,
					OrgFax								=			_OrgFax,
					UpdatedBy							=			_AdminId,
                    UpdatedOn							=			utc_date()
				where OrganizationId = _OrganizationId;
            end;
			end if;
					
			if not exists(select 1 from company where CompanyId = _CompanyId) then
			begin
				Set @@SESSION.information_schema_stats_expiry = 0;

				SELECT AUTO_INCREMENT into _CompanyId
				FROM information_schema.tables
				WHERE table_name = 'company'
				AND table_schema = DATABASE();	
				Insert into company values(
					_CompanyId,
					_OrganizationId,
					_OrganizationName,
					_CompanyName,
					_CompanyDetail,
					_SectorType,
					_Country,
					_State,
					_City,
					_FirstAddress,
					_SecondAddress,
					_ThirdAddress,
					_ForthAddress,
					_FullAddress,
					_MobileNo,
					_Email,
					_FirstEmail,
					_SecondEmail,
					_ThirdEmail,
					_ForthEmail,
					_PrimaryPhoneNo,
					_SecondaryPhoneNo,
					_Fax,
					_Pincode,
					_FileId,
					_LegalDocumentPath,
					_LegalEntity,
					_TypeOfBusiness,
					_InCorporationDate,
                    _PANNo,
					_GSTNo,
					_TradeLicenseNo,
					_IsPrimaryCompany,
					_FixedComponentsId,
					_AdminId,
					null,
					utc_date(),
					null
				);
			end;
            else
            begin
				update company set
					OrganizationId					=			_OrganizationId,
					OrganizationName				=			_OrganizationName,
					CompanyName						=			_CompanyName,
					CompanyDetail					=			_CompanyDetail,
					SectorType						=			_SectorType,
					Country							=			_Country,
					State							=			_State,
					City							=			_City,
					FirstAddress					=			_FirstAddress,
					SecondAddress					=			_SecondAddress,
					ThirdAddress					=			_ThirdAddress,
					ForthAddress					=			_ForthAddress,
					FullAddress						=			_FullAddress,
					MobileNo						=			_MobileNo,
					Email							=			_Email,
					FirstEmail						=			_FirstEmail,
					SecondEmail						=			_SecondEmail,
					ThirdEmail						=			_ThirdEmail,
					ForthEmail						=			_ForthEmail,
					PrimaryPhoneNo					=			_PrimaryPhoneNo,
					SecondaryPhoneNo				=			_SecondaryPhoneNo,
					Fax								=			_Fax,
					Pincode							=			_Pincode,
					FileId							=			_FileId,
					LegalDocumentPath				=			_LegalDocumentPath,
					LegalEntity						=			_LegalEntity,
					PANNo							=			_PANNo,
					GSTNo							=			_GSTNo,
					TradeLicenseNo					=			_TradeLicenseNo,
					TypeOfBusiness					=			_TypeOfBusiness,
					InCorporationDate				=			_InCorporationDate,
					IsPrimaryCompany				=			_IsPrimaryCompany,
					FixedComponentsId				=			_FixedComponentsId,
					UpdatedBy						=			_AdminId,
                    UpdatedOn						=			utc_date()
				where CompanyId 	= 	_CompanyId;
            end;
			end if;
			
			if not exists(select 1 from bank_accounts where BankAccountId = _BankAccountId) then
			begin
				insert into bank_accounts value(
					default,
					_OrganizationId,
					_CompanyId,
					_BankName,
					_BranchCode,
					_Branch,
					_IFSC,
					_AccountNo,
					_OpeningDate,
					_ClosingDate,
					_PANNo,
					_GSTNo,
					_TradeLicenseNo,
                    _IsPrimaryAccount,
					_AdminId,
					null,
					utc_date(),
					null
				);
			end;
            else
            begin
				update bank_accounts set
					BankName							=			_BankName,
					BranchCode							=			_BranchCode,
					Branch								=			_Branch,
					IFSC								=			_IFSC,
					AccountNo							=			_AccountNo,
					OpeningDate							=			_OpeningDate,
					ClosingDate							=			_ClosingDate,
					PANNo								=			_PANNo,
					GSTNo								=			_GSTNo,
					TradeLicenseNo						=			_TradeLicenseNo,
                    IsPrimaryAccount					=			_IsPrimaryAccount,
					UpdatedBy							=			_AdminId,
                    UpdatedOn							=			utc_date()
				where BankAccountId = _BankAccountId;
            end;
			end if;
		end;
		Commit;
		Set _ProcessingResult = 'updated';
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_organization_setting_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_organization_setting_get`(

/*

	Call sp_organization_setting_get();

*/
	
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_organization_setting_get', 1, 0, @Result);
			end;  
            
			Select * from organization_detail;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_organization_setting_getById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_organization_setting_getById`(
	_OrganizationId int,
	_OrganizationName varchar(250)

/*

	Call sp_organization_setting_getById(1, 'Test');

*/
	
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_organization_setting_getById', 1, 0, @Result);
			end;  
            
			Select * from organization_detail
			where OrganizationId = _OrganizationId
            and OrganizationName = _OrganizationName;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Password_GetByRole` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Password_GetByRole`(

/*

	Call sp_Password_GetByRole(0, NULL, 'sarfaraznawaz35@gmail.com');
    
    Call sp_Password_GetByRole(0, null, 'ravi@gmail.com', 2, 'welcome@123');

*/
	_UserId bigint,
	_MobileNo varchar(20),
	_EmailId varchar(50)
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_Password_GetByRole', 1, 0, @Result);
		end;  
	

		Select * from employeelogin where (Email = _EmailId Or Mobile = _MobileNo);
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_payroll_cycle_setting_getById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_payroll_cycle_setting_getById`(

/*

	Call sp_payroll_cycle_setting_getById(1);

*/
	
    _CompanyId int
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_payroll_cycle_setting_getById', 1, 0, @Result);
		end;  
		
		Select * from payroll_cycle_setting
        where CompanyId = _CompanyId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_payroll_cycle_setting_intupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_payroll_cycle_setting_intupd`(
/*

	
    Set @result = '';
	Call sp_payroll_cycle_setting_intupd(0, 1, 1, 'Monthly', 4, 30, 2, 0, 0, 1, @result);
    Select @result;

*/

	_PayrollCycleSettingId int,
    _CompanyId int,
    _OrganizationId int,
    _PayFrequency varchar(45),
    _PayCycleMonth int,
    _PayCycleDayOfMonth int,
    _PayCalculationId int,
    _IsExcludeWeeklyOffs bit,
    _IsExcludeHolidays bit,
    _AdminId bigint,
    out _ProcessingResult varchar(50)
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_payroll_cycle_setting_intupd', 1, 0, @Result);
		end;  
		
        if not exists(Select 1 from payroll_cycle_setting where PayrollCycleSettingId = _PayrollCycleSettingId) then
        begin
			Insert into payroll_cycle_setting
			values(
				default,
				_CompanyId,
				_OrganizationId,
				_PayFrequency,
				_PayCycleMonth,
				_PayCycleDayOfMonth,
				_PayCalculationId,
				_IsExcludeWeeklyOffs,
				_IsExcludeHolidays,
				_AdminId,
                now()
            );
            
            Set _ProcessingResult = 'inserted';
		end;
        else
        begin
			Update payroll_cycle_setting set
				CompanyId					=			_CompanyId,
				OrganizationId				=			_OrganizationId,
				PayFrequency				=			_PayFrequency,
				PayCycleMonth				=			_PayCycleMonth,
				PayCycleDayOfMonth			=			_PayCycleDayOfMonth,
				PayCalculationId			=			_PayCalculationId,
				IsExcludeWeeklyOffs			=			_IsExcludeWeeklyOffs,
				IsExcludeHolidays			=			_IsExcludeHolidays,
				CreatedBy 					=			_AdminId,
                CreatedOn					=			now()
			where PayrollCycleSettingId = _PayrollCycleSettingId;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_pay_calculation_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_pay_calculation_get`(

/*

	Call sp_pay_calculation_get();

*/
	
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_pay_calculation_get', 1, 0, @Result);
		end;  
		
		Select * from pay_calculation;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_pay_calculation_intupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_pay_calculation_intupd`(

	_PayCalculationId int,
	_PayCalculationDesc varchar(250),
    out _ProcessingResult varchar(50)
/*

	Set @result = '';
	Call sp_pay_calculation_intupd(0, 'Calculate base on only working days. i.e. 21 or 22 weekday from Mon to Sun based on Month.', @result);
    Call sp_pay_calculation_intupd(0, 'Calculate base total number of days in a month. (Take 30 day for the month of Feb.)', @result);
    select @result;

*/
	
)
Begin
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_pay_calculation_intupd', 1, 0, @Result);
		end;  
		
        if not exists(Select 1 from pay_calculation where PayCalculationId = _PayCalculationId) then
        begin
			Insert into pay_calculation
			values(default, _PayCalculationDesc);
            
            Set _ProcessingResult = 'inserted';
		end;
        else
        begin
			Update pay_calculation set
				_PayCalculationDesc		=	_PayCalculationDesc
			where PayCalculationId = _PayCalculationId;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_pf_esi_setting_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_pf_esi_setting_get`(

	_CompanyId int
/*

	Call sp_pf_esi_setting_get(1);

*/
	
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_pf_esi_setting_get', 1, 0, @Result);
			end;  
		
		select * from pf_esi_setting
        where CompanyId = _CompanyId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_pf_esi_setting_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_pf_esi_setting_insupd`(

/*

	Call sp_pf_esi_setting_insupd('BS', 'BASIC SALARY', 40, null, 1);

*/
	_PfEsi_setting_Id int,
    _CompanyId int,
	_PFEnable bit,
	_IsPfAmountLimitStatutory bit,
	_IsPfCalculateInPercentage bit,
	_IsAllowOverridingPf bit,
	_IsPfEmployerContribution bit,
	_EmployerPFLimit decimal,
	_IsHidePfEmployer bit,
	_IsPayOtherCharges bit,
	_IsAllowVPF bit,
	_EsiEnable bit,
	_MaximumGrossForESI decimal,
	_EsiEmployeeContribution decimal,
	_EsiEmployerContribution decimal,
	_IsAllowOverridingEsi bit,
	_IsHideEsiEmployer bit,
	_IsEsiExcludeEmployerShare bit,
	_IsEsiExcludeEmployeeGratuity bit,
	_IsEsiEmployerContributionOutside bit,
	_IsRestrictEsi bit,
	_IsIncludeBonusEsiEligibility bit,
	_IsIncludeBonusEsiContribution bit,
	_IsEmployerPFLimitContribution bit,
    _Admin bigint,
    out _ProcessingResult varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_pf_esi_setting_insupd', 1, 0, @Result);
			end;  
		
		if not exists(select 1 from pf_esi_setting where PfEsi_setting_Id = _PfEsi_setting_Id) then
        begin
			insert into pf_esi_setting
            values(
				default,
				_CompanyId,
				_PFEnable,
				_IsPfAmountLimitStatutory,
				_IsPfCalculateInPercentage,
				_IsAllowOverridingPf,
				_IsPfEmployerContribution,
				_EmployerPFLimit,
				_IsHidePfEmployer,
				_IsPayOtherCharges,
				_IsAllowVPF,
				_EsiEnable,
				_MaximumGrossForESI,
				_EsiEmployeeContribution,
				_EsiEmployerContribution,
				_IsAllowOverridingEsi,
				_IsHideEsiEmployer,
				_IsEsiExcludeEmployerShare,
				_IsEsiExcludeEmployeeGratuity,
				_IsEsiEmployerContributionOutside,
				_IsRestrictEsi,
				_IsIncludeBonusEsiEligibility,
				_IsIncludeBonusEsiContribution,
				_IsEmployerPFLimitContribution,
				utc_date(),
				null,
				_Admin,
				null
            );
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update pf_esi_setting set				
				PFEnable           						=       _PFEnable,
				IsPfAmountLimitStatutory        	    =       _IsPfAmountLimitStatutory,
				IsPfCalculateInPercentage       	    =       _IsPfCalculateInPercentage,
				IsAllowOverridingPf        				=       _IsAllowOverridingPf,
				IsPfEmployerContribution            	=       _IsPfEmployerContribution,
				IsHidePfEmployer            			=       _IsHidePfEmployer,
				IsPayOtherCharges           			=       _IsPayOtherCharges,
				IsAllowVPF          					=       _IsAllowVPF,
				EsiEnable         					  	=       _EsiEnable,
				IsAllowOverridingEsi            		=       _IsAllowOverridingEsi,
				IsHideEsiEmployer           			=       _IsHideEsiEmployer,
				IsEsiExcludeEmployerShare           	=       _IsEsiExcludeEmployerShare,
				IsEsiExcludeEmployeeGratuity            =       _IsEsiExcludeEmployeeGratuity,
				IsEsiEmployerContributionOutside        =       _IsEsiEmployerContributionOutside,
				IsRestrictEsi           				=       _IsRestrictEsi,
				IsIncludeBonusEsiEligibility            =       _IsIncludeBonusEsiEligibility,
				IsIncludeBonusEsiContribution           =       _IsIncludeBonusEsiContribution,
				IsEmployerPFLimitContribution           =       _IsEmployerPFLimitContribution,
				EmployerPFLimit         				=       _EmployerPFLimit,
				MaximumGrossForESI          			=       _MaximumGrossForESI,
				EsiEmployeeContribution         		=       _EsiEmployeeContribution,
				EsiEmployerContribution             	= 		_EsiEmployerContribution,
				UpdatedBy								=		_Admin,
                UpdatedOn								=		utc_date()
            where PfEsi_setting_Id = _PfEsi_setting_Id;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_professionalcandidates_filter` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_professionalcandidates_filter`(
	_searchString varchar(250),
	_sortBy varchar(50),
	_pageIndex int,
	_pageSize int
/*	

	Call sp_professionalcandidates_filter('1 AND Email = ''istiyaq.mi9@gmail.com''', '', 1, 10);
    
    Call sp_professionalcandidates_filter(' 1=1 ', '', 1, 5);

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
			Call sp_LogException(@Message, '', 'sp_professionalcandidates_filter', 1, 0, @Result);
		End;

        Begin
            If(_sortBy is NULL OR _sortBy = '') then
				Set _sortBy = 'UserId ';
			End if;
            Set @SelectQuery = CONCAT('Select * from (
				Select 
					Row_Number() over(Order by ', _sortBy, ') as `Index`,
                    UserId,
					Job_Title, 
                    Date_of_application, 
                    Name, 
                    Email_ID, 
                    Phone_Number, 
                    Total_Experience, 
                    Notice_Period,
					Count(1) Over() as Total from professionalcandidates
				Where ', _searchString, '
			)T where `Index` between ', ((_pageIndex - 1 ) * _pageSize + 1), ' and ', (_pageIndex * _pageSize));
            
			# Select @SelectQuery;
			prepare SelectQuery from @SelectQuery;
			execute SelectQuery;	
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_professionalcandidates_Get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_professionalcandidates_Get`(

/*

	Call sp_professionalcandidates_Get(-1, '9100544384', null);

*/
	_UserId bigint,
	_MobileNo varchar(20),
	_EmailId varchar(50)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_professionalcandidates_Get', 1, 0, @Result);
			end;  
            
            Set @UserExistsFlag = 0;
			Begin				
				Select * from professionalcandidates p
                where p.UserId = _UserId or p.Email_ID = _EmailId or
                p.Phone_Number = _MobileNo;
                
                Select * from candidatefiledetail
                where FileOwnerId = _UserId;
			End;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_ProfessionalCandidates_InsUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ProfessionalCandidates_InsUpdate`(
	_Source_Of_Application varchar(50),
	_Job_Title varchar(50),
	_Date_of_application Datetime,
	_Name varchar(50),
	_Email_ID varchar(100),
	_Phone_Number varchar(20),
    _Alternet_Numbers varchar(100),
	_Total_Experience float,
	_Annual_Salary double,
	_Notice_Period int,
	_Expeceted_CTC double,
	_Feedback varchar(500),
	_Current_Location varchar(100),
	_Preferred_Locations varchar(250),
	_Current_Company_name varchar(150),
	_Current_Company_Designation varchar(150),
	_Functional_Area varchar(100),
	_Role varchar(50),
	_Industry varchar(150),
	_Key_Skills varchar(500),
	_Resume_Headline varchar(500),
	_Summary text,
	_Under_Graduation_degree varchar(50),
	_UG_Specialization varchar(50),
	_UG_University_institute_Name varchar(150),
	_UG_Graduation_year int,
	_Post_graduation_degree varchar(50),
	_PG_specialization varchar(50),
	_PG_university_institute_name varchar(150),
	_PG_graduation_year int,
	_Doctorate_degree varchar(50),
	_Doctorate_specialization varchar(50),
	_Doctorate_university_institute_name varchar(150),
	_Doctorate_graduation_year int,
	_Gender varchar(10),
	_Marital_Status bit,
	_Home_Town_City varchar(50),
	_Pin_Code int,
	_Work_permit_for_USA varchar(50),
	_Date_of_Birth Datetime,
	_Latest_Star_Rating float,
	_Viewed varchar(50),
	_Viewed_By varchar(50),
	_Time_Of_View varchar(50),
	_Emailed varchar(50),
	_Emailed_By varchar(50),
	_Time_Of_Email Datetime,
	_Calling_Status varchar(50),
	_Calling_Status_updated_by varchar(50),
	_Time_of_Calling_activity_update Datetime,
	_Comment_1 varchar(50),
	_Comment_1_BY varchar(50),
	_Time_Comment_1_posted Datetime,
	_Comment_2 varchar(50),
	_Comment_2_BY varchar(50),
	_Time_Comment_2_posted Datetime,
	_Comment_3 varchar(50),
	_Comment_3_BY varchar(50),
	_Time_Comment_3_posted Datetime,
	_Comment_4 varchar(50),
	_Comment_4_BY varchar(50),
	_Time_Comment_4_posted Datetime,
	_Comment_5 varchar(50),
	_Comment_5_BY varchar(50),
	_Time_Comment_5_posted Datetime,
    out _ProcessingResult varchar(100)
    
/*

	Set @out = '';
    Call sp_ProfessionalCandidates_InsUpdate('Test', 'developer', '2022-01-09',	'demo', 'demo@gmail.com', '9000000000', null, 4, 400000, 60, 500000, null, 'Hyderabad', 'Hyderabad', 'ABC',
	'Developer', null, 'Team lead', 'IT', 'c++', 'demo resume', null, null, null, null, 0, null, null, null, 0, null, null, null, 0, 'male', 0, 'Asansol', 713302, 'NA', '1990-08-12',
	5, 0, null, 'NA', null, null, null, null, null, '2022-01-09', 'NA', 'NA', null, 'NA', 'NA', null, 'NA', 'NA', null, 'NA', 'NA', null, 'NA', 'NA', null, @out);
    Select @out;

*/
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
            Call sp_LogException (@Message, @OperationStatus, 'sp_ProfessionalCandidates_InsUpdate', 1, 0, @Result);
		end;
        
        Set _ProcessingResult = 'Starting';
        
		If not exists (Select 1 from ProfessionalCandidates Where Email_ID = _Email_ID) then
		Begin
			Insert into ProfessionalCandidates Values(
				default,
            	_Source_Of_Application,
				_Job_Title,
				_Date_of_application,
				_Name,
				_Email_ID,
				_Phone_Number,
                _Alternet_Numbers,
				_Total_Experience,
				_Annual_Salary,
				_Notice_Period,
				_Expeceted_CTC,
				_Feedback,
				_Current_Location,
				_Preferred_Locations,
				_Current_Company_name,
				_Current_Company_Designation,
				_Functional_Area,
				_Role,
				_Industry,
				_Key_Skills,
				_Resume_Headline,
				_Summary,
				_Under_Graduation_degree,
				_UG_Specialization,
				_UG_University_institute_Name,
				_UG_Graduation_year,
				_Post_graduation_degree,
				_PG_specialization,
				_PG_university_institute_name,
				_PG_graduation_year,
				_Doctorate_degree,
				_Doctorate_specialization,
				_Doctorate_university_institute_name,
				_Doctorate_graduation_year,
				_Gender,
				_Marital_Status,
				_Home_Town_City,
				_Pin_Code,
				_Work_permit_for_USA,
				_Date_of_Birth,
				_Latest_Star_Rating,
				_Viewed,
				_Viewed_By,
				_Time_Of_View,
				_Emailed,
				_Emailed_By,
				_Time_Of_Email,
				_Calling_Status,
				_Calling_Status_updated_by,
				_Time_of_Calling_activity_update,
				_Comment_1,
				_Comment_1_BY,
				_Time_Comment_1_posted,
				_Comment_2,
				_Comment_2_BY,
				_Time_Comment_2_posted,
				_Comment_3,
				_Comment_3_BY,
				_Time_Comment_3_posted,
				_Comment_4,
				_Comment_4_BY,
				_Time_Comment_4_posted,
				_Comment_5,
				_Comment_5_BY,
				_Time_Comment_5_posted
            );
            
			Set _ProcessingResult = 'Record inserted successfully';
		End;
        Else
        Begin
			Update ProfessionalCandidates Set
				Source_Of_Application                   =                    _Source_Of_Application,
				Job_Title                    			=                    _Job_Title,
                Alternet_Numbers						=					 _Alternet_Numbers,
				Date_of_application                    	=                    _Date_of_application,
				`Name`                    				=                    _Name,
				Total_Experience                    	=                    _Total_Experience,
				Annual_Salary                    		=                    _Annual_Salary,
				Notice_Period                    		=  	                 _Notice_Period,
				Expeceted_CTC                    		=                    _Expeceted_CTC,
				Feedback                    			=                    _Feedback,
				Current_Location                    	=                    _Current_Location,
				Preferred_Locations                    	=                    _Preferred_Locations,
				Current_Company_name                    =                    _Current_Company_name,
				Current_Company_Designation      		=                    _Current_Company_Designation,
				Functional_Area                    		=                    _Functional_Area,
				`Role`                    				=                    _Role,
				Industry                    			=                    _Industry,
				Key_Skills                    			=                    _Key_Skills,
				Resume_Headline                    		=                    _Resume_Headline,
				Summary                    				=                    _Summary,
				Under_Graduation_degree                 =                    _Under_Graduation_degree,
				UG_Specialization                    	=                    _UG_Specialization,
				UG_University_institute_Name            =                    _UG_University_institute_Name,
				UG_Graduation_year                    	=                    _UG_Graduation_year,
				Post_graduation_degree                  =                    _Post_graduation_degree,
				PG_specialization                    	=                    _PG_specialization,
				PG_university_institute_name            =                    _PG_university_institute_name,
				PG_graduation_year                    	=                    _PG_graduation_year,
				Doctorate_degree                    	=                    _Doctorate_degree,
				Doctorate_specialization                =                    _Doctorate_specialization,
				Doctorate_university_institute_name     =                    _Doctorate_university_institute_name,
				Doctorate_graduation_year               =                    _Doctorate_graduation_year,
				Gender                    				=                    _Gender,
				Marital_Status                    		=                    _Marital_Status,
				Home_Town_City                    		=                    _Home_Town_City,
				Pin_Code                    			=                    _Pin_Code,
				Work_permit_for_USA                    	=                    _Work_permit_for_USA,
				Date_of_Birth                    		=                    _Date_of_Birth,
				Latest_Star_Rating                    	=                    _Latest_Star_Rating,
				Viewed                    				=                    _Viewed,
				Viewed_By                    			=                    _Viewed_By,
				Time_Of_View                    		=                    _Time_Of_View,
				Emailed                    				=                    _Emailed,
				Emailed_By                    			=                    _Emailed_By,
				Time_Of_Email                    		=                    _Time_Of_Email,
				Calling_Status                    		=                    _Calling_Status,
				Calling_Status_updated_by               =                    _Calling_Status_updated_by,
				Time_of_Calling_activity_update         =                    _Time_of_Calling_activity_update,
				Comment_1                    			=                    _Comment_1,
				Comment_1_BY                    		=                    _Comment_1_BY,
				Time_Comment_1_posted                   =                    _Time_Comment_1_posted,
				Comment_2                    			=                    _Comment_2,
				Comment_2_BY                    		=                    _Comment_2_BY,
				Time_Comment_2_posted                   =                    _Time_Comment_2_posted,
				Comment_3                    			=                    _Comment_3,
				Comment_3_BY                    		=                    _Comment_3_BY,
				Time_Comment_3_posted                   =                    _Time_Comment_3_posted,
				Comment_4                    			=                    _Comment_4,
				Comment_4_BY                    		=                    _Comment_4_BY,
				Time_Comment_4_posted                   =                    _Time_Comment_4_posted,
				Comment_5                    			=                    _Comment_5,
				Comment_5_BY                    		=                    _Comment_5_BY,
				Time_Comment_5_posted                   =                    _Time_Comment_5_posted
			Where Email_ID = _Email_ID;
                
			Set _ProcessingResult = 'Record updated successfully';
		End;
        End if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_ProfessionalCandidates_UpdInfo` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ProfessionalCandidates_UpdInfo`(
	_UserId long,
    _Name varchar(50),
    _Alternet_Numbers varchar(100),
	_Resume_Headline varchar(500),
	_FileId long,
	_FilePath varchar(500), 
	_FileName varchar(100),
	_FileExtension varchar(10),
	_UserTypeId int,
    _IsProfileImageRequest int,
    out _ProcessingResult varchar(100)
    
/*

	Set @out = '';
    Call sp_ProfessionalCandidates_UpdInfo(89, 'Md Istiyak', null, null, null, 'Dotnet developer full stack', @out);
    Select @out;

*/
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            
            SET autocommit = 1;
            Call sp_LogException (@Message, '', 'sp_ProfessionalCandidates_UpdInfo', 1, 0, @Result);
		end;
        
        Set _ProcessingResult = '';
        
		If exists (Select 1 from professionalcandidates Where UserId = _UserId) then
        Begin
			Update professionalcandidates Set
                Alternet_Numbers						=					 _Alternet_Numbers,
				`Name`                    				=                    _Name,
                Resume_Headline 						=					_Resume_Headline
			Where UserId = _UserId;
            
            Set _ProcessingResult = 'Record updated successfully';
		End;
        End if;
        
        if(_IsProfileImageRequest = 1) then
        begin
			if not exists (select 1 from candidatefiledetail where FileId 	= _FileId) then
			Begin
				insert into candidatefiledetail values (
					default,
					_UserId,
					_FilePath,
					_FileName,
					_FileExtension, 
					_UserTypeId,
					now(), 
					null
				);
			End;
			Else
			Begin
				update candidatefiledetail set 	
						FileOwnerId 	= 	_UserId,
						FilePath 		= 	_FilePath,
						FileName		=	_FileName,
						FileExtension	=	_FileExtension, 
						UserTypeId		=	_UserTypeId, 
						UpdatedOn		=	now()
				where	FileId 			= 	_FileId;
			End;
			End if;
			
			Set _ProcessingResult = 'Record updated successfully';
		End;
		End if;
	End;	
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_professionaldetail_filter` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_professionaldetail_filter`(

/*

	Call sp_professionaldetail_filter(89, null, null);

*/
	_UserId bigint,
	_Mobile varchar(20),
	_Email varchar(50)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_professionaldetail_filter', 1, 0, @Result);
			end;  
            
            Select professionaldetail_Json from professionaldetail p
            where p.UserId = _UserId;
            
            Select * from candidatefiledetail c
            where c.FileOwnerId = _UserId;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_professionaldetail_get_byid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_professionaldetail_get_byid`(
	_EmployeeId bigint
/*	

	Call sp_professionaldetail_get_byid(1);

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
			Call sp_LogException(@Message, '', 'sp_professionaldetail_get_byid', 1, 0, @Result);
		End;

        Begin
			Select  e.EmployeeUid,
				e.FirstName,
				e.LastName,
				e.Mobile,
				e.Email,
				e.ReportingManagerId,
				e.DesignationId,
				ep.SecondaryMobile,
				e.IsActive,
                e.CompanyId,
                e.CreatedOn,
                ep.DOB,
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
				e.LeavePlanId
			from employees e 
			Inner Join employeepersonaldetail ep on e.EmployeeUid = ep.EmployeeUid
			Inner Join employeeprofessiondetail ef on e.EmployeeUid = ef.EmployeeUid
			Where e.EmployeeUid = _employeeId;

			select * from professionaldetail
			where EmployeeId = _EmployeeId;
            
            select * from userfiledetail
			where FileOwnerId = _EmployeeId;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_professionaldetail_insetupdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_professionaldetail_insetupdate`(

/*

	Call sp_professionaldetail_insetupdate(89, '9100544384', null);

*/
	_UserId bigint,
    _FileId bigint,
	_Mobile varchar(20),
	_Email varchar(50),
    _FirstName varchar(100),
    _LastName varchar(100),
    _Date_Of_Application Datetime,
    _Total_Experience_In_Months int,
    _Salary_Package double,
    _Notice_Period int,
    _Expeceted_CTC double,
    _Current_Location varchar(100),
    _Preferred_Location Json,
    _ProfessionalDetail_Json Json,
    _IsProfileImageRequest int,
    out _ProcessingResult varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_professionaldetail_insetupdate', 1, 0, @Result);
			end;  
            
            if not exists (select 1 from professionaldetail where UserId = _UserId) then
            begin
				Insert into professionaldetail values(
					default,
					_Mobile,
					_Email,
					_FirstName,
					_LastName,
					_Date_Of_Application,
					_Total_Experience_In_Months,
					_Salary_Package,
					_Notice_Period,
					_Expeceted_CTC,
					_Current_Location,
					_Preferred_Location,
					_ProfessionalDetail_Json,
                    utc_date(),
                    null
				);
			end;
            else
            begin
				Update professionaldetail Set
					Mobile_Number					=			_Mobile,
					Email							=			_Email,
					FirstName						=			_FirstName,
					LastName						=			_LastName,
					Date_Of_Application				=			_Date_Of_Application,
					Total_Experience_In_Months		=			_Total_Experience_In_Months,
					Salary_Package					=			_Salary_Package,
					Notice_Period					=			_Notice_Period,
					Expeceted_CTC					=			_Expeceted_CTC,
					Current_Location				=			_Current_Location,
					Preferred_Locations				=			_Preferred_Location,
					ProfessionalDetail_Json			=			_ProfessionalDetail_Json,
                    UpdatedOn						=			NOW()
				Where UserId = _UserId;
            end;
            end if;
            
			if(_IsProfileImageRequest = 1) then
			begin
				if not exists (select 1 from candidatefiledetail where FileId 	= _FileId) then
				Begin
					insert into candidatefiledetail values (
						default,
						_UserId,
						_FilePath,
						_FileName,
						_FileExtension, 
						_UserTypeId,
						now(), 
						null
					);
				End;
				Else
				Begin
					update candidatefiledetail set 	
							FileOwnerId 	= 	_UserId,
							FilePath 		= 	_FilePath,
							FileName		=	_FileName,
							FileExtension	=	_FileExtension, 
							UserTypeId		=	_UserTypeId, 
							UpdatedOn		=	now()
					where	FileId 			= 	_FileId;
				End;
				End if;
			End;
			End if;
            
            Set _ProcessingResult = _UserId;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_professionaldetail_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_professionaldetail_insupd`(

/*

	Call sp_professionaldetail_insupd(89, '9100544384', null);

*/
	_EmployeeId bigint,
	_Mobile varchar(20),
	_Email varchar(100),
	_FirstName varchar(100),
	_LastName varchar(100),
	_ProfessionalDetailJson json,
    out _ProcessingResult varchar(100)
)
Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_professionaldetail_insupd', 1, 0, @Result);
		end;  
		
		if not exists (select 1 from professionaldetail where EmployeeId = _EmployeeId) then
		begin
			Insert into professionaldetail values(
				_EmployeeId,
				_Mobile,
				_Email,
				_FirstName,
				_LastName,
				_ProfessionalDetailJson,
				utc_date(),
				null
			);
		end;
		else
		begin
			Update professionaldetail Set
				Mobile							=			_Mobile,
				Email							=			_Email,
				FirstName						=			_FirstName,
				LastName						=			_LastName,
				ProfessionalDetailJson			=			_ProfessionalDetailJson,
				UpdatedOn						=			NOW()
			Where EmployeeId 					= 			_EmployeeId;
		end;
		end if;
		
		Set _ProcessingResult = _EmployeeId;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_project_detail_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_project_detail_insupd`(
	   /*

    Call sp_project_detail_insupd(1);

*/
	_ProjectId bigint ,
    _ProjectName varchar(150),
    _ProjectDescription varchar(500),
    _ProjectManagerId bigint,
    _TeamMemberIds json,
    _ProjectStartedOn datetime,
    _ProjectEndedOn datetime,
    _ArchitectId bigint,
    _IsClientProject bit,
    _ClientId bigint,
    _HomePageUrl varchar(150),
    _PageIndexDetail json,
    _KeywordDetail json,
    _DocumentationDetail json,
	out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);

            Call sp_LogException (@Message, '', 'sp_project_detail_insupd', 1, 0, @Result);
		end;
        
        if not exists (select 1 from project where ProjectId = _ProjectId) then
        begin
			Insert into project values(
				default,
				_ProjectName,
				_ProjectDescription,
				_ProjectManagerId,
				_TeamMemberIds,
				_ProjectStartedOn,
				_ProjectEndedOn,
				_ArchitectId,
				_IsClientProject,
				_ClientId,
				_HomePageUrl,
				_PageIndexDetail,
				_KeywordDetail,
				_DocumentationDetail
			);
         
             Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update project set 
				ProjectName						=			_ProjectName,
				ProjectDescription				=			_ProjectDescription,
				ProjectManagerId				=			_ProjectManagerId,
				TeamMemberIds					=			_TeamMemberIds,
				ProjectStartedOn				=			_ProjectStartedOn,
				ProjectEndedOn					=			_ProjectEndedOn,
				ArchitectId						=			_ArchitectId,
				IsClientProject					=			_IsClientProject,
				ClientId						=			_ClientId,
				HomePageUrl						=			_HomePageUrl,
				PageIndexDetail					=			_PageIndexDetail,
				KeywordDetail					=			_KeywordDetail,
				DocumentationDetail				=			_DocumentationDetail
			where ProjectId 					= 			_ProjectId;
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Reset_Password` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Reset_Password`(

/*

	Set @result = '';
	Call sp_Reset_Password('9000000000', null, 'FFZEbzFaMWm5pp+VERSpIYUOSAY8oW83ZlXNRjj453A=', @result);
    Select @result;
    

*/
	_MobileNo varchar(20),
	_EmailId varchar(50),
    _NewPassword varchar(100),
    out _ProcessingResult varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_Reset_Password', 1, 0, @Result);
			end;  
            
            Set _ProcessingResult = ''; 
			update employeelogin Set 
				Password 	= 	_NewPassword 
			where (Email = _EmailId Or Mobile = _MobileNo);
				
            Set _ProcessingResult = 'Update';
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_RolesAndMenu_GetAll` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_RolesAndMenu_GetAll`(
	
/*	

	Call sp_RolesAndMenu_GetAll(1);

*/

	_accesslevelId int

)
Begin
    Begin
		Declare exit handler for sqlexception
		Begin
		
			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
										
			Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
			Call sp_LogException(@Message, '', 'sp_RolesAndMenu_GetAll', 1, 0, @Result);
		end;

		if (_accesslevelId = 1)  then
        begin
			Select RM.Catagory, RM.Childs, RM.Link, RM.Icon, RM.Badge, 
			RM.BadgeType, RM.AccessCode,  1 as Permission from rolesandmenu RM;
        end;
        else
        begin
			Select RM.Catagory, RM.Childs, RM.Link, RM.Icon, RM.Badge, 
			RM.BadgeType, RM.AccessCode, 
			case
				when AccessibilityId is null
				then 0
				else AccessibilityId
			end Permission from rolesandmenu RM
            left Join role_accessibility_mapping r on r.AccessCode = RM.AccessCode
            and r.AccessLevelId = _accesslevelId;
        end;
        end if;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_role_accessibility_mapping_InsUpd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_role_accessibility_mapping_InsUpd`(
 
 /*
 
	Call sp_role_accessibility_mapping_InsUpd(-1, 1, 1, 1);
 
 */
 
	_RoleAccessibilityMappingId int,
	_AccessLevelId int,
	_AccessCode int,
	_AccessibilityId int
 )
Begin
 	Begin
 		Declare exit handler for sqlexception
 		Begin
			
 			GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
 										@errorno = MYSQL_ERRNO,
 										@errortext = MESSAGE_TEXT;
 										
 			Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
 			Call sp_LogException(@Message, @OperationStatus, 'sp_role_accessibility_mapping_InsUpd', 1, 0, @Result);
 		end;
 
        Begin
			If not exists(Select 1 from role_accessibility_mapping where AccessLevelId = _AccessLevelId And AccessCode = _AccessCode)then
            Begin
				Insert into role_accessibility_mapping values(Default, _AccessLevelId, _AccessCode, _AccessibilityId);
            End;
            Else
            Begin
				Update role_accessibility_mapping Set AccessCode = _AccessCode, AccessibilityId = _AccessibilityId 
				where AccessLevelId = _AccessLevelId And AccessCode = _AccessCode;
            End;
            End if;
		End;
 	End;
 end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_components_get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_components_get`(

/*

	Call sp_salary_components_get();

*/
	
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_components_get', 1, 0, @Result);
			end;  
		
		select 
        	ComponentId,
            ComponentFullName,
            ComponentDescription,
			CalculateInPercentage,
			TaxExempt,
            ComponentTypeId,
            ComponentCatagoryId,
			PercentageValue,
			MaxLimit,
            DeclaredValue,
            AcceptedAmount,
			RejectedAmount,
			UploadedFileIds,
            Formula,
			EmployeeContribution,
			EmployerContribution,
			IncludeInPayslip,
			Section,
            SectionMaxLimit,
            IsAdHoc,
            AdHocId,
			IsOpted,
			IsActive,
			CreatedBy,
            UpdatedBy,
            CreatedOn,
            UpdatedOn
        from salary_components ;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_components_get_byId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_components_get_byId`(

/*

	Call sp_salary_components_get_byId('BS');

*/
	 _ComponentId varchar(10)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_components_get_byId', 1, 0, @Result);
			end;  
		
		select 
        	ComponentId,
            ComponentFullName,
            ComponentDescription,
			CalculateInPercentage,
			TaxExempt,
            ComponentTypeId,
			PercentageValue,
			MaxLimit,
            DeclaredValue,
            AcceptedAmount,
			RejectedAmount,
			UploadedFileIds,
            Formula,
			EmployeeContribution,
			EmployerContribution,
			IncludeInPayslip,
			Section,
            SectionMaxLimit,
            IsAdHoc,
            AdHocId,
			IsOpted,
			IsActive,
			CreatedBy,
            UpdatedBy,
            CreatedOn,
            UpdatedOn
        from salary_components 
        where ComponentId = _ComponentId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_components_get_type` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_components_get_type`(

/*

	Call sp_salary_components_get_type(0);

*/
	 _ComponentTypeId int
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_components_get_type', 1, 0, @Result);
			end;  
		
        if(_ComponentTypeId > 0) then
        begin
			select 
				*
			from salary_components 
			where ComponentTypeId = _ComponentTypeId;        
        end;
        else
        begin
			select 
				*
			from salary_components;
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_components_group_by_employeeid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
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
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_components_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_components_insupd`(

/*



	Set @result = '';
	Call sp_salary_components_insupd('BS', 'BASIC SALARY', 1, 0, 1, 40, 0, '', 0, 0, 1, 0, 0, -1, 1, 0, 0, 1, 1, @result);
    Select @result;
    
    

*/
	_ComponentId varchar(10),
    _ComponentFullName varchar(150),
    _ComponentDescription varchar(1024),
    _CalculateInPercentage bit,
    _TaxExempt bit,
    _ComponentTypeId int,
    _AcceptedAmount decimal,
    _RejectedAmount decimal,
    _UploadedFileIds decimal,
    _ComponentCatagoryId int,
    _PercentageValue decimal,
    _MaxLimit decimal,
    _DeclaredValue decimal,
    _Formula varchar(100),
    _EmployeeContribution decimal,
    _EmployerContribution decimal,
    _IncludeInPayslip bit,
    _IsAdHoc bit,
    _AdHocId int,
    _Section varchar(20),
    _SectionMaxLimit decimal,
    _IsAffectInGross bit,
    _RequireDocs bit,
    _IsOpted bit,
    _IsActive bit,
    _AdminId bigint,
    out _ProcessingResult varchar(100)
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_components_insupd', 1, 0, @Result);
			end;  
		
		if not exists(select 1 from salary_components where ComponentId = _ComponentId) then
        begin
			insert into salary_components
            values(
				_ComponentId,
                _ComponentFullName,
				_ComponentDescription,
                _CalculateInPercentage,
                _TaxExempt,
				_ComponentTypeId,
                _ComponentCatagoryId,
				_PercentageValue,
				_MaxLimit,
                _DeclaredValue,
                _AcceptedAmount,
				_RejectedAmount,
				_UploadedFileIds,
                _Formula,
				_EmployeeContribution,
				_EmployerContribution,
				_IncludeInPayslip,
                _IsAdHoc,
				_AdHocId,
                _Section,
				_SectionMaxLimit,
				_IsAffectInGross,
				_RequireDocs,
				_IsOpted,
                _IsActive,
				utc_date(),
                null,
                _AdminId,
                null
            );
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update salary_components set				
				ComponentDescription			=			_ComponentDescription,
                ComponentFullName				=			_ComponentFullName,
				CalculateInPercentage			=			_CalculateInPercentage,
				PercentageValue					=			_PercentageValue,
                TaxExempt						=			_TaxExempt,
				ComponentTypeId					=			_ComponentTypeId,
                ComponentCatagoryId				=			_ComponentCatagoryId,
				MaxLimit						=			_MaxLimit,
                DeclaredValue					=			_DeclaredValue,
                AcceptedAmount					=			_AcceptedAmount,
				RejectedAmount					=			_RejectedAmount,
				UploadedFileIds					=			_UploadedFileIds,
                Formula							=			_Formula,
                EmployeeContribution			= 			_EmployeeContribution,
                EmployerContribution			=			_EmployerContribution,
                IncludeInPayslip				=			_IncludeInPayslip,
                IsAdHoc							=			_IsAdHoc,
				AdHocId							=			_AdHocId,
                Section							=			_Section,
				SectionMaxLimit					=			_SectionMaxLimit,
				IsAffectInGross					=			_IsAffectInGross,
				RequireDocs						=			_RequireDocs,
                IsOpted							=			_IsOpted,
                IsActive						=			_IsActive,
				UpdatedBy						=			_AdminId,
                UpdatedOn						=			utc_date()
            where ComponentId = _ComponentId;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_group_getAll` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_getAll`(

/*

	Call sp_salary_group_getAll();

*/

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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_group_getAll', 1, 0, @Result);
			end;  
		
		select * from salary_group;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_group_getbyCompanyId` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_getbyCompanyId`(

/*

	Call sp_salary_group_getbyCompanyId(1);

*/

	_CompanyId int
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_group_getbyCompanyId', 1, 0, @Result);
			end;  
		
		select * from salary_group
        where CompanyId = _CompanyId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_group_getById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_getById`(

/*

	Call sp_salary_group_getById(1);

*/

	_SalaryGroupId int
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_group_getById', 1, 0, @Result);
			end;  
		
		select * from salary_group
        where SalaryGroupId = 	_SalaryGroupId;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_group_get_by_ctc` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_get_by_ctc`(

/*

	Call sp_salary_group_get_by_ctc(2000000);

*/

	_CTC decimal
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
				Call sp_LogException (@Message, @OperationStatus, 'sp_salary_group_getById', 1, 0, @Result);
			end;  
		
		select * from salary_group 
		where _CTC >= MinAmount 
		and _CTC <= MaxAmount;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_group_get_components` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_get_components`(

/*

	Call sp_salary_group_get_components(1);

*/
	 _SalaryGroupId int
)
Begin
	Declare componentsId Json;    
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_salary_group_get_components', 1, 0, @Result);
		end;  
		
		select 
			s.ComponentId Into componentsId
		from salary_group s
		where SalaryGroupId = _SalaryGroupId;
                
        Set @ids = '';
        Set @count = JSON_LENGTH(componentsId);
        Set @dm = '';
        Set @i = 0;
        while @i < @count do
        begin
			set @ids = Concat(@ids, @dm, '''', (select JSON_UNQUOTE(json_extract(componentsId, concat('$[', @i, ']')))), '''');
            
            set @i = @i + 1;
            if(@i > 0) then
				set @dm = ',';
			end if;
        end;
        end while;
        
        Set @SelectQuery = '';
        if(@ids is null OR @ids = '') then
        begin
			Select * from salary_components where IncludeInPayslip = 1;
        end;
        else
        begin
			Set @SelectQuery = Concat('
				Select * from salary_components where IncludeInPayslip = 1
					Union 
				Select * from salary_components
				where ComponentId in (', @ids, ')'
			);
		
			# Select @SelectQuery;
			prepare SelectQuery from @SelectQuery;
			execute SelectQuery;	
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_group_get_initial_components` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_get_initial_components`(

/*

	Call sp_salary_group_get_initial_components();

*/
)
Begin
	Declare salaryComponents Json;    
	Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_salary_group_get_initial_components', 1, 0, @Result);
		end;  
		
		Select * from salary_components where IsOPted = 1;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_salary_group_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_group_insupd`(

/*



	Set @result = '';
	Call sp_salary_group_insupd(0, 1, 'A', 'For people having salary more than 24L', 0, 500000, 1, @result);
    select @result;



*/
	_SalaryGroupId int,
    _CompanyId int,
    _SalaryComponents json,
    _GroupName varchar(45),
    _GroupDescription varchar(250),
    _MinAmount decimal,
    _MaxAmount decimal,
    _AdminId bigint,
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
			Call sp_LogException (@Message, '', 'sp_salary_group_insupd', 1, 0, @Result);
		end;  
		
		if not exists(select 1 from salary_group where SalaryGroupId = _SalaryGroupId) then
        begin
			insert into salary_group
            values(
				default,
				_SalaryComponents,
				_GroupName,
				_GroupDescription,
				_MinAmount,
				_MaxAmount,
				_AdminId,
				now(),
                _CompanyId
            );
            
            Set _ProcessingResult = 'inserted';
        end;
        else
        begin
			update salary_group set				
				SalaryComponents		=			_SalaryComponents,
				GroupName				=			_GroupName,
				GroupDescription		=			_GroupDescription,
				MinAmount				=			_MinAmount,
				MaxAMount				=			_MaxAmount,
				CreatedBy				=			_AdminId,
                CreatedOn				=			utc_date(),
                CompanyId				=			_CompanyId
            where SalaryGroupId 		= 			_SalaryGroupId;
            
            Set _ProcessingResult = 'updated';
        end;
        end if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_sendingbill_email_get_detail` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_sendingbill_email_get_detail`(
	_SenderId bigint,
	_ClientId bigint,
    _FileId bigint,
    _EmployeeId bigint
/*	

	Call sp_sendingbill_email_get_detail(1, 2, 1, 1);

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
			Call sp_LogException(@Message, '', 'SP_ClientsAndSender_Emails_By_Id', 1, 0, @Result);
		End;

		Select * from clients
        where ClientId = _SenderId
        
        Union
        
        Select * from clients
        where ClientId = _ClientId;
        
        select * from filedetail
        where FileId = _FileId;
        
        select * from employees
        where EmployeeUid = _EmployeeId;
        
        select * from email_setting_detail
        where CompanyId = _SenderId;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Students_InsertUpdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Students_InsertUpdate`(
	_StudentUid varchar(64),
	_ClassTeacherUid varchar(64),
	_StudentFirstName varchar(50), 
	_StudentLastName varchar(50),
	_FatherName varchar(100),
	_MotherName varchar(100),
	_Class varchar(4),
	_Section char(1),
	_Address varchar(250), 
	_City varchar(50),
	_State varchar(50), 
	_Pincode varchar(100), 
	_NickName varchar(100)
)
Begin
	Set @UUid:= (CAST(UNIX_TIMESTAMP(CURTIME(3)) * 1000 AS unsigned));
    Set @OperationStatus = '';
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_Students_InsertUpdate', 1, 0, @Result);
		end;

	Begin
		if not exists (Select 1 from Students where StudentUid = _StudentUid) then
			Begin
				Insert into Students 
					(
					StudentUid,
					ClassTeacherUid,
					StudentFirstName, 
					StudentLastName,
					FatherName,
					MotherName,
					Class,
					Section,
					Address, 
					City,
					State, 
					Pincode, 
					NickName
                    )
                values 
					(
					_StudentUid,
					_ClassTeacherUid,
					_StudentFirstName, 
					_StudentLastName,
					_FatherName,
					_MotherName,
					_Class,
					_Section,
					_Address, 
					_City,
					_State, 
					_Pincode, 
					_NickName
                    );
			End;
            Else
            Begin
				Update Students Set
					ClassTeacherUid	=	_ClassTeacherUid,
					StudentFirstName=	_StudentFirstName, 
					StudentLastName	=	_StudentLastName,
					FatherName		=	_FatherName,
					MotherName		=	_MotherName,
					Class			=	_Class,
					Section			=	_Section,
					Address			=	_Address, 
					City			=	_City,
					State			=	_State, 
					Pincode			=	_Pincode, 
					NickName		=	_NickName
			Where	StudentUid		=	_StudentUid;
            End;
            End if;
		End;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_timesheet_insupd` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_timesheet_insupd`(

/*

	Set @OutParam = '';
	Call sp_timesheet_insupd(0, 4, 1, '2022-02-15', 9.0, 0, 0, 7, 'working', '2022-02-15', 4, @OutParam);
    Select @OutParam;

*/

	_TimesheetId bigint,
    _EmployeeId bigint,
    _ClientId bigint,
    _UserTypeId int,
    _TimesheetMonthJson Json,
    _TotalDays int,
    _DaysAbsent int,
    _ExpectedBurnedMinutes int,
    _ActualBurnedMinutes int,
    _TotalWeekDays int,
    _TotalWorkingDays int,
    _TotalHolidays int,
    _MonthTimesheetApprovalState int,
    _ForYear int,
    _ForMonth int,
    _AdminId bigint,
    out _ProcessingResult varchar(100)
)
Begin
    Set @OperationStatus = '';
		Begin
			Declare Exit handler for sqlexception
			Begin
				Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
											@errorno = MYSQL_ERRNO,
											@errortext = MESSAGE_TEXT;
                             
				Set sql_safe_updates = 1;
				Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
				Call sp_LogException (@Message, @OperationStatus, 'sp_timesheet_insupd', 1, 0, @Result);
			end;
            
            Set _ProcessingResult = '';
			Begin
				If not exists (Select * from employee_timesheet where TimesheetId = _TimesheetId) then
                Begin
					Set @@SESSION.information_schema_stats_expiry = 0;

					SELECT AUTO_INCREMENT into _TimesheetId
					FROM information_schema.tables
					WHERE table_name = 'employee_timesheet'
					AND table_schema = DATABASE();	

					Insert into employee_timesheet values(
						_TimesheetId,
						_EmployeeId,
						_ClientId,
						_UserTypeId,
						_TimesheetMonthJson,
						_TotalDays,
						_DaysAbsent,
						_ExpectedBurnedMinutes,
						_ActualBurnedMinutes,
						_TotalWeekDays,
						_TotalWorkingDays,
						_TotalHolidays,
						_MonthTimesheetApprovalState,
						_ForYear,
						_ForMonth,
                        now(),
                        null,
                        _AdminId,
                        null
					);
                    
                    Set _ProcessingResult = 'inserted';
                End;
                Else
                Begin
					Update employee_timesheet Set
							TimesheetId						=				_TimesheetId,
							EmployeeId						=				_EmployeeId,
							ClientId						=				_ClientId,
							UserTypeId						=				_UserTypeId,
							TimesheetMonthJson				=				_TimesheetMonthJson,
							TotalDays						=				_TotalDays,
							DaysAbsent						=				_DaysAbsent,
							ExpectedBurnedMinutes			=				_ExpectedBurnedMinutes,
							ActualBurnedMinutes				=				_ActualBurnedMinutes,
							TotalWeekDays					=				_TotalWeekDays,
							TotalWorkingDays				=				_TotalWorkingDays,
							TotalHolidays					=				_TotalHolidays,
							MonthTimesheetApprovalState		=				_MonthTimesheetApprovalState,
							ForYear							=				_ForYear,
							ForMonth						=				_ForMonth,
							UpdatedOn						=				NOW(),
							UpdatedBy						= 				_AdminId
					where 	TimesheetId 					= 				_TimesheetId;
                    
                    Set _ProcessingResult = 'updated';
                End;
                End if;	
			End;
		End;
	End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_UpdateRefreshToken` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UpdateRefreshToken`(
	_UserId bigint,
	_RefreshToken varchar(500),
	_ExpiryTime Datetime
    
/*

    Call sp_UpdateRefreshToken(1, '25B9nCPsEwIJvGwl7twPcNll+nMTPuoSSCLCP9VMBK9WrVC13R0ASJe0DdgSv6yhjBSruHufoxmTxuc4NFWTzQ==', '2021-08-18');


*/    
    
    
)
Begin
	Declare Exit handler for sqlexception
	Begin
		Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
		Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
		Call sp_LogException (@Message, @OperationStatus, 'sp_UpdateRefreshToken', 1, 0, @Result);
	end;    
	Begin
		If not exists(Select 1 from refreshtoken where UserId = _UserId) then
        Begin
			Insert into refreshtoken values(
				_UserId,
                _RefreshToken,
                _ExpiryTime
            );
        End;
        Else
        Begin
			Update refreshtoken Set 
				RefreshToken = _RefreshToken,
				ExpiryTime = _ExpiryTime
			where UserId = _UserId;
        End;
        End if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_UserComments_Get` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_UserComments_Get`(
	_Email varchar(100)
/*	

	Call SP_UserComments_Get('istiyaq.mi9@gmail.com');

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
			Call sp_LogException(@Message, '', 'SP_UserComments_Get', 1, 0, @Result);
		End;

        Begin
            Select * from usercomments where Email = _Email;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_UserDetail_ById` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UserDetail_ById`(
	_userId bigint
/*	

	Call sp_UserDetail_ById(4);

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
			Call sp_LogException(@Message, '', 'sp_UserDetail_ById', 1, 0, @Result);
		End;

        Begin
			Select * from userdetail u
            where u.UserId = _userId;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_UserDetail_GetByMobileOrEmail` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UserDetail_GetByMobileOrEmail`(
	_email varchar(100),
	_mobile varchar(20)
/*	

	Call sp_UserDetail_GetByMobileOrEmail('istiyaq.mi9@gmail.com', '');

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
			Call sp_LogException(@Message, '', 'sp_UserDetail_GetByMobileOrEmail', 1, 0, @Result);
		End;

        Begin
			Select * from employeelogin u
            where u.Mobile = _mobile or
            u.Email = _email;
		End;
	End;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_UserDetail_Ins` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UserDetail_Ins`(
	_UserId bigint,
	_FirstName varchar(50),
	_LastName varchar(50),
	_MobileNo varchar(20),
	_EmailId varchar(50),
	_Address varchar(250),
    _CompanyName varchar(100),
    _AdminId bigint,
    out _ProcessingResult varchar(50)
    
/*

	Set @out = '';
    Call sp_UserDetail_Ins(-1, 'Mukesh', 'kumar', '9998781000', 'mukesh@gmail.com', null, 'BottomHalf Pvt. Ltd.', null, @out);
    Select @out


*/    
    
    
)
Begin
	Declare Exit handler for sqlexception
	Begin
		Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
		Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
		Call sp_LogException (@Message, @OperationStatus, 'sp_UserDetail_Ins', 1, 0, @Result);
	end;    
	Begin
		Set _ProcessingResult = Fn_Generate_newPassword();
        -- Select _ProcessingResult;
        
		If not exists(Select 1 from UserDetail Where Mobile = _MobileNo Or EmailId = _EmailId Or UserId = _UserId) then
		Begin
			Insert into UserDetail Values (Default, 
				_FirstName,
				_LastName,
				_Address,
				_EmailId,
				_MobileNo,
				_CompanyName,
				_AdminId,
				null,
				Now(),
				null
			);
			
			Insert into UserLogin values(Default,
				2,
				_ProcessingResult,
				_EmailId,
				_MobileNo,
				_AdminId,
				null,
				now(),
				null
			);
		End;
		End if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_userfiledetail_Upload` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_userfiledetail_Upload`(
	_FileId varchar(25),
	_FileOwnerId int,
	_FilePath varchar(500), 
	_FileName varchar(100),
	_FileExtension varchar(10),
    _UserTypeId int,
    _AdminId long
)
Begin
    Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, @OperationStatus, 'sp_userfiledetail_Upload', 1, 0, @Result);
		end;
        
        if not exists (select 1 from userfiledetail where FileId = _FileId) then
			Begin
            select * from userfiledetail;
            
				insert into userfiledetail values (
					default,
                    _FileOwnerId,
                    _FilePath,
                    null,
					_FileName,
					_FileExtension,
                    0,
					_UserTypeId,
                    _AdminId,
                    null,
					utc_timestamp(), 
					null
				);
			End;
			Else
			Begin
				Update userfiledetail Set
					FilePath			=		_FilePath,
					FileName			=		_FileName,
					FileExtension		=		_FileExtension,
					UserTypeId			=		_UserTypeId,
					UpdatedBy			=		_AdminId,
					UpdatedOn			=		utc_date()
				Where FileId = _FileId;
			End;
			End if;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_Userlogin_Auth` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Userlogin_Auth`(

/*

	Call sp_Userlogin_Auth(0, NULL, 'istiyaq.mi9@gmail.com', 1, 'istiyak');
   
    Call sp_Userlogin_Auth(0, null, 'ravi@gmail.com', 2, '12345');

*/
	_UserId bigint,
	_MobileNo varchar(20),
	_EmailId varchar(50),
	_UserTypeId int    
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_Userlogin_Auth', 1, 0, @Result);
		end;  

		Set @RoutePrefix = '';
		Set @UserExistsFlag = 0;
		Begin
			Set @pass = '';
			Select Password from userlogin
			where (Email = _EmailId Or Mobile = _MobileNo) and UserTypeId = _UserTypeId
			into @pass;

			Set @UserExistsFlag = 1;
		   
			If (@UserExistsFlag = 1)then
			Begin
			Set @RoutePrefix = 'admin';
			Set @AccessLevelId = 0;
				Select AccessLevelId from userlogin
				Where Email = _EmailId Or Mobile = _MobileNo
				into @AccessLevelId;
			   
				if(_UserTypeId = 1) then
				begin
					Select
						UserId,
						FirstName,
						LastName,
						Address,
						EmailId,
						Mobile,
						CompanyName,
						@AccessLevelId RoleId,
						_UserTypeId UserTypeId
					from userdetail
					Where EmailId = _EmailId Or Mobile = _MobileNo;
				end;
				else
				begin
					Set @RoutePrefix = 'user';
					Select
						EmployeeUid UserId,
						FirstName,
						LastName,
						'NA' Address,
						Email,
						Mobile,
						@AccessLevelId RoleId,
						_UserTypeId UserTypeId
					from employees
					Where Email = _EmailId Or Mobile = _MobileNo;
				end;
				end if;
			   
				if(@AccessLevelId = 1) then
				begin
					Select RM.Catagory, RM.Childs, concat(@RoutePrefix, '/', RM.Link) Link, RM.Icon, RM.Badge,
					RM.BadgeType, RM.AccessCode,  1 as Permission from rolesandmenu RM
					where Catagory <> 'Home' or Childs <> 'Home';
				end;
				else
				begin                  
					Select RM.Catagory, RM.Childs, concat(@RoutePrefix, '/', RM.Link) Link, RM.Icon, RM.Badge,
					RM.BadgeType, RM.AccessCode,
					AccessibilityId Permission from rolesandmenu RM
					left Join role_accessibility_mapping r on r.AccessCode = RM.AccessCode
					where r.AccessLevelId = @AccessLevelId
					and r.AccessibilityId > 0;
				end;
				end if;
			End;
			End if;
		End;
	End;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Final view structure for view `randview`
--

/*!50001 DROP VIEW IF EXISTS `randview`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`root`@`localhost` SQL SECURITY DEFINER */
/*!50001 VIEW `randview` AS select rand() AS `Value` */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2022-10-15 18:44:29
