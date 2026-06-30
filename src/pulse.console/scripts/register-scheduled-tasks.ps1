param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$PendingTaskName = "Pulse-PendingTasksSummary",
    [string]$NotificationTaskName = "Pulse-NotificationsTable",
    [string]$MilestoneReadyTaskName = "Pulse-MilestoneReadyNotifications",
    [datetime]$DailyStart = (Get-Date -Hour 8 -Minute 0 -Second 0),
    [int]$NotificationIntervalMinutes = 30
)

$ErrorActionPreference = "Stop"

$projectRootPath = (Resolve-Path $ProjectRoot).Path
$powershellExe = Join-Path $env:WINDIR "System32\WindowsPowerShell\v1.0\powershell.exe"
$pendingScript = Join-Path $projectRootPath "scripts\run-pending-tasks-summary.ps1"
$notificationScript = Join-Path $projectRootPath "scripts\run-notifications-table.ps1"
$milestoneReadyScript = Join-Path $projectRootPath "scripts\run-milestone-ready-notifications.ps1"

if (-not (Test-Path $pendingScript)) {
    throw "Pending task runner not found: $pendingScript"
}

if (-not (Test-Path $notificationScript)) {
    throw "Notifications runner not found: $notificationScript"
}

if (-not (Test-Path $milestoneReadyScript)) {
    throw "Milestone-ready runner not found: $milestoneReadyScript"
}

$pendingAction = New-ScheduledTaskAction -Execute $powershellExe -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$pendingScript`""
$notificationAction = New-ScheduledTaskAction -Execute $powershellExe -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$notificationScript`""
$milestoneReadyAction = New-ScheduledTaskAction -Execute $powershellExe -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$milestoneReadyScript`""

$pendingTrigger = New-ScheduledTaskTrigger -Daily -At $DailyStart
$notificationTrigger = New-ScheduledTaskTrigger -Once -At $DailyStart -RepetitionInterval (New-TimeSpan -Minutes $NotificationIntervalMinutes) -RepetitionDuration (New-TimeSpan -Days 3650)
$milestoneReadyTrigger = New-ScheduledTaskTrigger -Once -At $DailyStart -RepetitionInterval (New-TimeSpan -Minutes $NotificationIntervalMinutes) -RepetitionDuration (New-TimeSpan -Days 3650)

Register-ScheduledTask -TaskName $PendingTaskName -Action $pendingAction -Trigger $pendingTrigger -Description "Pulse daily pending task summary email job" -Force | Out-Null
Register-ScheduledTask -TaskName $NotificationTaskName -Action $notificationAction -Trigger $notificationTrigger -Description "Pulse notifications table email delivery job" -Force | Out-Null
Register-ScheduledTask -TaskName $MilestoneReadyTaskName -Action $milestoneReadyAction -Trigger $milestoneReadyTrigger -Description "Pulse next milestone ready email delivery job" -Force | Out-Null

Get-ScheduledTask -TaskName $PendingTaskName, $NotificationTaskName, $MilestoneReadyTaskName |
    Select-Object TaskName, State |
    Format-Table -AutoSize
