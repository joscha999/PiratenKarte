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
- Try to clamp map position to sensible world
- Add cleanup routine for invalid tokens
- Add custom input component for lat, lon (which can direct to map for selection)
- Add custom input component for select/combobox (should include auto-complete)
- Add password features to FormInput
- Add custom input component for checkboxes (clicking on the text should also set the checkbox)
- Split up update user/update user password, currently updating password also updates other data
- Add +/create button on lists