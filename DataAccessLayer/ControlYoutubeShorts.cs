using BulletinBoard.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlYoutubeShorts
	{
        public DataSet fetchYoutubeShorts(string contentsURL = null, string contentsText = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT Id, contentsURL, contentsText ");
            sb.AppendLine("FROM [dbo].[YoutubeShorts]");
            if (!string.IsNullOrEmpty(contentsURL))
            {
                sb.AppendFormat("WHERE contentsURL = '{0}'", contentsURL);
                sb.AppendLine();
            }
            else if (!string.IsNullOrEmpty(contentsText))
            {
                sb.AppendFormat("WHERE contentsText = '{0}'", SharedComm.appendSpecialChar(contentsText));
                sb.AppendLine();
            }
            sb.AppendLine("ORDER BY contentsText DESC");
            return DataAccessModule.commonQuery(sb.ToString());
        }
        public DataSet fetchYoutubeShortsByGroup()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" SELECT [contentsText], count(*) DCount  ");
            sb.AppendLine(" FROM [dbo].[YoutubeShorts]  ");
            sb.AppendLine(" group by [contentsText]"  );
            return DataAccessModule.commonQuery(sb.ToString());
        }

        public void upsertYoutubeShorts(string contentsURL, string contentsText)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("IF EXISTS (SELECT 1 FROM [dbo].[YoutubeShorts] WHERE contentsURL = @contentsURL)");
			sb.AppendLine("BEGIN");
			sb.AppendLine("    UPDATE [dbo].[YoutubeShorts]");
			sb.AppendLine("    SET contentsText = @contentsText, updateDT = GETDATE()");
			sb.AppendLine("    WHERE contentsURL = @contentsURL");
			sb.AppendLine("END");
			sb.AppendLine("ELSE");
			sb.AppendLine("BEGIN");
			sb.AppendLine("    INSERT INTO [dbo].[YoutubeShorts] (contentsURL, contentsText)");
			sb.AppendLine("    VALUES (@contentsURL, @contentsText)");
			sb.AppendLine("END");

			var parameters = new List<SqlParameter>
			{
				new SqlParameter("@contentsURL", contentsURL),
				new SqlParameter("@contentsText", contentsText)
			};

			DataAccessModule.executeNonQuery(sb.ToString(), parameters);
		}

		public void deleteYoutubeShorts(int id)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("DELETE FROM [dbo].[YoutubeShorts]");
			sb.AppendFormat("WHERE Id = {0}", id);
			sb.AppendLine();
			DataAccessModule.executeNonQuery(sb.ToString());
		}

	}
}
