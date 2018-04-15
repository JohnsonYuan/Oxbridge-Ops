using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;

namespace Nop.Data.Initializers
{
    public class DropCreateDatabaseIfModelChanges<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        private readonly string[] _tablesToValidate;
        private readonly string[] _customCommands;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="tablesToValidate">A list of existing table names to validate; null to don't validate table names</param>
        /// <param name="customCommands">A list of custom commands to execute</param>
        public DropCreateDatabaseIfModelChanges(string[] tablesToValidate, string [] customCommands)
        {
            this._tablesToValidate = tablesToValidate;
            this._customCommands = customCommands;
        }
        public void InitializeDatabase(TContext context)
        {
            bool dbExists;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                dbExists = context.Database.Exists();
            }

            if (dbExists)
            {
                try
                {
                    if (context.Database.CompatibleWithModel(throwIfNoMetadata: true))
                    {
                        bool createTables;
                        if (_tablesToValidate != null && _tablesToValidate.Length > 0)
                        {
                            //we have some table names to validate
                            var existingTableNames = new List<string>(context.Database.SqlQuery<string>("SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE'"));
                            createTables = !existingTableNames.Intersect(_tablesToValidate, StringComparer.InvariantCultureIgnoreCase).Any();
                        }
                        else
                        {
                            //check whether tables are already created
                            int numberOfTables = 0;
                            foreach (var t1 in context.Database.SqlQuery<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE' "))
                                numberOfTables = t1;

                            createTables = numberOfTables == 0;
                        }

                        if (!createTables)
                            return;

                        context.Database.Delete();
                    }
                }
                catch (Exception)
                {
                    context.Database.Delete();
                }
            }

            // Database didn't exist or we deleted it, so we now create it again.
            context.Database.Create();

            Seed(context);
            context.SaveChanges();

            if (_customCommands != null && _customCommands.Length > 0)
            {
                foreach (var command in _customCommands)
                    context.Database.ExecuteSqlCommand(command);
            }
        }

        #region Seeding methods

        /// <summary>
        /// A that should be overridden to actually add data to the context for seeding. 
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="context">The context to seed.</param>
        protected virtual void Seed(TContext context)
        {
        }

        #endregion
    }
}
