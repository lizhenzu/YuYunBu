public interface IUserUiPreferenceService
{
    char? GetEffectiveUserType(HttpContext http, string? userName);
    void SaveUserTypeForCurrent(HttpContext http, string? userName, char userType);
    void PromoteIfNeeded(HttpContext http, string userName);

    void ClearUserTypeForCurrent(HttpContext http, string? userName); // ✅ 추가

    Dictionary<string, string> GetMenuPolicyMap();
    bool IsMenuAllowed(Dictionary<string, string> map, char userType, string menuUrl);
}