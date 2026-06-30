export type PulseArea = 'Root' | 'Projects' | 'Templates' | 'Admin' | 'Sites' | 'Settings' | 'Error';
export type RouteStatus = 'ready-for-build' | 'api-backed' | 'legacy-dependent';

export type PulseRoute = {
  path: string;
  aliases?: string[];
  title: string;
  summary: string;
  area: PulseArea;
  section: string;
  apiResources?: string[];
  moduleCodes?: string[];
  status: RouteStatus;
};

const route = (definition: PulseRoute) => definition;

export const pulseRoutes: PulseRoute[] = [
  route({
    path: '/',
    aliases: ['/Home/Index'],
    title: 'Dashboard',
    summary: 'Main application dashboard and starting point for project work.',
    area: 'Root',
    section: 'Core',
    apiResources: ['api/activities', 'api/projects', 'api/notifications'],
    status: 'api-backed'
  }),
  route({
    path: '/Home/About',
    title: 'About',
    summary: 'Static product overview page from the legacy shell.',
    area: 'Root',
    section: 'Core',
    status: 'ready-for-build'
  }),
  route({
    path: '/Home/Contact',
    title: 'Contact',
    summary: 'Contact and support details page.',
    area: 'Root',
    section: 'Core',
    status: 'ready-for-build'
  }),
  route({
    path: '/Home/Guide',
    title: 'FAQ Guide',
    summary: 'Guide and FAQ surface available from all shell variants.',
    area: 'Root',
    section: 'Reports',
    status: 'ready-for-build'
  }),
  route({
    path: '/Home/ViewProjects',
    title: 'View Projects',
    summary: 'Cross-project browsing and read-only viewing entry point.',
    area: 'Root',
    section: 'Core',
    apiResources: ['api/projects'],
    status: 'api-backed'
  }),
  route({
    path: '/Home/Report1',
    title: 'Project Export',
    summary: 'Legacy reporting screen for project export.',
    area: 'Root',
    section: 'Reports',
    status: 'legacy-dependent'
  }),
  route({
    path: '/Home/Report2',
    title: 'Monitoring Matrix',
    summary: 'Legacy reporting screen for monitoring matrix.',
    area: 'Root',
    section: 'Reports',
    status: 'legacy-dependent'
  }),
  route({
    path: '/Home/Report3',
    title: 'Project Comparison',
    summary: 'Legacy reporting screen for project comparison.',
    area: 'Root',
    section: 'Reports',
    status: 'legacy-dependent'
  }),
  route({
    path: '/Home/Search',
    title: 'Global Search',
    summary: 'Global search route formerly rendered through MVC views.',
    area: 'Root',
    section: 'Core',
    apiResources: ['api/projects', 'api/roadmaps', 'api/forms'],
    status: 'api-backed'
  }),
  route({
    path: '/Chats/Index',
    aliases: ['/chats'],
    title: 'Chats',
    summary: 'Chat hub entry point relying on classic ASP.NET SignalR flows.',
    area: 'Root',
    section: 'Collaboration',
    apiResources: ['api/chat'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Projects/Index',
    title: 'Projects',
    summary: 'Primary projects listing and portfolio management workspace.',
    area: 'Projects',
    section: 'Projects',
    apiResources: ['api/projects'],
    status: 'api-backed'
  }),
  route({
    path: '/Projects/Create',
    title: 'Register Project',
    summary: 'Project registration flow with upload and metadata capture.',
    area: 'Projects',
    section: 'Projects',
    apiResources: ['api/projects', 'api/files', 'api/activeDirectory'],
    status: 'api-backed'
  }),
  route({
    path: '/Projects/Overview/:projectno',
    aliases: ['/projects/overview/:projectno', '/Projects/:projectno/Overview'],
    title: 'Project Overview',
    summary: 'High-level project workspace with summary and drill-in panels.',
    area: 'Projects',
    section: 'Projects',
    apiResources: ['api/projects', 'api/milestones', 'api/projecttasks', 'api/comments'],
    status: 'api-backed'
  }),
  route({
    path: '/Projects/Review/:projectno',
    aliases: ['/Projects/:projectno/Review'],
    title: 'Project Review',
    summary: 'Review screen with milestone, task, and form inspection flows.',
    area: 'Projects',
    section: 'Projects',
    apiResources: ['api/projects', 'api/projectforms', 'api/notifications'],
    status: 'api-backed'
  }),
  route({
    path: '/Projects/Details/:projectno',
    aliases: ['/Projects/:projectno/Details'],
    title: 'Project Details',
    summary: 'Detailed project workspace with drawers, modals, and collaboration features.',
    area: 'Projects',
    section: 'Projects',
    apiResources: ['api/projects', 'api/projecttasks', 'api/projectattachments', 'api/comments', 'api/notifications'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Projects/ProjectTasks/Index',
    title: 'Project Tasks',
    summary: 'Task index across projects.',
    area: 'Projects',
    section: 'Execution',
    apiResources: ['api/projecttasks'],
    status: 'api-backed'
  }),
  route({
    path: '/Projects/ProjectTasks/Edit/:id',
    title: 'Edit Task',
    summary: 'Task editing screen with attachments, comments, and activity data.',
    area: 'Projects',
    section: 'Execution',
    apiResources: ['api/projecttasks', 'api/files', 'api/comments', 'api/activities'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Projects/ProjectChats/Index',
    title: 'Project Chats',
    summary: 'Project-scoped chat room view.',
    area: 'Projects',
    section: 'Collaboration',
    apiResources: ['api/chat'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Projects/ProjectMembers/Index',
    title: 'Project Members',
    summary: 'Project member administration surface.',
    area: 'Projects',
    section: 'Collaboration',
    apiResources: ['api/projects', 'api/activeDirectory'],
    status: 'api-backed'
  }),
  route({
    path: '/Projects/ProjectFields/Index',
    title: 'Project Fields',
    summary: 'Project field configuration and metadata panel.',
    area: 'Projects',
    section: 'Execution',
    apiResources: ['api/projects'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Projects/ProjectMilestones/Index',
    title: 'Project Milestones',
    summary: 'Milestone list and progress tracking surface.',
    area: 'Projects',
    section: 'Execution',
    apiResources: ['api/milestones'],
    status: 'api-backed'
  }),
  route({
    path: '/Admin',
    aliases: ['/Admin/Home/Index'],
    title: 'Admin Console',
    summary: 'Admin dashboard and entry point for governance modules.',
    area: 'Admin',
    section: 'Admin',
    moduleCodes: ['MODULEVIEW', 'USRGRPVIEW'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Admin/Modules/Index',
    title: 'All Modules',
    summary: 'Module catalog management screen.',
    area: 'Admin',
    section: 'Modules',
    moduleCodes: ['MODULEVIEW'],
    apiResources: ['api/modules'],
    status: 'api-backed'
  }),
  route({
    path: '/Admin/Modules/Reports',
    title: 'Module Reports',
    summary: 'Reports view for modules administration.',
    area: 'Admin',
    section: 'Modules',
    moduleCodes: ['MODULEVIEW'],
    apiResources: ['api/modules'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Admin/Modules/New',
    title: 'New Module',
    summary: 'Module creation flow.',
    area: 'Admin',
    section: 'Modules',
    moduleCodes: ['MODULEVIEW'],
    apiResources: ['api/modules'],
    status: 'api-backed'
  }),
  route({
    path: '/Admin/Modules/Edit/:code',
    title: 'Edit Module',
    summary: 'Module edit form.',
    area: 'Admin',
    section: 'Modules',
    moduleCodes: ['MODULEVIEW'],
    apiResources: ['api/modules'],
    status: 'api-backed'
  }),
  route({
    path: '/Admin/Modules/Display/:code',
    title: 'Display Module',
    summary: 'Module detail screen.',
    area: 'Admin',
    section: 'Modules',
    moduleCodes: ['MODULEVIEW'],
    apiResources: ['api/modules'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Admin/UserGroups/Index',
    title: 'All User Groups',
    summary: 'User group listing and governance surface.',
    area: 'Admin',
    section: 'User Groups',
    moduleCodes: ['USRGRPVIEW'],
    apiResources: ['api/usergroups', 'api/modules'],
    status: 'api-backed'
  }),
  route({
    path: '/Admin/UserGroups/Reports',
    title: 'User Group Reports',
    summary: 'Reports view for user groups.',
    area: 'Admin',
    section: 'User Groups',
    moduleCodes: ['USRGRPVIEW'],
    apiResources: ['api/usergroups'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Admin/UserGroups/New',
    title: 'New User Group',
    summary: 'User group creation flow.',
    area: 'Admin',
    section: 'User Groups',
    moduleCodes: ['USRGRPVIEW'],
    apiResources: ['api/usergroups'],
    status: 'api-backed'
  }),
  route({
    path: '/Admin/UserGroups/Configure/:id',
    title: 'Configure User Group',
    summary: 'Permission and membership configuration screen for user groups.',
    area: 'Admin',
    section: 'User Groups',
    moduleCodes: ['USRGRPVIEW'],
    apiResources: ['api/usergroups', 'api/modules'],
    status: 'api-backed'
  }),
  route({
    path: '/Admin/UserGroups/Edit/:id',
    title: 'Edit User Group',
    summary: 'User group metadata edit screen.',
    area: 'Admin',
    section: 'User Groups',
    moduleCodes: ['USRGRPVIEW'],
    apiResources: ['api/usergroups'],
    status: 'api-backed'
  }),
  route({
    path: '/Admin/UserGroups/Display/:code',
    title: 'Display User Group',
    summary: 'Read-only user group detail view.',
    area: 'Admin',
    section: 'User Groups',
    moduleCodes: ['USRGRPVIEW'],
    apiResources: ['api/usergroups'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Admin/Users/Index',
    title: 'Users',
    summary: 'Legacy users administration landing page.',
    area: 'Admin',
    section: 'Users',
    status: 'ready-for-build'
  }),
  route({
    path: '/Admin/AccessRoles/Index',
    title: 'Access Roles',
    summary: 'Legacy role administration landing page.',
    area: 'Admin',
    section: 'Users',
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates',
    aliases: ['/Templates/Home/Index'],
    title: 'Templates Dashboard',
    summary: 'Templates domain landing page for all master data modules.',
    area: 'Templates',
    section: 'Templates',
    moduleCodes: ['CALNDRVIEW', 'CATGRYVIEW', 'FORMVIEW', 'MATVIEW', 'PLANTVIEW', 'PDIVVIEW', 'PGRPVIEW', 'RMAPVIEW'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/ProductionCalendars/Index',
    title: 'All Calendars',
    summary: 'Production calendar list.',
    area: 'Templates',
    section: 'Calendars',
    moduleCodes: ['CALNDRVIEW'],
    apiResources: ['api/productioncalendars'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/ProductionCalendars/Reports',
    title: 'Calendar Reports',
    summary: 'Production calendar reports.',
    area: 'Templates',
    section: 'Calendars',
    moduleCodes: ['CALNDRVIEW'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/ProductionCalendars/New',
    title: 'New Calendar',
    summary: 'Create a production calendar.',
    area: 'Templates',
    section: 'Calendars',
    moduleCodes: ['CALNDRVIEW'],
    apiResources: ['api/productioncalendars'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/ProductionCalendars/Edit/:code',
    title: 'Edit Calendar',
    summary: 'Edit a production calendar.',
    area: 'Templates',
    section: 'Calendars',
    moduleCodes: ['CALNDRVIEW'],
    apiResources: ['api/productioncalendars'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/ProductionCalendars/Display/:code',
    title: 'Display Calendar',
    summary: 'Read-only production calendar detail view.',
    area: 'Templates',
    section: 'Calendars',
    moduleCodes: ['CALNDRVIEW'],
    apiResources: ['api/productioncalendars'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/Categories/Index',
    title: 'All Categories',
    summary: 'Category list management screen.',
    area: 'Templates',
    section: 'Categories',
    moduleCodes: ['CATGRYVIEW'],
    apiResources: ['api/categories'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/Categories/Reports',
    title: 'Category Reports',
    summary: 'Category reporting screen.',
    area: 'Templates',
    section: 'Categories',
    moduleCodes: ['CATGRYVIEW'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/Categories/New',
    title: 'New Category',
    summary: 'Create a category.',
    area: 'Templates',
    section: 'Categories',
    moduleCodes: ['CATGRYVIEW'],
    apiResources: ['api/categories'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/Categories/Edit/:code',
    title: 'Edit Category',
    summary: 'Edit category metadata.',
    area: 'Templates',
    section: 'Categories',
    moduleCodes: ['CATGRYVIEW'],
    apiResources: ['api/categories'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/Categories/Display/:code',
    title: 'Display Category',
    summary: 'Read-only category detail view.',
    area: 'Templates',
    section: 'Categories',
    moduleCodes: ['CATGRYVIEW'],
    apiResources: ['api/categories'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/Forms/Index',
    title: 'All Forms',
    summary: 'Forms registry and management surface.',
    area: 'Templates',
    section: 'Forms',
    moduleCodes: ['FORMVIEW'],
    apiResources: ['api/forms'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/Forms/Reports',
    title: 'Form Reports',
    summary: 'Forms reporting screen.',
    area: 'Templates',
    section: 'Forms',
    moduleCodes: ['FORMVIEW'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/Forms/New',
    title: 'New Form',
    summary: 'Form builder create flow.',
    area: 'Templates',
    section: 'Forms',
    moduleCodes: ['FORMVIEW'],
    apiResources: ['api/forms'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Templates/Forms/Copy/:code',
    title: 'Copy Form',
    summary: 'Clone an existing form into a new draft.',
    area: 'Templates',
    section: 'Forms',
    moduleCodes: ['FORMVIEW'],
    apiResources: ['api/forms'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Templates/Forms/Edit/:code',
    title: 'Edit Form',
    summary: 'Form builder edit flow.',
    area: 'Templates',
    section: 'Forms',
    moduleCodes: ['FORMVIEW'],
    apiResources: ['api/forms'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Templates/Forms/Display/:code',
    title: 'Display Form',
    summary: 'Read-only form detail view.',
    area: 'Templates',
    section: 'Forms',
    moduleCodes: ['FORMVIEW'],
    apiResources: ['api/forms'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/MaturityLevels/Index',
    title: 'All Maturity Levels',
    summary: 'Maturity level list management screen.',
    area: 'Templates',
    section: 'Maturity Levels',
    moduleCodes: ['MATVIEW'],
    apiResources: ['api/maturitylevels'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/MaturityLevels/Reports',
    title: 'Maturity Level Reports',
    summary: 'Maturity level reporting screen.',
    area: 'Templates',
    section: 'Maturity Levels',
    moduleCodes: ['MATVIEW'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/MaturityLevels/New',
    title: 'New Maturity Level',
    summary: 'Create a maturity level.',
    area: 'Templates',
    section: 'Maturity Levels',
    moduleCodes: ['MATVIEW'],
    apiResources: ['api/maturitylevels'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/MaturityLevels/Edit/:code',
    title: 'Edit Maturity Level',
    summary: 'Edit a maturity level.',
    area: 'Templates',
    section: 'Maturity Levels',
    moduleCodes: ['MATVIEW'],
    apiResources: ['api/maturitylevels'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/MaturityLevels/Display/:code',
    title: 'Display Maturity Level',
    summary: 'Read-only maturity level detail view.',
    area: 'Templates',
    section: 'Maturity Levels',
    moduleCodes: ['MATVIEW'],
    apiResources: ['api/maturitylevels'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/Plants/Index',
    title: 'All Template Plants',
    summary: 'Template plant list and roadmap-link management.',
    area: 'Templates',
    section: 'Plants',
    moduleCodes: ['PLANTVIEW'],
    apiResources: ['api/plants', 'api/plantmembers', 'api/plantroadmaplinks'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/Plants/Reports',
    title: 'Template Plant Reports',
    summary: 'Template plant reporting screen.',
    area: 'Templates',
    section: 'Plants',
    moduleCodes: ['PLANTVIEW'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/Plants/New',
    title: 'New Template Plant',
    summary: 'Create a template plant.',
    area: 'Templates',
    section: 'Plants',
    moduleCodes: ['PLANTVIEW'],
    apiResources: ['api/plants'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/Plants/Overview/:code',
    title: 'Template Plant Overview',
    summary: 'Template plant overview with members and roadmap links.',
    area: 'Templates',
    section: 'Plants',
    moduleCodes: ['PLANTVIEW'],
    apiResources: ['api/plants', 'api/plantmembers', 'api/plantroadmaplinks'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Templates/Plants/Display/:code',
    title: 'Display Template Plant',
    summary: 'Read-only template plant detail view.',
    area: 'Templates',
    section: 'Plants',
    moduleCodes: ['PLANTVIEW'],
    apiResources: ['api/plants'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/ProductDivisions/Index',
    title: 'All Product Divisions',
    summary: 'Product division list management screen.',
    area: 'Templates',
    section: 'Product Divisions',
    moduleCodes: ['PDIVVIEW'],
    apiResources: ['api/productdivisions'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/ProductDivisions/Reports',
    title: 'Product Division Reports',
    summary: 'Product division reporting screen.',
    area: 'Templates',
    section: 'Product Divisions',
    moduleCodes: ['PDIVVIEW'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/ProductDivisions/New',
    title: 'New Product Division',
    summary: 'Create a product division.',
    area: 'Templates',
    section: 'Product Divisions',
    moduleCodes: ['PDIVVIEW'],
    apiResources: ['api/productdivisions'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/ProductDivisions/Edit/:code',
    title: 'Edit Product Division',
    summary: 'Edit product division metadata.',
    area: 'Templates',
    section: 'Product Divisions',
    moduleCodes: ['PDIVVIEW'],
    apiResources: ['api/productdivisions'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/ProductDivisions/Display/:code',
    title: 'Display Product Division',
    summary: 'Read-only product division detail view.',
    area: 'Templates',
    section: 'Product Divisions',
    moduleCodes: ['PDIVVIEW'],
    apiResources: ['api/productdivisions'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/ProductGroups/Index',
    title: 'All Product Groups',
    summary: 'Product group list management screen.',
    area: 'Templates',
    section: 'Product Groups',
    moduleCodes: ['PGRPVIEW'],
    apiResources: ['api/productgroups'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/ProductGroups/Reports',
    title: 'Product Group Reports',
    summary: 'Product group reporting screen.',
    area: 'Templates',
    section: 'Product Groups',
    moduleCodes: ['PGRPVIEW'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/ProductGroups/New',
    title: 'New Product Group',
    summary: 'Create a product group.',
    area: 'Templates',
    section: 'Product Groups',
    moduleCodes: ['PGRPVIEW'],
    apiResources: ['api/productgroups'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/ProductGroups/Edit/:code',
    title: 'Edit Product Group',
    summary: 'Edit product group metadata.',
    area: 'Templates',
    section: 'Product Groups',
    moduleCodes: ['PGRPVIEW'],
    apiResources: ['api/productgroups'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/ProductGroups/Display/:code',
    title: 'Display Product Group',
    summary: 'Read-only product group detail view.',
    area: 'Templates',
    section: 'Product Groups',
    moduleCodes: ['PGRPVIEW'],
    apiResources: ['api/productgroups'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/Roadmaps/Index',
    title: 'All Roadmaps',
    summary: 'Roadmap registry and management surface.',
    area: 'Templates',
    section: 'Roadmaps',
    moduleCodes: ['RMAPVIEW'],
    apiResources: ['api/roadmaps'],
    status: 'api-backed'
  }),
  route({
    path: '/Templates/Roadmaps/Reports',
    title: 'Roadmap Reports',
    summary: 'Roadmap reporting screen.',
    area: 'Templates',
    section: 'Roadmaps',
    moduleCodes: ['RMAPVIEW'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Templates/Roadmaps/New',
    title: 'New Roadmap',
    summary: 'Roadmap builder create flow.',
    area: 'Templates',
    section: 'Roadmaps',
    moduleCodes: ['RMAPVIEW'],
    apiResources: ['api/roadmaps'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Templates/Roadmaps/Copy/:code',
    title: 'Copy Roadmap',
    summary: 'Clone an existing roadmap into a new draft.',
    area: 'Templates',
    section: 'Roadmaps',
    moduleCodes: ['RMAPVIEW'],
    apiResources: ['api/roadmaps'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Templates/Roadmaps/Edit/:code',
    title: 'Edit Roadmap',
    summary: 'Roadmap builder edit flow.',
    area: 'Templates',
    section: 'Roadmaps',
    moduleCodes: ['RMAPVIEW'],
    apiResources: ['api/roadmaps'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Templates/Roadmaps/Display/:code',
    title: 'Display Roadmap',
    summary: 'Read-only roadmap detail view.',
    area: 'Templates',
    section: 'Roadmaps',
    moduleCodes: ['RMAPVIEW'],
    apiResources: ['api/roadmaps'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Sites',
    aliases: ['/Sites/Dashboard/Index'],
    title: 'Sites Dashboard',
    summary: 'Sites area landing page and site-level dashboard.',
    area: 'Sites',
    section: 'Sites',
    status: 'ready-for-build'
  }),
  route({
    path: '/Sites/Plants/Index',
    title: 'All Site Plants',
    summary: 'Site plants list and management screen.',
    area: 'Sites',
    section: 'Plants',
    apiResources: ['api/plants', 'api/plantmembers', 'api/plantroadmaplinks'],
    status: 'api-backed'
  }),
  route({
    path: '/Sites/Plants/Reports',
    title: 'Site Plant Reports',
    summary: 'Site plant reports.',
    area: 'Sites',
    section: 'Plants',
    status: 'ready-for-build'
  }),
  route({
    path: '/Sites/Plants/New',
    title: 'New Site Plant',
    summary: 'Create a site plant.',
    area: 'Sites',
    section: 'Plants',
    apiResources: ['api/plants'],
    status: 'api-backed'
  }),
  route({
    path: '/Sites/Plants/Overview/:code',
    title: 'Site Plant Overview',
    summary: 'Site plant overview with details, members, and roadmap local config.',
    area: 'Sites',
    section: 'Plants',
    apiResources: ['api/plants', 'api/plantmembers', 'api/plantroadmaplinks'],
    status: 'legacy-dependent'
  }),
  route({
    path: '/Sites/Plants/Display/:code',
    title: 'Display Site Plant',
    summary: 'Read-only site plant detail view.',
    area: 'Sites',
    section: 'Plants',
    apiResources: ['api/plants'],
    status: 'ready-for-build'
  }),
  route({
    path: '/Sites/RoadmapSettings/Index',
    title: 'Roadmap Settings',
    summary: 'Site roadmap settings landing page.',
    area: 'Sites',
    section: 'Settings',
    status: 'ready-for-build'
  }),
  route({
    path: '/Sites/UserGroupMembers/Index',
    title: 'User Group Members',
    summary: 'Site user group membership screen.',
    area: 'Sites',
    section: 'Settings',
    status: 'ready-for-build'
  }),
  route({
    path: '/Settings/Dashboard/Index',
    title: 'Settings Dashboard',
    summary: 'Settings dashboard placeholder from the MVC area.',
    area: 'Settings',
    section: 'Settings',
    status: 'ready-for-build'
  }),
  route({
    path: '/Settings/Profile/Index',
    title: 'Profile',
    summary: 'User profile management surface.',
    area: 'Settings',
    section: 'Settings',
    status: 'ready-for-build'
  }),
  route({
    path: '/Error/BadRequest',
    title: 'Bad Request',
    summary: '400 error page.',
    area: 'Error',
    section: 'Errors',
    status: 'ready-for-build'
  }),
  route({
    path: '/Error/Unauthorized',
    title: 'Unauthorized',
    summary: '401 error page.',
    area: 'Error',
    section: 'Errors',
    status: 'ready-for-build'
  }),
  route({
    path: '/Error/Forbidden',
    title: 'Forbidden',
    summary: '403 error page.',
    area: 'Error',
    section: 'Errors',
    status: 'ready-for-build'
  }),
  route({
    path: '/Error/NotFound',
    title: 'Not Found',
    summary: '404 error page.',
    area: 'Error',
    section: 'Errors',
    status: 'ready-for-build'
  }),
  route({
    path: '/Error/ServerError',
    title: 'Server Error',
    summary: '500 error page.',
    area: 'Error',
    section: 'Errors',
    status: 'ready-for-build'
  }),
  route({
    path: '/Error/General',
    title: 'General Error',
    summary: 'Generic error fallback screen.',
    area: 'Error',
    section: 'Errors',
    status: 'ready-for-build'
  })
];

export const routeStatusLabel: Record<RouteStatus, string> = {
  'ready-for-build': 'Shell ready',
  'api-backed': 'API-backed',
  'legacy-dependent': 'Legacy dependency'
};

export const shellAreas: PulseArea[] = ['Root', 'Projects', 'Templates', 'Admin', 'Sites', 'Settings'];