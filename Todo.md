# PiratenKarte TODO:
## TODO
- Gruppen (User ist teil von)
- Sichtbarkeit (Öffentlich, BV, LV, KV ...)

- Filter auf list
- MoveManyToStorage => Modal
- Toasts für erfolgreich/fehler: https://github.com/Blazored/Toast
- GPS für map: https://www.nuget.org/packages/Blazor.Geolocation.WebAssembly

## Issues
- Permission Groups (grouping together permissions + inheritance)
- Check results on HTTP requests (use extension ReadResultAsync, OneOf)
- Make comments use logged in user
- Check login if loaded from local storage
- Coordinates need to be clamped (or modulo'd) to world space
- Add cleanup routine for invalid tokens
- Add custom input component for lat, lon (which can direct to map for selection)
- Add custom input component for select/combobox (should include auto-complete)
- Add custom input component for checkboxes (clicking on the text should also set the checkbox)
- Add custom input component for password (including option for password repeat)
- Split up update user/update user password, currently updating password also updates other data
- Add +/create button on lists
- Server side validations for create/updates
- New user needs double password input
- Auto enable lower tier permissions if higher tier is set (e.g. _update enables _read)
- Local validation for username on create, update (i.e. making sure it's available)
- Check for a possible compression for the OneOfJsonConverter (e.g. hashing assembly, type names on both ends)
- Disable accounts if user didn't log in for some time
- Add claims (Required now: "")