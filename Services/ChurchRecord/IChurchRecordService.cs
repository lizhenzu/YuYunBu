using BulletinBoard.ViewModels;

namespace BulletinBoard.Services.Abstract
{
    public interface IChurchRecordService
    {
        ChurchRecordHomeVM GetHome(string? searchName = null);
        ClassRosterVM GetClassRoster(int year, int ban, string? searchName = null);

        // ✅ 추가
        PersonEditVM? GetPersonForEdit(int personId);
        bool UpdatePerson(PersonEditVM vm, out string message);
    }
}
