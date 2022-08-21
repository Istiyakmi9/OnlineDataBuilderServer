drop table if exists approval_request;

CREATE TABLE `approval_request` (
  `ApprovalRequestId` bigint NOT NULL AUTO_INCREMENT,
  `UserName` varchar(100) DEFAULT NULL,
  `Message` varchar(500) DEFAULT NULL,
  `UserId` bigint DEFAULT NULL,
  `UserTypeId` int DEFAULT NULL,
  `RequestedOn` datetime DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Mobile` varchar(14) DEFAULT NULL,
  `FromDate` datetime DEFAULT NULL,
  `ToDate` datetime DEFAULT NULL,
  `AssigneeId` bigint DEFAULT NULL,
  `ProjectId` bigint DEFAULT NULL,
  `ProjectName` varchar(100) DEFAULT NULL,
  `RequestStatusId` int DEFAULT '2',
  `AttendanceId` bigint NOT NULL,
  `RequestType` int DEFAULT NULL,
  `LeaveType` int DEFAULT NULL,
  `LeaveRequestId` bigint DEFAULT NULL,
  PRIMARY KEY (`ApprovalRequestId`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;




drop table if exists leave_plan;

create table leave_plan(
	LeavePlanId int primary key auto_increment,
    PlanName varchar(50),
    PlanDescription varchar(250),
    AssociatedPlanTypes Json,
    PlanStartCalendarDate datetime null,
    IsShowLeavePolicy bit,
    IsUploadedCustomLeavePolicy bit,
    IsDefaultPlan bit
);


DELIMITER $$

drop procedure if exists sp_leave_plan_insupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plan_insupd`(
	   /*

	set @result = '';
    Call sp_leave_plan_insupd(0, 'Test', 'Test group', @result);
	select @result;

*/
	_LeavePlanId int,
    _PlanName varchar(50),
    _PlanDescription varchar(250),
    _AssociatedPlanTypes Json,
    _PlanStartCalendarDate datetime,
    _IsShowLeavePolicy bit,
    _IsUploadedCustomLeavePolicy bit,
    _IsDefaultPlan bit,
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
				_PlanName,
				_PlanDescription,
                _AssociatedPlanTypes,
				_PlanStartCalendarDate,
				_IsShowLeavePolicy,
				_IsUploadedCustomLeavePolicy,
                _IsDefaultPlan
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
                IsDefaultPlan					= 				_IsDefaultPlan
			Where LeavePlanId = _LeavePlanId;
			
			Set _ProcessingResult = 'updated';
		End;
		End if;
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_leave_plans_get $$

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
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists sp_leave_plans_getbyId $$

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
End$$
DELIMITER ;




drop table if exists leave_plans_type;

create table leave_plans_type(
	LeavePlanTypeId int primary key auto_increment,
    LeavePlanCode varchar(10),
    PlanName varchar(50),
    PlanDescription varchar(250),
    MaxLeaveLimit int,
    ShowDescription bit,
    IsPaidLeave bit,
    IsSickLeave bit,
    IsStatutoryLeave bit,
    IsMale bit,
    IsMarried bit,
    IsRestrictOnGender bit,
    IsRestrictOnMaritalStatus bit,
    Reasons json,
    PlanConfigurationDetail json,
	CreatedBy bigint,
	UpdatedBy bigint,
	CreatedOn Datetime,
	UpdatedOn Datetime
);


DELIMITER $$

drop procedure if exists sp_leave_plans_type_insupd $$

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
            Call sp_LogException (@Message, @OperationStatus, 'sp_leave_plans_type_insupd', 1, 0, @Result);
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
End$$
DELIMITER ;





DELIMITER $$

drop procedure if exists sp_leave_plans_type_get $$

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
End$$
DELIMITER ;




DELIMITER $$

drop procedure if exists sp_leave_plans_type_getbyId $$

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
End$$
DELIMITER ;



drop table if exists leave_detail;
create table leave_detail(
	LeaveDetailId int primary key auto_increment,
    LeavePlanTypeId int,
    IsLeaveDaysLimit bit,
    LeaveLimit int,
    CanApplyExtraLeave bit,
    ExtraLeaveLimit int,
    IsNoLeaveAfterDate bit,
    LeaveNotAllocatedIfJoinAfter int,
	CanManagerAwardCausalLeave bit,
	CanCompoffAllocatedAutomatically bit,
    CanCompoffCreditedByManager bit
);
    
    
DELIMITER $$

drop procedure if exists sp_leave_detail_insupd $$

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
	_CanManagerAwardCausalLeave bit,
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
					_CanManagerAwardCausalLeave,
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
					CanManagerAwardCausalLeave				=				_CanManagerAwardCausalLeave,
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
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists sp_leave_plan_upd_configuration $$

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
End$$
DELIMITER ;

    
    
drop table if exists leave_accrual;
    
create table leave_accrual (    
    LeaveAccrualId int primary key auto_increment,
    LeavePlanTypeId int,
    CanApplyEntireLeave bit,
    IsLeaveAccruedPatternAvail bit,
    JoiningMonthLeaveDistribution json,
    ExitMonthLeaveDistribution json,
    LeaveDistributionSequence varchar(45), -- leave can be given to employee yearly, monthly or quaterly 
    -- e.g total leave 24 and quaterly option opted then after every quater 4 leave will be added to the employees bucket
    LeaveDistributionAppliedFrom decimal,  -- from which date of the month leave can to given to employee
    IsLeavesProratedForJoinigMonth bit,
    IsLeavesProratedOnNotice bit,
    IsNotAllowProratedOnNotice bit,
    IsNoLeaveOnNoticePeriod bit,
    IsVaryOnProbationOrExprience bit,
	IsAccrualStartsAfterJoining bit,
	IsAccrualStartsAfterProbationEnds bit,
	AccrualDaysAfterJoining decimal,
	AccrualDaysAfterProbationEnds decimal,
    AccrualProrateDetail json,
    IsImpactedOnWorkDaysEveryMonth bit,
    WeekOffAsAbsentIfAttendaceLessThen decimal,
    HolidayAsAbsentIfAttendaceLessThen decimal,    
    CanApplyForFutureDate bit,
    IsExtraLeaveBeyondAccruedBalance bit,
    IsNoExtraLeaveBeyondAccruedBalance bit,
    NoOfDaysForExtraLeave decimal,
    IsAccrueIfHavingLeaveBalance bit,
    AllowOnlyIfAccrueBalanceIsAlleast decimal,
    IsAccrueIfOnOtherLeave bit,
    NotAllowIfAlreadyOnLeaveMoreThan decimal,    
    RoundOffLeaveBalance bit,
    ToNearestHalfDay bit,
    ToNearestFullDay bit,
    ToNextAvailableHalfDay bit,
    ToNextAvailableFullDay bit,
    ToPreviousHalfDay bit,    
    DoesLeaveExpireAfterSomeTime bit,
    AfterHowManyDays decimal
);


    
drop table if exists leave_apply_detail;    
    
create table leave_apply_detail (    
    LeaveApplyDetailId int primary key auto_increment,
    LeavePlanTypeId int,
    IsAllowForHalfDay bit,
    EmployeeCanSeeAndApplyCurrentPlanLeave bit,
    RuleForLeaveInNotice json,
    ApplyPriorBeforeLeaveDate int,
    BackDateLeaveApplyNotBeyondDays int,
    RestrictBackDateLeaveApplyAfter int,
    CurrentLeaveRequiredComments bit,
    ProofRequiredIfDaysExceeds bit,
    NoOfDaysExceeded int
);



DELIMITER $$

drop procedure if exists sp_leave_apply_detail_InsUpdate $$

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
end$$
DELIMITER ;


drop table if exists leave_plan_restriction;
    
create table leave_plan_restriction (    
    LeavePlanRestrictionId int primary key auto_increment,
    LeavePlanId int,
    CanApplyAfterProbation bit,
    CanApplyAfterJoining bit,
    DaysAfterProbation int,
    DaysAfterJoining int,
    IsAvailRestrictedLeavesInProbation bit,
    LeaveLimitInProbation decimal,
    
    IsConsecutiveLeaveLimit bit,
    ConsecutiveDaysLimit int,
    -- this text will be changed based onthe given field below
    
    IsLeaveInNoticeExtendsNoticePeriod bit,
    NoOfTimesNoticePeriodExtended decimal,
    
    CanManageOverrideLeaveRestriction bit,
    
    GapBetweenTwoConsicutiveLeaveDates decimal,
    LimitOfMaximumLeavesInCalendarYear decimal,
    LimitOfMaximumLeavesInCalendarMonth decimal,    
    LimitOfMaximumLeavesInEntireTenure decimal,
    MinLeaveToApplyDependsOnAvailable decimal,
    AvailableLeaves decimal,    
    RestrictFromDayOfEveryMonth int,
    
    IsCurrentPlanDepnedsOnOtherPlan bit,
    -- id of other leave plan 
    -- e.g if 2 leave in sick and 1 in causal is pending if this field is for sick and restrict to not combine with cause
    -- and if one apply for 3 days assuming to combine sick and cause then based on this restrict system should stop here.
    AssociatedPlanTypeId int,
    IsCheckOtherPlanTypeBalance bit,
    DependentPlanTypeId int
);    


DELIMITER $$

drop procedure if exists sp_leave_plan_restriction_insupd $$

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
    _IsConsecutiveLeaveLimit bit,
    _ConsecutiveDaysLimit int,
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
				_IsConsecutiveLeaveLimit,
				_ConsecutiveDaysLimit,
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
				IsConsecutiveLeaveLimit							=						_IsConsecutiveLeaveLimit,
				ConsecutiveDaysLimit							=						_ConsecutiveDaysLimit,
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
End$$
DELIMITER ;



drop table if exists leave_holidays_and_weekoff;
    
create table leave_holidays_and_weekoff (
    LeaveHolidaysAndWeekOffId int primary key auto_increment,
    LeavePlanTypeId int,
    AdJoiningHolidayIsConsiderAsLeave bit,
    ConsiderLeaveIfNumOfDays decimal,
    IfLeaveLieBetweenTwoHolidays bit,
    IfHolidayIsRightBeforLeave bit,
    IfHolidayIsRightAfterLeave bit,
    IfHolidayIsBetweenLeave bit,
    IfHolidayIsRightBeforeAfterOrInBetween bit,
    AdjoiningHolidayRulesIsValidForHalfDay bit,
    AdjoiningWeekOffIsConsiderAsLeave bit,
    ConsiderLeaveIfIncludeDays decimal,
    IfLeaveLieBetweenWeekOff bit,
    IfWeekOffIsRightBeforLeave bit,
    IfWeekOffIsRightAfterLeave bit,
    IfWeekOffIsBetweenLeave bit,
    IfWeekOffIsRightBeforeAfterOrInBetween bit,
    AdjoiningWeekOffRulesIsValidForHalfDay bit,
    ClubSandwichPolicy bit
);



DELIMITER $$

drop procedure if exists sp_leave_holidays_and_weekoff_insupd $$

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
End$$
DELIMITER ;

    
drop table if exists leave_approval;
create table leave_approval (    
    LeaveApprovalId int primary key auto_increment,
    LeavePlanTypeId int,
    IsLeaveRequiredApproval bit,
    ApprovalLevels int,
    ApprovalChain json,
    IsRequiredAllLevelApproval bit,
    CanHigherRankPersonsIsAvailForAction bit,
    IsPauseForApprovalNotification bit,
    IsReportingManageIsDefaultForAction bit
);



DELIMITER $$

drop procedure if exists sp_leave_approval_insupd $$

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
End$$
DELIMITER ;
    

drop table if exists leave_endyear_processing;    
create table leave_endyear_processing ( 
    LeaveEndYearProcessingId int primary key auto_increment,
	LeavePlanTypeId int,
	IsLeaveBalanceExpiredOnEndOfYear bit,
	AllConvertedToPaid bit,
	AllLeavesCarryForwardToNextYear bit,
	PayFirstNCarryForwordRemaning bit,
	CarryForwordFirstNPayRemaning bit,
	PayNCarryForwardForPercent bit,
	PayNCarryForwardDefineType varchar(50),
	FixedPayNCarryForward json,
	PercentagePayNCarryForward json,
	DoestCarryForwardExpired bit,
	ExpiredAfter decimal,
	DoesExpiryLeaveRemainUnchange bit,
	DeductFromSalaryOnYearChange bit,
	ResetBalanceToZero bit,
	CarryForwardToNextYear bit
);




DELIMITER $$

drop procedure if exists sp_leave_endyear_processing_insupd $$

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
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists SP_Employee_GetAll $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employee_GetAll`(
/*	

	Call SP_Employee_GetAll(' 1=1', '', 1, 10, 1);

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
							JSON_ARRAYAGG(ClientUid)
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
							JSON_ARRAYAGG(ClientUid)
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



DELIMITER $$

drop procedure if exists sp_Employees_InsUpdate $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_Employees_InsUpdate`(
	_EmployeeUid bigint,
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
				if not exists (select 1 from employee_archive where EmployeeUid = _EmployeeUid) then
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
						Email, LeavePlanId, PayrollGroupId, NoticePeriodAppiedOn, IsActive, CreatedBy, UpdatedBy,  CreatedOn, UpdatedOn, 
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
						MotherName, Address,  State,  City, Pincode, IsPermanent, ActualPackage, FinalPackage, TakeHomeByCandidate, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn
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
						null
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
                        _CompanyId,
						_AdminId, 
						null, 
						utc_date(), 
						null
					);
                end;
                else 
                begin
					Set @EmpId = _EmployeeUid;
					Update employee_archive SET 
						FirstName				=		_FirstName,
						LastName				=		_LastName,
						Mobile					=		_Mobile,
						Email					=		_Email,
						UpdatedBy				=		_AdminId, 
						UpdatedOn				=		now(),
                        ReportingManagerId		=		_ReportingManagerId,
                        DesignationId			=		_DesignationId,
                        UserTypeId				=		_UserTypeId
					Where 	EmployeeUid 	= _EmployeeUid;
                
					Update employeepersonaldetail_archive Set
						Mobile						=	_Mobile,
						SecondaryMobile				=	_SecondaryMobile,
						Email						=	_Email,
						Gender						=	_Gender,
						FatherName					=	_FatherName,
						SpouseName					=	_SpouseName,
						MotherName					=	_MotherName,
						Address						=	_Address, 
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
                
					Update	employeeprofessiondetail_archive Set
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
						Email 		= _Email,
						UserTypeId 	= _UserTypeId,
						Mobile 		= _Mobile,
                        CompanyId	= _CompanyId,
						UpdatedBy 	= _AdminId,
						UpdatedOn   = utc_date()
					Where EmployeeId = _EmployeeUid;
                end;
                end if;				
			End;
			Else
			Begin
				Set _ProcessingResult = '0';
				Set @EmpId = _EmployeeUid;
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
						Email 		= 	_Email,
                        UserTypeId 	=	_UserTypeId,
						Mobile 		= 	_Mobile,
                        CompanyId	= _CompanyId,
						UpdatedBy 	= 	_AdminId,
						UpdatedOn   = 	utc_date()
					Where EmployeeId = _EmployeeUid;
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
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists SP_ApplicationData_Get $$

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
			Select *
            from clients;            
            
            Select e.EmployeeUid,
            e.FirstName,
            e.LastName,
            e.Mobile,
            e.Email,
            e.IsActive,
            ep.AadharNo,
            ep.PANNo,
            ep.AccountNumber,
            ep.BankName,
            ep.IFSCCode,
            ep.Domain,
            ep.Specification,
            ep.ExprienceInYear,
            0 ActualPackage,
            0 FinalPackage,
			ReportingManagerId,
            DesignationId,
            0 TakeHomeByCandidate,
			(
				Select 
					JSON_ARRAYAGG(ClientUid)
				from employeemappedclients 
				where EmployeeUid = e.EmployeeUid
			) as ClientJson
            from employees e 
            Inner Join employeeprofessiondetail ep on ep.EmployeeUid = e.EmployeeUid;
            
            select * from accesslevel;
            
            select * from company;
		End;
	End;
end$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_leave_plan_upd_configuration $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plan_upd_configuration`(
/*
	
    Call sp_leave_plan_upd_configuration(1, '[]');
    
*/

	_LeavePlanTypeId int,
    _LeavePlanConfiguration json,
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
        
        set _ProcessingResult = 'updated';
	End;
End$$
DELIMITER ;


alter table leave_plan
modify column IsDefaultPlan bit default false;




DELIMITER $$

drop procedure if exists SP_ApplicationData_Get $$

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
			Select *
            from clients;            
            
            Select e.EmployeeUid,
            e.FirstName,
            e.LastName,
            e.Mobile,
            e.Email,
            e.IsActive,
            ep.AadharNo,
            ep.PANNo,
            ep.AccountNumber,
            ep.BankName,
            ep.IFSCCode,
            ep.Domain,
            ep.Specification,
            ep.ExprienceInYear,
            0 ActualPackage,
            0 FinalPackage,
			ReportingManagerId,
            DesignationId,
            0 TakeHomeByCandidate,
			(
				Select 
					JSON_ARRAYAGG(ClientUid)
				from employeemappedclients 
				where EmployeeUid = e.EmployeeUid
			) as ClientJson
            from employees e 
            Inner Join employeeprofessiondetail ep on ep.EmployeeUid = e.EmployeeUid;
            
            select * from accesslevel;
            
            select * from company;
            
            select * from employeemappedclients;
		End;
	End;
end$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_leave_plan_set_default $$

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
End$$
DELIMITER ;




DELIMITER $$

drop procedure if exists sp_leave_page_getby_employeeId $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_detail_getby_employeeId`(
/*
	
    Call sp_leave_detail_getby_employeeId(1);
    
    
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
        
        if exists(select 1 from employee_leaveplan_mapping where EmployeeId = _EmployeeId) then
        begin
			select lp.* from leave_plan lp
            inner join employee_leaveplan_mapping lm on lp.LeavePlanId = lm.LeavePlanId;
        end;
        else
        begin
			select * from leave_plan
            where IsDefaultPlan = 1;
        end;
        end if;
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_leave_plan_upd_configuration $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_plan_upd_configuration`(
/*
	
    Call sp_leave_plan_upd_configuration(1, '[]');
    
*/

	_LeavePlanTypeId int,
    _LeavePlanConfiguration json,
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
        
        set _ProcessingResult = 'updated';
	End;
End$$
DELIMITER ;




DELIMITER $$

drop procedure if exists sp_employee_declaration_insupd $$

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
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_employee_declaration_get_byEmployeeId $$

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
End$$
DELIMITER ;


DELIMITER $$

drop procedure if exists sp_employee_declaration_get_byId $$

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
End$$
DELIMITER ;



drop table if exists employee_leave_request;

CREATE TABLE `employee_leave_request` (
  `LeaveRequestId` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeId` bigint DEFAULT NULL,
  `LeaveDetail` json DEFAULT NULL,
  `Year` int,
  PRIMARY KEY (`LeaveRequestId`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;





DELIMITER $$

drop procedure if exists sp_employee_leave_request_GetById $$

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
End$$
DELIMITER ;




DELIMITER $$

drop procedure if exists sp_employee_leave_request_detail $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_leave_request_detail`(
	_EmployeeId bigint,
	_Year int
/*	

	Call sp_employee_leave_request_detail(4, 2022);

*/

)
Begin
	Declare exit handler for sqlexception
	Begin
	
		GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
									@errorno = MYSQL_ERRNO,
									@errortext = MESSAGE_TEXT;
									
		Set @Message = CONCAT('ERROR ', @errorno ,  ' (', @sqlstate, '): ', @errortext);
		Call sp_LogException(@Message, '', 'sp_employee_leave_request_detail', 1, 0, @Result);
	End;
									
	select * from employee_leave_request
	where EmployeeId = _EmployeeId and 
	`Year` = _Year;
    
    Set @LeavePlanId = 0;
    if exists (select 1 from employee_leaveplan_mapping where EmployeeId = _EmployeeId) then
    begin
		Select LeavePlanId into @LeavePlanId from employee_leaveplan_mapping
		where EmployeeId = _EmployeeId;
    end;
    end if;
    
    if (@LeavePlanId = 0) then
    begin
		Select * from leave_plan
        where IsDefaultPlan = 1;
    end;
    else
    begin
		Select * from leave_plan
        where LeavePlanId = @LeavePlanId;
    end;
    end if;
    
	select * from employees
	where DesignationId = 1 Or DesignationId = 4;	
end$$
DELIMITER ;




DELIMITER $$

drop procedure if exists sp_employee_leave_request_InsUpdate $$

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
					_Year
				);
			End;
			Else
			Begin
				Select LeaveRequestId into @LeaveRequestId from employee_leave_request 
                Where LeaveRequestId = _LeaveRequestId Or (EmployeeId = _EmployeeId and `Year` = _Year);
				
                Update employee_leave_request SET 
					LeaveDetail		=	_LeaveDetail,
                    Year			=	_Year
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
End$$
DELIMITER ;




DELIMITER $$

drop procedure if exists sp_employee_declaration_ins_json $$

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
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_approval_request_GetById $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_approval_request_GetById`(
	_ApprovalRequestId bigint,
    _LeaveRequestId int,
    _RequestType int
    
#	Call sp_approval_request_GetById(31, 0, 2);    
    
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
						Set LeaveDetail = _LeaveDetail
					where LeaveRequestId = _LeaveRequestId;                
                end;
                end if;
                
                Set _ProcessingResult = 'updated';
			End;
			End if;
            
			Select * from approval_request
			where AssigneeId = _ManagerId
			and RequestStatusId = 2
			order by RequestedOn desc;        
            
            COMMIT;
		End;
	End;
End$$
DELIMITER ;




DELIMITER $$

drop procedure if exists sp_attendance_update_timesheet $$

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
                @outCome
			);
		End;
        Commit;
	End;
End$$
DELIMITER ;





DELIMITER $$

drop procedure if exists sp_attendance_get_pending_requests $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attendance_get_pending_requests`(

/*

    Call sp_attendance_get_pending_requests(0, 0);

*/

    _ManagerId bigint,
    _StatusId int
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
			Call sp_LogException (@Message, @OperationStatus, 'sp_attendance_get_pending_requests', 1, 0, @Result);
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
	End;
End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_attandence_detail_by_employeeId $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_attandence_detail_by_employeeId`(

/*

    Call sp_attandence_detail_by_employeeId(4);

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
End$$
DELIMITER ;




drop table if exists employee_timesheet;

create table employee_timesheet(
	TimesheetId bigint primary key auto_increment,
    EmployeeId bigint,
    ClientId bigint,
    UserTypeId int,
    TimesheetMonthJson Json,
    TotalDays int,
    DaysAbsent int,
    ExpectedBurnedMinutes int,
    ActualBurnedMinutes int,
    TotalWeekDays int,
    TotalWorkingDays int,
    TotalHolidays int,
    MonthTimesheetApprovalState int,
    ForYear int,
    ForMonth int, 
    SubmittedOn Datetime,
    UpdatedOn datetime null,
    SubmittedBy bigint,
    UpdatedBy bigint null
);



DELIMITER $$

drop procedure if exists sp_employee_timesheet_get $$

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
End$$
DELIMITER ;




DELIMITER $$

drop procedure if exists sp_timesheet_insupd $$

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
                End;
                End if;	
                
                Set _ProcessingResult = 'updated';
			End;
		End;
	End$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_salary_group_get_initial_components;

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
End$$
DELIMITER ;





DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_salary_components_group_by_employeeid`(

/*

	Call sp_salary_components_group_by_employeeid(0, 1000000);

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
        
        if(_CTC = 0 And _EmployeeId <= 0) then
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




DELIMITER $$

drop procedure if exists sp_salary_group_get_by_ctc $$

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
End$$
DELIMITER ;





DELIMITER $$
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
End$$
DELIMITER ;




DELIMITER $$

drop procedure if exists SP_Employees_ById $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_Employees_ById`(
	_EmployeeId int,
    _IsActive int
/*	

	Call SP_Employees_ById(11, 1);

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
	End;
end$$
DELIMITER ;



alter table employees
add column SalaryGroupId int default 0;

alter table employees
add column CompanyId int default 0;

alter table employees
add column NoticePeriodId int default 0;


DELIMITER $$

drop procedure if exists sp_employee_leaveplan_upd $$

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
end$$
DELIMITER ;



drop table if exists employee_notice_period;

create table employee_notice_period (
	EmployeeNoticePeriodId bigint primary key auto_increment,
    EmployeeId bigint unique key,
    ApprovedOn datetime,
    ApplicableFrom datetime,
    ApproverManagerId int,
    ManagerDescription varchar(500),
    AttachmentPath varchar(200),
    EmailTitle varchar(100),
    OtherApproverManagerIds text,
    ITClearanceStatus int,
    ReportingManagerClearanceStatus int,
    CanteenClearanceStatus int,
    ClientClearanceStatus int,
    HRClearanceStatus int,
    OfficialLastWorkingDay datetime,
    PeriodDuration int,
    EarlyLeaveStatus int,
    Reason varchar(500),
    CreatedBy bigint,
    UpdatedBy bigint,
    CreatedOn datetime,
    UpdatedOn datetime
);


drop table if exists employee_notice_period_desc;

create table employee_notice_period_desc (
	StatusId bigint primary key auto_increment,
    Status varchar(50),
    StatusDecription varchar(100)
);


DELIMITER $$

drop procedure if exists sp_employee_notice_period_insupd $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_employee_notice_period_insupd`(	
    _EmployeeNoticePeriodId bigint,
    _EmployeeId bigint,
    _ApprovedOn datetime,
    _ApplicableFrom datetime,
    _ApproverManagerId int,
    _ManagerDescription varchar(500),
    _AttachmentPath varchar(200),
    _EmailTitle varchar(100),
    _OtherApproverManagerIds text,
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
end$$
DELIMITER ;


drop table if exists company_setting;

create table company_setting (
	SettingId bigint primary key auto_increment,
    CompanyId int,
    ProbationPeriodInDays int,
    NoticePeriodInDays int,
    CreatedBy bigint,
    UpdatedBy bigint,
    CreatedOn datetime,
    UpdatedOn datetime
); 

DELIMITER $$

drop procedure if exists sp_employee_notice_period_insupd $$

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
end$$
DELIMITER ;



DELIMITER $$

drop procedure if exists sp_leave_plan_calculation_get $$

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
        
        Drop table if exists employeeDetail;
	End;
end$$
DELIMITER ;

