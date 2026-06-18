# TODO - Google Login Integration (Acoustican)

- [ ] Inspect existing admin login JS (`wwwroot/js/admin.js`) to match token storage/flow.
- [ ] Inspect auth model (`Models/AdminUser.cs`) to understand fields for Google sign-in.
- [ ] Add Google OAuth config to `src/Acoustican.Web/appsettings.json`.
- [ ] Update `src/Acoustican.Web/Acoustican.Web.csproj` with `Microsoft.AspNetCore.Authentication.Google` (+ any cookie auth deps).
- [ ] Update `src/Acoustican.Web/Program.cs`:
  - add cookie auth + Google auth
  - keep existing JWT bearer for API authorization
- [ ] Add Google login/callback endpoints (new controller or extend `AuthController`).
- [ ] Implement DB upsert for Google users:
  - find by Google email
  - if missing, create `AdminUser` with `Role="User"` and `IsActive=true`
  - update `LastLoginAt`
  - issue existing JWT via `GenerateJwtToken`
- [ ] Update `Views/Admin/Login.cshtml` UI with “Login with Google” button.
- [ ] Update `wwwroot/js/admin.js` to call Google login endpoint and store JWT in `localStorage`.
- [ ] Build and run the project; verify the complete flow.

