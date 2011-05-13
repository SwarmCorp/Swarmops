using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using Activizr.Basic.Enums;
using Activizr.Basic.Types;
using System.Data;

namespace Activizr.Database
{
    public partial class PirateDb
    {

        public BasicPermission[] GetPermissionsTable ()
        {
            List<BasicPermission> result = new List<BasicPermission>();

            using (DbConnection connection = GetMySqlDbConnection())
            {
                connection.Open();

                DbCommand command = GetDbCommand("Select RoleType, PermissionType from PermissionSpecifications", connection);

                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new BasicPermission(
                                        (RoleType)(Enum.Parse(typeof(RoleType), reader.GetString(0), true)),
                                        (Permission)(Enum.Parse(typeof(Permission), reader.GetString(1), true))
                                        )
                                    );
                    }
                }
            }

            return result.ToArray();
        }

        public void StoreOnePermission (int RoleTypeId, int PermissionId, bool allow)
        {

            using (DbConnection connection = GetMySqlDbConnection())
            {

                connection.Open();

                DbCommand command = GetDbCommand("StoreOnePermission", connection);
                command.CommandType = CommandType.StoredProcedure;

                AddParameterWithName(command, "p_RoleType", ((RoleType)RoleTypeId).ToString());
                AddParameterWithName(command, "p_PermissionType", ((Permission)PermissionId).ToString());
                AddParameterWithName(command, "p_allow", allow ? 1 : 0);

                command.ExecuteNonQuery();
            }

        }

    }
}