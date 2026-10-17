// using BulletinBoard.DataAccessLayer;
// using BulletinBoard.Models;
// using BulletinBoard.Common;
// using System.Collections.Generic;
// using System.Data;
// using System.Linq;
// using System;

// namespace BulletinBoard.BusinessLayer
// {
//     public class ManageBibleGeomorphological
//     {
//         //public List<eBiblePeople> getBibleGeomorphological(string Keyword)
//         //{
//         //    ControleBibleGeomorphological controleBibleGeomorphological = new ControleBibleGeomorphological();
//         //    DataSet ds = controleBibleGeomorphological.fetcheBiblePeople(Keyword);           
   
//         //    List<eBiblePeople> value = ds.Tables[0].ToList<eBiblePeople>();

//         //    return value;
//         //}
//         public void CreateBiblePeopleWeeklyWordToReadAssoc(int WeeklyWordToReadID, List<BibleGeomorphological> selectedBibleGeomorphological)
//         {
//             ControleBibleGeomorphological controleBibleGeomorphological = new ControleBibleGeomorphological();
//             controleBibleGeomorphological.CreateBibleGeomorphologicalWeeklyWordToReadAssoc(WeeklyWordToReadID, selectedBibleGeomorphological);
//         }
//         public void CreateBibleGeomorphological(string bibleGeomorphologicalTEXT, string bibleGeomorphologicalURL)
//         {
//             ControleBibleGeomorphological controleBibleGeomorphological = new ControleBibleGeomorphological();
//             controleBibleGeomorphological.CreateBibleGeomorphological(bibleGeomorphologicalTEXT, bibleGeomorphologicalURL);
//         }

        

//         //public void CreatePrayPhrase(PrayPhrase PrayPhrase)
//         //{
//         //    ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();

//         //    controlPrayPhrase.CreatePrayPhrase(PrayPhrase.ExposeDate, PrayPhrase.PraryBeginMesssage, PrayPhrase.PrayContent1, PrayPhrase.PrayContent2, PrayPhrase.PrayContent3, PrayPhrase.PraryEndMesssage, PrayPhrase.PrayTitle, PrayPhrase.BiblePhrase);
//         //}

//         //public void UpdatePrayPhrase(PrayPhrase PrayPhrase)
//         //{
//         //    ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();

//         //    controlPrayPhrase.UpdatePrayPhrase(PrayPhrase.Id, PrayPhrase.ExposeDate, PrayPhrase.PraryBeginMesssage, PrayPhrase.PrayContent1, PrayPhrase.PrayContent2, PrayPhrase.PrayContent3, PrayPhrase.PraryEndMesssage, PrayPhrase.PrayTitle, PrayPhrase.BiblePhrase);
//         //}

//         //public void DeletePrayPhrase(int Id)
//         //{
//         //    ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();

//         //    controlPrayPhrase.DeletePrayPhrase(Id);
//         //}

//     }
// }
