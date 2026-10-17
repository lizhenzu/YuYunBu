using System.Collections.Generic;
using BulletinBoard.ViewModels.AddressBook;

namespace BulletinBoard.Services.Abstract
{
    public interface IAddressBookService
    {
        List<AddressBookPersonCardVM> Search(string q, int take = 30);
    }
}
