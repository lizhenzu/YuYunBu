using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageYoutubeShorts
	{
        public List<YoutubeShort> getYoutubeShorts(string contentsURL = null, string contentsText = null)
        {
            ControlYoutubeShorts controlYoutubeShorts = new ControlYoutubeShorts();
            DataSet ds = controlYoutubeShorts.fetchYoutubeShorts(contentsURL, contentsText);

            List<YoutubeShort> value = ds.Tables[0].ToList<YoutubeShort>();

            return value;
        }
        public List<YoutubeShortByGroups> getYoutubeShortsByGroup()
        {
            ControlYoutubeShorts controlYoutubeShorts = new ControlYoutubeShorts();
            DataSet ds = controlYoutubeShorts.fetchYoutubeShortsByGroup();

            List<YoutubeShortByGroups> value = ds.Tables[0].ToList<YoutubeShortByGroups>();

            return value;
        }

        public void DeleteUploadData(int id)
        {
			ControlYoutubeShorts controlYoutubeShorts = new ControlYoutubeShorts();
			controlYoutubeShorts.deleteYoutubeShorts(id);
        }
         
        public void upsertYoutubeShorts(string contentsURL, string contentsText)
		{
			ControlYoutubeShorts controlYoutubeShorts = new ControlYoutubeShorts();
			controlYoutubeShorts.upsertYoutubeShorts(contentsURL, contentsText);
        } 
    }
}
