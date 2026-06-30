using System;
using System.Configuration;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Activators.Reflection;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using log4net;
using Microsoft.AspNet.SignalR;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using Pulse.Infrastructure.Events;
using Pulse.Infrastructure.Repositories;
using Pulse.Services.Implementations;

namespace Pulse.Web
{
    public class ContainerConfig
    {
        public static IContainer Container { get; private set; }

        public static void RegisterContainer(HttpConfiguration httpConfiguration)
        {
            var container = Configure();

            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));

        }
        public static IContainer Configure()
        {
            Container = Builder().Build();
            // Build and return the container
            return Container;
        }



        public static ContainerBuilder Builder()
        {
            var builder = new ContainerBuilder();
            // Read configuration values from web.config
            string domain = ConfigurationManager.AppSettings["ad:domain"];
            string domainContainer = ConfigurationManager.AppSettings["ad:domaincontainer"];
            string domainWeb = ConfigurationManager.AppSettings["ad:domainweb"];
            string ldapServer = ConfigurationManager.AppSettings["ad:ldapserver"];
            string ldapContainer = ConfigurationManager.AppSettings["ad:ldapservercontainer"];
            string adGroupExtEmail = ConfigurationManager.AppSettings["ad:adgroup-emailext"];

            // Read configuration values from web.config  
            string smtpHost = ConfigurationManager.AppSettings["smtp:ClientHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["smtp:ClientPort"]);
            string smtpFromAddress = ConfigurationManager.AppSettings["smtp:ClientMailFromAddress"];
            string stmpFromDisplay = ConfigurationManager.AppSettings["smtp:ClientMailFromDisplay"];

            // Register MVC controllers
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            // Register SignalR hubs
            builder.RegisterHubs(Assembly.GetExecutingAssembly());

            // Register Oracle Connection
            //builder.Register(c => new OracleDataAccessLayer(ConfigurationManager.AppSettings["defaultconnection"])).AsSelf().InstancePerLifetimeScope();
            //builder.Register(c => new OracleDataAccessLayer(ConfigurationManager.AppSettings["defaultconnection"])).AsSelf().InstancePerLifetimeScope();


            builder.RegisterType<ActiveDirectoryService>()
               .As<IActiveDirectoryService>()
               .WithParameter("domain", domain)
               .WithParameter("domainContainer", domainContainer)
               .WithParameter("domainWeb", domainWeb)
               .WithParameter("ldapServer", ldapServer)
               .WithParameter("ldapContainer", ldapContainer)
               .WithParameter("adGroupExtEmail", adGroupExtEmail);

            builder.Register(c =>
            {
                var connection = "PulseConnection";
                var connectionString = ConfigurationManager.ConnectionStrings["defaultconnection"]?.ConnectionString ?? "";
                var logger = LogManager.GetLogger(typeof(Pulse.Infrastructure.DataAccess.OracleDataAccessLayer));
                return new OracleDataAccessLayer(connection, connectionString, logger);
            })
            .AsSelf()
            .InstancePerLifetimeScope();


            // Register repositories
            void RegisterRepository<TRepo, TInterface>(ContainerBuilder _builder)
                where TRepo : TInterface
            {
                _builder.RegisterType<TRepo>()
                    .As<TInterface>()
                    .WithParameter(
                        (pi, ctx) => pi.ParameterType == typeof(ILog),
                        (pi, ctx) => LogManager.GetLogger(typeof(TRepo))
                    )
                    .InstancePerLifetimeScope();
            }


            RegisterRepository<ActiveDirectoryGroupRepository, IActiveDirectoryGroupRepository>(builder);
            RegisterRepository<PlantMemberRepository, IPlantMemberRepository>(builder);
            RegisterRepository<ProjectMemberRepository, IProjectMemberRepository>(builder);
            RegisterRepository<ProjectMilestoneRepository, IProjectMilestoneRepository>(builder);
            RegisterRepository<ProjectRepository, IProjectRepository>(builder);
            RegisterRepository<ProjectStatusChangeRepository, IProjectStatusChangeRepository>(builder);
            RegisterRepository<ProjectTaskRepository, IProjectTaskRepository>(builder);
            RegisterRepository<ProjectTargetRevisionRepository, IProjectTargetRevisionRepository>(builder);
            RegisterRepository<UserRepository, IUserRepository>(builder);
            RegisterRepository<UserGroupMemberRepository, IUserGroupMemberRepository>(builder);
            RegisterRepository<ProjectChatRepository, IProjectChatRepository>(builder);
            RegisterRepository<NotificationRepository, INotificationRepository>(builder);
            RegisterRepository<UserGroupRepository, IUserGroupRepository>(builder);
            RegisterRepository<UserGroupAccessRightRepository, IUserGroupAccessRightRepository>(builder);
            RegisterRepository<PlantUserGroupMemberRepository, IPlantUserGroupMemberRepository>(builder);


            // Register services
            //builder.RegisterType<UserService>().As<IUserService>().SingleInstance().InstancePerLifetimeScope();
            //builder.RegisterType<ProjectChatService>().As<IProjectChatService>().InstancePerRequest();

            builder.RegisterType<UserService>().As<IUserService>().FindConstructorsWith(new DefaultConstructorFinder()).InstancePerLifetimeScope();
            builder.RegisterType<UserGroupService>().As<IUserGroupService>().FindConstructorsWith(new DefaultConstructorFinder()).InstancePerLifetimeScope();
            builder.RegisterType<NotificationService>().As<INotificationService>().FindConstructorsWith(new DefaultConstructorFinder()).InstancePerLifetimeScope();
            builder.RegisterType<ProjectTaskService>().As<IProjectTaskService>().FindConstructorsWith(new DefaultConstructorFinder()).InstancePerLifetimeScope();
            
            builder.RegisterType<EventBus>().As<IEventPublisher>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectChatService>().As<IProjectChatService>().FindConstructorsWith(new DefaultConstructorFinder()).InstancePerLifetimeScope();

             

            return builder;
        }
    }
}