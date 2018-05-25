using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Services.Installation;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Install;

namespace Web.ZhiXiao.Controllers
{
    public class InstallController : BasePublicController
    {
        #region Fields

        private readonly NopConfig _config;

        #endregion

        #region Ctor

        public InstallController(NopConfig config)
        {
            this._config = config;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// A value indicating whether we use MARS (Multiple Active Result Sets)
        /// </summary>
        protected virtual bool UseMars
        {
            get { return false; }
        }

        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Returns true if the database exists.</returns>
        [NonAction]
        protected virtual bool SqlServerDatabaseExists(string connectionString)
        {
            try
            {
                //just try to connect
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a database on the server.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="collation">Server collation; the default one will be used if not specified</param>
        /// <param name="triesToConnect">
        /// Number of times to try to connect to database. 
        /// If connection cannot be open, then error will be returned. 
        /// Pass 0 to skip this validation.
        /// </param>
        /// <returns>Error</returns>
        [NonAction]
        protected virtual string CreateDatabase(string connectionString, string collation, int triesToConnect = 10)
        {
            try
            {
                //parse database name
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                //now create connection string to 'master' dabatase. It always exists.
                builder.InitialCatalog = "master";
                var masterCatalogConnectionString = builder.ToString();
                string query = string.Format("CREATE DATABASE [{0}]", databaseName);
                if (!String.IsNullOrWhiteSpace(collation))
                    query = string.Format("{0} COLLATE {1}", query, collation);
                using (var conn = new SqlConnection(masterCatalogConnectionString))
                {
                    conn.Open();
                    using (var command = new SqlCommand(query, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                //try connect
                if (triesToConnect > 0)
                {
                    //Sometimes on slow servers (hosting) there could be situations when database requires some time to be created.
                    //But we have already started creation of tables and sample data.
                    //As a result there is an exception thrown and the installation process cannot continue.
                    //That's why we are in a cycle of "triesToConnect" times trying to connect to a database with a delay of one second.
                    for (var i = 0; i <= triesToConnect; i++)
                    {
                        if (i == triesToConnect)
                            throw new Exception("Unable to connect to the new database. Please try one more time");

                        if (!this.SqlServerDatabaseExists(connectionString))
                            Thread.Sleep(1000);
                        else
                            break;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Create contents of connection strings used by the SqlConnection class
        /// </summary>
        /// <param name="trustedConnection">Avalue that indicates whether User ID and Password are specified in the connection (when false) or whether the current Windows account credentials are used for authentication (when true)</param>
        /// <param name="serverName">The name or network address of the instance of SQL Server to connect to</param>
        /// <param name="databaseName">The name of the database associated with the connection</param>
        /// <param name="userName">The user ID to be used when connecting to SQL Server</param>
        /// <param name="password">The password for the SQL Server account</param>
        /// <param name="timeout">The connection timeout</param>
        /// <returns>Connection string</returns>
        [NonAction]
        protected virtual string CreateConnectionString(bool trustedConnection,
            string serverName, string databaseName,
            string userName, string password, int timeout = 0)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.IntegratedSecurity = trustedConnection;
            builder.DataSource = serverName;
            builder.InitialCatalog = databaseName;
            if (!trustedConnection)
            {
                builder.UserID = userName;
                builder.Password = password;
            }
            builder.PersistSecurityInfo = false;
            if (this.UseMars)
            {
                builder.MultipleActiveResultSets = true;
            }
            if (timeout > 0)
            {
                builder.ConnectTimeout = timeout;
            }
            return builder.ConnectionString;
        }

        #endregion

        // GET: Install
        public ActionResult Index()
        {
            if (DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

            var model = new InstallModel
            {
                AdminEmail = "yijiayi168@yijiayifr.com",
                InstallSampleData = true,
                DatabaseConnectionString = "",
                DataProvider = "sqlserver",
                //fast installation service does not support SQL compact
                DisableSqlCompact = _config.UseFastInstallationService,
                DisableSampleDataOption = _config.DisableSampleDataDuringInstallation,
                SqlAuthenticationType = "sqlauthentication",
                SqlConnectionInfo = "sqlconnectioninfo_values",
                SqlServerCreateDatabase = true,
                UseCustomCollation = false,
                Collation = "SQL_Latin1_General_CP1_CI_AS",


                // default password, and sql server account
                AdminPassword = "123456",
                ConfirmPassword = "13456",
                SqlServerName =  ".",
                SqlDatabaseName = "zhixiao",
                SqlServerUsername = "sa",
                SqlServerPassword = "123456"
            };

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Index(InstallModel model)
        {
            if (DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;


            model.DisableSqlCompact = _config.UseFastInstallationService;
            model.DisableSampleDataOption = _config.DisableSampleDataDuringInstallation;

            //SQL Server
            if (model.DataProvider.Equals("sqlserver", StringComparison.InvariantCultureIgnoreCase))
            {
                //values
                if (string.IsNullOrEmpty(model.SqlServerName))
                    ModelState.AddModelError("", "SqlServerName Required");
                if (string.IsNullOrEmpty(model.SqlDatabaseName))
                    ModelState.AddModelError("", "DatabaseName Required");

                //authentication type - SQL authentication
                if (string.IsNullOrEmpty(model.SqlServerUsername))
                    ModelState.AddModelError("", "SqlServerUsername Required");
                if (string.IsNullOrEmpty(model.SqlServerPassword))
                    ModelState.AddModelError("", "SqlServerPassword Required");

            }

            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            if (ModelState.IsValid)
            {
                var settingsManager = new DataSettingsManager();
                try
                {
                    string connectionString;
                    if (model.DataProvider.Equals("sqlserver", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //values
                        connectionString = CreateConnectionString(model.SqlAuthenticationType == "windowsauthentication",
                            model.SqlServerName, model.SqlDatabaseName,
                            model.SqlServerUsername, model.SqlServerPassword);

                        if (model.SqlServerCreateDatabase)
                        {
                            if (!SqlServerDatabaseExists(connectionString))
                            {
                                //create database
                                var collation = model.UseCustomCollation ? model.Collation : "";
                                var errorCreatingDatabase = CreateDatabase(connectionString, collation);
                                if (!String.IsNullOrEmpty(errorCreatingDatabase))
                                    throw new Exception(errorCreatingDatabase);
                            }
                        }
                    }
                    else
                    {
                        //SQL CE
                        string databaseFileName = "Nop.Db.sdf";
                        string databasePath = @"|DataDirectory|\" + databaseFileName;
                        connectionString = "Data Source=" + databasePath + ";Persist Security Info=False";

                        //drop database if exists
                        string databaseFullPath = CommonHelper.MapPath("~/App_Data/") + databaseFileName;
                        if (System.IO.File.Exists(databaseFullPath))
                        {
                            System.IO.File.Delete(databaseFullPath);
                        }
                    }

                    //save settings
                    var dataProvider = model.DataProvider;
                    var settings = new DataSettings
                    {
                        DataProvider = dataProvider,
                        DataConnectionString = connectionString
                    };
                    settingsManager.SaveSettings(settings);

                    //init data provider
                    var dataProviderInstance = EngineContext.Current.Resolve<BaseDataProviderManager>().LoadDataProvider();
                    dataProviderInstance.InitDatabase();

                    //now resolve installation service
                    var installationService = EngineContext.Current.Resolve<IInstallationService>();
                    installationService.InstallData(model.AdminEmail, model.AdminPassword, true);

                    //reset cache
                    DataSettingsHelper.ResetCache();

                    //restart application
                    webHelper.RestartAppDomain();

                    //register default permissions
                    //var permissionProviders = EngineContext.Current.Resolve<ITypeFinder>().FindClassesOfType<IPermissionProvider>();
                    var permissionProviders = new List<Type>();
                    permissionProviders.Add(typeof(StandardPermissionProvider));
                    foreach (var providerType in permissionProviders)
                    {
                        dynamic provider = Activator.CreateInstance(providerType);
                        EngineContext.Current.Resolve<IPermissionService>().InstallPermissions(provider);
                    }

                    //Redirect to home page
                    return RedirectToRoute("HomePage");
                }
                catch (Exception exception)
                {
                    //reset cache
                    DataSettingsHelper.ResetCache();

                    var cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
                    cacheManager.Clear();

                    //clear provider settings if something got wrong
                    settingsManager.SaveSettings(new DataSettings
                    {
                        DataProvider = null,
                        DataConnectionString = null
                    });

                    ModelState.AddModelError("", exception.Message);
                }
            }

            return View(model);
        }

        /// <summary>
        /// 安装时如果没有初始化数据, 可以调用该步骤
        /// </summary>
        /// <returns></returns>
        public ActionResult InstallSampleData()
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            return View();
        }

        [HttpPost]
        public ActionResult InstallSampleData(string adminEmail, string adminPassword, string confirmPassword)
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("Install");

            if (string.IsNullOrEmpty(adminEmail))
                ModelState.AddModelError("", "adminEmail Required");
            if (string.IsNullOrEmpty(adminPassword))
                ModelState.AddModelError("", "adminPassword Required");
            if (confirmPassword != adminPassword)
                ModelState.AddModelError("", "adminPassword != confirmPassword");

            //now resolve installation service
            var installationService = EngineContext.Current.Resolve<IInstallationService>();
            installationService.InstallData(adminEmail, adminPassword, true);

            //Redirect to home page
            return RedirectToRoute("HomePage");
        }
    }
}