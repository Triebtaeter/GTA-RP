﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace GTA_RP.Database
{
    /// <summary>
    /// SQL SELECT query wrapper
    /// </summary>
    class SelectQuery : Query
    {
        public SelectQuery(string query, Action<MySqlDataReader> code) : base(query, code) { }

        public override void Execute()
        {
            var reader = mysqlCommand.ExecuteReader();
            while (reader.Read())
                if (code != null) code(reader);
            reader.Close();
        }
    }
}
