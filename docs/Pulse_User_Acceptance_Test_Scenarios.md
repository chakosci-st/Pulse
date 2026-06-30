# Pulse User Acceptance Test Scenarios

## Document Control

| Item | Value |
|---|---|
| Application | Pulse |
| Document Type | User Acceptance Test Scenarios |
| Audience | Business users, project owners, template managers, administrators, QA testers |
| Created Date | May 28, 2026 |
| Scope | Functional UAT for Pulse web workflows |
| Status | Draft for execution |

## Purpose

This document defines extensive User Acceptance Test scenarios for validating Pulse from an end-user business workflow perspective. The scenarios cover project registration, project execution, task updates, collaboration, notifications, template setup, roadmap governance, reporting, and role-based visibility.

## Testing Objectives

- Confirm that business users can complete core Pulse workflows successfully.
- Validate that project owners can register, manage, configure, and monitor projects.
- Confirm that task owners can update assigned work, provide evidence, and close tasks.
- Validate that template and setup managers can maintain reusable configuration records.
- Confirm that collaboration features such as chats, comments, attachments, and notifications work as expected.
- Validate that reports support project monitoring, export, and comparison needs.
- Confirm that role-based access behaves correctly and prevents unauthorized actions.

## UAT Execution Guidance

Each tester should record the result for every scenario using one of these values:

| Result | Meaning |
|---|---|
| Pass | Scenario completed and expected result was met. |
| Fail | Scenario completed but expected result was not met. |
| Blocked | Scenario could not be executed due to missing setup, access, data, or system issue. |
| Not Applicable | Scenario does not apply to the tested role, plant, or deployment scope. |

For each executed scenario, capture evidence such as screenshots, exported files, project numbers, task IDs, notification references, chat timestamps, or defect IDs.

## Test Roles

| Role | Primary Test Focus |
|---|---|
| Daily User / Task Owner | Dashboard, Notifications, Tasks, comments, attachments, task closure |
| Project Owner | Register, Projects, Project Review, Project Configuration, Status Board |
| Project Member | Assigned work, collaboration, task updates, project review visibility |
| Template Manager | Categories, Fields, Forms, Maturity, Product Division, Product Group, Roadmap |
| Administrator | Access-controlled actions, setup governance, module visibility |
| Report Viewer | Project Export, Monitoring Matrix, Project Comparison |

## Required Test Data

Before execution, prepare or confirm the following data exists:

| Data Type | Minimum Requirement |
|---|---|
| Plants / Sites | At least one active plant/site available to testers |
| Users | At least two project users plus one administrator or template manager |
| Product Division | At least one active product division |
| Product Group | At least one active product group |
| Product Codes | At least two product codes for add/remove testing |
| Category | At least one active category |
| Maturity | At least one active maturity level |
| Roadmap | At least one roadmap with multiple milestones and tasks |
| Forms / Fields | At least one form with editable field values, including a text field for DMS number or similar |
| Project | At least one project in active execution and one project suitable for configuration changes |
| Files | One small test file for attachment upload and removal |

## Table Of Contents

1. Projects
2. Status Board
3. Tasks
4. View
5. Chats
6. Notifications
7. Categories
8. Fields
9. Forms
10. Maturity
11. Product Division
12. Product Group
13. Roadmap
14. Reports
15. Negative And Access Control Scenarios
16. Regression Checklist
17. Sign-Off
18. Defect Log

---

# 1. Projects

## UAT-PROJ-001: Register Project - Auto-Start On Specific Milestone

| Field | Detail |
|---|---|
| Objective | Verify that a project can be created and started directly on a selected milestone. |
| Primary Role | Project Owner |
| Preconditions | User has Register access. A roadmap exists with at least three milestones. Required plant, product, category, maturity, owner, and date values are available. |
| Test Data | Roadmap with Milestone 1, Milestone 2, and Milestone 3. Select Milestone 2 as starting milestone. |

### Steps

1. Open **Register**.
2. Enter required project details, including plant, title, owner, category, dates, product context, and roadmap/template selection.
3. Select the option to start the project immediately.
4. Choose a specific starting milestone, such as Milestone 2.
5. Save the project.
6. Open the created project from **Projects**, **Project Review**, or **Status Board**.
7. Verify the project status and milestone status.

### Expected Result

- The project is created successfully.
- The selected milestone is active, current, or positioned as the starting point according to Pulse rules.
- The project appears in project lists and status views.
- Progress and milestone indicators reflect the selected start state.

### Pass Criteria

- Tester can confirm the project started from the selected milestone.
- No required project data is lost after saving.

### Evidence To Capture

- Project number or ID.
- Screenshot of selected starting milestone.
- Screenshot of Project Review or Status Board after creation.

---

## UAT-PROJ-002: Register Project - Create Only To Be Started In Status Board

| Field | Detail |
|---|---|
| Objective | Verify that a project can be created without immediately starting execution. |
| Primary Role | Project Owner |
| Preconditions | User has Register access and project creation permission. |

### Steps

1. Open **Register**.
2. Enter all required project details.
3. Select the create-only, draft, inactive, or non-start option if available.
4. Save the project.
5. Open **Status Board**.
6. Search for or locate the created project.
7. Start or activate the project from Status Board if the test includes start behavior.

### Expected Result

- The project record is created successfully.
- The project is not actively progressing until started or moved from Status Board.
- The project can be found by authorized users.

### Pass Criteria

- The created project remains in the expected non-started state before Status Board action.

### Evidence To Capture

- Project number or ID.
- Screenshot of project status before start.
- Screenshot after start if start action is tested.

---

## UAT-PROJ-003: Project Review - Add ADHOC Task Using My Tasks

| Field | Detail |
|---|---|
| Objective | Verify that a user can add an unplanned ADHOC task from My Tasks in Project Review. |
| Primary Role | Project Owner or Project Member |
| Preconditions | Active project exists. User has permission to add tasks. |

### Steps

1. Open **Projects**.
2. Select an active project.
3. Open **Project Review**.
4. Go to **My Tasks** or the member task area.
5. Select the add task or ADHOC task action.
6. Enter task title, assignee, target date, and description.
7. Save the task.
8. Refresh the task list if needed.

### Expected Result

- The ADHOC task is created.
- The task appears in My Tasks and the project task list.
- The assignee can see the task if they have access.

### Pass Criteria

- Task is visible, editable according to role, and included in tracking.

### Evidence To Capture

- Task title and ID.
- Screenshot of task in My Tasks.

---

## UAT-PROJ-004: Project Review - Switch Project-Centric To Member-Centric

| Field | Detail |
|---|---|
| Objective | Verify that Project Review supports switching between project-centric and member-centric views. |
| Primary Role | Project Owner |
| Preconditions | Project has multiple tasks assigned to at least two members. |

### Steps

1. Open **Projects**.
2. Select a project with multiple assigned tasks.
3. Open **Project Review**.
4. Select the project-centric view.
5. Confirm tasks are grouped by milestone, phase, or project structure.
6. Switch to the member-centric view.
7. Confirm tasks are grouped by user/member.
8. Switch back to project-centric view.

### Expected Result

- Project-centric view displays the project execution structure.
- Member-centric view displays assignments by person.
- No task data changes unexpectedly while switching views.

### Pass Criteria

- Tester can navigate between both views and validate assignment accuracy.

---

## UAT-PROJ-005: Project Review - Add Attachment

| Field | Detail |
|---|---|
| Objective | Verify that attachments can be added to a project or task in Project Review. |
| Primary Role | Project Owner or Project Member |
| Preconditions | User has permission to upload attachments. Test file is available. |

### Steps

1. Open **Project Review** for an active project.
2. Select the relevant project item, milestone, or task.
3. Open **Attachments**.
4. Upload the prepared test file.
5. Save or confirm upload if required.
6. Reopen the attachment area.

### Expected Result

- Uploaded file appears in the attachment list.
- File name, uploader, and timestamp are shown where supported.
- File can be viewed or downloaded if supported.

### Pass Criteria

- Attachment persists after page refresh or reopening the record.

---

## UAT-PROJ-006: Project Review - Remove Attachment

| Field | Detail |
|---|---|
| Objective | Verify that permitted users can remove an attachment from a project or task. |
| Primary Role | Project Owner or Authorized Project Member |
| Preconditions | Attachment exists on the selected project or task. User has remove permission. |

### Steps

1. Open the project or task attachment list.
2. Select an existing test attachment.
3. Choose remove or delete.
4. Confirm the removal if prompted.
5. Refresh or reopen the attachment list.

### Expected Result

- Attachment is removed from the list.
- Unauthorized users cannot remove attachments.
- Removal does not delete unrelated comments, task data, or project data.

### Pass Criteria

- Attachment list accurately reflects the removal.

---

## UAT-PROJ-007: Project Review - Add Comment

| Field | Detail |
|---|---|
| Objective | Verify that comments can be added in project execution context. |
| Primary Role | Project Owner or Project Member |
| Preconditions | User has access to project review and comments. |

### Steps

1. Open **Project Review**.
2. Select the project, milestone, or task area where the comment should be recorded.
3. Open **Comments**.
4. Enter a comment with status, decision, blocker, or next action.
5. Save or post the comment.
6. Reopen the record.

### Expected Result

- Comment appears in the correct context.
- Comment displays author and timestamp where supported.
- Other authorized users can see the comment.

### Pass Criteria

- Comment remains visible after refresh and is associated with the correct item.

---

## UAT-PROJ-008: Project Review - Update Task Status

| Field | Detail |
|---|---|
| Objective | Verify that task status can be updated from Project Review. |
| Primary Role | Task Owner or Project Owner |
| Preconditions | Project has at least one editable open task. |

### Steps

1. Open **Project Review**.
2. Locate an open task.
3. Open task update controls.
4. Change status from not started to in progress.
5. Save.
6. Reopen the task.
7. Change status from in progress to completed or closed if allowed.
8. Save.

### Expected Result

- Task status changes are saved accurately.
- Project and milestone progress indicators update where applicable.
- Any validation messages appear when required fields are incomplete.

### Pass Criteria

- Task status persists after refresh.

---

## UAT-PROJ-009: Project Review - Update Task Target Date

| Field | Detail |
|---|---|
| Objective | Verify that task target dates can be updated and reflected in schedule views. |
| Primary Role | Task Owner or Project Owner |
| Preconditions | Editable task exists with target date field. |

### Steps

1. Open **Project Review**.
2. Select an editable task.
3. Change the target date to a future valid date.
4. Save.
5. Reopen the task.
6. Check schedule or project review indicators.

### Expected Result

- New target date is saved.
- Date displays consistently across Project Review, Tasks, and schedule-related views.
- Invalid date entries are rejected with a clear validation message.

### Pass Criteria

- Target date persists and is visible in expected task/project views.

---

## UAT-PROJ-010: Project Review - Update Task Form Value Such As DMS Number

| Field | Detail |
|---|---|
| Objective | Verify that task form values can be entered and saved. |
| Primary Role | Task Owner or Project Member |
| Preconditions | Task has a form field such as DMS number. User has edit permission. |

### Steps

1. Open **Project Review**.
2. Select a task with data-entry fields.
3. Enter or update a DMS number or equivalent form value.
4. Save the task.
5. Reopen the task.
6. Review the form value in the task details.

### Expected Result

- Form value is saved correctly.
- Required fields prevent closure if incomplete where configured.
- Values are displayed in Project Review and reports where applicable.

### Pass Criteria

- Saved form value persists after refresh.

---

## UAT-PROJ-011: Project Review - Check Progress Bars And Percentages

| Field | Detail |
|---|---|
| Objective | Verify that progress bars and percentages reflect task completion accurately. |
| Primary Role | Project Owner |
| Preconditions | Project has multiple tasks with different statuses. |

### Steps

1. Open **Project Review**.
2. Record current project and milestone progress percentages.
3. Complete or close one open task.
4. Save the task.
5. Refresh Project Review if needed.
6. Compare updated progress bars and percentages.

### Expected Result

- Progress bars and percentages update based on task status.
- Progress does not exceed 100%.
- Closed/completed tasks contribute correctly to progress calculations.

### Pass Criteria

- Tester can validate that progress changed in the expected direction after task update.

---

## UAT-PROJ-012: Project Review - Close Task

| Field | Detail |
|---|---|
| Objective | Verify that a task can be closed when required work is complete. |
| Primary Role | Task Owner or Project Owner |
| Preconditions | Open task exists. Required form values and evidence are available. |

### Steps

1. Open **Project Review**.
2. Select an open task.
3. Complete required data-entry fields.
4. Add final comment or attachment if required.
5. Set status to closed, completed, or equivalent.
6. Save.
7. Refresh the project review page.

### Expected Result

- Task closes successfully.
- Task is no longer treated as active open work.
- Progress indicators update.
- Closure is blocked if mandatory fields are missing.

### Pass Criteria

- Closed task remains closed after refresh and appears correctly in related views.

---

## UAT-PROJ-013: Project Review - Add Chat

| Field | Detail |
|---|---|
| Objective | Verify that a chat message can be added from Project Review. |
| Primary Role | Project Owner or Project Member |
| Preconditions | Project chat is enabled. At least two users have project access. |

### Steps

1. Open **Project Review**.
2. Locate the project chat area.
3. Enter a message related to project coordination.
4. Send the message.
5. Log in or test as another project user.
6. Confirm the message is visible.

### Expected Result

- Chat message is sent and displayed.
- Other authorized users can view and respond.
- Message remains tied to the project context.

### Pass Criteria

- Sent message is visible to intended project participants.

---

## UAT-PROJ-014: Project Configuration - Update Project Information

| Field | Detail |
|---|---|
| Objective | Verify that authorized users can update project information. |
| Primary Role | Project Owner |
| Preconditions | User has project configuration access. Existing project is available. |

### Steps

1. Open **Projects**.
2. Select the target project.
3. Open **Configure** or **Project Configuration**.
4. Update editable project fields such as title, description, owner, dates, category, or notes.
5. Save changes.
6. Open project details and Project Review.

### Expected Result

- Updated project information is saved.
- Changes appear consistently in project details, review, and lists.
- Read-only fields cannot be changed by unauthorized users.

### Pass Criteria

- Updated values persist after refresh.

---

## UAT-PROJ-015: Project Configuration - Add Product Code

| Field | Detail |
|---|---|
| Objective | Verify that product codes can be added to a project. |
| Primary Role | Project Owner |
| Preconditions | Available product codes exist. User has configuration access. |

### Steps

1. Open **Project Configuration**.
2. Go to the product code or product assignment section.
3. Add a product code that is not currently assigned.
4. Save.
5. Reopen the product assignment section.

### Expected Result

- Product code is added successfully.
- Project product scope reflects the added product.
- Duplicate product codes are prevented or handled clearly.

### Pass Criteria

- Added product code remains assigned after refresh.

---

## UAT-PROJ-016: Project Configuration - Remove Product Code

| Field | Detail |
|---|---|
| Objective | Verify that product codes can be removed from a project when allowed. |
| Primary Role | Project Owner |
| Preconditions | Project has at least two product codes or one removable test product code. |

### Steps

1. Open **Project Configuration**.
2. Go to product code assignment.
3. Select a removable product code.
4. Remove it.
5. Save.
6. Reopen the project configuration.

### Expected Result

- Product code is removed from project scope.
- Removal is blocked if the product code is required by business rules.
- Remaining product codes are unaffected.

### Pass Criteria

- Product assignment list reflects the expected removal.

---

## UAT-PROJ-017: Project Configuration - Add Member

| Field | Detail |
|---|---|
| Objective | Verify that project members can be added. |
| Primary Role | Project Owner or Administrator |
| Preconditions | User account exists and is eligible for project membership. |

### Steps

1. Open **Project Configuration**.
2. Go to **Members**.
3. Add a new member.
4. Assign role, responsibility, or member type if available.
5. Save.
6. Confirm the member appears in project membership and member-centric review.

### Expected Result

- New member is added successfully.
- Member can access the project according to role and plant scope.

### Pass Criteria

- Added member appears in project member list after refresh.

---

## UAT-PROJ-018: Project Configuration - Update Member

| Field | Detail |
|---|---|
| Objective | Verify that member role or responsibility can be updated. |
| Primary Role | Project Owner or Administrator |
| Preconditions | Project has at least one editable member. |

### Steps

1. Open **Project Configuration**.
2. Go to **Members**.
3. Select an existing member.
4. Update role, responsibility, assignment, or ownership where available.
5. Save.
6. Verify changes in Project Review and member-centric view.

### Expected Result

- Member changes are saved.
- Task ownership and member display update where applicable.

### Pass Criteria

- Updated member details persist after refresh.

---

## UAT-PROJ-019: Project Configuration - Remove Member

| Field | Detail |
|---|---|
| Objective | Verify that project members can be removed when allowed. |
| Primary Role | Project Owner or Administrator |
| Preconditions | Project has a removable member. Member does not own blocking open work, or reassignment rules are satisfied. |

### Steps

1. Open **Project Configuration**.
2. Go to **Members**.
3. Select a removable member.
4. Remove the member.
5. Confirm removal if prompted.
6. Save.
7. Verify the member no longer appears in project membership.

### Expected Result

- Member is removed or system clearly blocks removal due to open work or permissions.
- Removed member cannot access project-specific areas if access depends on membership.

### Pass Criteria

- Member list accurately reflects the removal or validation block.

---

## UAT-PROJ-020: Project Configuration - Add Notification

| Field | Detail |
|---|---|
| Objective | Verify that project notifications can be added. |
| Primary Role | Project Owner or Administrator |
| Preconditions | User has notification configuration access. Recipient user exists. |

### Steps

1. Open **Project Configuration**.
2. Go to **Notifications**.
3. Add a notification rule or reminder.
4. Enter recipient, timing, trigger, message, and due details as required.
5. Save.
6. Verify the notification appears in configuration.

### Expected Result

- Notification is created successfully.
- Notification details display accurately.

### Pass Criteria

- Notification remains configured after refresh.

---

## UAT-PROJ-021: Project Configuration - Update Notification

| Field | Detail |
|---|---|
| Objective | Verify that configured notifications can be updated. |
| Primary Role | Project Owner or Administrator |
| Preconditions | Existing notification is configured. |

### Steps

1. Open **Project Configuration**.
2. Go to **Notifications**.
3. Select an existing notification.
4. Update recipient, timing, trigger, or message.
5. Save.
6. Reopen notification details.

### Expected Result

- Notification changes are saved.
- Updated notification behavior is reflected when due or triggered.

### Pass Criteria

- Updated values persist after refresh.

---

## UAT-PROJ-022: Project Configuration - Remove Notification

| Field | Detail |
|---|---|
| Objective | Verify that project notifications can be removed. |
| Primary Role | Project Owner or Administrator |
| Preconditions | Removable notification exists. |

### Steps

1. Open **Project Configuration**.
2. Go to **Notifications**.
3. Select an existing notification.
4. Remove or delete the notification.
5. Confirm if prompted.
6. Save.

### Expected Result

- Notification is removed from configuration.
- Removed notification no longer appears as active or scheduled.

### Pass Criteria

- Notification list reflects the removal.

---

## UAT-PROJ-023: Project Configuration - Activate Project

| Field | Detail |
|---|---|
| Objective | Verify that a deactivated or inactive project can be activated. |
| Primary Role | Project Owner or Administrator |
| Preconditions | Project exists in inactive/deactivated state. |

### Steps

1. Open **Project Configuration**.
2. Locate project lifecycle or activation controls.
3. Select activate.
4. Confirm if prompted.
5. Open **Projects** or **Status Board**.

### Expected Result

- Project becomes active.
- Project appears in active project lists and status views according to access.

### Pass Criteria

- Project active status is visible and consistent across relevant pages.

---

## UAT-PROJ-024: Project Configuration - Deactivate Project

| Field | Detail |
|---|---|
| Objective | Verify that an active project can be deactivated or paused from active use. |
| Primary Role | Project Owner or Administrator |
| Preconditions | Active project exists and user has lifecycle permission. |

### Steps

1. Open **Project Configuration**.
2. Locate active/inactive controls.
3. Select deactivate.
4. Confirm if prompted.
5. Review project visibility in **Projects** and **Status Board**.

### Expected Result

- Project is deactivated according to system behavior.
- Historical project information is retained.
- Deactivated project is hidden or marked inactive where expected.

### Pass Criteria

- Project lifecycle status changes and persists.

---

## UAT-PROJ-025: Project Configuration - Delete Project

| Field | Detail |
|---|---|
| Objective | Verify that project deletion is available only to authorized users and behaves correctly. |
| Primary Role | Administrator or Authorized Project Owner |
| Preconditions | Test project exists specifically for deletion. No production record should be used. |

### Steps

1. Open the test project.
2. Open **Project Configuration**.
3. Select delete project.
4. Review warning or confirmation message.
5. Confirm deletion.
6. Search for the project in **Projects**, **View**, and **Status Board**.

### Expected Result

- Project is deleted, archived, or marked removed according to Pulse behavior.
- Deleted project no longer appears in active working lists.
- Unauthorized users cannot delete projects.

### Pass Criteria

- Delete behavior matches business expectations and governance rules.

---

## UAT-PROJ-026: Project Configuration - Retrieve Latest Roadmap Structure

| Field | Detail |
|---|---|
| Objective | Verify that an existing project can retrieve changes from its source roadmap. |
| Primary Role | Project Owner and Template Manager |
| Preconditions | Existing project uses a roadmap. User can update roadmap and configure project. |

### Steps

1. Open **Templates > Roadmap**.
2. Select the roadmap used by the test project.
3. Add a new milestone or task, or update an existing roadmap item.
4. Save the roadmap.
5. Open the project that uses this roadmap.
6. Open **Project Configuration**.
7. Select **Retrieve Latest Roadmap Structure**.
8. Review detected differences.
9. Apply selected updates.
10. Open **Project Review** to confirm the structure.

### Expected Result

- Pulse detects roadmap differences.
- Selected roadmap updates can be applied to the project.
- Existing project-specific execution data, comments, attachments, and completed task history are not lost.

### Pass Criteria

- Project structure reflects selected roadmap updates after retrieval.

---

## UAT-PROJ-027: Project Configuration - Insert Project Milestone

| Field | Detail |
|---|---|
| Objective | Verify that an additional milestone can be inserted into a project. |
| Primary Role | Project Owner |
| Preconditions | User has project configuration access. Project supports milestone insertion. |

### Steps

1. Open **Project Configuration**.
2. Locate milestone structure controls.
3. Select insert milestone.
4. Enter or select milestone details.
5. Choose placement if available.
6. Save.
7. Open **Project Review**.

### Expected Result

- Inserted milestone appears in the project structure.
- Milestone sequence and related tasks are correct.

### Pass Criteria

- New milestone persists and appears in expected project views.

---

## UAT-PROJ-028: Project Configuration - Arrange Milestones

| Field | Detail |
|---|---|
| Objective | Verify that project milestones can be reordered. |
| Primary Role | Project Owner |
| Preconditions | Project has at least three milestones and user has configuration access. |

### Steps

1. Open **Project Configuration**.
2. Locate milestone arrangement controls.
3. Move one milestone before or after another milestone.
4. Save.
5. Open **Project Review** and **Status Board**.

### Expected Result

- Milestone order updates correctly.
- Project Review and Status Board show the new sequence.
- Task and progress data remain associated with the correct milestones.

### Pass Criteria

- Milestone order persists after refresh.

---

## UAT-PROJ-029: Project View - Read Project Information

| Field | Detail |
|---|---|
| Objective | Verify that users can inspect project information in a read-oriented project view. |
| Primary Role | Viewer, Project Member, Project Owner |
| Preconditions | User has visibility to at least one project. |

### Steps

1. Open **View** or project view page.
2. Locate a project.
3. Open the project record.
4. Review summary information, schedule, ownership, progress, and risk indicators.
5. Confirm edit controls are hidden or disabled for read-only users.

### Expected Result

- Project information loads correctly.
- User sees only data and actions allowed by role.

### Pass Criteria

- Read-only project view is accurate and role appropriate.

---

## UAT-PROJ-030: Copy Project

| Field | Detail |
|---|---|
| Objective | Verify that users can copy an existing project to create a similar new project. |
| Primary Role | Project Owner |
| Preconditions | Source project exists. User has permission to copy/create projects. |

### Steps

1. Open **Projects**.
2. Select a source project.
3. Choose **Copy Project**.
4. Review copied details.
5. Update title, dates, owner, product codes, members, and other project-specific values.
6. Save the copied project.
7. Open the new project.

### Expected Result

- A new project is created from the source structure.
- Source project remains unchanged.
- Copied project values can be updated before or after saving as permitted.

### Pass Criteria

- New project is distinct from source project and has correct copied/updated values.

---

## UAT-PROJ-031: Quick Edit Project

| Field | Detail |
|---|---|
| Objective | Verify that common project fields can be updated quickly without full configuration. |
| Primary Role | Project Owner or Authorized User |
| Preconditions | Project list or workspace provides quick edit action. |

### Steps

1. Open **Projects**.
2. Locate the target project.
3. Select **Quick Edit**.
4. Update available fields such as status, owner, dates, category, or description where supported.
5. Save.
6. Reopen the project or list.

### Expected Result

- Quick edit changes are saved.
- Only quick-editable fields are available.
- Full configuration remains available for broader changes if permitted.

### Pass Criteria

- Updated values display correctly in project list and details.

---

# 2. Status Board

## UAT-STAT-001: Change Status At Project List Level

| Field | Detail |
|---|---|
| Objective | Verify that project-level status can be changed from Status Board. |
| Primary Role | Project Owner or Coordinator |
| Preconditions | Project appears in Status Board and user has status update permission. |

### Steps

1. Open **Status Board**.
2. Locate the target project in the project list or board.
3. Select the current project status.
4. Choose a new valid status.
5. Save or confirm the change.
6. Refresh the board.

### Expected Result

- Project status updates successfully.
- Project appears in the correct status grouping or indicator.
- Status change is visible in related project views.

### Pass Criteria

- Project-level status persists after refresh.

---

## UAT-STAT-002: Change Status At Milestone List Level

| Field | Detail |
|---|---|
| Objective | Verify that milestone-level status can be changed from Status Board. |
| Primary Role | Project Owner or Coordinator |
| Preconditions | Project has milestones visible in Status Board. |

### Steps

1. Open **Status Board**.
2. Expand or select a project.
3. Locate the milestone list.
4. Change a milestone status.
5. Save or confirm.
6. Review project progress and milestone indicators.

### Expected Result

- Milestone status updates successfully.
- Project progress or status indicators update where applicable.
- Milestone status is visible in Project Review.

### Pass Criteria

- Milestone status persists after refresh.

---

## UAT-STAT-003: Change Status At Task List Level

| Field | Detail |
|---|---|
| Objective | Verify that task-level status can be changed from Status Board. |
| Primary Role | Task Owner, Project Member, or Project Owner |
| Preconditions | Project has task list visible in Status Board. |

### Steps

1. Open **Status Board**.
2. Expand the target project and milestone.
3. Locate a task.
4. Change task status.
5. Save or confirm.
6. Open the same task from **Tasks** or **Project Review**.

### Expected Result

- Task status updates successfully.
- Related project and milestone progress reflect the update where applicable.
- If required fields are missing, closure is blocked with a validation message.

### Pass Criteria

- Task status is consistent across Status Board, Tasks, and Project Review.

---

# 3. Tasks

## UAT-TASK-001: Update Task

| Field | Detail |
|---|---|
| Objective | Verify that a user can update assigned task details. |
| Primary Role | Task Owner |
| Preconditions | User has at least one assigned editable task. |

### Steps

1. Open **Tasks**.
2. Select an assigned task.
3. Review task details, owner, status, target date, and required values.
4. Update status.
5. Update target date if allowed.
6. Save.
7. Reopen the task.

### Expected Result

- Task updates are saved.
- Changes display in Tasks, Project Review, and Status Board where applicable.

### Pass Criteria

- Updated task data persists after refresh.

---

## UAT-TASK-002: Update Task Data Entry

| Field | Detail |
|---|---|
| Objective | Verify that task data-entry fields can be updated. |
| Primary Role | Task Owner |
| Preconditions | Task includes editable data-entry fields. |

### Steps

1. Open **Tasks**.
2. Select a task with form fields.
3. Enter a DMS number or other required data-entry value.
4. Save.
5. Reopen the task.
6. Check the saved value.

### Expected Result

- Data-entry value is saved correctly.
- Required-field validation is enforced.

### Pass Criteria

- Data-entry value remains visible after refresh.

---

## UAT-TASK-003: Add Task Attachment

| Field | Detail |
|---|---|
| Objective | Verify that a task attachment can be added. |
| Primary Role | Task Owner or Project Member |
| Preconditions | User has task attachment permission and a test file. |

### Steps

1. Open **Tasks**.
2. Select a task.
3. Open **Attachments**.
4. Upload a test file.
5. Save or confirm.
6. Reopen attachments.

### Expected Result

- Uploaded file appears in task attachment list.
- File is associated with the correct task.

### Pass Criteria

- Attachment persists after refresh.

---

## UAT-TASK-004: View Task Attachment

| Field | Detail |
|---|---|
| Objective | Verify that a user can view or download a task attachment. |
| Primary Role | Task Owner, Project Member, or Viewer |
| Preconditions | Task has at least one attachment. |

### Steps

1. Open **Tasks**.
2. Select a task with an attachment.
3. Open **Attachments**.
4. Select view or download.

### Expected Result

- Attachment opens, downloads, or previews according to supported behavior.
- User cannot view attachments without proper access.

### Pass Criteria

- Attachment content is accessible to authorized users.

---

## UAT-TASK-005: Remove Task Attachment

| Field | Detail |
|---|---|
| Objective | Verify that a permitted user can remove a task attachment. |
| Primary Role | Task Owner or Project Owner |
| Preconditions | Task has removable attachment. |

### Steps

1. Open the task.
2. Open **Attachments**.
3. Select the test attachment.
4. Remove or delete it.
5. Confirm if prompted.
6. Reopen attachments.

### Expected Result

- Attachment is removed from the task.
- Removal is blocked for unauthorized users.

### Pass Criteria

- Attachment list reflects the removal.

---

## UAT-TASK-006: Add Text Comment To Task

| Field | Detail |
|---|---|
| Objective | Verify that a plain text comment can be added to a task. |
| Primary Role | Task Owner or Project Member |
| Preconditions | User has task comment access. |

### Steps

1. Open **Tasks**.
2. Select a task.
3. Open **Comments**.
4. Enter a plain text update.
5. Save or post.
6. Reopen the task.

### Expected Result

- Text comment appears in the task comment history.
- Author and timestamp display where supported.

### Pass Criteria

- Comment persists and is visible to authorized users.

---

## UAT-TASK-007: Add Rich Text Comment To Task

| Field | Detail |
|---|---|
| Objective | Verify that rich text comments can be added where supported. |
| Primary Role | Task Owner or Project Member |
| Preconditions | Rich text editor is available for task comments. |

### Steps

1. Open a task.
2. Open **Comments**.
3. Enter a formatted comment with bold text, bullet list, or line breaks.
4. Save or post.
5. Reopen the task comment history.

### Expected Result

- Rich text formatting is preserved according to supported editor behavior.
- Comment remains readable and associated with the correct task.

### Pass Criteria

- Rich text content displays correctly after refresh.

---

## UAT-TASK-008: Manage Task

| Field | Detail |
|---|---|
| Objective | Verify that task owners can manage all required task execution details. |
| Primary Role | Task Owner |
| Preconditions | User has assigned task requiring updates. |

### Steps

1. Open **Tasks**.
2. Select an assigned task.
3. Confirm task owner, due date, status, required form values, comments, and attachments.
4. Update incomplete values.
5. Add evidence or comments if needed.
6. Save.
7. Close the task if complete and allowed.

### Expected Result

- Task details can be managed from the task page.
- System enforces required fields before closure.
- Updates appear in related project views.

### Pass Criteria

- Task reaches the intended state with accurate supporting details.

---

## UAT-TASK-009: View Task

| Field | Detail |
|---|---|
| Objective | Verify that users can view task details according to access. |
| Primary Role | Viewer, Project Member, Task Owner |
| Preconditions | User has access to a project or task. |

### Steps

1. Open **Tasks**, **Project Review**, or a task notification link.
2. Select a task.
3. Review details, due date, status, comments, attachments, and form values.
4. Attempt to edit only if the role should allow it.

### Expected Result

- Task details display correctly.
- Unauthorized users have read-only behavior or no access.

### Pass Criteria

- Task visibility and edit permissions match role expectations.

---

# 4. View

## UAT-VIEW-001: Switch To Overview

| Field | Detail |
|---|---|
| Objective | Verify that the Overview tab or section displays project summary information. |
| Primary Role | Viewer, Project Owner, Project Member |
| Preconditions | User has access to View and at least one project. |

### Steps

1. Open **View**.
2. Search for and select a project.
3. Open **Overview**.
4. Review project summary, status, health, and key indicators.

### Expected Result

- Overview information loads correctly and matches project data.

### Pass Criteria

- Tester can confirm project summary is accurate.

---

## UAT-VIEW-002: Switch To Schedule

| Field | Detail |
|---|---|
| Objective | Verify that the Schedule view displays timing and date-related information. |
| Primary Role | Viewer or Project Owner |
| Preconditions | Project has milestone/task dates. |

### Steps

1. Open **View**.
2. Select a project.
3. Switch to **Schedule**.
4. Review dates, milestones, tasks, and timing information.

### Expected Result

- Schedule information displays correctly.
- Date changes from tasks or project configuration are reflected.

### Pass Criteria

- Schedule view matches project execution data.

---

## UAT-VIEW-003: Switch To Risk

| Field | Detail |
|---|---|
| Objective | Verify that the Risk view displays blockers, delayed items, or risk indicators. |
| Primary Role | Viewer or Project Owner |
| Preconditions | Project has at least one delayed, blocked, or risk-relevant item if possible. |

### Steps

1. Open **View**.
2. Select a project.
3. Switch to **Risk**.
4. Review risk indicators, delayed tasks, blockers, or status warnings.

### Expected Result

- Risk view loads correctly.
- Risk indicators are consistent with project/task state.

### Pass Criteria

- Risk information is visible and accurate for authorized users.

---

## UAT-VIEW-004: Switch To Ownership

| Field | Detail |
|---|---|
| Objective | Verify that the Ownership view displays member and responsibility information. |
| Primary Role | Viewer or Project Owner |
| Preconditions | Project has assigned owner and members. |

### Steps

1. Open **View**.
2. Select a project.
3. Switch to **Ownership**.
4. Review owner, members, responsibilities, and task assignments where shown.

### Expected Result

- Ownership details display correctly.
- Member information matches Project Configuration and task assignments.

### Pass Criteria

- Ownership view accurately reflects project membership and responsibility.

---

## UAT-VIEW-005: Search Project

| Field | Detail |
|---|---|
| Objective | Verify that project search returns correct results. |
| Primary Role | Any authorized user |
| Preconditions | User has access to View and test projects exist. |

### Steps

1. Open **View**.
2. Search by project number or ID.
3. Search by project title keyword.
4. Search by owner, plant, product, or category where supported.
5. Select a matching project.

### Expected Result

- Search returns matching projects.
- User only sees projects they are authorized to view.
- No unrelated records are returned for exact identifiers.

### Pass Criteria

- Tester can locate the expected project through search.

---

# 5. Chats

## UAT-CHAT-001: Send Chat Message

| Field | Detail |
|---|---|
| Objective | Verify that a user can send a chat message. |
| Primary Role | Project Member or Project Owner |
| Preconditions | Chat feature is available. User has access to conversation or project chat. |

### Steps

1. Open **Chats** or a project chat area.
2. Select a conversation or project context.
3. Type a message.
4. Send the message.
5. Refresh or revisit the conversation.

### Expected Result

- Message appears in the conversation.
- Message shows sender and timestamp where supported.
- Message remains visible after refresh.

### Pass Criteria

- Sent message is visible in the expected chat context.

---

## UAT-CHAT-002: Receive Message With Two Or More Users

| Field | Detail |
|---|---|
| Objective | Verify that chat messages are delivered between two or more authorized users. |
| Primary Role | Project Members |
| Preconditions | Two or more test users can access the same conversation or project chat. |

### Steps

1. User A opens the chat conversation.
2. User B opens the same chat conversation in another session or browser.
3. User A sends a message.
4. User B confirms the message appears.
5. User B replies.
6. User A confirms the reply appears.
7. If available, User C joins or views the conversation and confirms visibility.

### Expected Result

- Messages are delivered to all authorized participants.
- Users outside the conversation or project cannot view the chat.

### Pass Criteria

- Two-way chat communication succeeds between authorized users.

---

# 6. Notifications

## UAT-NOTIF-001: View Notifications

| Field | Detail |
|---|---|
| Objective | Verify that users can view notifications and open related records. |
| Primary Role | Any authorized user |
| Preconditions | User has active or unread notifications. |

### Steps

1. Open **Notifications**.
2. Review unread and active notifications.
3. Select a notification linked to a project or task.
4. Open the related record.
5. Return to Notifications.
6. Mark as read if available.

### Expected Result

- Notifications display correctly.
- Links open the correct project or task.
- Read/unread state updates where supported.

### Pass Criteria

- User can identify and act on notifications.

---

## UAT-NOTIF-002: Switch Between Archived Hidden And Archived Only

| Field | Detail |
|---|---|
| Objective | Verify archive filtering in Notifications. |
| Primary Role | Any authorized user |
| Preconditions | User has active and archived notifications, or test data can be created. |

### Steps

1. Open **Notifications**.
2. Select **Archived Hidden**.
3. Confirm archived notifications are hidden and active notifications are shown.
4. Select **Archived Only**.
5. Confirm only archived notifications are shown.
6. Switch back to Archived Hidden.

### Expected Result

- Notification list changes according to archive filter.
- Filter selection does not alter notification content unexpectedly.

### Pass Criteria

- Archive visibility behaves consistently.

---

## UAT-NOTIF-003: Switch Between Read Visible And Read Hidden

| Field | Detail |
|---|---|
| Objective | Verify read/unread filtering in Notifications. |
| Primary Role | Any authorized user |
| Preconditions | User has both read and unread notifications if possible. |

### Steps

1. Open **Notifications**.
2. Select **Read Hidden**.
3. Confirm read notifications are hidden.
4. Select **Read Visible**.
5. Confirm read notifications appear with unread notifications.
6. Mark an unread notification as read if available and retest filters.

### Expected Result

- Notification list changes based on read visibility filter.
- Read state updates correctly after user action.

### Pass Criteria

- Read and unread notification filters work as expected.

---

# 7. Categories

## UAT-CAT-001: Add Category

| Field | Detail |
|---|---|
| Objective | Verify that a category can be added. |
| Primary Role | Template Manager or Administrator |
| Preconditions | User has Templates > Categories access. |

### Steps

1. Open **Templates > Categories**.
2. Select **Add** or **New**.
3. Enter category code, name, description, active state, and required fields.
4. Save.
5. Search for the category.

### Expected Result

- Category is created successfully.
- Category appears in category lists and applicable project/template selections.

### Pass Criteria

- Added category is searchable and visible after refresh.

---

## UAT-CAT-002: Update Category

| Field | Detail |
|---|---|
| Objective | Verify that a category can be updated. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Existing editable category is available. |

### Steps

1. Open **Templates > Categories**.
2. Search for an existing category.
3. Open edit/update mode.
4. Change allowed fields such as description, name, or active state.
5. Save.
6. Reopen the category.

### Expected Result

- Category updates are saved.
- Restricted fields are protected where applicable.

### Pass Criteria

- Updated category values persist after refresh.

---

## UAT-CAT-003: View Category

| Field | Detail |
|---|---|
| Objective | Verify that a category can be viewed. |
| Primary Role | Template Manager, Administrator, or authorized viewer |
| Preconditions | Category exists. |

### Steps

1. Open **Templates > Categories**.
2. Search or filter for a category.
3. Open **View** or **Display**.
4. Review category details.

### Expected Result

- Category details display correctly.
- User sees fields appropriate to role.

### Pass Criteria

- Category can be viewed without data errors.

---

# 8. Fields

## UAT-FIELD-001: Add Field

| Field | Detail |
|---|---|
| Objective | Verify that a reusable field can be added. |
| Primary Role | Template Manager or Administrator |
| Preconditions | User has Templates > Fields access. |

### Steps

1. Open **Templates > Fields**.
2. Select **Add** or **New**.
3. Enter field name, label, type, options if applicable, required behavior, and active state.
4. Save.
5. Search for the field.

### Expected Result

- Field is created successfully.
- Field can be selected for forms where applicable.

### Pass Criteria

- Added field is visible after refresh.

---

## UAT-FIELD-002: Update Field

| Field | Detail |
|---|---|
| Objective | Verify that a reusable field can be updated. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Existing editable field is available. |

### Steps

1. Open **Templates > Fields**.
2. Search for a field.
3. Open edit/update mode.
4. Change allowed properties such as label, description, options, active state, or required behavior.
5. Save.
6. Reopen the field.

### Expected Result

- Field updates are saved.
- Changes do not break forms using the field.
- Restricted changes are blocked if field is already in use.

### Pass Criteria

- Updated field values persist and remain usable.

---

## UAT-FIELD-003: Delete Field

| Field | Detail |
|---|---|
| Objective | Verify that field deletion is governed correctly. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Test field exists. Ideally use a field not attached to production forms. |

### Steps

1. Open **Templates > Fields**.
2. Select a test field.
3. Choose delete.
4. Confirm deletion if prompted.
5. Search for the deleted field.
6. Attempt deletion of a field used by a form if safe test data is available.

### Expected Result

- Unused test field can be deleted if permitted.
- Field in use is blocked or protected according to business rules.
- Deletion does not remove unrelated form data.

### Pass Criteria

- Field deletion behavior matches governance expectations.

---

## UAT-FIELD-004: View Field

| Field | Detail |
|---|---|
| Objective | Verify that field details can be viewed. |
| Primary Role | Template Manager, Administrator, or authorized viewer |
| Preconditions | Field exists. |

### Steps

1. Open **Templates > Fields**.
2. Search for a field.
3. Open **View** or **Display**.
4. Review field type, label, options, required behavior, and active state.

### Expected Result

- Field details display correctly.

### Pass Criteria

- User can inspect field configuration without editing.

---

# 9. Forms

## UAT-FORM-001: Add Form

| Field | Detail |
|---|---|
| Objective | Verify that a form can be created with fields. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Reusable fields exist. User has Templates > Forms access. |

### Steps

1. Open **Templates > Forms**.
2. Select **Add** or **New**.
3. Enter form name, description, active state, and required setup.
4. Add one or more fields to the form.
5. Arrange fields if available.
6. Save.
7. Search for the form.

### Expected Result

- Form is created successfully.
- Selected fields appear in the form structure.
- Form can be linked to roadmap, task, milestone, or project setup where applicable.

### Pass Criteria

- New form is visible and contains expected fields.

---

## UAT-FORM-002: Update Form

| Field | Detail |
|---|---|
| Objective | Verify that an existing form can be updated. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Editable form exists. |

### Steps

1. Open **Templates > Forms**.
2. Search for an existing form.
3. Open edit/update mode.
4. Update form details.
5. Add, remove, or rearrange fields where allowed.
6. Save.
7. Reopen the form.

### Expected Result

- Form updates are saved.
- Linked processes are not corrupted.
- System prevents unsafe changes where required.

### Pass Criteria

- Form displays the updated structure after refresh.

---

## UAT-FORM-003: Delete Form

| Field | Detail |
|---|---|
| Objective | Verify that form deletion is controlled correctly. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Test form exists. |

### Steps

1. Open **Templates > Forms**.
2. Select a test form.
3. Choose delete.
4. Confirm if prompted.
5. Search for the deleted form.
6. If safe, attempt to delete a form currently used by a roadmap or task template.

### Expected Result

- Unused test form is removed if deletion is allowed.
- In-use form deletion is blocked or clearly governed.
- Existing projects are not damaged.

### Pass Criteria

- Delete behavior protects downstream usage.

---

## UAT-FORM-004: View Form

| Field | Detail |
|---|---|
| Objective | Verify that users can view form configuration. |
| Primary Role | Template Manager, Administrator, or authorized viewer |
| Preconditions | Form exists. |

### Steps

1. Open **Templates > Forms**.
2. Search for a form.
3. Open **View** or **Display**.
4. Review form details, fields, ordering, and active state.

### Expected Result

- Form structure displays correctly.

### Pass Criteria

- Form can be reviewed without editing.

---

## UAT-FORM-005: Copy Form

| Field | Detail |
|---|---|
| Objective | Verify that a form can be copied and adjusted. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Source form exists and user has copy/create permission. |

### Steps

1. Open **Templates > Forms**.
2. Select an existing form.
3. Choose **Copy**.
4. Rename the copied form.
5. Adjust field list, ordering, or details.
6. Save.
7. Search for the new copied form.

### Expected Result

- New form is created based on source form.
- Source form remains unchanged.
- Copied form can be edited independently.

### Pass Criteria

- Copied form is distinct and usable.

---

# 10. Maturity

## UAT-MAT-001: Add Maturity

| Field | Detail |
|---|---|
| Objective | Verify that a maturity level can be added. |
| Primary Role | Template Manager or Administrator |
| Preconditions | User has Templates > Maturity access. |

### Steps

1. Open **Templates > Maturity**.
2. Select **Add** or **New**.
3. Enter code, name, description, order, active state, and required fields.
4. Save.
5. Search for the maturity level.

### Expected Result

- Maturity level is created successfully.
- Maturity can be used in roadmap or project setup where applicable.

### Pass Criteria

- Added maturity record is visible after refresh.

---

## UAT-MAT-002: Update Maturity

| Field | Detail |
|---|---|
| Objective | Verify that a maturity level can be updated. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Existing maturity record is available. |

### Steps

1. Open **Templates > Maturity**.
2. Search for a maturity record.
3. Open edit/update mode.
4. Change allowed fields.
5. Save.
6. Reopen the record.

### Expected Result

- Maturity changes are saved.
- Restricted fields are protected where applicable.

### Pass Criteria

- Updated maturity values persist.

---

## UAT-MAT-003: View Maturity

| Field | Detail |
|---|---|
| Objective | Verify that maturity details can be viewed. |
| Primary Role | Template Manager, Administrator, or authorized viewer |
| Preconditions | Maturity record exists. |

### Steps

1. Open **Templates > Maturity**.
2. Search for a maturity record.
3. Open **View** or **Display**.
4. Review details.

### Expected Result

- Maturity details display correctly.

### Pass Criteria

- User can inspect maturity record without data errors.

---

# 11. Product Division

## UAT-PDIV-001: Add Product Division

| Field | Detail |
|---|---|
| Objective | Verify that a product division can be added. |
| Primary Role | Template Manager or Administrator |
| Preconditions | User has Templates > Product Division access. |

### Steps

1. Open **Templates > Product Division**.
2. Select **Add** or **New**.
3. Enter product division code, name, description, active state, and required fields.
4. Save.
5. Search for the product division.

### Expected Result

- Product division is created successfully.
- Product division is available for related product setup where applicable.

### Pass Criteria

- Added product division is visible after refresh.

---

## UAT-PDIV-002: Update Product Division

| Field | Detail |
|---|---|
| Objective | Verify that a product division can be updated. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Existing product division is available. |

### Steps

1. Open **Templates > Product Division**.
2. Search for a product division.
3. Open edit/update mode.
4. Change allowed fields.
5. Save.
6. Reopen the record.

### Expected Result

- Product division updates are saved.
- Related product group relationships remain valid.

### Pass Criteria

- Updated values persist after refresh.

---

## UAT-PDIV-003: View Product Division

| Field | Detail |
|---|---|
| Objective | Verify that product division details can be viewed. |
| Primary Role | Template Manager, Administrator, or authorized viewer |
| Preconditions | Product division exists. |

### Steps

1. Open **Templates > Product Division**.
2. Search or filter for a product division.
3. Open **View** or **Display**.
4. Review details.

### Expected Result

- Product division details display correctly.

### Pass Criteria

- User can inspect product division record.

---

# 12. Product Group

## UAT-PGRP-001: Add Product Group

| Field | Detail |
|---|---|
| Objective | Verify that a product group can be added. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Product division exists if product group requires division mapping. |

### Steps

1. Open **Templates > Product Group**.
2. Select **Add** or **New**.
3. Enter product group code, name, description, product division, active state, and required fields.
4. Save.
5. Search for the product group.

### Expected Result

- Product group is created successfully.
- Product group is available for project registration, filtering, or reporting where applicable.

### Pass Criteria

- Added product group is visible after refresh.

---

## UAT-PGRP-002: Update Product Group

| Field | Detail |
|---|---|
| Objective | Verify that a product group can be updated. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Existing product group is available. |

### Steps

1. Open **Templates > Product Group**.
2. Search for a product group.
3. Open edit/update mode.
4. Change allowed fields.
5. Save.
6. Reopen the record.

### Expected Result

- Product group updates are saved.
- Product division relationship remains valid.

### Pass Criteria

- Updated product group values persist after refresh.

---

## UAT-PGRP-003: View Product Group

| Field | Detail |
|---|---|
| Objective | Verify that product group details can be viewed. |
| Primary Role | Template Manager, Administrator, or authorized viewer |
| Preconditions | Product group exists. |

### Steps

1. Open **Templates > Product Group**.
2. Search or filter for a product group.
3. Open **View** or **Display**.
4. Review details.

### Expected Result

- Product group details display correctly.

### Pass Criteria

- User can inspect product group record.

---

# 13. Roadmap

## UAT-RMAP-001: Add Roadmap

| Field | Detail |
|---|---|
| Objective | Verify that a roadmap can be created with milestones and tasks. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Categories, maturity levels, fields, and forms exist as needed. |

### Steps

1. Open **Templates > Roadmap**.
2. Select **Add** or **New**.
3. Enter roadmap name, description, category, maturity, active state, and required values.
4. Add at least two milestones.
5. Add at least one task or activity under each milestone.
6. Attach forms or prerequisites where supported.
7. Save.
8. Search for the roadmap.
9. Use the roadmap during project registration if applicable.

### Expected Result

- Roadmap is created successfully.
- Roadmap structure includes expected milestones and tasks.
- Roadmap is available for project registration where applicable.

### Pass Criteria

- Roadmap can be found and used as a reusable structure.

---

## UAT-RMAP-002: Update Roadmap

| Field | Detail |
|---|---|
| Objective | Verify that roadmap details, milestones, tasks, forms, or prerequisites can be updated. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Existing roadmap is available. |

### Steps

1. Open **Templates > Roadmap**.
2. Search for a roadmap.
3. Open edit/update mode.
4. Update roadmap description or setup details.
5. Add, edit, remove, or reorder a milestone or task if allowed.
6. Update linked forms or prerequisites where available.
7. Save.
8. Reopen the roadmap.

### Expected Result

- Roadmap updates are saved.
- Future project registration uses updated structure.
- Existing projects are not unexpectedly modified until retrieve/update behavior is triggered.

### Pass Criteria

- Updated roadmap structure persists after refresh.

---

## UAT-RMAP-003: View Roadmap

| Field | Detail |
|---|---|
| Objective | Verify that roadmap structure can be viewed. |
| Primary Role | Template Manager, Administrator, or authorized viewer |
| Preconditions | Roadmap exists. |

### Steps

1. Open **Templates > Roadmap**.
2. Search for a roadmap.
3. Open **View** or **Display**.
4. Review milestones, tasks, forms, prerequisites, category, and maturity.

### Expected Result

- Roadmap details and structure display correctly.

### Pass Criteria

- User can inspect roadmap structure without editing.

---

## UAT-RMAP-004: Copy Roadmap

| Field | Detail |
|---|---|
| Objective | Verify that a roadmap can be copied and modified independently. |
| Primary Role | Template Manager or Administrator |
| Preconditions | Source roadmap exists. User has copy/create permission. |

### Steps

1. Open **Templates > Roadmap**.
2. Select an existing roadmap.
3. Choose **Copy**.
4. Rename the copied roadmap.
5. Adjust category, maturity, milestones, tasks, forms, or prerequisites.
6. Save.
7. Search for the copied roadmap.
8. Confirm the source roadmap remains unchanged.

### Expected Result

- New roadmap is created from source roadmap.
- Copied roadmap can be edited independently.
- Source roadmap remains unchanged.

### Pass Criteria

- Copied roadmap is distinct and available for future project registration.

---

# 14. Reports

## UAT-REP-001: Project Export

| Field | Detail |
|---|---|
| Objective | Verify that users can generate and export project records. |
| Primary Role | Report Viewer, Project Owner, Administrator |
| Preconditions | User has Reports > Project Export access. Projects exist matching filter criteria. |

### Steps

1. Open **Reports > Project Export**.
2. Set filters such as plant, status, date range, owner, category, product group, or product code where supported.
3. Run or generate the report.
4. Review results on screen.
5. Export or download the report if available.
6. Open the exported file.

### Expected Result

- Report returns records matching selected filters.
- Export file opens successfully.
- Exported columns and values match on-screen results and project data.

### Pass Criteria

- Tester can generate and validate a project export.

---

## UAT-REP-002: Monitoring Matrix

| Field | Detail |
|---|---|
| Objective | Verify that users can review projects in a milestone/task matrix. |
| Primary Role | Report Viewer or Project Owner |
| Preconditions | User has Reports > Monitoring Matrix access. Multiple projects exist with roadmap structures. |

### Steps

1. Open **Reports > Monitoring Matrix**.
2. Apply filters such as plant, category, roadmap, status, or date range where supported.
3. Generate the matrix.
4. Review project rows and milestone/task columns.
5. Use frozen, pinned, or scrolling fields if available.
6. Export the matrix if available.

### Expected Result

- Matrix displays matching projects.
- Milestone and task columns show correct status/progress data.
- Export, if available, matches the displayed matrix.

### Pass Criteria

- Monitoring Matrix supports operational review of multiple projects.

---

## UAT-REP-003: Project Comparison

| Field | Detail |
|---|---|
| Objective | Verify that users can compare projects side by side. |
| Primary Role | Report Viewer or Project Owner |
| Preconditions | At least two comparable projects exist. User has Reports > Project Comparison access. |

### Steps

1. Open **Reports > Project Comparison**.
2. Select two or more projects.
3. Run or load the comparison.
4. Compare schedules, milestones, task status, progress, owners, and form values.
5. Export or capture the comparison if available.

### Expected Result

- Selected projects display side by side.
- Differences in schedule, progress, task state, and form values are clear.
- User only sees projects they are authorized to view.

### Pass Criteria

- Tester can compare selected projects and identify meaningful differences.

---

# 15. Negative And Access Control Scenarios

## UAT-ACCESS-001: Missing Menu For Unauthorized User

| Field | Detail |
|---|---|
| Objective | Verify that users without access do not see restricted menus. |
| Primary Role | Any restricted user |
| Preconditions | User does not have access to at least one controlled module such as Admin, Templates, or Reports. |

### Steps

1. Log in as a user without template management access.
2. Check navigation for **Templates**.
3. Attempt direct navigation to a restricted page if a test-safe URL is available.
4. Repeat for Reports, Admin, or Project Configuration where applicable.

### Expected Result

- Restricted menus or actions are hidden or disabled.
- Direct access is denied safely.
- User receives clear access behavior without seeing unauthorized data.

### Pass Criteria

- Role-based access boundaries are enforced.

---

## UAT-ACCESS-002: Unauthorized User Cannot Delete Project

| Field | Detail |
|---|---|
| Objective | Verify that project deletion is limited to authorized roles. |
| Primary Role | Restricted project member or viewer |
| Preconditions | User can view a project but should not delete it. |

### Steps

1. Log in as restricted user.
2. Open a visible project.
3. Open available project actions or configuration if visible.
4. Look for delete project action.
5. Attempt deletion only if the action is visible in a test environment.

### Expected Result

- Delete action is hidden, disabled, or blocked.
- Project remains intact.

### Pass Criteria

- Restricted user cannot delete project.

---

## UAT-ACCESS-003: Unauthorized User Cannot Modify Templates

| Field | Detail |
|---|---|
| Objective | Verify that template maintenance is restricted. |
| Primary Role | Daily User or Project Member |
| Preconditions | User lacks template manager role. |

### Steps

1. Log in as a daily user.
2. Attempt to open **Templates**.
3. If visible, attempt to add or update Category, Field, Form, Maturity, Product Division, Product Group, or Roadmap.

### Expected Result

- Template maintenance actions are hidden, disabled, or blocked.
- Existing setup data cannot be modified by unauthorized users.

### Pass Criteria

- Template governance is enforced.

---

## UAT-ACCESS-004: User Sees Only Authorized Projects

| Field | Detail |
|---|---|
| Objective | Verify that project visibility respects plant, project membership, or role scope. |
| Primary Role | Daily User, Viewer, Project Member |
| Preconditions | There are projects inside and outside the user's expected access scope. |

### Steps

1. Log in as the test user.
2. Open **Projects**.
3. Search for an authorized project.
4. Search for a project outside the user's scope.
5. Open **View**, **Tasks**, **Status Board**, and **Reports** where available.

### Expected Result

- User sees only authorized projects and tasks.
- Out-of-scope projects are hidden or inaccessible.

### Pass Criteria

- Project visibility matches access rules.

---

# 16. Regression Checklist

Use this checklist after fixing defects or completing a release candidate.

| Area | Check | Result | Notes |
|---|---|---|---|
| Register | New project can be created |  |  |
| Register | Auto-start milestone works |  |  |
| Register | Create-only project can later be started |  |  |
| Projects | Project Review loads |  |  |
| Projects | Tasks update from Project Review |  |  |
| Projects | Attachments upload and remove |  |  |
| Projects | Comments save correctly |  |  |
| Projects | Project Configuration saves changes |  |  |
| Projects | Product codes can be maintained |  |  |
| Projects | Members can be maintained |  |  |
| Projects | Notifications can be maintained |  |  |
| Projects | Roadmap retrieval works |  |  |
| Status Board | Project status changes |  |  |
| Status Board | Milestone status changes |  |  |
| Status Board | Task status changes |  |  |
| Tasks | Task details update |  |  |
| Tasks | Task data entry saves |  |  |
| Tasks | Task comments and attachments work |  |  |
| View | Overview/Schedule/Risk/Ownership load |  |  |
| View | Project search works |  |  |
| Chats | Messages send and receive |  |  |
| Notifications | Notifications load and filters work |  |  |
| Templates | Categories maintenance works |  |  |
| Templates | Fields maintenance works |  |  |
| Templates | Forms maintenance works |  |  |
| Templates | Maturity maintenance works |  |  |
| Templates | Product Division maintenance works |  |  |
| Templates | Product Group maintenance works |  |  |
| Templates | Roadmap maintenance works |  |  |
| Reports | Project Export works |  |  |
| Reports | Monitoring Matrix works |  |  |
| Reports | Project Comparison works |  |  |
| Access | Unauthorized users are blocked |  |  |

---

# 17. UAT Sign-Off

| Role | Name | Signature | Date | Overall Result | Notes |
|---|---|---|---|---|---|
| Business Owner |  |  |  |  |  |
| Project Owner Representative |  |  |  |  |  |
| Template Manager Representative |  |  |  |  |  |
| Administrator Representative |  |  |  |  |  |
| QA Lead |  |  |  |  |  |

---

# 18. Defect Log

| Defect ID | Scenario ID | Summary | Steps To Reproduce | Expected Result | Actual Result | Severity | Priority | Owner | Status | Retest Result | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|
|  |  |  |  |  |  |  |  |  |  |  |  |
|  |  |  |  |  |  |  |  |  |  |  |  |
|  |  |  |  |  |  |  |  |  |  |  |  |

---

# Appendix A: Suggested UAT Execution Order

1. Validate access and test data setup.
2. Validate template setup records: Categories, Fields, Forms, Maturity, Product Division, Product Group, and Roadmap.
3. Register projects using both auto-start and create-only paths.
4. Validate Project Review, task updates, attachments, comments, chats, and progress.
5. Validate Project Configuration changes.
6. Validate Status Board status changes.
7. Validate Tasks and View workflows.
8. Validate Notifications and Chats with multiple users.
9. Validate Reports.
10. Execute access control and negative scenarios.
11. Complete regression checklist.
12. Review defects and obtain sign-off.

# Appendix B: Sample Evidence Naming Convention

Use a consistent naming convention for screenshots and exports:

```text
<ScenarioID>_<TesterInitials>_<YYYYMMDD>_<ShortDescription>
```

Examples:

```text
UAT-PROJ-001_CL_20260528_AutoStartMilestone.png
UAT-REP-001_CL_20260528_ProjectExport.xlsx
UAT-CHAT-002_CL_20260528_TwoUserMessage.png
```

# Appendix C: Severity Guidance

| Severity | Definition |
|---|---|
| Critical | Prevents a core business workflow from being completed and no workaround exists. |
| High | Major workflow issue with limited workaround or significant user impact. |
| Medium | Functional issue with acceptable workaround or limited workflow impact. |
| Low | Cosmetic, wording, layout, or minor usability issue with minimal impact. |

# Appendix D: Common UAT Notes

- If a tester cannot see a menu or action, first confirm role, module access, plant scope, and project membership.
- Do not treat missing access as a defect until access setup has been confirmed.
- Use comments for traceable execution decisions and chat for active discussion.
- Attach evidence to the most specific record possible, usually the task when the evidence supports task completion.
- Do not delete shared setup records unless the data is explicitly created for testing.
- For roadmap update tests, use a controlled test roadmap and project to avoid affecting active business work.
