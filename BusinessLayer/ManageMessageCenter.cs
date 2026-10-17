using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace BulletinBoard.BusinessLayer
{
    public class ManageMessageCenter
    {
        public List<PopMessages> getPopMessage(IWebHostEnvironment _hostingEnvironment, int Id, string FromWhom, string ToWhom, PopMessageReadMehod pmrm, string currentUserId)
        {
            ControlMessageCenter controlMessageCenter = new ControlMessageCenter();
            DataSet ds = controlMessageCenter.fetchPopMessage(Id, FromWhom, ToWhom, pmrm, currentUserId);

            List<PopMessages> value = new List<PopMessages>();

            var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];
                //// Read the varbinary(max) field as byte array
                //byte[] profilePictureBytes = dr["ProfilePicture"] as byte[];
                //string ProfilePicture = null;
                //// Convert byte array to Base64 string
                //if (profilePictureBytes != null)
                //{
                //    // ProfilePicture = Convert.ToBase64String(profilePictureBytes);
                //    //ProfilePicture = SharedComm.CompressAndConvertToBase64(profilePictureBytes);
                //    ProfilePicture = SharedComm.CompressAndConvertToBase64(profilePictureBytes);


                //}

                PopMessages popMessages = new PopMessages();
                popMessages.sequenceNo = long.Parse(dr["sequenceNo"].ToString());
                popMessages.Id = int.Parse(dr["Id"].ToString());

                popMessages.MessageText = dr["MessageText"].ToString() as string;
                popMessages.readYN = dr["readYN"].ToString() as string;

                popMessages.FromWhom = dr["FromWhom"].ToString() as string;
                popMessages.FromWhomName = dr["FromWhomName"].ToString() as string;
                popMessages.FromWhomID = dr["FromWhomID"].ToString() as string;

                popMessages.ToWhom = dr["ToWhom"].ToString() as string;
                popMessages.ToWhomName = dr["ToWhomName"].ToString() as string;
                popMessages.ToWhomID = dr["ToWhomID"].ToString() as string;



                popMessages.ProfilePicturePathFromWhom = ConstatsCommon.representFaceImage;
                popMessages.ProfilePicturePathToWhom = ConstatsCommon.representFaceImage;


                string filePath = Path.Combine(profilePath, $"{popMessages.FromWhomID}.png");

                if (System.IO.File.Exists(filePath))
                {
                    popMessages.ProfilePicturePathFromWhom = $"/images/profile/{popMessages.FromWhomID}.png";
                } 

                filePath = Path.Combine(profilePath, $"{popMessages.ToWhomID}.png");

                if (System.IO.File.Exists(filePath))
                {
                    popMessages.ProfilePicturePathToWhom = $"/images/profile/{popMessages.ToWhomID}.png";
                }

             
                popMessages.Visited = int.Parse(dr["Visited"].ToString());
                popMessages.updateDT = DateTime.Parse(dr["updateDT"].ToString());


                value.Add(popMessages);


            }



            //   List<PopMessages> value = ds.Tables[0].ToList<PopMessages>();
            return value;
        }

        public void CreatePopMessage(PopMessage param)
        {
            ControlMessageCenter controlMessageCenter = new ControlMessageCenter();
            controlMessageCenter.CreatePopMessage(param.MessageText, param.FromWhom, param.ToWhom);
        }

        public void UpdatePopMessage(PopMessage param)
        {
            ControlMessageCenter controlMessageCenter = new ControlMessageCenter();
            controlMessageCenter.UpdatePopMessage(param.Id, param.MessageText, param.FromWhom, param.ToWhom);
        }

        public void DeletePopMessage(int Id)
        {
            ControlMessageCenter controlMessageCenter = new ControlMessageCenter();
            controlMessageCenter.DeletePopMessage(Id);
        }

        internal void UpdateHitPopMessage(string ToWhom)
        {
            ControlMessageCenter controlMessageCenter = new ControlMessageCenter();
            controlMessageCenter.UpdateHitPopMessage(ToWhom);
        }
    }
}
