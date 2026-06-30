import type { PulseArea } from './routes';

export type NavigationItem = {
  title: string;
  to?: string;
  icon: string;
  moduleCodes?: string[];
  children?: Array<{
    title: string;
    to: string;
  }>;
};

export const navigationByArea: Record<PulseArea, NavigationItem[]> = {
  Root: [
    { title: 'Dashboard', to: '/', icon: 'bi-speedometer2' },
    { title: 'Register', to: '/Projects/Create', icon: 'bi-journal-plus' },
    { title: 'Projects', to: '/Projects/Index', icon: 'bi-columns-gap' },
    { title: 'Tasks', to: '/Projects/ProjectTasks/Index', icon: 'bi-ui-checks' },
    { title: 'View', to: '/Home/ViewProjects', icon: 'bi-binoculars-fill' },
    { title: 'Chats', to: '/Chats/Index', icon: 'bi-chat-dots-fill' },
    { title: 'Templates', to: '/Templates', icon: 'bi-boxes', moduleCodes: ['CALNDRVIEW', 'CATGRYVIEW', 'FORMVIEW', 'MATVIEW', 'PLANTVIEW', 'PDIVVIEW', 'PGRPVIEW', 'RMAPVIEW'] },
    { title: 'Admin', to: '/Admin', icon: 'bi-shield-lock-fill', moduleCodes: ['MODULEVIEW', 'USRGRPVIEW'] },
    {
      title: 'Reports',
      icon: 'bi-file-earmark-bar-graph-fill',
      children: [
        { title: 'Project Export', to: '/Home/Report1' },
        { title: 'Monitoring Matrix', to: '/Home/Report2' },
        { title: 'Project Comparison', to: '/Home/Report3' }
      ]
    },
    { title: 'FAQ', to: '/Home/Guide', icon: 'bi-question-circle-fill' }
  ],
  Projects: [
    { title: 'Dashboard', to: '/', icon: 'bi-speedometer2' },
    { title: 'Projects', to: '/Projects/Index', icon: 'bi-columns-gap' },
    { title: 'Register', to: '/Projects/Create', icon: 'bi-journal-plus' },
    { title: 'Tasks', to: '/Projects/ProjectTasks/Index', icon: 'bi-ui-checks' },
    { title: 'Chats', to: '/Projects/ProjectChats/Index', icon: 'bi-chat-dots-fill' }
  ],
  Admin: [
    { title: 'Admin Console', to: '/Admin', icon: 'bi-shield-lock-fill' },
    {
      title: 'Modules',
      icon: 'bi-puzzle',
      moduleCodes: ['MODULEVIEW'],
      children: [
        { title: 'Reports', to: '/Admin/Modules/Reports' },
        { title: 'All Modules', to: '/Admin/Modules/Index' },
        { title: 'New Module', to: '/Admin/Modules/New' }
      ]
    },
    {
      title: 'User Groups',
      icon: 'bi-people-fill',
      moduleCodes: ['USRGRPVIEW'],
      children: [
        { title: 'Reports', to: '/Admin/UserGroups/Reports' },
        { title: 'All User Groups', to: '/Admin/UserGroups/Index' },
        { title: 'New User Group', to: '/Admin/UserGroups/New' }
      ]
    },
    {
      title: 'Reports',
      icon: 'bi-file-earmark-bar-graph-fill',
      children: [
        { title: 'Project Export', to: '/Home/Report1' },
        { title: 'Monitoring Matrix', to: '/Home/Report2' },
        { title: 'Project Comparison', to: '/Home/Report3' }
      ]
    },
    { title: 'FAQ', to: '/Home/Guide', icon: 'bi-question-circle-fill' }
  ],
  Templates: [
    { title: 'Templates', to: '/Templates', icon: 'bi-boxes' },
    {
      title: 'Calendars',
      icon: 'bi-calendar3',
      moduleCodes: ['CALNDRVIEW'],
      children: [
        { title: 'Reports', to: '/Templates/ProductionCalendars/Reports' },
        { title: 'All Calendars', to: '/Templates/ProductionCalendars/Index' },
        { title: 'New Calendar', to: '/Templates/ProductionCalendars/New' }
      ]
    },
    {
      title: 'Categories',
      icon: 'bi-circle-square',
      moduleCodes: ['CATGRYVIEW'],
      children: [
        { title: 'Reports', to: '/Templates/Categories/Reports' },
        { title: 'All Categories', to: '/Templates/Categories/Index' },
        { title: 'New Category', to: '/Templates/Categories/New' }
      ]
    },
    {
      title: 'Forms',
      icon: 'bi-code-square',
      moduleCodes: ['FORMVIEW'],
      children: [
        { title: 'Reports', to: '/Templates/Forms/Reports' },
        { title: 'All Forms', to: '/Templates/Forms/Index' },
        { title: 'New Form', to: '/Templates/Forms/New' }
      ]
    },
    {
      title: 'Maturities',
      icon: 'bi-clock-history',
      moduleCodes: ['MATVIEW'],
      children: [
        { title: 'Reports', to: '/Templates/MaturityLevels/Reports' },
        { title: 'All Maturity Levels', to: '/Templates/MaturityLevels/Index' },
        { title: 'New Maturity Level', to: '/Templates/MaturityLevels/New' }
      ]
    },
    {
      title: 'Plants',
      icon: 'bi-building-fill',
      moduleCodes: ['PLANTVIEW'],
      children: [
        { title: 'Reports', to: '/Templates/Plants/Reports' },
        { title: 'All Plants', to: '/Templates/Plants/Index' },
        { title: 'New Plant', to: '/Templates/Plants/New' }
      ]
    },
    {
      title: 'Product Divisions',
      icon: 'bi-layers-half',
      moduleCodes: ['PDIVVIEW'],
      children: [
        { title: 'Reports', to: '/Templates/ProductDivisions/Reports' },
        { title: 'All Product Divisions', to: '/Templates/ProductDivisions/Index' },
        { title: 'New Product Division', to: '/Templates/ProductDivisions/New' }
      ]
    },
    {
      title: 'Product Groups',
      icon: 'bi-node-plus-fill',
      moduleCodes: ['PGRPVIEW'],
      children: [
        { title: 'Reports', to: '/Templates/ProductGroups/Reports' },
        { title: 'All Product Groups', to: '/Templates/ProductGroups/Index' },
        { title: 'New Product Group', to: '/Templates/ProductGroups/New' }
      ]
    },
    {
      title: 'Roadmaps',
      icon: 'bi-signpost-split',
      moduleCodes: ['RMAPVIEW'],
      children: [
        { title: 'Reports', to: '/Templates/Roadmaps/Reports' },
        { title: 'All Roadmaps', to: '/Templates/Roadmaps/Index' },
        { title: 'New Roadmap', to: '/Templates/Roadmaps/New' }
      ]
    },
    { title: 'FAQ', to: '/Home/Guide', icon: 'bi-question-circle-fill' }
  ],
  Sites: [
    { title: 'Dashboard', to: '/Sites', icon: 'bi-speedometer2' },
    {
      title: 'Plants',
      icon: 'bi-building',
      children: [
        { title: 'All Plants', to: '/Sites/Plants/Index' },
        { title: 'New Plant', to: '/Sites/Plants/New' }
      ]
    },
    {
      title: 'Reports',
      icon: 'bi-file-earmark-bar-graph-fill',
      children: [
        { title: 'Project Export', to: '/Home/Report1' },
        { title: 'Monitoring Matrix', to: '/Home/Report2' },
        { title: 'Project Comparison', to: '/Home/Report3' }
      ]
    },
    { title: 'FAQ', to: '/Home/Guide', icon: 'bi-question-circle-fill' }
  ],
  Settings: [
    { title: 'Dashboard', to: '/Settings/Dashboard/Index', icon: 'bi-gear-fill' },
    { title: 'Profile', to: '/Settings/Profile/Index', icon: 'bi-person-badge-fill' }
  ],
  Error: [
    { title: 'Back to Dashboard', to: '/', icon: 'bi-house-fill' }
  ]
};